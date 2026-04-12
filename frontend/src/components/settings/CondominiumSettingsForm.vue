<template>
  <q-card flat bordered>
    <q-card-section>
      <div class="text-h6 q-mb-md">{{ t('settings.condoSettings') }}</div>
      <q-form @submit="onSubmit" class="q-gutter-md">
        <q-input v-model="form.name" :label="t('settings.condoName')" filled :readonly="!authStore.isAdmin" />
        <q-input v-model="form.address" :label="t('settings.address')" filled :readonly="!authStore.isAdmin" />
        <q-input v-model="form.monthlyFee" :label="t('settings.monthlyFee')" type="number" filled :readonly="!authStore.isAdmin" />
        <q-input v-model="form.currency" :label="t('settings.currency')" filled :readonly="!authStore.isAdmin" />
        <q-input v-model="form.rules" :label="t('settings.rules')" type="textarea" filled :readonly="!authStore.isAdmin" />
        <q-toggle v-model="form.isActive" :label="t('settings.active')" :disable="!authStore.isAdmin" />
        <q-btn color="primary" :label="t('common.save')" type="submit" :loading="loading" :disable="!authStore.isAdmin" />
      </q-form>
    </q-card-section>
  </q-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { Condominium } from '@/types/models'
import { useAuthStore } from '@/stores/auth'

const props = defineProps<{
  condominium: Condominium | null
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'submit', formData: any): void
}>()

const { t } = useI18n()
const authStore = useAuthStore()

const form = ref({
  name: '',
  address: '',
  monthlyFee: 0,
  currency: 'BOB',
  rules: '',
  isActive: true
})

watch(() => props.condominium, (newVal) => {
  if (newVal) {
    form.value = {
      name: newVal.name,
      address: newVal.address,
      monthlyFee: newVal.monthlyFee,
      currency: newVal.currency,
      rules: newVal.rules || '',
      isActive: newVal.isActive
    }
  }
}, { immediate: true })

const onSubmit = () => {
  emit('submit', form.value)
}
</script>