<template>
  <q-card :style="$q.screen.lt.sm ? 'width: 100%; max-width: 100%;' : 'min-width: 450px; max-width: 90vw;'">
    <q-card-section class="row items-center q-pb-none">
      <div class="text-h6">{{ editingUser ? t('users.editUser') : t('users.newUser') }}</div>
      <q-space />
      <q-btn icon="close" flat round dense v-close-popup @click="$emit('cancel')" aria-label="Cerrar" />
    </q-card-section>

    <q-card-section class="q-pt-md">
      <q-form @submit="onSubmit" class="q-gutter-md">
        <q-input
          v-model="form.fullName"
          :label="t('auth.fullName') + ' *'"
          filled
          dense
          :rules="[v => !!v || t('common.required')]"
        />

        <q-input
          v-model="form.email"
          :label="t('auth.email') + ' *'"
          type="email"
          filled
          dense
          :rules="[v => !!v || t('common.required'), v => /.+@.+\..+/.test(v) || t('auth.invalidEmail')]"
          :disable="!!editingUser"
        />

        <q-input
          v-if="!editingUser"
          v-model="form.password"
          :label="t('auth.password') + ' *'"
          :type="showPassword ? 'text' : 'password'"
          filled
          dense
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
          dense
        />

        <q-input
          v-model="form.streetAddress"
          :label="t('users.address')"
          filled
          dense
        />

        <q-select
          v-if="authStore.isAdmin"
          v-model="form.role"
          :options="roleOptions"
          :label="t('users.role') + ' *'"
          filled
          dense
          emit-value
          map-options
        />

        <!-- Admin can change user password when editing -->
        <div v-if="editingUser && authStore.isAdmin" class="q-mt-md">
          <q-btn 
            outline 
            color="warning" 
            icon="lock_reset" 
            label="Cambiar Contraseña" 
            class="full-width"
            @click="showAdminChangePassword = true"
          />
        </div>

        <div class="row justify-end q-mt-lg">
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

    <!-- Admin Password Reset Dialog -->
    <q-dialog v-model="showAdminChangePassword" persistent>
      <q-card style="min-width: 300px">
        <q-card-section class="row items-center">
          <div class="text-h6">Resetear Contraseña</div>
          <q-space />
          <q-btn icon="close" flat round dense v-close-popup />
        </q-card-section>

        <q-card-section>
          <div class="text-body2 q-mb-md">Ingrese la nueva contraseña para {{ form.fullName }}</div>
          <q-input
            v-model="adminNewPassword"
            label="Nueva Contraseña"
            :type="showAdminPassword ? 'text' : 'password'"
            filled
            dense
            autofocus
            :rules="[v => !!v || 'Campo requerido', v => v.length >= 6 || 'Min 6 caracteres']"
          >
            <template v-slot:append>
              <q-icon
                :name="showAdminPassword ? 'visibility' : 'visibility_off'"
                class="cursor-pointer"
                @click="showAdminPassword = !showAdminPassword"
              />
            </template>
          </q-input>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat label="Cancelar" v-close-popup />
          <q-btn color="warning" label="Actualizar" @click="onAdminUpdatePassword" :loading="adminPasswordLoading" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { User } from '@/types/models'
import { UserRole, UserRoleLabel } from '@/utils/appEnums'
import { useAuthStore } from '@/stores/auth'
import { useUserStore } from '@/stores/user'
import { useQuasar } from 'quasar'

const props = defineProps<{
  user?: User | null
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'submit', formData: any): void 
  (e: 'cancel'): void
}>()

const { t } = useI18n()
const $q = useQuasar()
const authStore = useAuthStore()
const userStore = useUserStore()

const editingUser = ref<User | null>(null)
const showPassword = ref(false)

const showAdminChangePassword = ref(false)
const adminNewPassword = ref('')
const showAdminPassword = ref(false)
const adminPasswordLoading = ref(false)

const form = ref({
  email: '',
  fullName: '',
  phone: '',
  password: '',
  role: UserRole.Neighbor,
  streetAddress: ''
})

const roleOptions = computed(() => [
  { label: UserRoleLabel(UserRole.Administrator), value: UserRole.Administrator },
  { label: UserRoleLabel(UserRole.Manager), value: UserRole.Manager },
  { label: UserRoleLabel(UserRole.Guard), value: UserRole.Guard },
  { label: UserRoleLabel(UserRole.Neighbor), value: UserRole.Neighbor },
  { label: UserRoleLabel(UserRole.Restricted), value: UserRole.Restricted }
])

watch(() => props.user, (newVal) => {
  if (newVal) {
    editingUser.value = newVal
    form.value = {
      email: newVal.email,
      fullName: newVal.fullName,
      phone: newVal.phone || '',
      password: '', 
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

const onAdminUpdatePassword = async () => {
  if (!editingUser.value || adminNewPassword.value.length < 6) return
  
  adminPasswordLoading.value = true
  try {
    const success = await userStore.updatePassword(editingUser.value.id, adminNewPassword.value)
    if (success) {
      $q.notify({ type: 'positive', message: 'Contraseña actualizada correctamente' })
      showAdminChangePassword.value = false
      adminNewPassword.value = ''
    } else {
      $q.notify({ type: 'negative', message: 'Error al actualizar contraseña' })
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: 'Error inesperado' })
  } finally {
    adminPasswordLoading.value = false
  }
}
</script>