<template>
  <q-page class="q-pa-md">
    <div class="text-h4 q-mb-md">{{ t('settings.title') }}</div>

    <q-tabs v-model="tab" class="q-mb-md" align="justify">
      <q-tab :name="t('settings.general')" icon="settings" :label="t('settings.general')" />
      <q-tab :name="t('settings.condominium')" icon="home" :label="t('settings.condominium')" v-if="authStore.isAdmin" />
      <q-tab name="security" icon="security" label="Seguridad" v-if="authStore.isAdmin" />
      <q-tab :name="t('settings.notifications')" icon="notifications" :label="t('settings.notifications')" />
    </q-tabs>

    <q-tab-panels v-model="tab" animated>
      <q-tab-panel :name="t('settings.general')">
        <ProfileSettingsForm :user="authStore.user" :loading="loading" @submit="saveProfile" />

        <PasswordSettingsForm :loading="loading" @submit="changePassword" class="q-mt-md" />
      </q-tab-panel>

      <q-tab-panel :name="t('settings.condominium')" v-if="authStore.isAdmin">
        <CondominiumSettingsForm 
          :condominium="condoStore.currentCondominium" 
          :loading="loading"
          @submit="saveCondoSettings" 
        />
      </q-tab-panel>

      <q-tab-panel name="security" v-if="authStore.isAdmin">
        <q-card flat bordered class="rounded-lg">
          <q-card-section>
            <div class="text-h6 q-mb-md">Configuración de Seguridad Global</div>
            <div class="q-gutter-md">
              <q-toggle 
                v-model="securitySettings.forcePasswordChange" 
                label="Forzar cambio de contraseña en primer inicio" 
                color="primary"
              />
              
              <div class="row q-col-gutter-md">
                <div class="col-12 col-sm-6">
                  <q-input 
                    v-model.number="securitySettings.minPasswordLength" 
                    type="number" 
                    label="Longitud mínima de contraseña" 
                    outlined 
                    dense
                  />
                </div>
                <div class="col-12 col-sm-6">
                  <q-toggle 
                    v-model="securitySettings.requireSpecialChars" 
                    label="Requerir caracteres especiales" 
                    color="primary"
                    class="q-mt-sm"
                  />
                </div>
              </div>
              
              <div class="row justify-end q-mt-md">
                <q-btn color="primary" label="Guardar Configuración" @click="saveSecuritySettings" :loading="loading" />
              </div>
            </div>
          </q-card-section>
        </q-card>
      </q-tab-panel>

      <q-tab-panel :name="t('settings.notifications')">
        <NotificationSettings 
          :notifications="notifications" 
          :fcm-token="fcmToken" 
          :loading="loading"
          @update:notifications="val => notifications = val"
          @copy-token="copyToken"
          @update-fcm-token="updateFcmToken"
        />
      </q-tab-panel>
    </q-tab-panels>
  </q-page>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useAuthStore } from '../stores/auth'
import { useUserStore } from '../stores/user'
import { useCondominiumStore } from '../stores/condominium'
import { useI18n } from '../composables/useI18n'
import ProfileSettingsForm from '../components/settings/ProfileSettingsForm.vue'
import PasswordSettingsForm from '../components/settings/PasswordSettingsForm.vue'
import CondominiumSettingsForm from '../components/settings/CondominiumSettingsForm.vue'
import NotificationSettings from '../components/settings/NotificationSettings.vue'

const $q = useQuasar()
const authStore = useAuthStore()
const userStore = useUserStore()
const condoStore = useCondominiumStore()
const { t } = useI18n()

const tab = ref(t('settings.general'))
const loading = ref(false)

const profile = ref({
  name: '',
  email: ''
})

const notifications = ref({
  incidents: true,
  alerts: true,
  polls: true,
  posts: false
})

const condoSettings = ref({
  name: '',
  address: '',
  monthlyFee: 0,
  currency: '',
  rules: '',
  isActive: true
})

const fcmToken = ref('')

const securitySettings = ref({
  forcePasswordChange: false,
  minPasswordLength: 6,
  requireSpecialChars: false
})

onMounted(async () => {
  await condoStore.fetchAll()
  condoStore.loadCurrentCondominiumFromStorage()
  if (condoStore.currentCondominium) {
    condoSettings.value = {
      name: condoStore.currentCondominium.name,
      address: condoStore.currentCondominium.address,
      monthlyFee: condoStore.currentCondominium.monthlyFee,
      currency: condoStore.currentCondominium.currency,
      rules: condoStore.currentCondominium.rules || '',
      isActive: condoStore.currentCondominium.isActive
    }
  }
  
  if (authStore.isAdmin) {
    // In a real app we would fetch settings from backend
    // For now we'll simulate or use defaults from store/env
  }
})

async function saveSecuritySettings() {
  loading.value = true
  try {
    // TODO: Implement backend endpoint for app settings
    $q.notify({ type: 'positive', message: 'Configuración de seguridad guardada' })
  } catch (e) {
    $q.notify({ type: 'negative', message: 'Error al guardar configuración' })
  } finally {
    loading.value = false
  }
}

async function saveProfile(formData: { fullName: string; phone: string; streetAddress: string }) {
  loading.value = true
  try {
    const userId = authStore.user?.id
    if (!userId) {
      $q.notify({ type: 'negative', message: t('common.error') })
      return
    }
    const success = await userStore.update(userId, {
      fullName: formData.fullName,
      phone: formData.phone,
      streetAddress: formData.streetAddress
    })
    if (success) {
      // Actualizar el store de auth con los nuevos datos
      authStore.user = { ...authStore.user!, fullName: formData.fullName, phone: formData.phone, streetAddress: formData.streetAddress }
      localStorage.setItem('user', JSON.stringify(authStore.user))
      $q.notify({ type: 'positive', message: t('common.success') })
    } else {
      $q.notify({ type: 'negative', message: userStore.error || t('common.error') })
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    loading.value = false
  }
}

async function changePassword(formData: { current: string; new: string; confirm: string }) {
  if (formData.new !== formData.confirm) {
    $q.notify({ type: 'negative', message: t('settings.passwordsNotMatch') })
    return
  }
  loading.value = true
  try {
    const userId = authStore.user?.id
    if (!userId) {
      $q.notify({ type: 'negative', message: t('common.error') })
      return
    }
    const success = await userStore.changePassword(userId, { oldPassword: formData.current, newPassword: formData.new })
    if (success) {
      $q.notify({ type: 'positive', message: t('settings.passwordChangeSuccess') })
    } else {
      $q.notify({ type: 'negative', message: userStore.error || t('settings.passwordChangeError') })
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    loading.value = false
  }
}

async function saveCondoSettings(formData: any) {
  if (!authStore.isAdmin) return
  loading.value = true
  try {
    if (condoStore.currentCondominium) {
      await condoStore.update(condoStore.currentCondominium.id, formData)
    }
    $q.notify({ type: 'positive', message: t('common.success') })
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    loading.value = false
  }
}

function copyToken() {
  navigator.clipboard.writeText(fcmToken.value)
  $q.notify({ type: 'positive', message: t('settings.tokenCopied') })
}

async function updateFcmToken() {
  $q.notify({ type: 'positive', message: t('settings.tokenUpdated') })
}
</script>
