<template>
  <q-page class="q-pa-md">
    <div class="text-h4 q-mb-md">{{ t('settings.title') }}</div>

    <q-tabs v-model="tab" class="q-mb-md" align="justify">
      <q-tab :name="t('settings.general')" icon="settings" :label="t('settings.general')" />
      <q-tab :name="t('settings.condominium')" icon="home" :label="t('settings.condominium')" v-if="authStore.isAdmin" />
      <q-tab :name="t('settings.notifications')" icon="notifications" :label="t('settings.notifications')" />
    </q-tabs>

    <q-tab-panels v-model="tab" animated>
      <q-tab-panel :name="t('settings.general')">
        <q-card flat bordered>
          <q-card-section>
            <div class="text-h6 q-mb-md">{{ t('settings.profile') }}</div>
            <q-form class="q-gutter-md">
              <q-input v-model="profile.fullName" :label="t('settings.fullName')" outlined />
              <q-input v-model="profile.email" :label="t('settings.email')" type="email" outlined readonly />
              <q-input v-model="profile.phone" :label="t('settings.phone')" outlined />
              <q-input v-model="profile.streetAddress" :label="t('settings.address')" outlined />
              <q-btn color="primary" :label="t('common.save')" @click="saveProfile" :loading="loading" />
            </q-form>
          </q-card-section>
        </q-card>

        <q-card flat bordered class="q-mt-md">
          <q-card-section>
            <div class="text-h6 q-mb-md">{{ t('settings.security') }}</div>
            <q-form class="q-gutter-md">
              <q-input v-model="password.current" :label="t('settings.currentPassword')" type="password" outlined />
              <q-input v-model="password.new" :label="t('settings.newPassword')" type="password" outlined />
              <q-input v-model="password.confirm" :label="t('settings.confirmPassword')" type="password" outlined />
              <q-btn color="primary" :label="t('settings.changePassword')" @click="changePassword" :loading="loading" />
            </q-form>
          </q-card-section>
        </q-card>
      </q-tab-panel>

      <q-tab-panel :name="t('settings.condominium')" v-if="authStore.isAdmin">
        <q-card flat bordered>
          <q-card-section>
            <div class="text-h6 q-mb-md">{{ t('settings.condoSettings') }}</div>
            <q-form class="q-gutter-md">
              <q-input v-model="condoSettings.name" :label="t('settings.condoName')" outlined :readonly="!authStore.isAdmin" />
              <q-input v-model="condoSettings.address" :label="t('settings.address')" outlined :readonly="!authStore.isAdmin" />
              <q-input v-model="condoSettings.monthlyFee" :label="t('settings.monthlyFee')" type="number" outlined :readonly="!authStore.isAdmin" />
              <q-input v-model="condoSettings.currency" :label="t('settings.currency')" outlined :readonly="!authStore.isAdmin" />
              <q-input v-model="condoSettings.rules" :label="t('settings.rules')" type="textarea" outlined :readonly="!authStore.isAdmin" />
              <q-toggle v-model="condoSettings.isActive" :label="t('settings.active')" :disable="!authStore.isAdmin" />
              <q-btn color="primary" :label="t('common.save')" @click="saveCondoSettings" :loading="loading" :disable="!authStore.isAdmin" />
            </q-form>
          </q-card-section>
        </q-card>
      </q-tab-panel>

      <q-tab-panel :name="t('settings.notifications')">
        <q-card flat bordered>
          <q-card-section>
            <div class="text-h6 q-mb-md">{{ t('settings.notifPrefs') }}</div>
            <q-list>
              <q-item tag="label">
                <q-item-section>
                  <q-item-label>{{ t('settings.notifIncidents') }}</q-item-label>
                  <q-item-label caption>{{ t('settings.notifIncidentsDesc') }}</q-item-label>
                </q-item-section>
                <q-item-section side>
                  <q-toggle v-model="notifications.incidents" />
                </q-item-section>
              </q-item>
              <q-item tag="label">
                <q-item-section>
                  <q-item-label>{{ t('settings.notifAlerts') }}</q-item-label>
                  <q-item-label caption>{{ t('settings.notifAlertsDesc') }}</q-item-label>
                </q-item-section>
                <q-item-section side>
                  <q-toggle v-model="notifications.alerts" />
                </q-item-section>
              </q-item>
              <q-item tag="label">
                <q-item-section>
                  <q-item-label>{{ t('settings.notifPolls') }}</q-item-label>
                  <q-item-label caption>{{ t('settings.notifPollsDesc') }}</q-item-label>
                </q-item-section>
                <q-item-section side>
                  <q-toggle v-model="notifications.polls" />
                </q-item-section>
              </q-item>
              <q-item tag="label">
                <q-item-section>
                  <q-item-label>{{ t('settings.notifPosts') }}</q-item-label>
                  <q-item-label caption>{{ t('settings.notifPostsDesc') }}</q-item-label>
                </q-item-section>
                <q-item-section side>
                  <q-toggle v-model="notifications.posts" />
                </q-item-section>
              </q-item>
            </q-list>
          </q-card-section>
        </q-card>

        <q-card flat bordered class="q-mt-md">
          <q-card-section>
            <div class="text-h6 q-mb-md">{{ t('settings.fcmToken') }}</div>
            <q-input v-model="fcmToken" :label="t('settings.fcmToken')" outlined readonly>
              <template v-slot:append>
                <q-btn flat round dense icon="content_copy" @click="copyToken" />
              </template>
            </q-input>
            <q-btn color="primary" :label="t('settings.updateToken')" @click="updateFcmToken" class="q-mt-md" />
          </q-card-section>
        </q-card>
      </q-tab-panel>
    </q-tab-panels>
  </q-page>
</template>

<script setup>
import { ref, watch } from 'vue'
import { useQuasar } from 'quasar'
import { useAuthStore } from '../stores/auth'
import { useUserStore } from '../stores/user'
import { useCondominiumStore } from '../stores/condominium'
import { useI18n } from '../composables/useI18n'

const $q = useQuasar()
const authStore = useAuthStore()
const userStore = useUserStore()
const condoStore = useCondominiumStore()
const { t } = useI18n()

const tab = ref(t('settings.general'))
const loading = ref(false)

const profile = ref({
  fullName: authStore.user?.fullName || '',
  email: authStore.user?.email || '',
  phone: authStore.user?.phone || '',
  streetAddress: authStore.user?.streetAddress || ''
})

const password = ref({
  current: '',
  new: '',
  confirm: ''
})

const condoSettings = ref({
  name: '',
  address: '',
  monthlyFee: 0,
  currency: 'BOB',
  rules: '',
  isActive: true
})

const notifications = ref({
  incidents: true,
  alerts: true,
  polls: true,
  posts: false
})

const fcmToken = ref('')

loadCondoSettings()

async function loadCondoSettings() {
  const currentCondoId = localStorage.getItem('currentCondoId')
  if (currentCondoId) {
    const condos = await condoStore.fetchAll()
    const condo = condos.find(c => c.id === currentCondoId)
    if (condo) {
      condoSettings.value = {
        name: condo.name,
        address: condo.address,
        monthlyFee: condo.monthlyFee,
        currency: condo.currency,
        rules: condo.rules || '',
        isActive: condo.isActive
      }
    }
  }
}

async function saveProfile() {
  loading.value = true
  try {
    const userId = authStore.currentUser?.id
    if (!userId) {
      $q.notify({ type: 'negative', message: t('common.error') })
      return
    }
    const success = await userStore.update(userId, {
      fullName: profile.value.fullName,
      phone: profile.value.phone
    })
    if (success) {
      authStore.user = { ...authStore.user, fullName: profile.value.fullName, phone: profile.value.phone }
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

async function changePassword() {
  if (password.value.new !== password.value.confirm) {
    $q.notify({ type: 'negative', message: t('settings.passwordsNotMatch') })
    return
  }
  loading.value = true
  try {
    const userId = authStore.currentUser?.id
    if (!userId) {
      $q.notify({ type: 'negative', message: t('common.error') })
      return
    }
    const result = await userStore.changePassword(userId, password.value.current, password.value.new)
    if (result.success) {
      $q.notify({ type: 'positive', message: result.message })
      password.value = { current: '', new: '', confirm: '' }
    } else {
      $q.notify({ type: 'negative', message: result.message })
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    loading.value = false
  }
}

async function saveCondoSettings() {
  if (!authStore.isAdmin) return
  loading.value = true
  try {
    const currentCondoId = localStorage.getItem('currentCondoId')
    if (currentCondoId) {
      await condoStore.update(currentCondoId, condoSettings.value)
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
