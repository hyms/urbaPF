<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">Votaciones</div>
      <q-space />
      <q-btn color="primary" icon="add" label="Nueva" @click="showCreateDialog = true" />
    </div>

    <div class="row q-col-gutter-md">
      <div class="col-12 col-md-6" v-for="poll in polls" :key="poll.id">
        <q-card>
          <q-card-section>
            <div class="row items-center">
              <div class="col">
                <div class="text-h6">{{ poll.title }}</div>
                <div class="text-caption text-grey">{{ poll.description }}</div>
              </div>
              <q-chip :color="getStatusColor(poll.status)" text-color="white" size="sm">
                {{ getStatusLabel(poll.status) }}
              </q-chip>
            </div>
          </q-card-section>
          <q-separator />
          <q-card-section>
            <div class="q-mb-sm">
              <strong>Opciones:</strong>
            </div>
            <q-option-group
              v-model="selectedOptions[poll.id]"
              :options="parseOptions(poll.options)"
              type="radio"
              :disable="poll.status !== 2"
            />
          </q-card-section>
          <q-separator />
          <q-card-actions>
            <q-btn flat color="primary" label="Votar" @click="vote(poll)" :disable="poll.status !== 2 || hasVoted(poll.id)" />
            <q-btn flat label="Ver resultados" @click="viewResults(poll)" />
            <q-space />
            <div class="text-caption text-grey">
              {{ formatDate(poll.startsAt) }} - {{ formatDate(poll.endsAt) }}
            </div>
          </q-card-actions>
        </q-card>
      </div>
    </div>

    <div v-if="polls.length === 0" class="text-center text-grey q-pa-xl">
      No hay votaciones disponibles
    </div>

    <q-dialog v-model="showCreateDialog" persistent>
      <q-card style="min-width: 500px">
        <q-card-section>
          <div class="text-h6">Nueva Votación</div>
        </q-card-section>

        <q-card-section>
          <q-form class="q-gutter-md">
            <q-input v-model="newPoll.title" label="Título" outlined :rules="[v => !!v || 'Requerido']" />
            <q-input v-model="newPoll.description" label="Descripción" type="textarea" outlined />
            <q-input v-model="newPoll.optionsText" label="Opciones (una por línea)" type="textarea" outlined rows="3" :rules="[v => !!v || 'Requerido']" />
            <q-select v-model="newPoll.pollType" :options="pollTypeOptions" label="Tipo de votación" outlined emit-value map-options />
            <div class="row q-col-gutter-md">
              <div class="col-6">
                <q-input v-model="newPoll.startsAt" label="Inicio" type="datetime-local" outlined />
              </div>
              <div class="col-6">
                <q-input v-model="newPoll.endsAt" label="Fin" type="datetime-local" outlined />
              </div>
            </div>
            <q-toggle v-model="newPoll.requiresJustification" label="Requiere justificación" />
          </q-form>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat label="Cancelar" v-close-popup />
          <q-btn color="primary" label="Crear" @click="createPoll" :loading="loading" />
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

const $q = useQuasar()
const pollStore = usePollStore()
const condoStore = useCondominiumStore()

const polls = ref([])
const showCreateDialog = ref(false)
const loading = ref(false)
const selectedOptions = ref({})
const votedPolls = ref([])

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

async function createPoll() {
  loading.value = true
  try {
    const condos = await condoStore.fetchAll()
    if (condos.length > 0) {
      await pollStore.create(condos[0].id, {
        ...newPoll.value,
        options: JSON.stringify(newPoll.value.optionsText.split('\n').filter(o => o.trim())),
        startsAt: new Date(newPoll.value.startsAt).toISOString(),
        endsAt: new Date(newPoll.value.endsAt).toISOString()
      })
      $q.notify({ type: 'positive', message: 'Votación creada' })
      showCreateDialog.value = false
      polls.value = await pollStore.fetchByCondominium(condos[0].id)
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: 'Error al crear' })
  } finally {
    loading.value = false
  }
}

async function vote(poll) {
  const optionIndex = selectedOptions.value[poll.id]
  if (optionIndex === undefined) {
    $q.notify({ type: 'warning', message: 'Selecciona una opción' })
    return
  }
  
  const result = await pollStore.vote(poll.id, optionIndex)
  if (result) {
    $q.notify({ type: 'positive', message: 'Voto registrado' })
    votedPolls.value.push(poll.id)
  } else {
    $q.notify({ type: 'negative', message: 'Error al votar' })
  }
}

async function viewResults(poll) {
  const results = await pollStore.getResults(poll)
  if (results) {
    const options = parseOptions(poll.options)
    let message = ''
    options.forEach((opt, idx) => {
      const count = results.results[idx] || 0
      message += `${opt.label}: ${count} votos\n`
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

function parseOptions(optionsStr) {
  try {
    const options = JSON.parse(optionsStr)
    return options.map((opt, idx) => ({ label: opt, value: idx }))
  } catch {
    return []
  }
}

function getStatusLabel(status) {
  return pollStore.getStatusLabel(status)
}

function getStatusColor(status) {
  const colors = { 1: 'grey', 2: 'green', 3: 'red', 4: 'orange' }
  return colors[status] || 'grey'
}

function formatDate(date) {
  return new Date(date).toLocaleDateString('es-ES', { day: 'numeric', month: 'short' })
}
</script>
