<template>
  <q-item>
    <q-item-section avatar>
      <q-icon :name="getIcon()" :color="getColor()" />
    </q-item-section>
    <q-item-section>
      <q-item-label>{{ incident.title || 'Incidente' }}</q-item-label>
      <q-item-label caption>
        {{ formatDate(incident.created_at) }}
      </q-item-label>
    </q-item-section>
    <q-item-section side>
      <q-badge :color="getStatusColor(incident.status)">
        {{ getStatusLabel(incident.status) }}
      </q-badge>
    </q-item-section>
  </q-item>
</template>

<script setup lang="ts">
import { useI18n } from '@/composables/useI18n'
import { Incident } from '@/types/models'
import { IncidentStatusLabel, IncidentStatusColor, IncidentTypeIcon } from '@/utils/appEnums'

const props = defineProps<{
  incident: Incident
}>()

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const { t } = useI18n()

function getIcon(): string {
  return IncidentTypeIcon(props.incident.type)
}

function getColor(): string {
  return IncidentStatusColor(props.incident.status)
}

function getStatusLabel(status: number): string {
  return IncidentStatusLabel(status)
}

function getStatusColor(status: number): string {
  return IncidentStatusColor(status)
}

function formatDate(dateStr: string): string {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleDateString('es-BO')
}
</script>
