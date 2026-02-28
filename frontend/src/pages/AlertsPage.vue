<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">Alertas</div>
      <q-space />
      <q-btn color="red" icon="add" label="Nueva Alerta" @click="showCreateDialog = true" />
    </div>

    <q-card>
      <q-list separator>
        <q-item v-for="alert in alerts" :key="alert.id" :class="getAlertClass(alert.status)">
          <q-item-section avatar>
            <q-avatar :color="getTypeColor(alert.alertType)" text-color="white">
              <q-icon :name="getTypeIcon(alert.alertType)" />
            </q-avatar>
          </q-item-section>
          <q-item-section>
            <q-item-label>
              <q-chip :color="getStatusColor(alert.status)" text-color="white" size="sm" class="q-mr-sm">
                {{ getStatusLabel(alert.status) }}
              </q-chip>
              {{ getTypeLabel(alert.alertType) }}
            </q-item-label>
            <q-item-label>{{ alert.message }}</q-item-label>
            <q-item-label caption v-if="alert.destinationAddress">
              Destino: {{ alert.destinationAddress }}
            </q-item-label>
            <q-item-label caption>
              {{ alert.createdByName }} | {{ formatDate(alert.createdAt) }}
            </q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-btn flat round dense icon="more_vert">
              <q-menu>
                <q-list dense style="min-width: 150px">
                  <q-item clickable v-close-popup @click="updateStatus(alert, 2)" v-if="alert.status === 1">
                    <q-item-section>Acknowledged</q-item-section>
                  </q-item>
                  <q-item clickable v-close-popup @click="updateStatus(alert, 4)" v-if="alert.status === 2 || alert.status === 3">
                    <q-item-section>Marcar llegado</q-item-section>
                  </q-item>
                  <q-item clickable v-close-popup @click="updateStatus(alert, 5)">
                    <q-item-section>Completar</q-item-section>
                  </q-item>
                </q-list>
              </q-menu>
            </q-btn>
          </q-item-section>
        </q-item>
        <q-item v-if="alerts.length === 0">
          <q-item-section class="text-grey text-center">
            No hay alertas activas
          </q-item-section>
        </q-item>
      </q-list>
    </q-card>

    <q-dialog v-model="showCreateDialog" persistent>
      <q-card style="min-width: 400px">
        <q-card-section>
          <div class="text-h6 text-red">Nueva Alerta</div>
        </q-card-section>

        <q-card-section>
          <q-form class="q-gutter-md">
            <q-select v-model="newAlert.alertType" :options="typeOptions" label="Tipo de alerta" outlined emit-value map-options :rules="[v => !!v || 'Requerido']" />
            <q-input v-model="newAlert.message" label="Mensaje" type="textarea" outlined :rules="[v => !!v || 'Requerido']" />
            <q-input v-model="newAlert.destinationAddress" label="Dirección de destino" outlined />
            <q-input v-model="newAlert.estimatedArrival" label="Llegada estimada" type="datetime-local" outlined />
          </q-form>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat label="Cancelar" v-close-popup />
          <q-btn color="red" label="Enviar Alerta" @click="createAlert" :loading="loading" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useAlertStore } from '../stores/alert'
import { useCondominiumStore } from '../stores/condominium'

const $q = useQuasar()
const alertStore = useAlertStore()
const condoStore = useCondominiumStore()

const alerts = ref([])
const showCreateDialog = ref(false)
const loading = ref(false)

const newAlert = ref({
  alertType: 1,
  message: '',
  destinationAddress: '',
  estimatedArrival: new Date().toISOString().slice(0, 16)
})

const typeOptions = [
  { label: 'Emergencia', value: 1 },
  { label: 'Información', value: 2 },
  { label: 'Advertencia', value: 3 },
  { label: 'Otro', value: 4 }
]

onMounted(async () => {
  const condos = await condoStore.fetchAll()
  if (condos.length > 0) {
    alerts.value = await alertStore.fetchByCondominium(condos[0].id)
  }
})

async function createAlert() {
  loading.value = true
  try {
    const condos = await condoStore.fetchAll()
    if (condos.length > 0) {
      await alertStore.create(condos[0].id, {
        ...newAlert.value,
        alertType: parseInt(newAlert.value.alertType),
        estimatedArrival: new Date(newAlert.value.estimatedArrival).toISOString()
      })
      $q.notify({ type: 'positive', message: 'Alerta enviada' })
      showCreateDialog.value = false
      alerts.value = await alertStore.fetchByCondominium(condos[0].id)
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: 'Error al enviar' })
  } finally {
    loading.value = false
  }
}

async function updateStatus(alert, status) {
  await alertStore.updateStatus(alert.id, status)
  const condos = await condoStore.fetchAll()
  if (condos.length > 0) {
    alerts.value = await alertStore.fetchByCondominium(condos[0].id)
  }
}

function getTypeLabel(type) {
  return alertStore.getTypeLabel(type)
}

function getTypeColor(type) {
  const colors = { 1: 'red', 2: 'blue', 3: 'orange', 4: 'grey' }
  return colors[type] || 'grey'
}

function getTypeIcon(type) {
  const icons = { 1: 'emergency', 2: 'info', 3: 'warning', 4: 'help' }
  return icons[type] || 'notifications'
}

function getStatusLabel(status) {
  return alertStore.getStatusLabel(status)
}

function getStatusColor(status) {
  const colors = { 1: 'red', 2: 'orange', 3: 'blue', 4: 'green', 5: 'grey', 6: 'black' }
  return colors[status] || 'grey'
}

function getAlertClass(status) {
  if (status === 1) return 'bg-red-1'
  return ''
}

function formatDate(date) {
  return new Date(date).toLocaleString('es-ES')
}
</script>
