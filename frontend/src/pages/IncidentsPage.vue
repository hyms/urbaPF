<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">{{ t('incidents.title') }}</div>
      <q-space />
      <q-btn
        color="primary"
        icon="add"
        :label="t('incidents.report')"
        @click="openCreateDialog"
        :disable="authStore.isRestricted"
      />
    </div>

    <div class="row q-mb-md">
      <q-btn-toggle
        v-model="statusFilter"
        toggle-color="primary"
        :options="statusOptions"
        unelevated
      />
    </div>

    <div class="row q-col-gutter-md">
      <template v-if="loading">
        <div class="col-12 col-md-6" v-for="n in 4" :key="n">
          <q-card>
            <q-card-section>
              <q-skeleton type="text" class="q-mb-sm" style="width: 60%" />
              <q-skeleton type="text" class="q-mb-sm" style="width: 40%" />
            </q-card-section>
          </q-card>
        </div>
      </template>
      <template v-else>
        <div
          v-for="incident in filteredIncidents"
          :key="incident.id"
          class="col-12 col-md-6"
        >
          <IncidentCard
            :incident="incident"
            :canManage="canManage(incident)"
            @edit="editIncident"
            @delete="deleteIncident"
          >
            <template #actions>
              <q-btn
                v-if="incident.status === 1"
                flat
                color="orange"
                :label="t('incidents.markInProcess')"
                @click="updateStatus(incident.id, 2)"
              />
              <q-btn
                v-if="incident.status === 2"
                flat
                color="positive"
                :label="t('incidents.markResolved')"
                @click="resolveDialog(incident)"
              />
              <q-btn
                v-if="incident.status === 4"
                flat
                color="grey"
                :label="t('incidents.markClosed')"
                @click="updateStatus(incident.id, 5)"
              />
            </template>
          </IncidentCard>
        </div>

        <div v-if="filteredIncidents.length === 0" class="col-12 text-center text-grey q-pa-xl">
          {{ t('incidents.noIncidents') }}
        </div>
      </template>
    </div>

    <q-dialog v-model="showDialog" persistent>
      <IncidentFormCmp
        :incident="editingIncident"
        :loading="saving"
        @submit="saveIncident"
        @cancel="closeDialog"
      />
    </q-dialog>

    <q-dialog v-model="showResolveDialog">
      <IncidentResolveDialog @submit="confirmResolve" @cancel="showResolveDialog = false" />
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useIncidentStore } from '../stores/incident'
import { useCondominiumStore } from '../stores/condominium'
import { useAuthStore } from '../stores/auth'
import { useI18n } from '../composables/useI18n'
import { formatDate, parseMedia } from '@/utils/format'
import IncidentCard from '../components/IncidentCard.vue'
import IncidentFormCmp from '../components/IncidentForm.vue'
import IncidentResolveDialog from '../components/incident/IncidentResolveDialog.vue'

const $q = useQuasar()
const incidentStore = useIncidentStore()
const condoStore = useCondominiumStore()
const authStore = useAuthStore()
const { t } = useI18n()

const loading = ref(true)
const saving = ref(false)
const incidents = ref([])
const showDialog = ref(false)
const showResolveDialog = ref(false)
const editingIncident = ref(null)
const statusFilter = ref(null)
const currentIncidentId = ref<string | null>(null)

const statusOptions = computed(() => [
  { label: t('incidents.all'), value: null },
  { label: t('incidents.reported'), value: 1 },
  { label: t('incidents.inProcess'), value: 2 },
  { label: t('incidents.pending'), value: 3 },
  { label: t('incidents.resolved'), value: 4 },
  { label: t('incidents.closed'), value: 5 }
])

const filteredIncidents = computed(() => {
  if (statusFilter.value === null) return incidents.value
  return incidents.value.filter(i => i.status === statusFilter.value)
})

onMounted(async () => {
  await fetchIncidents()
})

async function fetchIncidents() {
  loading.value = true
  try {
    const condos = await condoStore.fetchAll()
    if (condos.length > 0) {
      incidents.value = await incidentStore.fetchByCondominium(condos[0].id)
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    loading.value = false
  }
}

function openCreateDialog() {
  editingIncident.value = null
  showDialog.value = true
}

function closeDialog() {
  showDialog.value = false
  editingIncident.value = null
}

function editIncident(incident) {
  const media = parseMedia(incident.media || '[]')
  editingIncident.value = {
    ...incident,
    media: media.map(m => ({ file: null, preview: m.path })),
    latitude: incident.location ? parseFloat(incident.location.split(' ')[0]) : undefined,
    longitude: incident.location ? parseFloat(incident.location.split(' ')[1]) : undefined
  }
  showDialog.value = true
}

async function saveIncident(data) {
  saving.value = true
  try {
    const condos = await condoStore.fetchAll()
    if (condos.length > 0) {
      if (editingIncident.value) {
        await incidentStore.update(editingIncident.value.id, data)
      } else {
        await incidentStore.create(condos[0].id, data)
      }
      $q.notify({ type: 'positive', message: t('incidents.reportSuccess') })
      closeDialog()
      await fetchIncidents()
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    saving.value = false
  }
}

function resolveDialog(incident) {
  currentIncidentId.value = incident.id
  resolutionNotes.value = ''
  showResolveDialog.value = true
}

async function confirmResolve(notes: string) {
  await updateStatus(currentIncidentId.value, 4, notes)
  showResolveDialog.value = false
}

async function updateStatus(id, status, notes = null) {
  const result = await incidentStore.updateStatus(id, status, notes)
  if (result) {
    $q.notify({ type: 'positive', message: t('common.success') })
    await fetchIncidents()
  } else {
    $q.notify({ type: 'negative', message: t('common.error') })
  }
}

async function deleteIncident(incident) {
  $q.dialog({
    title: t('common.confirmDelete'),
    message: t('common.deleteMessage').replace('{item}', incident.title),
    cancel: true
  }).onOk(async () => {
    const result = await incidentStore.remove(incident.id)
    if (result) {
      $q.notify({ type: 'positive', message: t('common.success') })
      await fetchIncidents()
    }
  })
}

function canManage(incident) {
  return authStore.isAdmin || authStore.isManager
}
</script>
