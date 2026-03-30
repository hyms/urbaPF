<template>
  <div class="poll-results">
    <div class="text-subtitle2 q-mb-md">{{ t('polls.results') }}</div>
    
    <div v-for="(option, idx) in options" :key="idx" class="q-mb-md">
      <div class="row items-center justify-between q-mb-xs">
        <div class="text-body2">{{ option }}</div>
        <div class="text-caption text-grey">
          {{ getCount(idx) }} {{ t('polls.votes') }} ({{ getPercentage(idx) }}%)
        </div>
      </div>
      <q-linear-progress
        :value="getPercentage(idx) / 100"
        :color="getBarColor(idx)"
        track-color="grey-3"
        rounded
        size="12px"
      />
    </div>

    <q-separator class="q-my-md" />

    <div class="row justify-between text-caption text-grey">
      <span>{{ t('polls.totalVotes') }}: {{ totalVotes }}</span>
      <span>{{ t('polls.participants') }}: {{ results?.votersCount || 0 }}</span>
    </div>

    <div v-if="showSignature" class="q-mt-md">
      <div class="text-caption text-grey">
        <q-icon name="verified" class="q-mr-xs" />
        {{ t('polls.signatureVerified') }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from '../composables/useI18n'

interface PollResults {
  results: number[]
  votersCount: number
  signature?: string
}

const props = defineProps<{
  options: string[]
  results?: PollResults | null
  showSignature?: boolean
  userVotedOption?: number
}>()

const { t } = useI18n()

const totalVotes = computed(() => {
  if (!props.results?.results) return 0
  return props.results.results.reduce((sum, count) => sum + count, 0)
})

function getCount(index: number): number {
  return props.results?.results?.[index] || 0
}

function getPercentage(index: number): number {
  if (totalVotes.value === 0) return 0
  return Math.round((getCount(index) / totalVotes.value) * 100)
}

function getBarColor(index: number): string {
  if (props.userVotedOption === index) return 'primary'
  const maxVotes = Math.max(...(props.results?.results || [0]))
  if (getCount(index) === maxVotes && maxVotes > 0) return 'positive'
  return 'grey-6'
}
</script>
