<template>
  <q-card style="min-width: 350px">
    <q-card-section>
      <div class="text-h6">{{ t('users.changePassword') }}</div>
    </q-card-section>

    <q-card-section>
      <q-form @submit="onSubmit" class="q-gutter-md">
        <q-input
          v-if="isCurrentUser"
          v-model="form.oldPassword"
          :label="t('users.oldPassword') + ' *'"
          :type="showOldPassword ? 'text' : 'password'"
          outlined
          :rules="[v => !!v || t('common.required')]"
        >
          <template v-slot:append>
            <q-icon
              :name="showOldPassword ? 'visibility' : 'visibility_off'"
              class="cursor-pointer"
              @click="showOldPassword = !showOldPassword"
            />
          </template>
        </q-input>

        <q-input
          v-model="form.newPassword"
          :label="t('users.newPassword') + ' *'"
          :type="showNewPassword ? 'text' : 'password'"
          outlined
          :rules="[
            v => !!v || t('common.required'),
            v => v.length >= 6 || t('users.passwordMinLength')
          ]"
        >
          <template v-slot:append>
            <q-icon
              :name="showNewPassword ? 'visibility' : 'visibility_off'"
              class="cursor-pointer"
              @click="showNewPassword = !showNewPassword"
            />
          </template>
        </q-input>

        <q-input
          v-model="form.confirmPassword"
          :label="t('users.confirmNewPassword') + ' *'"
          :type="showConfirmPassword ? 'text' : 'password'"
          outlined
          :rules="[
            v => !!v || t('common.required'),
            v => v === form.newPassword || t('users.passwordsMatch')
          ]"
        >
          <template v-slot:append>
            <q-icon
              :name="showConfirmPassword ? 'visibility' : 'visibility_off'"
              class="cursor-pointer"
              @click="showConfirmPassword = !showConfirmPassword"
            />
          </template>
        </q-input>

        <q-card-actions align="right">
          <q-btn flat :label="t('common.cancel')" @click="$emit('cancel')" :disable="loading" />
          <q-btn color="primary" :label="t('common.save')" type="submit" :loading="loading" no-caps />
        </q-card-actions>
      </q-form>
    </q-card-section>
  </q-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'

const props = defineProps<{
  userId: string
  isAdmin: boolean
  isCurrentUser: boolean
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'submit', formData: { oldPassword?: string; newPassword: string; confirmPassword: string }): void
  (e: 'cancel'): void
}>()

const { t } = useI18n()

const showOldPassword = ref(false)
const showNewPassword = ref(false)
const showConfirmPassword = ref(false)

const form = ref({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const onSubmit = () => {
  emit('submit', form.value)
}
</script>