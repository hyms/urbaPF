<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">Condominios</div>
      <q-space />
      <q-btn color="primary" icon="add" label="Nuevo" @click="showCreateDialog = true" v-if="authStore.isAdmin" />
    </div>

    <div class="row q-col-gutter-md">
      <div class="col-12 col-sm-6 col-md-4" v-for="condo in condominiums" :key="condo.id">
        <q-card class="cursor-pointer" @click="selectCondo(condo)">
          <q-card-section>
            <div class="text-h6">{{ condo.name }}</div>
            <div class="text-caption text-grey">{{ condo.address }}</div>
          </q-card-section>
          <q-separator />
          <q-card-section>
            <div class="row">
              <div class="col-6">
                <div class="text-caption text-grey">Cuota mensual</div>
                <div class="text-body1">{{ condo.monthlyFee }} {{ condo.currency }}</div>
              </div>
              <div class="col-6">
                <div class="text-caption text-grey">Estado</div>
                <q-chip :color="condo.isActive ? 'green' : 'red'" text-color="white" size="sm">
                  {{ condo.isActive ? 'Activo' : 'Inactivo' }}
                </q-chip>
              </div>
            </div>
          </q-card-section>
        </q-card>
      </div>
    </div>

    <q-dialog v-model="showCreateDialog" persistent>
      <q-card style="min-width: 400px">
        <q-card-section>
          <div class="text-h6">Nuevo Condominio</div>
        </q-card-section>

        <q-card-section>
          <q-form class="q-gutter-md">
            <q-input v-model="newCondo.name" label="Nombre" outlined :rules="[v => !!v || 'Requerido']" />
            <q-input v-model="newCondo.address" label="Dirección" outlined :rules="[v => !!v || 'Requerido']" />
            <q-input v-model="newCondo.description" label="Descripción" type="textarea" outlined />
            <q-input v-model="newCondo.monthlyFee" label="Cuota mensual" type="number" outlined />
            <q-input v-model="newCondo.currency" label="Moneda" outlined />
          </q-form>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat label="Cancelar" v-close-popup />
          <q-btn color="primary" label="Crear" @click="createCondo" :loading="loading" />
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

const $q = useQuasar()
const condoStore = useCondominiumStore()
const authStore = useAuthStore()

const condominiums = ref([])
const showCreateDialog = ref(false)
const loading = ref(false)

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
  $q.notify({ type: 'positive', message: `Seleccionado: ${condo.name}` })
}

async function createCondo() {
  loading.value = true
  try {
    await condoStore.create(newCondo.value)
    $q.notify({ type: 'positive', message: 'Condominio creado' })
    showCreateDialog.value = false
    condominiums.value = await condoStore.fetchAll()
  } catch (e) {
    $q.notify({ type: 'negative', message: 'Error al crear' })
  } finally {
    loading.value = false
  }
}
</script>
