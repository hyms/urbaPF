<template>
  <q-card style="min-width: 500px">
    <q-card-section>
      <div class="text-h6">{{ isEditing ? t('polls.editPoll') : t('polls.newPoll') }}</div>
    </q-card-section>

    <q-card-section>
      <q-form class="q-gutter-md" @submit.prevent="submit">
        <q-input
          v-model="form.title"
          :label="t('polls.title')"
          outlined
          :rules="[v => !!v || t('common.required')]"
        />

        <q-input
          v-model="form.description"
          :label="t('polls.description')"
          type="textarea"
          outlined
          rows="3"
        />

        <div>
          <div class="text-subtitle2 q-mb-sm">{{ t('polls.options') }}</div>
          <div v-for="(opt, idx) in form.options" :key="idx" class="row items-center q-mb-sm">
            <q-input
              v-model="form.options[idx]"
              :label="`${t('polls.option')} ${idx + 1}`"
              outlined
              dense
              class="col"
              :rules="[v => !!v || t('common.required')]"
            />
            <q-btn
              flat
              round
              dense
              icon="delete"
              color="negative"
              @click="removeOption(idx)"
              :disable="form.options.length <= 2"
              class="q-ml-sm"
            />
          </div>
          <q-btn
            flat
            color="primary"
            icon="add"
            :label="t('polls.addOption')"
            @click="addOption"
          />
        </div>

        <q-select
          v-model="form.pollType"
          :options="pollTypeOptions"
          :label="t('polls.type')"
          outlined
          emit-value
          map-options
        />

        <div class="row q-col-gutter-md">
          <div class="col-6">
            <q-input
              v-model="form.startsAt"
              :label="t('polls.startsAt')"
              type="datetime-local"
              outlined
            />
          </div>
          <div class="col-6">
            <q-input
              v-model="form.endsAt"
              :label="t('polls.endsAt')"
              type="datetime-local"
              outlined
            />
          </div>
        </div>

        <q-select
          v-model="form.minRoleToVote"
          :options="roleOptions"
          :label="t('polls.minRoleToVote')"
          outlined
          emit-value
          map-options
        />

        <q-toggle
          v-model="form.requiresJustification"
          :label="t('polls.requiresJustification')"
        />
      </q-form>
    </q-card-section>

    <q-card-actions align="right">
      <q-btn flat :label="t('common.cancel')" @click="$emit('cancel')" />
      <q-btn
        color="primary"
        :label="t('common.save')"
        @click="submit"
        :loading="loading"
      />
    </q-card-actions>
  </q-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { useI18n } from '../composables/useI18n'

interface PollFormData {
  title: string
  description: string
  options: string[]
  pollType: number
  startsAt: string
  endsAt: string
  requiresJustification: boolean
  minRoleToVote: number
}

const props = defineProps<{
  poll?: PollFormData | null
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'submit', data: PollFormData): void
  (e: 'cancel'): void
}>()

const { t } = useI18n()

const defaultForm = (): PollFormData => ({
  title: '',
  description: '',
  options: ['', ''],
  pollType: 1,
  startsAt: new Date().toISOString().slice(0, 16),
  endsAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().slice(0, 16),
  requiresJustification: false,
  minRoleToVote: 2
})

const form = ref<PollFormData>(defaultForm())

const isEditing = computed(() => !!props.poll)

const pollTypeOptions = [
  { label: 'Opción única', value: 1 },
  { label: 'Opción múltiple', value: 2 }
]

const roleOptions = [
  { label: 'Vecino', value: 2 },
  { label: 'Encargado', value: 3 },
  { label: 'Administrador', value: 4 }
]

watch(() => props.poll, (newPoll) => {
  if (newPoll) {
    form.value = {
      ...newPoll,
      options: Array.isArray(newPoll.options) ? [...newPoll.options] : JSON.parse(newPoll.options as unknown as string || '[]'),
      startsAt: newPoll.startsAt?.slice(0, 16) || '',
      endsAt: newPoll.endsAt?.slice(0, 16) || ''
    }
  } else {
    form.value = defaultForm()
  }
}, { immediate: true })

function addOption() {
  form.value.options.push('')
}

function removeOption(index: number) {
  if (form.value.options.length > 2) {
    form.value.options.splice(index, 1)
  }
}

function submit() {
  if (!form.value.title || form.value.options.some(o => !o.trim())) {
    return
  }
  emit('submit', {
    ...form.value,
    options: form.value.options.filter(o => o.trim()),
    startsAt: new Date(form.value.startsAt).toISOString(),
    endsAt: new Date(form.value.endsAt).toISOString()
  })
}
</script>
