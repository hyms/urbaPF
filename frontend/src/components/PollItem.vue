<template>
  <q-card>
    <q-card-section>
      <div class="row items-center">
        <div class="col">
          <div class="text-h6">{{ poll.title }}</div>
          <div class="text-caption text-grey">{{ poll.description }}</div>
        </div>
        <q-chip :color="getStatusColor(poll.status)" text-color="white" size="sm">
          {{ getStatusLabel(poll.status) }}
        </q-chip>
        <q-btn flat round dense icon="more_vert" v-if="showAdminControls">
          <q-menu>
            <q-list dense style="min-width: 150px">
              <q-item clickable v-close-popup @click="$emit('edit', poll)">
                <q-item-section>{{ t('common.update') }}</q-item-section>
              </q-item>
              <q-item clickable v-close-popup @click="$emit('delete', poll)">
                <q-item-section class="text-negative">{{ t('common.delete') }}</q-item-section>
              </q-item>
            </q-list>
          </q-menu>
        </q-btn>
      </div>
    </q-card-section>
    <q-separator />
    <q-card-section>
      <div class="q-mb-sm">
        <strong>{{ t('polls.options') }}:</strong>
      </div>
      <q-option-group
        :model-value="selectedOption"
        @update:model-value="$emit('update:selectedOption', $event)"
        :options="parsedOptions"
        type="radio"
        :disable="poll.status !== 2"
      />
    </q-card-section>
    <q-separator />
    <q-card-actions>
      <q-btn flat color="primary" :label="t('polls.vote')" @click="$emit('vote', poll)" :disable="poll.status !== 2 || hasVoted" />
      <q-btn flat :label="t('polls.results')" @click="$emit('view-results', poll)" />
      <q-space />
      <div class="text-caption text-grey">
        {{ formatDate(poll.startsAt) }} - {{ formatDate(poll.endsAt) }}
      </div>
    </q-card-actions>
  </q-card>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { usePollStore } from '../stores/poll'
import { useI18n } from '../composables/useI18n'
import { formatDate } from '../utils/format'
import { PollStatusLabel, PollStatusColor } from '../utils/appEnums'

interface Poll {
  id: string
  title: string
  description?: string
  options: string
  status: number
  startsAt: string
  endsAt: string
}

const props = defineProps<{
  poll: Poll
  selectedOption?: number
  hasVoted: boolean
  showAdminControls: boolean
}>()

const emit = defineEmits<{
  (e: 'update:selectedOption', value: number): void
  (e: 'vote', poll: Poll): void
  (e: 'view-results', poll: Poll): void
  (e: 'edit', poll: Poll): void
  (e: 'delete', poll: Poll): void
}>()

const { t } = useI18n()

const parsedOptions = computed(() => {
  try {
    const options = JSON.parse(props.poll.options)
    return options.map((opt: string, idx: number) => ({ label: opt, value: idx }))
  } catch {
    return []
  }
})

function getStatusLabel(status: number): string {
  return PollStatusLabel(status)
}

function getStatusColor(status: number): string {
  return PollStatusColor(status)
}
</script>
