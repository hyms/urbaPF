<template>
  <q-item>
    <q-item-section avatar>
      <q-icon :name="getIcon()" :color="getColor()" />
    </q-item-section>
    <q-item-section>
      <q-item-label>{{ alert.title || 'Alerta' }}</q-item-label>
      <q-item-label caption>
        {{ formatDate(alert.createdAt) }}
      </q-item-label>
    </q-item-section>
    <q-item-section side>
      <q-badge :color="getStatusColor(alert.status)">
        {{ getStatusLabel(alert.status) }}
      </q-badge>
    </q-item-section>
  </q-item>
</template>

<script setup lang="ts">
import { useI18n } from '../composables/useI18n'
import { AlertStatusLabel, AlertStatusColor, AlertTypeIcon } from '../utils/appEnums'

interface Alert {
  id: string
  title: string
  status: number
  type: number
  createdAt: string
}

const props = defineProps<{
  alert: Alert
}>()

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const { t } = useI18n()

function getIcon() {
  return AlertTypeIcon(props.alert.type)
}

function getColor() {
  return AlertStatusColor(props.alert.status)
}

function getStatusLabel(status: number) {
  return AlertStatusLabel(status)
}

function getStatusColor(status: number) {
  return AlertStatusColor(status)
}

function formatDate(dateStr: string) {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleDateString('es-BO')
}
</script>
