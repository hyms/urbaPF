<template>
  <q-item clickable v-ripple @click="$emit('click', alert)">
    <q-item-section avatar>
      <q-avatar :color="getTypeColor(alert.alertType)" text-color="white">
        <q-icon :name="getTypeIcon(alert.alertType)" />
      </q-avatar>
    </q-item-section>
    <q-item-section>
      <q-item-label>
        <q-chip :color="getStatusColor(alert.status)" text-color="white" size="sm" class="q-mr-sm">
          {{ getStatusLabel(alert.status) }}
        </q-chip>
        {{ getTypeLabel(alert.alertType) }}
      </q-item-label>
      <q-item-label>{{ alert.message }}</q-item-label>
      <q-item-label caption v-if="alert.destinationAddress">
        Destino: {{ alert.destinationAddress }}
      </q-item-label>
      <q-item-label caption>
        {{ alert.createdByName }} | {{ formatDate(alert.createdAt) }}
      </q-item-label>
    </q-item-section>
    <q-item-section side>
      <q-btn flat round dense icon="more_vert" @click.stop>
        <q-menu>
          <q-list dense style="min-width: 150px">
            <q-item clickable v-close-popup @click="$emit('update-status', alert, 2)" v-if="alert.status === 1">
              <q-item-section>Acknowledged</q-item-section>
            </q-item>
            <q-item clickable v-close-popup @click="$emit('update-status', alert, 4)" v-if="alert.status === 2 || alert.status === 3">
              <q-item-section>Marcar llegado</q-item-section>
            </q-item>
            <q-item clickable v-close-popup @click="$emit('update-status', alert, 5)">
              <q-item-section>Completar</q-item-section>
            </q-item>
          </q-list>
        </q-menu>
      </q-btn>
    </q-item-section>
  </q-item>
</template>

<script setup>
import { useAlertStore } from '../stores/alert'
import { formatDate } from '../utils/format'

const props = defineProps({
  alert: { type: Object, required: true }
})

const emit = defineEmits(['click', 'update-status'])

const alertStore = useAlertStore()

function getTypeLabel(type) {
  return alertStore.getTypeLabel(type)
}

function getTypeColor(type) {
  const colors = { 1: 'red', 2: 'blue', 3: 'orange', 4: 'grey' }
  return colors[type] || 'grey'
}

function getTypeIcon(type) {
  const icons = { 1: 'emergency', 2: 'info', 3: 'warning', 4: 'help' }
  return icons[type] || 'notifications'
}

function getStatusLabel(status) {
  return alertStore.getStatusLabel(status)
}

function getStatusColor(status) {
  const colors = { 1: 'red', 2: 'orange', 3: 'blue', 4: 'green', 5: 'grey', 6: 'black' }
  return colors[status] || 'grey'
}
</script>
