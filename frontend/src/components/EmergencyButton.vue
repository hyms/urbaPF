<template>
  <q-page class="q-pa-md flex flex-center">
    <div class="text-center">
      <div class="text-h5 q-mb-md text-grey-8">Botón de Emergencia</div>
      <div class="text-body2 q-mb-lg text-grey-6">
        Mantén presionado por 3 segundos para enviar una alerta de emergencia a todo el condominio
      </div>

      <q-btn
        unelevated
        :color="isPressed ? 'red-9' : 'red'"
        :text-color="isPressed ? 'white' : 'white'"
        :label="buttonLabel"
        :loading="loading"
        :disable="loading"
        class="emergency-btn"
        @mousedown="startPress"
        @mouseup="cancelPress"
        @mouseleave="cancelPress"
        @touchstart.prevent="startPress"
        @touchend.prevent="cancelPress"
      >
        <q-icon name="emergency" size="48px" class="q-mr-sm" />
      </q-btn>

      <div v-if="lastAlert" class="q-mt-lg">
        <q-banner class="bg-grey-3 rounded-borders">
          <template v-slot:avatar>
            <q-icon name="info" color="primary" />
          </template>
          <div class="text-body2">
            Tu última alerta: <strong>{{ lastAlert.title }}</strong>
            <div class="text-caption">
              Estado: {{ getStatusLabel(lastAlert.status) }}
              <span v-if="lastAlert.needs_approval && lastAlert.status === 1">
                (Pendiente de aprobación)
              </span>
            </div>
          </div>
        </q-banner>
      </div>
    </div>

    <q-dialog v-model="showConfirmDialog">
      <q-card style="min-width: 350px">
        <q-card-section>
          <div class="text-h6 text-negative">Confirmar Emergencia</div>
        </q-card-section>
        <q-card-section>
          <q-form class="q-gutter-md">
            <q-select
              v-model="alertForm.type"
              :options="typeOptions"
              label="Tipo de emergencia"
              outlined
              emit-value
              map-options
            />
            <q-input
              v-model="alertForm.title"
              label="Título breve"
              outlined
              :rules="[v => !!v || 'Requerido']"
            />
            <q-input
              v-model="alertForm.description"
              label="Descripción (opcional)"
              type="textarea"
              outlined
              rows="2"
            />
          </q-form>
        </q-card-section>
        <q-card-actions align="right">
          <q-btn flat label="Cancelar" v-close-popup />
          <q-btn color="negative" label="Enviar Alerta" @click="confirmAlert" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useQuasar } from 'quasar'
import { useAlertStore } from '../stores/alert'
import { useCondominiumStore } from '../stores/condominium'

const $q = useQuasar()
const alertStore = useAlertStore()
const condoStore = useCondominiumStore()

const loading = ref(false)
const isPressed = ref(false)
const showConfirmDialog = ref(false)
const lastAlert = ref<any>(null)

const alertForm = ref({
  type: 1,
  title: '',
  description: ''
})

const typeOptions = [
  { label: 'Emergencia General', value: 1 },
  { label: 'Robo / Asalto', value: 2 },
  { label: 'Incendio', value: 3 },
  { label: 'Emergencia Médica', value: 4 },
  { label: 'Otro', value: 5 }
]

const buttonLabel = ref('MANTENER 3s')

let countdownInterval: ReturnType<typeof setInterval> | null = null
let countdown = 3

function startPress() {
  if (loading.value) return
  
  isPressed.value = true
  countdown = 3
  buttonLabel.value = `MANTENER ${countdown}s`
  
  countdownInterval = setInterval(() => {
    countdown--
    if (countdown > 0) {
      buttonLabel.value = `MANTENER ${countdown}s`
    } else {
      clearInterval(countdownInterval!)
      countdownInterval = null
      triggerAlert()
    }
  }, 1000)
}

function cancelPress() {
  isPressed.value = false
  buttonLabel.value = 'MANTENER 3s'
  
  if (countdownInterval) {
    clearInterval(countdownInterval)
    countdownInterval = null
  }
}

function triggerAlert() {
  isPressed.value = false
  buttonLabel.value = 'MANTENER 3s'
  showConfirmDialog.value = true
}

async function confirmAlert() {
  loading.value = true
  try {
    const condos = await condoStore.fetchAll()
    if (condos.length > 0) {
      const alertId = await alertStore.panicButton(condos[0].id, {
        type: alertForm.value.type,
        title: alertForm.value.title,
        description: alertForm.value.description,
        location: null
      })

      if (alertId) {
        $q.notify({
          type: 'positive',
          message: 'Alerta de emergencia enviada',
          caption: 'Los encargados han sido notificados'
        })
        
        const alerts = await alertStore.fetchByCondominium(condos[0].id)
        lastAlert.value = alerts[0]
      } else {
        $q.notify({
          type: 'negative',
          message: 'Error al enviar la alerta'
        })
      }
    }
  } catch (e) {
    $q.notify({
      type: 'negative',
      message: 'Error al enviar la alerta'
    })
  } finally {
    loading.value = false
    showConfirmDialog.value = false
    alertForm.value = { type: 1, title: '', description: '' }
  }
}

function getStatusLabel(status: number) {
  const labels: Record<number, string> = {
    1: 'Pendiente',
    2: 'Aprobada',
    3: 'Notificada',
    4: 'En Proceso',
    5: 'Resuelta',
    6: 'Cancelada'
  }
  return labels[status] || 'Desconocido'
}

onMounted(async () => {
  const condos = await condoStore.fetchAll()
  if (condos.length > 0) {
    const alerts = await alertStore.fetchByCondominium(condos[0].id)
    if (alerts.length > 0) {
      lastAlert.value = alerts[0]
    }
  }
})

onUnmounted(() => {
  if (countdownInterval) {
    clearInterval(countdownInterval)
  }
})
</script>

<style scoped>
.emergency-btn {
  width: 200px;
  height: 200px;
  border-radius: 50%;
  font-size: 18px;
  font-weight: bold;
  transition: all 0.2s ease;
  box-shadow: 0 8px 24px rgba(211, 47, 47, 0.4);
}

.emergency-btn:active {
  transform: scale(0.95);
  box-shadow: 0 4px 12px rgba(211, 47, 47, 0.3);
}
</style>
