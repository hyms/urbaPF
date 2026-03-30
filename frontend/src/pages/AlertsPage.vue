<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">{{ t('alerts.title') }}</div>
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
        <div class="col-12" v-for="n in 3" :key="n">
          <q-card>
            <q-card-section>
              <q-skeleton type="text" class="q-mb-sm" style="width: 60%" />
              <q-skeleton type="text" style="width: 40%" />
            </q-card-section>
          </q-card>
        </div>
      </template>
      <template v-else>
        <div
          v-for="alert in filteredAlerts"
          :key="alert.id"
          class="col-12"
        >
          <q-card :class="getAlertClass(alert.status)">
            <q-card-section>
              <div class="row items-center">
                <div class="col">
                  <div class="text-h6">
                    <q-icon :name="getTypeIcon(alert.type)" class="q-mr-sm" />
                    {{ alert.title }}
                  </div>
                  <div class="text-caption text-grey">
                    {{ formatDate(alert.created_at) }}
                    <span v-if="alert.needs_approval && alert.status === 1" class="text-warning q-ml-sm">
                      <q-icon name="warning" />
                      {{ t('alerts.needsApproval') }}
                    </span>
                  </div>
                </div>
                <q-badge :color="getStatusColor(alert.status)" size="md">
                  {{ getStatusLabel(alert.status) }}
                </q-badge>
              </div>
              <div v-if="alert.description" class="q-mt-sm text-body2">
                {{ alert.description }}
              </div>
              <div class="q-mt-sm">
                <q-chip
                  v-if="alert.reputation_level < 4"
                  dense
                  color="warning"
                  text-color="white"
                  size="sm"
                >
                  Reputación: {{ alert.reputation_level }}
                </q-chip>
                <q-chip
                  v-else
                  dense
                  color="positive"
                  text-color="white"
                  size="sm"
                >
                  Alta Reputación
                </q-chip>
              </div>
            </q-card-section>

            <q-separator />

            <q-card-actions>
              <q-btn
                v-if="authStore.isManager && alert.status === 1"
                flat
                color="positive"
                :label="t('alerts.approve')"
                @click="approveAlert(alert.id)"
              />
              <q-btn
                v-if="authStore.isManager && alert.status >= 2"
                flat
                color="primary"
                :label="t('alerts.acknowledge')"
                @click="acknowledgeAlert(alert.id)"
              />
              <q-btn
                v-if="alert.status >= 3"
                flat
                color="grey"
                :label="t('alerts.resolve')"
                @click="resolveAlert(alert.id)"
              />
              <q-btn
                v-if="canResend(alert)"
                flat
                color="info"
                :label="t('alerts.resend')"
                @click="resendAlert(alert.id)"
              />
              <q-space />
              <q-btn
                v-if="authStore.isAdmin"
                flat
                round
                dense
                icon="delete"
                color="negative"
                @click="deleteAlert(alert)"
              />
            </q-card-actions>
          </q-card>
        </div>

        <div v-if="filteredAlerts.length === 0" class="col-12 text-center text-grey q-pa-xl">
          {{ t('alerts.noAlerts') }}
        </div>
      </template>
    </div>
  </q-page>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useAlertStore } from '../stores/alert'
import { useCondominiumStore } from '../stores/condominium'
import { useAuthStore } from '../stores/auth'
import { useI18n } from '../composables/useI18n'

const $q = useQuasar()
const alertStore = useAlertStore()
const condoStore = useCondominiumStore()
const authStore = useAuthStore()
const { t } = useI18n()

const loading = ref(true)
const alerts = ref([])
const statusFilter = ref(null)

const statusOptions = computed(() => [
  { label: t('alerts.all'), value: null },
  { label: t('alerts.pending'), value: 1 },
  { label: t('alerts.active'), value: 3 }
])

const filteredAlerts = computed(() => {
  if (statusFilter.value === null) return alerts.value
  return alerts.value.filter(a => a.status === statusFilter.value)
})

onMounted(async () => {
  await fetchAlerts()
})

async function fetchAlerts() {
  loading.value = true
  try {
    const condos = await condoStore.fetchAll()
    if (condos.length > 0) {
      alerts.value = await alertStore.fetchByCondominium(condos[0].id)
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    loading.value = false
  }
}

async function approveAlert(alertId) {
  const result = await alertStore.approve(alertId)
  if (result) {
    $q.notify({ type: 'positive', message: t('common.success') })
    await fetchAlerts()
  }
}

async function acknowledgeAlert(alertId) {
  const result = await alertStore.acknowledge(alertId)
  if (result) {
    $q.notify({ type: 'positive', message: t('common.success') })
    await fetchAlerts()
  }
}

async function resolveAlert(alertId) {
  const result = await alertStore.resolve(alertId)
  if (result) {
    $q.notify({ type: 'positive', message: t('common.success') })
    await fetchAlerts()
  }
}

async function resendAlert(alertId) {
  const result = await alertStore.resend(alertId)
  if (result) {
    $q.notify({ type: 'positive', message: 'Alerta re-enviada' })
    await fetchAlerts()
  }
}

async function deleteAlert(alert) {
  $q.dialog({
    title: t('common.confirmDelete'),
    message: t('common.deleteMessage').replace('{item}', alert.title),
    cancel: true
  }).onOk(async () => {
    try {
      await alertStore.remove(alert.id)
      $q.notify({ type: 'positive', message: t('common.success') })
      await fetchAlerts()
    } catch (e) {
      $q.notify({ type: 'negative', message: t('common.error') })
    }
  })
}

function canResend(alert) {
  if (!authStore.isManager && !authStore.isAdmin) return false
  if (alert.needs_approval && alert.status < 2) return false
  return alert.status >= 3
}

function formatDate(dateStr) {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleDateString('es-BO', {
    day: '2-digit',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function getTypeIcon(type) {
  const icons = { 1: 'emergency', 2: 'security', 3: 'local_fire_department', 4: 'medical_services', 5: 'warning' }
  return icons[type] || 'warning'
}

function getStatusColor(status) {
  const colors = { 1: 'warning', 2: 'blue', 3: 'orange', 4: 'primary', 5: 'positive', 6: 'grey' }
  return colors[status] || 'grey'
}

function getStatusLabel(status) {
  const labels = {
    1: t('alerts.pending'),
    2: t('alerts.approved'),
    3: t('alerts.notified'),
    4: t('alerts.inProgress'),
    5: t('alerts.resolved'),
    6: t('alerts.cancelled')
  }
  return labels[status] || 'Desconocido'
}

function getAlertClass(status) {
  if (status === 1) return 'border-warning'
  if (status >= 3 && status < 5) return 'border-red'
  return ''
}
</script>

<style scoped>
.border-warning {
  border-left: 4px solid #ff9800;
}
.border-red {
  border-left: 4px solid #f44336;
}
</style>
