<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">{{ t('polls.title') }}</div>
      <q-space />
      <q-btn color="primary" icon="add" :label="t('polls.newPoll')" @click="showCreateDialog = true" v-if="authStore.isManager" />
    </div>

    <div class="row q-col-gutter-md">
      <div class="col-12 col-md-6" v-for="poll in polls" :key="poll.id">
        <PollItem
          :poll="poll"
          :selected-option="selectedOptions[poll.id]"
          :has-voted="hasVoted(poll.id)"
          :show-admin-controls="authStore.isManager"
          @update:selected-option="val => selectedOptions[poll.id] = val"
          @vote="vote"
          @view-results="viewResults"
          @edit="editPoll"
          @delete="deletePoll"
        />
      </div>
    </div>

    <div v-if="polls.length === 0" class="text-center text-grey q-pa-xl">
      {{ t('polls.noPolls') }}
    </div>

    <q-dialog v-model="showCreateDialog" persistent>
      <q-card style="min-width: 500px">
        <q-card-section>
          <div class="text-h6">{{ editingPoll ? t('polls.editPoll') : t('polls.newPoll') }}</div>
        </q-card-section>

        <q-card-section>
          <q-form class="q-gutter-md">
            <q-input v-model="newPoll.title" :label="t('common.required')" outlined :rules="[v => !!v || t('common.required')]" />
            <q-input v-model="newPoll.description" :label="t('incidents.description')" type="textarea" outlined />
            <q-input v-model="newPoll.optionsText" :label="t('polls.options')" type="textarea" outlined rows="3" :rules="[v => !!v || t('common.required')]" />
            <q-select v-model="newPoll.pollType" :options="pollTypeOptions" :label="t('polls.type')" outlined emit-value map-options />
            <div class="row q-col-gutter-md">
              <div class="col-6">
                <q-input v-model="newPoll.startsAt" :label="t('polls.startsAt')" type="datetime-local" outlined />
              </div>
              <div class="col-6">
                <q-input v-model="newPoll.endsAt" :label="t('polls.endsAt')" type="datetime-local" outlined />
              </div>
            </div>
            <q-toggle v-model="newPoll.requiresJustification" :label="t('polls.requiresJustification')" />
          </q-form>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat :label="t('common.cancel')" v-close-popup />
          <q-btn color="primary" :label="t('common.save')" @click="savePoll" :loading="loading" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { usePollStore } from '../stores/poll'
import { useCondominiumStore } from '../stores/condominium'
import { useAuthStore } from '../stores/auth'
import { useI18n } from '../composables/useI18n'
import PollItem from '../components/PollItem.vue'

const $q = useQuasar()
const pollStore = usePollStore()
const condoStore = useCondominiumStore()
const authStore = useAuthStore()
const { t } = useI18n()

const polls = ref([])
const showCreateDialog = ref(false)
const loading = ref(false)
const selectedOptions = ref({})
const votedPolls = ref([])
const editingPoll = ref(null)

const newPoll = ref({
  title: '',
  description: '',
  optionsText: '',
  pollType: 1,
  startsAt: new Date().toISOString().slice(0, 16),
  endsAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().slice(0, 16),
  requiresJustification: false,
  minRoleToVote: 2
})

const pollTypeOptions = [
  { label: 'Opción única', value: 1 },
  { label: 'Multiple', value: 2 }
]

onMounted(async () => {
  const condos = await condoStore.fetchAll()
  if (condos.length > 0) {
    polls.value = await pollStore.fetchByCondominium(condos[0].id)
  }
})

function editPoll(poll) {
  editingPoll.value = poll
  newPoll.value = {
    title: poll.title,
    description: poll.description,
    optionsText: JSON.parse(poll.options).join('\n'),
    pollType: poll.pollType,
    startsAt: poll.startsAt.slice(0, 16),
    endsAt: poll.endsAt.slice(0, 16),
    requiresJustification: poll.requiresJustification,
    minRoleToVote: poll.minRoleToVote
  }
  showCreateDialog.value = true
}

async function savePoll() {
  loading.value = true
  try {
    const condos = await condoStore.fetchAll()
    if (condos.length > 0) {
      const data = {
        ...newPoll.value,
        options: JSON.stringify(newPoll.value.optionsText.split('\n').filter(o => o.trim())),
        startsAt: new Date(newPoll.value.startsAt).toISOString(),
        endsAt: new Date(newPoll.value.endsAt).toISOString()
      }

      if (editingPoll.value) {
        await pollStore.update(editingPoll.value.id, data)
        $q.notify({ type: 'positive', message: t('common.success') })
      } else {
        await pollStore.create(condos[0].id, data)
        $q.notify({ type: 'positive', message: t('common.success') })
      }
      
      showCreateDialog.value = false
      polls.value = await pollStore.fetchByCondominium(condos[0].id)
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    loading.value = false
    editingPoll.value = null
  }
}

async function deletePoll(poll) {
  $q.dialog({
    title: t('common.confirmDelete'),
    message: t('common.deleteMessage').replace('{item}', poll.title),
    cancel: true
  }).onOk(async () => {
    await pollStore.remove(poll.id)
    const condos = await condoStore.fetchAll()
    if (condos.length > 0) {
      polls.value = await pollStore.fetchByCondominium(condos[0].id)
    }
    $q.notify({ type: 'positive', message: t('common.success') })
  })
}

async function vote(poll) {
  const optionIndex = selectedOptions.value[poll.id]
  if (optionIndex === undefined) {
    $q.notify({ type: 'warning', message: t('polls.selectOption') })
    return
  }
  
  const result = await pollStore.vote(poll.id, optionIndex)
  if (result) {
    $q.notify({ type: 'positive', message: t('polls.voteSuccess') })
    votedPolls.value.push(poll.id)
  } else {
    $q.notify({ type: 'negative', message: t('polls.voteError') })
  }
}

async function viewResults(poll) {
  const results = await pollStore.getResults(poll.id)
  if (results) {
    const options = JSON.parse(poll.options)
    let message = ''
    options.forEach((opt, idx) => {
      const count = results.results[idx] || 0
      message += `${opt}: ${count} votos\n`
    })
    $q.dialog({
      title: `Resultados: ${poll.title}`,
      message,
      ok: 'Cerrar'
    })
  }
}

function hasVoted(pollId) {
  return votedPolls.value.includes(pollId)
}
</script>
