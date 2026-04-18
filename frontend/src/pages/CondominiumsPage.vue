<template>
  <q-page class="q-pa-md">
    <div class="text-h4 q-mb-md">{{ t('condominiums.title') }}</div>

    <div v-if="condoStore.currentCondominium" class="condo-detail">
      <q-card flat bordered class="rounded-lg q-mb-md">
        <q-card-section>
          <div class="row items-center q-mb-md">
            <div class="col">
              <div class="text-h5">{{ condoStore.currentCondominium.name }}</div>
              <div class="text-subtitle2 text-grey">
                <q-icon name="location_on" class="q-mr-xs" />
                {{ condoStore.currentCondominium.address }}
              </div>
            </div>
            <q-btn
              v-if="authStore.isAdmin"
              flat
              round
              icon="edit"
              color="primary"
              @click="openEditDialog"
              aria-label="Editar condominio"
            />
          </div>

          <div class="row q-col-gutter-md q-mb-md">
            <div class="col-12 col-sm-6">
              <q-card flat color="primary" text-color="white" class="rounded-lg">
                <q-card-section class="text-center">
                  <div class="text-caption">Cuota Mensual</div>
                  <div class="text-h5 text-weight-bold">
                    {{ condoStore.currentCondominium.currency }}
                    {{ condoStore.currentCondominium.monthlyFee?.toLocaleString() }}
                  </div>
                </q-card-section>
              </q-card>
            </div>
            <div class="col-12 col-sm-6">
              <q-card flat color="orange" text-color="white" class="rounded-lg">
                <q-card-section class="text-center">
                  <div class="text-caption">Miembros</div>
                  <div class="text-h5 text-weight-bold">{{ memberCount }}</div>
                </q-card-section>
              </q-card>
            </div>
          </div>
        </q-card-section>
      </q-card>

      <div class="row q-col-gutter-md q-mb-md">
        <div class="col-12 col-md-6">
          <q-card flat bordered class="rounded-lg">
            <q-card-section>
              <div class="text-h6 q-mb-md">
                <q-icon name="description" class="q-mr-sm" />
                {{ t('condominiums.description') }}
              </div>
              <p v-if="condoStore.currentCondominium.description" class="text-body1">
                {{ condoStore.currentCondominium.description }}
              </p>
              <p v-else class="text-grey-5">Sin descripción disponible</p>
            </q-card-section>
          </q-card>
        </div>

        <div class="col-12 col-md-6">
          <q-card flat bordered class="rounded-lg">
            <q-card-section>
              <div class="text-h6 q-mb-md">
                <q-icon name="gavel" class="q-mr-sm" />
                {{ t('condominiums.rules') }}
              </div>
              <div v-if="condoStore.currentCondominium.rules" class="rules-content text-body1">
                <div v-for="(rule, index) in condoStore.currentCondominium.rules.split('\n').filter(r => r.trim())" :key="index" class="rule-item q-mb-sm">
                  <q-icon name="check_circle" color="positive" size="18px" class="q-mr-sm" />
                  {{ rule }}
                </div>
              </div>
              <p v-else class="text-grey-5">Sin reglas establecidas</p>
            </q-card-section>
          </q-card>
        </div>
      </div>

      <q-card flat bordered class="rounded-lg q-mb-md">
        <q-card-section>
          <div class="text-h6 q-mb-md">
            <q-icon name="people" class="q-mr-sm" />
            {{ t('condominiums.team') }}
          </div>
          <div v-if="managers.length > 0" class="row q-col-gutter-md">
            <div v-for="manager in managers" :key="manager.id" class="col-12 col-sm-6 col-md-4">
              <q-card flat bordered class="rounded-lg">
                <q-card-section class="row items-center q-pa-sm">
                  <q-avatar size="48px" color="primary" text-color="white" class="q-mr-md">
                    <q-img v-if="manager.photoUrl" :src="manager.photoUrl" loading="lazy" />
                    <span v-else>{{ getInitials(manager.fullName) }}</span>
                  </q-avatar>
                  <div class="col">
                    <div class="text-weight-medium">{{ manager.fullName }}</div>
                    <div class="text-caption text-grey">{{ getRoleLabel(manager.role) }}</div>
                    <q-badge v-if="manager.isValidated" color="positive" class="q-mt-xs">
                      Validado
                    </q-badge>
                  </div>
                </q-card-section>
              </q-card>
            </div>
          </div>
          <div v-else class="text-grey-5 text-center q-pa-md">
            No hay encargados asignados
          </div>
        </q-card-section>
      </q-card>

      <q-card flat bordered class="rounded-lg">
        <q-card-section>
          <div class="row items-center q-mb-md">
            <div class="text-h6">
              <q-icon name="map" class="q-mr-sm" />
              Ubicación
            </div>
            <q-space />
            <q-btn
              v-if="authStore.isAdmin"
              flat
              round
              dense
              icon="edit_location"
              color="primary"
              @click="showLocationDialog = true; openLocationDialog()"
              aria-label="Editar coordenadas"
            />
          </div>
          
          <div class="row q-col-gutter-sm q-mb-md" v-if="condoStore.currentCondominium.latitude && condoStore.currentCondominium.longitude">
            <div class="col-6">
              <q-input
                :model-value="condoStore.currentCondominium.latitude"
                :label="t('incidents.latitude')"
                type="number"
                step="any"
                outlined
                dense
                readonly
              />
            </div>
            <div class="col-6">
              <q-input
                :model-value="condoStore.currentCondominium.longitude"
                :label="t('incidents.longitude')"
                type="number"
                step="any"
                outlined
                dense
                readonly
              />
            </div>
          </div>

          <SecurityMap
            v-if="condoStore.currentCondominium.latitude && condoStore.currentCondominium.longitude"
            :incidents="[]"
            :alerts="[]"
            :center="[condoStore.currentCondominium.latitude!, condoStore.currentCondominium.longitude!]"
            :zoom="16"
          />
          <div v-else class="text-grey-5 text-center q-pa-md">
            Sin ubicación disponible
          </div>
        </q-card-section>
      </q-card>
    </div>

    <div v-else class="text-center text-grey q-pa-xl">
      <q-icon name="home_work" size="64px" class="q-mb-md" />
      <div>{{ t('condominiums.noCondos') }}</div>
    </div>
  </q-page>

  <q-dialog v-model="showEditDialog" persistent :maximized="$q.screen.lt.sm">
    <q-card style="min-width: 350px; width: 100%; max-width: 500px">
      <q-card-section class="row items-center">
        <div class="text-h6">{{ t('condominiums.editCondo') }}</div>
        <q-space />
        <q-btn icon="close" flat round dense v-close-popup v-if="$q.screen.lt.sm" />
      </q-card-section>

      <q-card-section class="q-pt-none">
        <q-form class="q-gutter-md">
          <q-input v-model="editCondo.name" :label="t('condominiums.name')" filled :rules="[v => !!v || t('common.required')]"></q-input>
          <q-input v-model="editCondo.address" :label="t('condominiums.address')" filled :rules="[v => !!v || t('common.required')]"></q-input>
          <q-input v-model="editCondo.description" :label="t('condominiums.description')" type="textarea" filled rows="3"></q-input>
          <q-input v-model="editCondo.rules" :label="t('condominiums.rules')" type="textarea" filled rows="4" hint="Una regla por línea"></q-input>
          <q-input v-model="editCondo.monthlyFee" :label="t('condominiums.monthlyFee')" type="number" filled></q-input>
          <q-input v-model="editCondo.currency" :label="t('condominiums.currency')" filled></q-input>
        </q-form>
      </q-card-section>

      <q-card-actions align="right" class="q-pa-md">
        <q-btn flat :label="t('common.cancel')" v-close-popup aria-label="Cancelar"></q-btn>
        <q-btn color="primary" :label="t('common.save')" @click="saveCondo" :loading="loading" aria-label="Guardar condominio"></q-btn>
      </q-card-actions>
    </q-card>
  </q-dialog>

  <q-dialog v-model="showLocationDialog" persistent :maximized="$q.screen.lt.sm">
    <q-card style="min-width: 350px; width: 100%; max-width: 500px">
      <q-card-section class="row items-center">
        <div class="text-h6">{{ t('condominiums.location') }}</div>
        <q-space />
        <q-btn icon="close" flat round dense v-close-popup v-if="$q.screen.lt.sm" />
      </q-card-section>

      <q-card-section class="q-pt-none">
        <div class="row q-col-gutter-sm">
          <div class="col-6">
            <q-input
              v-model.number="locationCoords.latitude"
              :label="t('incidents.latitude')"
              type="number"
              step="any"
              outlined
              dense
            />
          </div>
          <div class="col-6">
            <q-input
              v-model.number="locationCoords.longitude"
              :label="t('incidents.longitude')"
              type="number"
              step="any"
              outlined
              dense
            />
          </div>
        </div>
        <q-btn
          flat
          color="primary"
          icon="my_location"
          :label="t('incidents.getCurrentLocation')"
          @click="getCurrentLocation"
          class="q-mt-md full-width"
          :loading="loadingLocation"
        />
      </q-card-section>

      <q-card-actions align="right" class="q-pa-md">
        <q-btn flat :label="t('common.cancel')" v-close-popup aria-label="Cancelar"></q-btn>
        <q-btn color="primary" :label="t('common.save')" @click="saveLocation" :loading="loading" aria-label="Guardar ubicación"></q-btn>
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useCondominiumStore } from '../stores/condominium'
import { useAuthStore } from '../stores/auth'
import { useUserStore } from '../stores/user'
import { useI18n } from '../composables/useI18n'
import { getInitials } from '@/utils/format'
import { UserRole } from '../utils/appEnums'
import SecurityMap from '../components/SecurityMap.vue'

const $q = useQuasar()
const condoStore = useCondominiumStore()
const authStore = useAuthStore()
const userStore = useUserStore()
const { t } = useI18n()

const showEditDialog = ref(false)
const showLocationDialog = ref(false)
const loading = ref(false)
const loadingLocation = ref(false)

const editCondo = ref({
  name: '',
  address: '',
  description: '',
  rules: '',
  monthlyFee: 0,
  currency: 'BOB'
})

const locationCoords = ref({
  latitude: undefined as number | undefined,
  longitude: undefined as number | undefined
})

const managers = computed(() => {
  const currentCondoId = condoStore.currentCondominium?.id
  if (!currentCondoId) return []
  return userStore.users.filter(u => 
    u.condominiumId === currentCondoId && 
    (u.role === UserRole.Manager || u.role === UserRole.Administrator)
  )
})

const memberCount = computed(() => {
  const currentCondoId = condoStore.currentCondominium?.id
  if (!currentCondoId) return 0
  return userStore.users.filter(u => u.condominiumId === currentCondoId).length
})

onMounted(async () => {
  if (condoStore.currentCondominium) {
    await userStore.fetchAll()
  }
})

function getRoleLabel(role: number): string {
  const roles: Record<number, string> = {
    0: t('roles.restrictedAccess'),
    1: t('roles.neighbor'),
    2: t('roles.neighbor'),
    3: t('roles.manager'),
    4: t('roles.admin')
  }
  return roles[role] || t('roles.unknown')
}

function openEditDialog() {
  if (condoStore.currentCondominium) {
    editCondo.value = {
      name: condoStore.currentCondominium.name,
      address: condoStore.currentCondominium.address,
      description: condoStore.currentCondominium.description || '',
      rules: condoStore.currentCondominium.rules || '',
      monthlyFee: condoStore.currentCondominium.monthlyFee,
      currency: condoStore.currentCondominium.currency
    }
    showEditDialog.value = true
  }
}

async function saveCondo() {
  if (!condoStore.currentCondominium) return
  
  loading.value = true
  try {
    await condoStore.update(condoStore.currentCondominium.id, editCondo.value)
    $q.notify({ type: 'positive', message: t('common.success') })
    showEditDialog.value = false
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    loading.value = false
  }
}

function openLocationDialog() {
  const condo = condoStore.currentCondominium
  if (condo) {
    locationCoords.value = {
      latitude: condo.latitude,
      longitude: condo.longitude
    }
  }
}

async function getCurrentLocation() {
  if (!navigator.geolocation) {
    $q.notify({ type: 'negative', message: 'Geolocalización no disponible' })
    return
  }

  loadingLocation.value = true
  navigator.geolocation.getCurrentPosition(
    (position) => {
      locationCoords.value.latitude = position.coords.latitude
      locationCoords.value.longitude = position.coords.longitude
      loadingLocation.value = false
    },
    (error) => {
      $q.notify({ type: 'negative', message: 'Error al obtener ubicación: ' + error.message })
      loadingLocation.value = false
    }
  )
}

async function saveLocation() {
  if (!condoStore.currentCondominium) return
  
  loading.value = true
  try {
    await condoStore.update(condoStore.currentCondominium.id, {
      latitude: locationCoords.value.latitude,
      longitude: locationCoords.value.longitude
    })
    $q.notify({ type: 'positive', message: t('common.success') })
    showLocationDialog.value = false
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.condo-detail {
  max-width: 1200px;
  margin: 0 auto;
}

.rule-item {
  display: flex;
  align-items: flex-start;
}

.rules-content {
  white-space: pre-line;
}

.map-container iframe {
  border-radius: 8px;
}
</style>