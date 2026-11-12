<script setup lang="ts">
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const drawer = ref(true)
</script>

<template>
  <v-app-bar color="primary" flat>
    <v-app-bar-nav-icon @click="drawer = !drawer" />
    <v-app-bar-title>{{ t('app.title') }}</v-app-bar-title>
    <v-btn to="/login" variant="text">{{ t('nav.login') }}</v-btn>
  </v-app-bar>

  <v-navigation-drawer v-model="drawer" color="surface">
    <v-list nav>
      <v-list-item to="/health" prepend-icon="mdi-heart-pulse" :title="t('nav.health')" />
    </v-list>
  </v-navigation-drawer>

  <v-container class="py-6" fluid>
    <router-view />
  </v-container>
</template>