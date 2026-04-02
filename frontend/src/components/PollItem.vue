<template>
  <q-card>
    <q-card-section>
      <div class="row items-center q-mb-sm">
        <div class="col">
          <div class="text-h6">{{ poll.title }}</div>
          <div class="text-caption text-grey">{{ poll.description }}</div>
        </div>
        <q-chip :color="getStatusColor(poll.status)" text-color="white" size="sm">
          {{ getStatusLabel(poll.status) }}
        </q-chip>
      </div>
      <div class="row items-center text-caption text-grey">
        <q-icon name="schedule" class="q-mr-xs" />
        {{ formatDate(poll.startsAt) }} - {{ formatDate(poll.endsAt) }}
        <q-space />
        <q-btn flat dense size="sm" icon="more_vert" v-if="canManage">
          <q-menu>
            <q-list dense style="min-width: 150px">
              <q-item clickable v-close-popup @click="$emit('edit', poll)">
                <q-item-section>{{ t('common.update') }}</q-item-section>
              </q-item>
              <q-item clickable v-close-popup @click="$emit('delete', poll)" class="text-negative">
                <q-item-section>{{ t('common.delete') }}</q-item-section>
              </q-item>
            </q-list>
          </q-menu>
        </q-btn>
      </div>
    </q-card-section>

    <q-separator />

    <q-card-section>
      <PollResultsDisplay
        v-if="showResults"
        :options="parsedOptions"
        :results="pollResults"
        :show-signature="poll.status >= 4"
        :user-voted-option="userVotedOption"
      />
      
      <div v-else>
        <div class="q-mb-sm"><strong>{{ t('polls.options') }}:</strong></div>
        <q-option-group
          :model-value="selectedOption"
          @update:model-value="$emit('update:selectedOption', $event)"
          :options="parsedOptions"
          :type="poll.pollType === 2 ? 'checkbox' : 'radio'"
          :disable="!canVote"
        />
      </div>
    </q-card-section>

    <q-separator />

    <q-card-actions>
      <q-btn
        v-if="canVote && !hasVoted"
        flat
        color="primary"
        :label="t('polls.vote')"
        @click="$emit('vote', poll)"
        :disable="selectedOption === undefined"
      />
      <q-btn
        flat
        :label="showResults ? t('polls.backToVoting') : t('polls.results')"
        @click="toggleView"
      />
      <q-space />
      <div class="text-caption text-grey" v-if="hasVoted">
        <q-icon name="check_circle" color="positive" class="q-mr-xs" />
        {{ t('polls.youVoted') }}
      </div>
    </q-card-actions>
  </q-card>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useI18n } from '@/composables/useI18n'
import { formatDate } from '@/utils/format'
import { PollStatusLabel, PollStatusColor } from '@/utils/appEnums'
import { Poll } from '@/types/models'
import PollResultsDisplay from './PollResultsDisplay.vue'

interface PollResults {
  results: number[]
  votersCount: number
}

const props = defineProps<{
  poll: Poll
  selectedOption?: number | number[]
  hasVoted: boolean
  canManage: boolean
  pollResults?: PollResults | null
}>()

const emit = defineEmits<{
  (e: 'update:selectedOption', value: number | number[]): void
  (e: 'vote', poll: Poll): void
  (e: 'edit', poll: Poll): void
  (e: 'delete', poll: Poll): void
}>()

const { t } = useI18n()

const showResultsMode = ref(false)

const parsedOptions = computed(() => {
  try {
    const options = JSON.parse(props.poll.options)
    return options.map((opt: string, idx: number) => ({ label: opt, value: idx }))
  } catch {
    return []
  }
})

const canVote = computed(() => {
  return props.poll.status === 3
})

const canManage = computed(() => {
  return props.canManage && (props.poll.status === 1 || props.poll.status === 2)
})

const showResults = computed(() => {
  return showResultsMode.value || props.hasVoted || props.poll.status >= 4
})

const userVotedOption = computed(() => {
  if (!props.hasVoted) return undefined
  if (typeof props.selectedOption === 'number') return props.selectedOption
  return (props.selectedOption as number[])?.[0]
})

function toggleView() {
  showResultsMode.value = !showResultsMode.value
}

function getStatusLabel(status: number): string {
  return PollStatusLabel(status)
}

function getStatusColor(status: number): string {
  return PollStatusColor(status)
}
</script>

