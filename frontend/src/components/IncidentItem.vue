<template>
  <q-item>
    <q-item-section avatar>
      <q-icon :name="getIcon()" :color="getColor()" />
    </q-item-section>
    <q-item-section>
      <q-item-label>{{ incident.title || 'Incidente' }}</q-item-label>
      <q-item-label caption>
        {{ formatDate(incident.createdAt) }}
      </q-item-label>
    </q-item-section>
    <q-item-section side>
      <q-badge :color="getStatusColor(incident.status)">
        {{ getStatusLabel(incident.status) }}
      </q-badge>
    </q-item-section>
  </q-item>
</template>

<script setup>
import { useI18n } from '../composables/useI18n'

const props = defineProps({
  incident: {
    type: Object,
    required: true
  }
})

const { t } = useI18n()

function getIcon() {
  const icons = {
    1: 'warning',
    2: 'build',
    3: 'pending',
    4: 'check_circle',
    5: 'cancel'
  }
  return icons[props.incident.status] || 'report_problem'
}

function getColor() {
  const colors = {
    1: 'orange',
    2: 'blue',
    3: 'yellow',
    4: 'green',
    5: 'grey'
  }
  return colors[props.incident.status] || 'grey'
}

function getStatusLabel(status) {
  const labels = {
    1: 'Reportado',
    2: 'En Proceso',
    3: 'Pendiente',
    4: 'Resuelto',
    5: 'Cerrado'
  }
  return labels[status] || 'Desconocido'
}

function getStatusColor(status) {
  const colors = {
    1: 'orange',
    2: 'blue',
    3: 'yellow',
    4: 'green',
    5: 'grey'
  }
  return colors[status] || 'grey'
}

function formatDate(dateStr) {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleDateString('es-BO')
}
</script>
