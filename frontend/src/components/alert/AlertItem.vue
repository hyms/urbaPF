<template>
  <q-card :class="getAlertClass(alert.status)">
    <q-card-section>
      <div class="row items-center">
        <div class="col">
          <div class="text-h6">
            <q-icon :name="AlertTypeIcon(alert.type)" class="q-mr-sm" />
            {{ alert.title }}
          </div>
          <div class="text-caption text-grey">
            {{ formatDate(alert.created_at) }}
            <span v-if="alert.needs_approval && alert.status === 1" class="text-warning q-ml-sm">
              <q-icon name="warning" />
              {{ t('alerts.needsApproval') }}
            </span>
          </div>
        </div>
        <q-badge :color="AlertStatusColor(alert.status)" size="md">
          {{ AlertStatusLabel(alert.status) }}
        </q-badge>
      </div>
      <div v-if="alert.description" class="q-mt-sm text-body2">
        {{ alert.description }}
      </div>
      <div class="q-mt-sm">
        <q-chip
          v-if="alert.reputation_level < 4"
          dense
          color="warning"
          text-color="white"
          size="sm"
        >
          Reputación: {{ alert.reputation_level }}
        </q-chip>
        <q-chip
          v-else
          dense
          color="positive"
          text-color="white"
          size="sm"
        >
          Alta Reputación
        </q-chip>
      </div>
    </q-card-section>

    <q-separator />

    <q-card-actions>
      <q-btn
        v-if="authStore.isManager && alert.status === 1"
        flat
        color="positive"
        :label="t('alerts.approve')"
        @click="$emit('approve', alert.id)"
      />
      <q-btn
        v-if="authStore.isManager && alert.status >= 2"
        flat
        color="primary"
        :label="t('alerts.acknowledge')"
        @click="$emit('acknowledge', alert.id)"
      />
      <q-btn
        v-if="alert.status >= 3"
        flat
        color="grey"
        :label="t('alerts.resolve')"
        @click="$emit('resolve', alert.id)"
      />
      <q-btn
        v-if="canResend(alert)"
        flat
        color="info"
        :label="t('alerts.resend')"
        @click="$emit('resend', alert.id)"
      />
      <q-space />
      <q-btn
        v-if="authStore.isAdmin"
        flat
        round
        dense
        icon="delete"
        color="negative"
        aria-label="Eliminar alerta"
        @click="$emit('delete', alert)"
      />
    </q-card-actions>
  </q-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { Alert } from '@/types/models'
import { formatDate } from '@/utils/format'
import { AlertTypeIcon, AlertStatusColor, AlertStatusLabel } from '@/utils/appEnums'
import { useAuthStore } from '@/stores/auth'

const props = defineProps<{
  alert: Alert
}>()

const emit = defineEmits<{
  (e: 'approve', id: string): void
  (e: 'acknowledge', id: string): void
  (e: 'resolve', id: string): void
  (e: 'resend', id: string): void
  (e: 'delete', alert: Alert): void
}>()

const { t } = useI18n()
const authStore = useAuthStore()

function getAlertClass(status: number) {
  if (status === 1) return 'alert-border-warning'
  if (status >= 3 && status < 5) return 'alert-border-negative'
  return ''
}

function canResend(alert: Alert) {
  if (!authStore.isManager && !authStore.isAdmin) return false
  if (alert.needs_approval && alert.status < 2) return false
  return alert.status >= 3
}
</script>

<style scoped>
.alert-border-warning {
  border-left: 4px solid #ff9800;
}
.alert-border-negative {
  border-left: 4px solid #f44336;
}
</style>