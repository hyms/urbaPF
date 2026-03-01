<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">{{ t('condominiums.title') }}</div>
      <q-space />
      <q-btn color="primary" icon="add" :label="t('condominiums.newCondo')" @click="showCreateDialog = true" v-if="authStore.isAdmin" />
    </div>

    <div class="row q-col-gutter-md">
      <div class="col-12 col-sm-6 col-md-4" v-for="condo in condominiums" :key="condo.id">
        <CondoItem
          :condo="condo"
          :show-admin-controls="authStore.isAdmin"
          @click="selectCondo"
          @edit="editCondo"
          @delete="deleteCondo"
        />
      </div>
    </div>

    <q-dialog v-model="showCreateDialog" persistent>
      <q-card style="min-width: 400px">
        <q-card-section>
          <div class="text-h6">{{ editingCondo ? t('condominiums.editCondo') : t('condominiums.newCondo') }}</div>
        </q-card-section>

        <q-card-section>
          <q-form class="q-gutter-md">
            <q-input v-model="newCondo.name" :label="t('condominiums.name')" outlined :rules="[v => !!v || t('common.required')]" />
            <q-input v-model="newCondo.address" :label="t('condominiums.address')" outlined :rules="[v => !!v || t('common.required')]" />
            <q-input v-model="newCondo.description" :label="t('condominiums.description')" type="textarea" outlined />
            <q-input v-model="newCondo.monthlyFee" :label="t('condominiums.monthlyFee')" type="number" outlined />
            <q-input v-model="newCondo.currency" :label="t('condominiums.currency')" outlined />
          </q-form>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat :label="t('common.cancel')" v-close-popup />
          <q-btn color="primary" :label="t('common.save')" @click="saveCondo" :loading="loading" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useCondominiumStore } from '../stores/condominium'
import { useAuthStore } from '../stores/auth'
import { useI18n } from '../composables/useI18n'
import CondoItem from '../components/CondoItem.vue'

const $q = useQuasar()
const condoStore = useCondominiumStore()
const authStore = useAuthStore()
const { t } = useI18n()

const condominiums = ref([])
const showCreateDialog = ref(false)
const loading = ref(false)
const editingCondo = ref(null)

const newCondo = ref({
  name: '',
  address: '',
  description: '',
  monthlyFee: 0,
  currency: 'BOB'
})

onMounted(async () => {
  condominiums.value = await condoStore.fetchAll()
})

function selectCondo(condo) {
  localStorage.setItem('currentCondoId', condo.id)
  $q.notify({ type: 'positive', message: `${t('condominiums.select')}: ${condo.name}` })
}

function editCondo(condo) {
  editingCondo.value = condo
  newCondo.value = {
    name: condo.name,
    address: condo.address,
    description: condo.description,
    monthlyFee: condo.monthlyFee,
    currency: condo.currency
  }
  showCreateDialog.value = true
}

async function saveCondo() {
  loading.value = true
  try {
    if (editingCondo.value) {
      await condoStore.update(editingCondo.value.id, newCondo.value)
      $q.notify({ type: 'positive', message: t('common.success') })
    } else {
      await condoStore.create(newCondo.value)
      $q.notify({ type: 'positive', message: t('common.success') })
    }
    showCreateDialog.value = false
    condominiums.value = await condoStore.fetchAll()
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    loading.value = false
    editingCondo.value = null
  }
}

async function deleteCondo(condo) {
  $q.dialog({
    title: t('common.confirmDelete'),
    message: t('common.deleteMessage').replace('{item}', condo.name),
    cancel: true
  }).onOk(async () => {
    await condoStore.remove(condo.id)
    condominiums.value = await condoStore.fetchAll()
    $q.notify({ type: 'positive', message: t('common.success') })
  })
}
</script>
