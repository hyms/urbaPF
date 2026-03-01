<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">{{ t('alerts.title') }}</div>
      <q-space />
      <q-btn color="red" icon="add" :label="t('alerts.newAlert')" @click="showCreateDialog = true" />
    </div>

    <q-card>
      <q-list separator>
        <AlertItem
          v-for="alert in alerts"
          :key="alert.id"
          :alert="alert"
          @update-status="updateStatus"
        />
        <q-item v-if="alerts.length === 0">
          <q-item-section class="text-grey text-center">
            {{ t('alerts.noAlerts') }}
          </q-item-section>
        </q-item>
      </q-list>
    </q-card>

    <q-dialog v-model="showCreateDialog" persistent>
      <q-card style="min-width: 400px">
        <q-card-section>
          <div class="text-h6 text-red">{{ t('alerts.newAlert') }}</div>
        </q-card-section>

        <q-card-section>
          <q-form class="q-gutter-md">
            <q-select v-model="newAlert.alertType" :options="typeOptions" :label="t('alerts.type')" outlined emit-value map-options :rules="[v => !!v || t('common.required')]" />
            <q-input v-model="newAlert.message" :label="t('alerts.message')" type="textarea" outlined :rules="[v => !!v || t('common.required')]" />
            <q-input v-model="newAlert.destinationAddress" :label="t('alerts.destination')" outlined />
            <q-input v-model="newAlert.estimatedArrival" :label="t('alerts.arrival')" type="datetime-local" outlined />
          </q-form>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat :label="t('common.cancel')" v-close-popup />
          <q-btn color="red" :label="t('alerts.send')" @click="createAlert" :loading="loading" />
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
import { useI18n } from '../composables/useI18n'
import AlertItem from '../components/AlertItem.vue'

const $q = useQuasar()
const alertStore = useAlertStore()
const condoStore = useCondominiumStore()
const { t } = useI18n()

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
      $q.notify({ type: 'positive', message: t('common.success') })
      showCreateDialog.value = false
      alerts.value = await alertStore.fetchByCondominium(condos[0].id)
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
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
</script>
