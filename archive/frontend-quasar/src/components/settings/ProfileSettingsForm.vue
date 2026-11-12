<template>
  <q-card flat bordered>
    <q-card-section>
      <div class="text-h6 q-mb-md">{{ t('settings.profile') }}</div>
      <q-form @submit="onSubmit" class="q-gutter-md">
        <q-input v-model="form.fullName" :label="t('settings.fullName')" filled />
        <q-input v-model="form.email" :label="t('settings.email')" type="email" filled readonly />
        <q-input v-model="form.phone" :label="t('settings.phone')" type="tel" filled />
        <q-input v-model="form.streetAddress" :label="t('settings.address')" filled />
        <q-btn color="primary" :label="t('common.save')" type="submit" :loading="loading" />
      </q-form>
    </q-card-section>
  </q-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { User } from '@/types/models'

const props = defineProps<{
  user: User | null
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'submit', formData: { fullName: string; phone: string; streetAddress: string }): void
}>()

const { t } = useI18n()

const form = ref({
  fullName: '',
  email: '',
  phone: '',
  streetAddress: ''
})

watch(() => props.user, (newVal) => {
  if (newVal) {
    form.value = {
      fullName: newVal.fullName,
      email: newVal.email,
      phone: newVal.phone || '',
      streetAddress: newVal.streetAddress || ''
    }
  }
}, { immediate: true })

const onSubmit = () => {
  emit('submit', form.value)
}
</script>