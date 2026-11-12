<template>
  <q-layout view="lHh Lpr lFf">
    <q-page-container>
      <q-page class="q-pa-md">
        <div class="row items-center q-mb-md">
          <div class="text-h4">{{ t('condominiums.title') }}</div>
          <q-space />
          <q-btn color="primary" icon="add" :label="t('condominiums.newCondo')" @click="openCreateDialog" v-if="authStore.isAdmin" aria-label="Crear nuevo condominio"></q-btn>
        </div>

        <div class="row q-col-gutter-md" v-if="condominiums.length > 0">
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
        <div v-else class="col-12 text-center text-grey q-pa-xl">
          {{ t('condominiums.noCondos') }}
        </div>
      </q-page>
    </q-page-container>
  </q-layout>

  <q-dialog v-model="showCreateDialog" persistent>
    <q-card style="min-width: 400px">
      <q-card-section>
        <div class="text-h6">{{ editingCondo ? t('condominiums.editCondo') : t('condominiums.newCondo') }}</div>
      </q-card-section>

      <q-card-section>
        <q-form class="q-gutter-md">
          <q-input v-model="newCondo.name" :label="t('condominiums.name')" filled :rules="[v => !!v || t('common.required')]"></q-input>
          <q-input v-model="newCondo.address" :label="t('condominiums.address')" filled :rules="[v => !!v || t('common.required')]"></q-input>
          <q-input v-model="newCondo.description" :label="t('condominiums.description')" type="textarea" filled></q-input>
          <q-input v-model="newCondo.monthlyFee" :label="t('condominiums.monthlyFee')" type="number" filled></q-input>
          <q-input v-model="newCondo.currency" :label="t('condominiums.currency')" filled></q-input>
        </q-form>
      </q-card-section>

      <q-card-actions align="right">
        <q-btn flat :label="t('common.cancel')" v-close-popup aria-label="Cancelar"></q-btn>
        <q-btn color="primary" :label="t('common.save')" @click="saveCondo" :loading="loading" aria-label="Guardar condominio"></q-btn>
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
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

function openCreateDialog() {
  editingCondo.value = null
  newCondo.value = {
    name: '',
    address: '',
    description: '',
    monthlyFee: 0,
    currency: 'BOB'
  }
  showCreateDialog.value = true
}

function selectCondo(condo: any) {
  localStorage.setItem('currentCondoId', condo.id)
  $q.notify({ type: 'positive', message: `${t('condominiums.select')}: ${condo.name}` })
}

function editCondo(condo: any) {
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

async function deleteCondo(condo: any) {
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
