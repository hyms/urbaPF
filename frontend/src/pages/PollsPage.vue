<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">{{ t('polls.title') }}</div>
      <q-space />
      <q-btn
        v-if="authStore.isManager"
        color="primary"
        icon="add"
        :label="t('polls.newPoll')"
        @click="openCreateDialog"
      />
    </div>

    <q-tabs v-model="activeTab" class="q-mb-md" align="left" dense>
      <q-tab name="active" :label="t('polls.active')" />
      <q-tab name="finished" :label="t('polls.finished')" />
      <q-tab name="mine" :label="t('polls.myProposals')" />
    </q-tabs>

    <q-tab-panels v-model="activeTab" animated>
      <q-tab-panel name="active" class="q-pa-none">
        <div class="row q-col-gutter-md">
          <template v-if="loading">
            <div class="col-12 col-md-6" v-for="n in 4" :key="n">
              <q-card>
                <q-card-section>
                  <q-skeleton type="text" class="q-mb-sm" style="width: 60%" />
                  <q-skeleton type="text" class="q-mb-sm" style="width: 40%" />
                  <q-skeleton type="text" style="width: 80%" />
                </q-card-section>
                <q-separator />
                <q-card-section>
                  <q-skeleton type="rect" height="60px" />
                </q-card-section>
              </q-card>
            </div>
          </template>
          <template v-else>
            <div
              v-for="poll in filteredPolls.active"
              :key="poll.id"
              class="col-12 col-md-6"
            >
              <PollItem
                :poll="poll"
                :selected-option="selectedOptions[poll.id]"
                :has-voted="hasVoted(poll.id)"
                :can-manage="authStore.isManager"
                :poll-results="pollResults[poll.id]"
                @update:selected-option="val => selectedOptions[poll.id] = val"
                @vote="vote"
                @edit="editPoll"
                @delete="deletePoll"
              />
            </div>
            <div v-if="filteredPolls.active.length === 0" class="col-12 text-center text-grey q-pa-xl">
              {{ t('polls.noPolls') }}
            </div>
          </template>
        </div>
      </q-tab-panel>

      <q-tab-panel name="finished" class="q-pa-none">
        <div class="row q-col-gutter-md">
          <template v-if="loading">
            <div class="col-12 col-md-6" v-for="n in 4" :key="n">
              <q-card>
                <q-card-section>
                  <q-skeleton type="text" class="q-mb-sm" style="width: 60%" />
                  <q-skeleton type="text" class="q-mb-sm" style="width: 40%" />
                </q-card-section>
                <q-separator />
                <q-card-section>
                  <q-skeleton type="rect" height="80px" />
                </q-card-section>
              </q-card>
            </div>
          </template>
          <template v-else>
            <div
              v-for="poll in filteredPolls.finished"
              :key="poll.id"
              class="col-12 col-md-6"
            >
              <PollItem
                :poll="poll"
                :has-voted="hasVoted(poll.id)"
                :can-manage="authStore.isManager"
                :poll-results="pollResults[poll.id]"
                @edit="editPoll"
                @delete="deletePoll"
              />
            </div>
            <div v-if="filteredPolls.finished.length === 0" class="col-12 text-center text-grey q-pa-xl">
              {{ t('polls.noPolls') }}
            </div>
          </template>
        </div>
      </q-tab-panel>

      <q-tab-panel name="mine" class="q-pa-none">
        <div class="row q-col-gutter-md">
          <template v-if="loading">
            <div class="col-12 col-md-6" v-for="n in 2" :key="n">
              <q-card>
                <q-card-section>
                  <q-skeleton type="text" class="q-mb-sm" style="width: 60%" />
                </q-card-section>
              </q-card>
            </div>
          </template>
          <template v-else>
            <div
              v-for="poll in filteredPolls.mine"
              :key="poll.id"
              class="col-12 col-md-6"
            >
              <PollItem
                :poll="poll"
                :has-voted="hasVoted(poll.id)"
                :can-manage="authStore.isManager"
                :poll-results="pollResults[poll.id]"
                @edit="editPoll"
                @delete="deletePoll"
              />
            </div>
            <div v-if="filteredPolls.mine.length === 0" class="col-12 text-center text-grey q-pa-xl">
              {{ t('polls.noPolls') }}
            </div>
          </template>
        </div>
      </q-tab-panel>
    </q-tab-panels>

    <q-dialog v-model="showDialog" persistent>
      <PollForm
        :poll="editingPoll"
        :loading="saving"
        @submit="savePoll"
        @cancel="closeDialog"
      />
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { usePollStore } from '../stores/poll'
import { useCondominiumStore } from '../stores/condominium'
import { useAuthStore } from '../stores/auth'
import { useI18n } from '../composables/useI18n'
import PollItem from '../components/PollItem.vue'
import PollForm from '../components/PollForm.vue'

const $q = useQuasar()
const pollStore = usePollStore()
const condoStore = useCondominiumStore()
const authStore = useAuthStore()
const { t } = useI18n()

const activeTab = ref('active')
const loading = ref(true)
const saving = ref(false)
const polls = ref([])
const showDialog = ref(false)
const editingPoll = ref(null)
const selectedOptions = ref({})
const votedPolls = ref([])
const pollResults = ref({})

const filteredPolls = computed(() => {
  const now = new Date()
  const active = []
  const finished = []
  const mine = []

  for (const poll of polls.value) {
    const endsAt = new Date(poll.endsAt)
    const isActive = poll.status === 3 || (poll.status === 2 && endsAt > now)
    const isMine = poll.createdBy === authStore.user?.id

    if (isActive) {
      active.push(poll)
    } else {
      finished.push(poll)
    }

    if (isMine) {
      mine.push(poll)
    }
  }

  return { active, finished, mine }
})

onMounted(async () => {
  await fetchPolls()
})

async function fetchPolls() {
  loading.value = true
  try {
    const condos = await condoStore.fetchAll()
    if (condos.length > 0) {
      polls.value = await pollStore.fetchByCondominium(condos[0].id)
      for (const poll of polls.value) {
        if (poll.status >= 4) {
          pollResults.value[poll.id] = await pollStore.getResults(poll.id)
        }
      }
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    loading.value = false
  }
}

function openCreateDialog() {
  editingPoll.value = null
  showDialog.value = true
}

function closeDialog() {
  showDialog.value = false
  editingPoll.value = null
}

function editPoll(poll) {
  editingPoll.value = {
    ...poll,
    options: JSON.parse(poll.options)
  }
  showDialog.value = true
}

async function savePoll(data) {
  saving.value = true
  try {
    const condos = await condoStore.fetchAll()
    if (condos.length > 0) {
      if (editingPoll.value) {
        await pollStore.update(editingPoll.value.id, data)
        $q.notify({ type: 'positive', message: t('common.success') })
      } else {
        await pollStore.create(condos[0].id, data)
        $q.notify({ type: 'positive', message: t('common.success') })
      }

      closeDialog()
      await fetchPolls()
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    saving.value = false
  }
}

async function deletePoll(poll) {
  $q.dialog({
    title: t('common.confirmDelete'),
    message: t('common.deleteMessage').replace('{item}', poll.title),
    cancel: true
  }).onOk(async () => {
    try {
      await pollStore.remove(poll.id)
      await fetchPolls()
      $q.notify({ type: 'positive', message: t('common.success') })
    } catch (e) {
      $q.notify({ type: 'negative', message: t('common.error') })
    }
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
    pollResults.value[poll.id] = await pollStore.getResults(poll.id)
  } else {
    $q.notify({ type: 'negative', message: t('polls.voteError') })
  }
}

function hasVoted(pollId) {
  return votedPolls.value.includes(pollId)
}
</script>
