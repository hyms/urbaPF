<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">{{ t('incidents.title') }}</div>
      <q-space />
      <q-btn color="primary" icon="add" :label="t('incidents.report')" @click="showCreateDialog = true" />
    </div>

    <q-card class="q-mb-md">
      <q-card-section>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-sm-4">
            <q-select
              v-model="filterStatus"
              :options="statusOptions"
              :label="t('incidents.filterStatus')"
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
        <IncidentItem v-for="incident in filteredIncidents" :key="incident.id" :incident="incident" @click="viewIncident">
          <template #menu>
            <q-btn flat round dense icon="more_vert" @click.stop>
              <q-menu>
                <q-list dense style="min-width: 150px">
                  <q-item clickable v-close-popup @click="viewIncident(incident)">
                    <q-item-section>{{ t('incidents.viewDetails') }}</q-item-section>
                  </q-item>
                  <q-item clickable v-close-popup @click="updateStatus(incident)" v-if="authStore.isManager">
                    <q-item-section>{{ t('incidents.updateStatus') }}</q-item-section>
                  </q-item>
                </q-list>
              </q-menu>
            </q-btn>
          </template>
        </IncidentItem>
        <q-item v-if="filteredIncidents.length === 0">
          <q-item-section class="text-grey text-center">
            {{ t('incidents.noIncidents') }}
          </q-item-section>
        </q-item>
      </q-list>
    </q-card>

    <q-dialog v-model="showCreateDialog" persistent>
      <q-card style="min-width: 400px">
        <q-card-section>
          <div class="text-h6">{{ t('incidents.reportTitle') }}</div>
        </q-card-section>

        <q-card-section>
          <q-form @submit="createIncident" class="q-gutter-md">
            <q-input
              v-model="newIncident.title"
              label="Título"
              outlined
              :rules="[val => !!val || t('common.required')]"
            />
            <q-input
              v-model="newIncident.description"
              :label="t('incidents.description')"
              type="textarea"
              outlined
              :rules="[val => !!val || t('common.required')]"
            />
            <q-select
              v-model="newIncident.type"
              :options="typeOptions"
              :label="t('incidents.type')"
              outlined
              emit-value
              map-options
            />
            <q-select
              v-model="newIncident.priority"
              :options="priorityOptions"
              :label="t('incidents.priority')"
              outlined
              emit-value
              map-options
            />
            <q-input
              v-model="newIncident.locationDescription"
              :label="t('incidents.location')"
              outlined
            />
            <q-input
              v-model="newIncident.occurredAt"
              :label="t('incidents.date')"
              type="datetime-local"
              outlined
            />
          </q-form>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat :label="t('common.cancel')" v-close-popup />
          <q-btn color="primary" :label="t('incidents.report')" @click="createIncident" :loading="loading" />
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
import { useCondominiumStore } from '../stores/condominium'
import { useI18n } from '../composables/useI18n'
import IncidentItem from '../components/IncidentItem.vue'
import { getIncidentStatusLabel, getPriorityLabel } from '../utils/format'

const $q = useQuasar()
const incidentStore = useIncidentStore()
const authStore = useAuthStore()
const condoStore = useCondominiumStore()
const { t } = useI18n()

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
  const condos = await condoStore.fetchAll()
  if (condos.length > 0) {
    incidents.value = await incidentStore.fetchByCondominium(condos[0].id)
  }
})

async function createIncident() {
  loading.value = true
  try {
    const condos = await condoStore.fetchAll()
    if (condos.length > 0) {
      await incidentStore.create(condos[0].id, {
        ...newIncident.value,
        type: parseInt(newIncident.value.type),
        priority: parseInt(newIncident.value.priority),
        occurredAt: new Date(newIncident.value.occurredAt).toISOString()
      })
      $q.notify({ type: 'positive', message: t('incidents.reportSuccess') })
      showCreateDialog.value = false
      incidents.value = await incidentStore.fetchByCondominium(condos[0].id)
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: t('incidents.reportError') })
  } finally {
    loading.value = false
  }
}

function viewIncident(incident) {
  $q.dialog({
    title: incident.title,
    message: `${incident.description}\n\n${t('incidents.reportedBy')}: ${incident.reportedByName}\n${t('incidents.status')}: ${getIncidentStatusLabel(incident.status)}\n${t('incidents.priority')}: ${getPriorityLabel(incident.priority)}`,
    ok: t('common.yes')
  })
}

function updateStatus(incident) {
  $q.dialog({
    title: t('incidents.updateStatus'),
    options: {
      type: 'radio',
      model: incident.status,
      items: statusOptions
    },
    cancel: true
  }).onOk(async (status) => {
    await incidentStore.updateStatus(incident.id, status)
    const condos = await condoStore.fetchAll()
    if (condos.length > 0) {
      incidents.value = await incidentStore.fetchByCondominium(condos[0].id)
    }
  })
}
</script>
