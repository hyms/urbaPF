<template>
  <q-card style="min-width: 350px">
    <q-card-section>
      <div class="text-h6">{{ t('incidents.markResolved') }}</div>
    </q-card-section>
    <q-card-section>
      <q-input
        v-model="resolutionNotes"
        type="textarea"
        :label="t('incidents.resolutionNotes')"
        outlined
        rows="3"
      />
    </q-card-section>
    <q-card-actions align="right">
      <q-btn flat :label="t('common.cancel')" v-close-popup @click="$emit('cancel')" />
      <q-btn color="positive" :label="t('common.save')" @click="onSubmit" />
    </q-card-actions>
  </q-card>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'

const emit = defineEmits<{
  (e: 'submit', notes: string): void
  (e: 'cancel'): void
}>()

const { t } = useI18n()

const resolutionNotes = ref('')

const onSubmit = () => {
  emit('submit', resolutionNotes.value)
}
</script>