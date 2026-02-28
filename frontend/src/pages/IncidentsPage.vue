<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">Incidentes</div>
      <q-space />
      <q-btn color="primary" icon="add" label="Reportar" @click="showCreateDialog = true" />
    </div>

    <q-card class="q-mb-md">
      <q-card-section>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-sm-4">
            <q-select
              v-model="filterStatus"
              :options="statusOptions"
              label="Filtrar por estado"
              emit-value
              map-options
              clearable
              dense
              outlined
            />
          </div>
        </div>
      </q-card-section>
    </q-card>

    <q-card>
      <q-list separator>
        <q-item v-for="incident in filteredIncidents" :key="incident.id" clickable v-ripple>
          <q-item-section avatar>
            <q-avatar :color="getPriorityColor(incident.priority)" text-color="white">
              <q-icon name="warning" />
            </q-avatar>
          </q-item-section>
          <q-item-section>
            <q-item-label>{{ incident.title }}</q-item-label>
            <q-item-label caption>
              {{ incident.description }}<br>
              Reportado por: {{ incident.reportedByName }} | {{ formatDate(incident.createdAt) }}
            </q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-chip :color="getStatusColor(incident.status)" text-color="white" size="sm">
              {{ getStatusLabel(incident.status) }}
            </q-chip>
          </q-item-section>
          <q-item-section side>
            <q-btn flat round dense icon="more_vert">
              <q-menu>
                <q-list dense style="min-width: 150px">
                  <q-item clickable v-close-popup @click="viewIncident(incident)">
                    <q-item-section>Ver detalles</q-item-section>
                  </q-item>
                  <q-item clickable v-close-popup @click="updateStatus(incident)" v-if="authStore.isManager">
                    <q-item-section>Actualizar estado</q-item-section>
                  </q-item>
                </q-list>
              </q-menu>
            </q-btn>
          </q-item-section>
        </q-item>
        <q-item v-if="filteredIncidents.length === 0">
          <q-item-section class="text-grey text-center">
            No hay incidentes
          </q-item-section>
        </q-item>
      </q-list>
    </q-card>

    <q-dialog v-model="showCreateDialog" persistent>
      <q-card style="min-width: 400px">
        <q-card-section>
          <div class="text-h6">Reportar Incidente</div>
        </q-card-section>

        <q-card-section>
          <q-form @submit="createIncident" class="q-gutter-md">
            <q-input
              v-model="newIncident.title"
              label="Título"
              outlined
              :rules="[val => !!val || 'Requerido']"
            />
            <q-input
              v-model="newIncident.description"
              label="Descripción"
              type="textarea"
              outlined
              :rules="[val => !!val || 'Requerido']"
            />
            <q-select
              v-model="newIncident.type"
              :options="typeOptions"
              label="Tipo"
              outlined
              emit-value
              map-options
            />
            <q-select
              v-model="newIncident.priority"
              :options="priorityOptions"
              label="Prioridad"
              outlined
              emit-value
              map-options
            />
            <q-input
              v-model="newIncident.locationDescription"
              label="Ubicación"
              outlined
            />
            <q-input
              v-model="newIncident.occurredAt"
              label="Fecha del incidente"
              type="datetime-local"
              outlined
            />
          </q-form>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat label="Cancelar" v-close-popup />
          <q-btn color="primary" label="Reportar" @click="createIncident" :loading="loading" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useIncidentStore } from '../stores/incident'
import { useAuthStore } from '../stores/auth'

const $q = useQuasar()
const incidentStore = useIncidentStore()
const authStore = useAuthStore()

const incidents = ref([])
const filterStatus = ref(null)
const showCreateDialog = ref(false)
const loading = ref(false)

const newIncident = ref({
  title: '',
  description: '',
  type: 1,
  priority: 2,
  locationDescription: '',
  occurredAt: new Date().toISOString().slice(0, 16)
})

const statusOptions = [
  { label: 'Reportado', value: 1 },
  { label: 'En proceso', value: 2 },
  { label: 'Pendiente', value: 3 },
  { label: 'Resuelto', value: 4 }
]

const typeOptions = [
  { label: 'Infraestructura', value: 1 },
  { label: 'Seguridad', value: 2 },
  { label: 'Limpieza', value: 3 },
  { label: 'Ruidos', value: 4 },
  { label: 'Otro', value: 5 }
]

const priorityOptions = [
  { label: 'Baja', value: 1 },
  { label: 'Media', value: 2 },
  { label: 'Alta', value: 3 },
  { label: 'Urgente', value: 4 }
]

const filteredIncidents = computed(() => {
  if (!filterStatus.value) return incidents.value
  return incidents.value.filter(i => i.status === filterStatus.value)
})

onMounted(async () => {
  const condos = await useCondominiumStore().fetchAll()
  if (condos.length > 0) {
    incidents.value = await incidentStore.fetchByCondominium(condos[0].id)
  }
})

async function createIncident() {
  loading.value = true
  try {
    const condos = await useCondominiumStore().fetchAll()
    if (condos.length > 0) {
      await incidentStore.create(condos[0].id, {
        ...newIncident.value,
        type: parseInt(newIncident.value.type),
        priority: parseInt(newIncident.value.priority),
        occurredAt: new Date(newIncident.value.occurredAt).toISOString()
      })
      $q.notify({ type: 'positive', message: 'Incidente reportado exitosamente' })
      showCreateDialog.value = false
      incidents.value = await incidentStore.fetchByCondominium(condos[0].id)
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: 'Error al reportar incidente' })
  } finally {
    loading.value = false
  }
}

function viewIncident(incident) {
  $q.dialog({
    title: incident.title,
    message: `${incident.description}\n\nReportado por: ${incident.reportedByName}\nEstado: ${getStatusLabel(incident.status)}\nPrioridad: ${getPriorityLabel(incident.priority)}`,
    ok: 'Cerrar'
  })
}

function updateStatus(incident) {
  $q.dialog({
    title: 'Actualizar Estado',
    options: {
      type: 'radio',
      model: incident.status,
      items: statusOptions
    },
    cancel: true
  }).onOk(async (status) => {
    await incidentStore.updateStatus(incident.id, status)
    const condos = await useCondominiumStore().fetchAll()
    if (condos.length > 0) {
      incidents.value = await incidentStore.fetchByCondominium(condos[0].id)
    }
  })
}

function getStatusLabel(status) {
  return incidentStore.getStatusLabel(status)
}

function getStatusColor(status) {
  const colors = { 1: 'blue', 2: 'orange', 3: 'yellow', 4: 'green', 5: 'grey', 6: 'red' }
  return colors[status] || 'grey'
}

function getPriorityLabel(priority) {
  return incidentStore.getPriorityLabel(priority)
}

function getPriorityColor(priority) {
  const colors = { 1: 'green', 2: 'yellow', 3: 'orange', 4: 'red' }
  return colors[priority] || 'green'
}

function formatDate(date) {
  return new Date(date).toLocaleDateString('es-ES')
}
</script>
