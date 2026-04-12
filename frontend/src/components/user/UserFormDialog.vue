<template>
  <q-card style="min-width: 450px; max-width: 90vw;">
    <q-card-section class="row items-center q-pb-none">
      <div class="text-h6">{{ editingUser ? t('users.editUser') : t('users.newUser') }}</div>
      <q-space />
      <q-btn icon="close" flat round dense v-close-popup @click="$emit('cancel')" aria-label="Cerrar" />
    </q-card-section>

    <q-card-section>
      <q-form @submit="onSubmit" class="q-gutter-md">
        <q-input
          v-model="form.fullName"
          :label="t('auth.fullName') + ' *'"
          filled
          :rules="[v => !!v || t('common.required')]"
        />

        <q-input
          v-model="form.email"
          :label="t('auth.email') + ' *'"
          type="email"
          filled
          :rules="[v => !!v || t('common.required'), v => /.+@.+\..+/.test(v) || t('auth.invalidEmail')]"
          :disable="!!editingUser"
        />

        <q-input
          v-if="!editingUser"
          v-model="form.password"
          :label="t('auth.password') + ' *'"
          :type="showPassword ? 'text' : 'password'"
          filled
          :rules="[v => !!v || t('common.required'), v => v.length >= 6 || t('users.passwordMinLength')]"
        >
          <template v-slot:append>
            <q-icon
              :name="showPassword ? 'visibility' : 'visibility_off'"
              class="cursor-pointer"
              :aria-label="showPassword ? t('auth.hidePassword') : t('auth.showPassword')"
              @click="showPassword = !showPassword"
            />
          </template>
        </q-input>

        <q-input
          v-model="form.phone"
          :label="t('auth.phone')"
          type="tel"
          filled
        />

        <q-input
          v-model="form.streetAddress"
          :label="t('users.address')"
          filled
        />

        <q-select
          v-if="authStore.isAdmin"
          v-model="form.role"
          :options="roleOptions"
          :label="t('users.role') + ' *'"
          filled
          emit-value
          map-options
        />

        <div class="row justify-end q-mt-md">
          <q-btn 
            flat 
            :label="t('common.cancel')" 
            @click="$emit('cancel')" 
            class="q-mr-sm"
            :disable="loading"
          />
          <q-btn 
            color="primary" 
            :label="t('common.save')" 
            type="submit" 
            :loading="loading"
            no-caps
          />
        </div>
      </q-form>
    </q-card-section>
  </q-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { User } from '@/types/models'
import { UserRole, UserRoleLabel } from '@/utils/appEnums'
import { useAuthStore } from '@/stores/auth'

const props = defineProps<{
  user?: User | null
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'submit', formData: any): void // Use `any` for now, define specific DTO later
  (e: 'cancel'): void
}>()

const { t } = useI18n()
const authStore = useAuthStore()

const editingUser = ref<User | null>(null)
const showPassword = ref(false)

const form = ref({
  email: '',
  fullName: '',
  phone: '',
  password: '',
  role: UserRole.Neighbor,
  streetAddress: ''
})

const roleOptions = computed(() => [
  { label: UserRoleLabel(UserRole.Admin), value: UserRole.Admin },
  { label: UserRoleLabel(UserRole.Manager), value: UserRole.Manager },
  { label: UserRoleLabel(UserRole.Guard), value: UserRole.Guard },
  { label: UserRoleLabel(UserRole.Neighbor), value: UserRole.Neighbor },
  { label: UserRoleLabel(UserRole.RestrictedAccess), value: UserRole.RestrictedAccess }
])

watch(() => props.user, (newVal) => {
  if (newVal) {
    editingUser.value = newVal
    form.value = {
      email: newVal.email,
      fullName: newVal.fullName,
      phone: newVal.phone || '',
      password: '', // Password is not edited via this form
      role: newVal.role,
      streetAddress: newVal.streetAddress || ''
    }
  } else {
    editingUser.value = null
    form.value = {
      email: '',
      fullName: '',
      phone: '',
      password: '',
      role: UserRole.Neighbor,
      streetAddress: ''
    }
  }
}, { immediate: true })

const onSubmit = () => {
  emit('submit', form.value)
}
</script>