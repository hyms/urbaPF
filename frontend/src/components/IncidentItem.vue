<template>
  <q-item clickable v-ripple @click="$emit('click', incident)">
    <q-item-section avatar>
      <q-avatar :color="getPriorityColor(incident.priority)" text-color="white">
        <q-icon name="warning" />
      </q-avatar>
    </q-item-section>
    <q-item-section>
      <q-item-label>{{ incident.title }}</q-item-label>
      <q-item-label caption>
        {{ incident.description }}<br>
        {{ t('incidents.reportedBy') }}: {{ incident.reportedByName }} | {{ formatDate(incident.createdAt) }}
      </q-item-label>
    </q-item-section>
    <q-item-section side>
      <q-chip :color="getIncidentStatusColor(incident.status)" text-color="white" size="sm">
        {{ getIncidentStatusLabel(incident.status) }}
      </q-chip>
    </q-item-section>
    <q-item-section side v-if="$slots.menu">
      <slot name="menu"></slot>
    </q-item-section>
  </q-item>
</template>

<script setup>
import { formatDate, getPriorityColor, getIncidentStatusLabel, getIncidentStatusColor } from '../utils/format'
import { useI18n } from '../composables/useI18n'

const props = defineProps({
  incident: {
    type: Object,
    required: true
  }
})

const { t } = useI18n()
</script>
