<template>
  <q-card>
    <q-card-section>
      <div class="row items-center q-mb-sm">
        <div class="col">
          <div class="text-h6">{{ incident.title }}</div>
          <div class="text-caption text-grey">
            {{ formatDate(incident.createdAt) }}
          </div>
        </div>
        <q-badge :color="getPriorityColor(incident.priority)">
          {{ getPriorityLabel(incident.priority) }}
        </q-badge>
      </div>
      <div class="row items-center q-mb-sm">
        <q-chip dense :color="getTypeColor(incident.type)" text-color="white" size="sm">
          <q-icon :name="getTypeIcon(incident.type)" class="q-mr-xs" />
          {{ getTypeLabel(incident.type) }}
        </q-chip>
        <q-chip dense :color="getStatusColor(incident.status)" text-color="white" size="sm" class="q-ml-sm">
          {{ getStatusLabel(incident.status) }}
        </q-chip>
      </div>
      <div v-if="incident.description" class="text-body2 text-grey-7">
        {{ incident.description }}
      </div>
      <div v-if="incident.media" class="q-mt-sm">
        <q-img
          v-for="(media, idx) in parseMedia(incident.media)"
          :key="idx"
          :src="media.path"
          class="rounded-borders q-mr-sm"
          style="width: 60px; height: 60px; display: inline-block"
        />
      </div>
    </q-card-section>

    <q-separator />

    <q-card-actions v-if="canManage">
      <slot name="actions" />
      <q-space />
      <q-btn flat round dense icon="more_vert" aria-label="Más acciones del incidente">
        <q-menu>
          <q-list dense>
            <q-item clickable v-close-popup @click="$emit('edit', incident)">
              <q-item-section>{{ t('common.update') }}</q-item-section>
            </q-item>
            <q-item
              v-if="incident.status < 4"
              clickable
              v-close-popup
              @click="$emit('delete', incident)"
              class="text-negative"
            >
              <q-item-section>{{ t('common.delete') }}</q-item-section>
            </q-item>
          </q-list>
        </q-menu>
      </q-btn>
    </q-card-actions>
  </q-card>
</template>

<script setup lang="ts">
import { Incident } from '@/types/models'
import {
  IncidentTypeLabel,
  IncidentTypeIcon,
  IncidentPriorityLabel,
  IncidentPriorityColor,
  IncidentStatusLabel,
  IncidentStatusColor
} from '@/utils/appEnums'
import { useI18n } from '@/composables/useI18n'
import { formatDate, parseMedia } from '@/utils/format'

defineProps<{
  incident: Incident
  canManage: boolean
}>()

defineEmits<{
  (e: 'edit', incident: Incident): void
  (e: 'delete', incident: Incident): void
}>()

const { t } = useI18n()

function getTypeLabel(type: number) { return IncidentTypeLabel(type) }
function getTypeIcon(type: number) { return IncidentTypeIcon(type) }
function getTypeColor(type: number) {
  const colors: Record<number, string> = { 1: 'blue', 2: 'red', 3: 'green', 4: 'orange', 5: 'grey' }
  return colors[type] || 'grey'
}
function getPriorityLabel(priority: number) { return IncidentPriorityLabel(priority) }
function getPriorityColor(priority: number) { return IncidentPriorityColor(priority) }
function getStatusLabel(status: number) { return IncidentStatusLabel(status) }
function getStatusColor(status: number) { return IncidentStatusColor(status) }
</script>