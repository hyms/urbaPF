<template>
  <q-layout view="lHh Lpr lFf">
    <q-header elevated class="bg-primary">
      <q-toolbar>
        <q-btn
          flat
          dense
          round
          icon="menu"
          aria-label="Menu"
          @click="toggleLeftDrawer"
        />

        <q-toolbar-title class="text-weight-bold">
          UrbaPF
        </q-toolbar-title>

        <q-btn 
          flat 
          round 
          dense 
          icon="notifications" 
          class="q-mr-xs"
          aria-label="Notificaciones"
        >
          <q-badge color="red" floating>3</q-badge>
        </q-btn>

        <q-btn flat round dense aria-label="Menú de usuario">
          <q-avatar size="32px" color="white" text-color="primary">
            {{ userInitials }}
          </q-avatar>
          <q-menu>
            <q-list style="min-width: 150px">
              <q-item clickable v-close-popup>
                <q-item-section avatar>
                  <q-icon name="person" />
                </q-item-section>
                <q-item-section>{{ t('common.profile') }}</q-item-section>
              </q-item>
              <q-separator />
              <q-item clickable v-close-popup @click="logout">
                <q-item-section avatar>
                  <q-icon name="logout" />
                </q-item-section>
                <q-item-section>{{ t('common.logout') }}</q-item-section>
              </q-item>
            </q-list>
          </q-menu>
        </q-btn>
      </q-toolbar>
    </q-header>

    <q-drawer
      v-model="leftDrawerOpen"
      show-if-above
      bordered
      class="bg-grey-1"
      v-if="authStore.isAuthenticated"
    >
      <q-list>
        <q-item-label header class="text-grey-8">
          {{ t('common.menu') }}
        </q-item-label>

        <q-item clickable v-ripple to="/" exact active-class="text-primary bg-blue-1" aria-label="Ir al dashboard">
          <q-item-section avatar>
            <q-icon name="dashboard" />
          </q-item-section>
          <q-item-section>
            <q-item-label>{{ t('common.dashboard') }}</q-item-label>
            <q-item-label caption v-if="authStore.isManager">{{ t('dashboard.myActivity') }} / {{ t('dashboard.pendingApprovals') }}</q-item-label>
          </q-item-section>
          <q-item-section side v-if="authStore.isManager">
            <q-badge color="red" floating>{{ pendingCount }}</q-badge>
          </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/condominiums" active-class="text-primary bg-blue-1" aria-label="Ir a condominios">
          <q-item-section avatar>
            <q-icon name="home_work" />
          </q-item-section>
          <q-item-section>
            <q-item-label>{{ t('common.condominiums') }}</q-item-label>
          </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/posts" active-class="text-primary bg-blue-1" aria-label="Ir a publicaciones">
          <q-item-section avatar>
            <q-icon name="article" />
          </q-item-section>
          <q-item-section>
            <q-item-label>{{ t('common.posts') }}</q-item-label>
          </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/polls" active-class="text-primary bg-blue-1" aria-label="Ir a votaciones">
          <q-item-section avatar>
            <q-icon name="poll" color="purple" />
          </q-item-section>
          <q-item-section>
            <q-item-label>{{ t('common.polls') }}</q-item-label>
          </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/incidents" active-class="text-primary bg-blue-1" aria-label="Ir a incidentes">
          <q-item-section avatar>
            <q-icon name="warning" color="orange" />
          </q-item-section>
          <q-item-section>
            <q-item-label>{{ t('common.incidents') }}</q-item-label>
          </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/emergency" active-class="text-primary bg-red-1" aria-label="Emergencia">
          <q-item-section avatar>
            <q-icon name="emergency" color="red" />
          </q-item-section>
          <q-item-section>
            <q-item-label class="text-red">Emergencia</q-item-label>
          </q-item-section>
        </q-item>

        <q-separator class="q-my-md" />

        <q-item-label header class="text-grey-8" v-if="authStore.isAdmin">
          {{ t('common.admin') }}
        </q-item-label>

        <q-item clickable v-ripple to="/alerts" v-if="authStore.isManager" active-class="text-primary bg-blue-1" aria-label="Ir a alertas">
          <q-item-section avatar>
            <q-icon name="notifications_active" color="red" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Alertas</q-item-label>
          </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/security" v-if="authStore.isManager" active-class="text-primary bg-blue-1" aria-label="Ir a dashboard de seguridad">
          <q-item-section avatar>
            <q-icon name="security" color="green" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Seguridad</q-item-label>
          </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/users" v-if="authStore.isAdmin" active-class="text-primary bg-blue-1" aria-label="Ir a gestión de usuarios">
          <q-item-section avatar>
            <q-icon name="people" />
          </q-item-section>
          <q-item-section>
            <q-item-label>{{ t('common.users') }}</q-item-label>
          </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/settings" active-class="text-primary bg-blue-1" aria-label="Ir a configuración">
          <q-item-section avatar>
            <q-icon name="settings" />
          </q-item-section>
          <q-item-section>
            <q-item-label>{{ t('common.settings') }}</q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
    </q-drawer>

    <q-page-container>
      <router-view />
    </q-page-container>

    <q-footer v-if="authStore.isAuthenticated" class="lt-md bg-white text-primary shadow-3">
      <q-tabs no-caps active-color="primary" indicator-color="primary" class="text-grey">
        <q-route-tab to="/" icon="dashboard" label="Inicio" />
        <q-route-tab to="/posts" icon="article" label="Avisos" />
        <q-route-tab to="/polls" icon="poll" label="Votaciones" />
        <q-route-tab to="/incidents" icon="warning" label="Incidentes" />
        <q-route-tab to="/emergency" icon="emergency" label="Emergencia" class="text-red" />
        <q-route-tab to="/settings" icon="settings" label="Ajustes" />
      </q-tabs>
    </q-footer>
  </q-layout>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { useAuthStore } from '../stores/auth'
import { useCondominiumStore } from '../stores/condominium'
import { usePostStore } from '../stores/post'
import { usePollStore } from '../stores/poll'
import { useIncidentStore } from '../stores/incident'
import { useAlertStore } from '../stores/alert'
import { useI18n } from '../composables/useI18n'

const router = useRouter()
const $q = useQuasar()
const authStore = useAuthStore()
const condoStore = useCondominiumStore()
const postStore = usePostStore()
const pollStore = usePollStore()
const incidentStore = useIncidentStore()
const alertStore = useAlertStore()
const { t } = useI18n()

const leftDrawerOpen = ref(false)

const pendingCount = computed(() => {
  if (!authStore.isManager) return 0
  
  let count = 0
  if (postStore.posts) {
    count += postStore.posts.filter((p) => p.status === 1).length
  }
  if (pollStore.polls) {
    count += pollStore.polls.filter((p) => p.status === 1).length
  }
  if (incidentStore.incidents) {
    count += incidentStore.incidents.filter((i) => i.status === 1).length
  }
  if (alertStore.alerts) {
    count += alertStore.alerts.filter((a) => a.status === 1).length
  }
  return count
})

const userInitials = computed(() => {
  const name = authStore.currentUser?.fullName || 'Usuario'
  const parts = name.split(' ')
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return name.substring(0, 2).toUpperCase()
})

function toggleLeftDrawer() {
  leftDrawerOpen.value = !leftDrawerOpen.value
}

async function logout() {
  await authStore.logout()
  router.push('/login')
}
</script>
