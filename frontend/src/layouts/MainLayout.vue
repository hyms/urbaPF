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

        <q-btn flat round dense icon="notifications" class="q-mr-xs">
          <q-badge color="red" floating>3</q-badge>
        </q-btn>

        <q-btn flat round dense>
          <q-avatar size="32px" color="white" text-color="primary">
            {{ userInitials }}
          </q-avatar>
          <q-menu>
            <q-list style="min-width: 150px">
              <q-item clickable v-close-popup>
                <q-item-section avatar>
                  <q-icon name="person" />
                </q-item-section>
                <q-item-section>Perfil</q-item-section>
              </q-item>
              <q-separator />
              <q-item clickable v-close-popup @click="logout">
                <q-item-section avatar>
                  <q-icon name="logout" />
                </q-item-section>
                <q-item-section>Cerrar sesión</q-item-section>
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
    >
      <q-list>
        <q-item-label header class="text-grey-8">
          Menú Principal
        </q-item-label>

        <q-item clickable v-ripple to="/" exact active-class="text-primary bg-blue-1">
          <q-item-section avatar>
            <q-icon name="dashboard" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Dashboard</q-item-label>
          </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/condominiums" active-class="text-primary bg-blue-1">
          <q-item-section avatar>
            <q-icon name="home_work" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Condominios</q-item-label>
          </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/posts" active-class="text-primary bg-blue-1">
          <q-item-section avatar>
            <q-icon name="article" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Publicaciones</q-item-label>
          </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/incidents" active-class="text-primary bg-blue-1">
          <q-item-section avatar>
            <q-icon name="warning" color="orange" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Incidentes</q-item-label>
          </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/polls" active-class="text-primary bg-blue-1">
          <q-item-section avatar>
            <q-icon name="poll" color="purple" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Votaciones</q-item-label>
          </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/alerts" active-class="text-primary bg-blue-1">
          <q-item-section avatar>
            <q-icon name="notifications_active" color="red" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Alertas</q-item-label>
          </q-item-section>
        </q-item>

        <q-separator class="q-my-md" />

        <q-item-label header class="text-grey-8" v-if="authStore.isAdmin">
          Administración
        </q-item-label>

        <q-item clickable v-ripple to="/users" v-if="authStore.isAdmin" active-class="text-primary bg-blue-1">
          <q-item-section avatar>
            <q-icon name="people" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Usuarios</q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
    </q-drawer>

    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const leftDrawerOpen = ref(false)

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

function logout() {
  authStore.logout()
  router.push('/login')
}
</script>
