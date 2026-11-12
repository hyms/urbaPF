<template>
  <q-card flat bordered>
    <q-card-section>
      <div class="text-h6 q-mb-md">{{ t('settings.security') }}</div>
      <q-form @submit="onSubmit" class="q-gutter-md">
        <q-input
          v-model="form.current"
          :label="t('settings.currentPassword') + ' *'"
          type="password"
          filled
          :rules="[v => !!v || t('common.required')]"
        />
        <q-input
          v-model="form.new"
          :label="t('settings.newPassword') + ' *'"
          type="password"
          filled
          :rules="[
            v => !!v || t('common.required'),
            v => v.length >= 6 || t('users.passwordMinLength')
          ]"
        />
        <q-input
          v-model="form.confirm"
          :label="t('settings.confirmPassword') + ' *'"
          type="password"
          filled
          :rules="[
            v => !!v || t('common.required'),
            v => v === form.new || t('settings.passwordsNotMatch')
          ]"
        />
        <q-btn color="primary" :label="t('settings.changePassword')" type="submit" :loading="loading" />
      </q-form>
    </q-card-section>
  </q-card>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'

const props = defineProps<{
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'submit', formData: { current: string; new: string; confirm: string }): void
}>()

const { t } = useI18n()

const form = ref({
  current: '',
  new: '',
  confirm: ''
})

const onSubmit = () => {
  emit('submit', form.value)
}
</script>