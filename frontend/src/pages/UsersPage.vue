<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">{{ t('users.title') }}</div>
      <q-space />
      <q-btn 
        color="primary" 
        icon="add" 
        :label="t('users.newUser')" 
        @click="openCreateDialog()"
        :disable="!authStore.isAdmin"
      />
    </div>

    <q-card flat bordered>
      <q-table
        :rows="users"
        :columns="columns"
        row-key="id"
        :loading="userStore.loading"
        :filter="filter"
        :pagination="{ rowsPerPage: 10 }"
        flat
      >
        <template v-slot:top-right>
          <q-input 
            borderless 
            dense 
            debounce="300" 
            v-model="filter" 
            :placeholder="t('common.search')"
          >
            <template v-slot:append>
              <q-icon name="search" />
            </template>
          </q-input>
        </template>

        <template v-slot:body-cell-avatar="props">
          <q-td :props="props">
            <q-avatar size="36px" color="primary" text-color="white">
              <q-img 
                v-if="props.row.photoUrl" 
                :src="props.row.photoUrl" 
              />
              <span v-else>{{ getInitials(props.row.fullName) }}</span>
            </q-avatar>
          </q-td>
        </template>

        <template v-slot:body-cell-fullName="props">
          <q-td :props="props">
            <div class="text-weight-medium">{{ props.row.fullName }}</div>
            <div class="text-caption text-grey-6">{{ props.row.email }}</div>
          </q-td>
        </template>

        <template v-slot:body-cell-role="props">
          <q-td :props="props">
            <q-chip 
              :color="userStore.getRoleColor(props.row.role)" 
              text-color="white" 
              size="sm"
              :label="userStore.getRoleLabel(props.row.role)"
            />
          </q-td>
        </template>

        <template v-slot:body-cell-credibilityLevel="props">
          <q-td :props="props">
            <q-badge v-if="authStore.isManager" :color="getCredibilityColor(props.row.credibilityLevel)">
              {{ props.row.credibilityLevel }}/5
            </q-badge>
          </q-td>
        </template>

        <template v-slot:body-cell-status="props">
          <q-td :props="props">
            <q-chip 
              :color="props.row.status === 1 ? 'positive' : 'negative'" 
              text-color="white" 
              size="sm"
            >
              {{ props.row.status === 1 ? t('users.active') : t('users.inactive') }}
            </q-chip>
          </q-td>
        </template>

        <template v-slot:body-cell-actions="props">
          <q-td :props="props">
            <q-btn 
              flat 
              round 
              dense 
              icon="edit" 
              @click="editUser(props.row)"
              :disable="!canEdit(props.row)"
            >
              <q-tooltip>{{ t('common.edit') }}</q-tooltip>
            </q-btn>
            <q-btn 
              flat 
              round 
              dense 
              icon="delete" 
              color="negative"
              @click="confirmDelete(props.row)"
              :disable="!canDelete(props.row)"
            >
              <q-tooltip>{{ t('common.delete') }}</q-tooltip>
            </q-btn>
          </q-td>
        </template>

        <template v-slot:no-data>
          <div class="full-width column items-center q-pa-lg">
            <q-icon name="people" size="64px" color="grey-4" />
            <div class="text-h6 text-grey-6 q-mt-md">{{ t('users.noUsers') }}</div>
          </div>
        </template>
      </q-table>
    </q-card>

    <!-- Create/Edit Dialog -->
    <q-dialog v-model="showDialog" persistent>
      <q-card style="min-width: 450px; max-width: 90vw;">
        <q-card-section class="row items-center q-pb-none">
          <div class="text-h6">{{ editingUser ? t('users.editUser') : t('users.newUser') }}</div>
          <q-space />
          <q-btn icon="close" flat round dense v-close-popup />
        </q-card-section>

        <q-card-section>
          <q-form @submit="saveUser" class="q-gutter-md">
            <q-input
              v-model="userForm.fullName"
              :label="t('auth.fullName') + ' *'"
              outlined
              :rules="[v => !!v || t('common.required')]"
            />

            <q-input
              v-model="userForm.email"
              :label="t('auth.email') + ' *'"
              type="email"
              outlined
              :rules="[v => !!v || t('common.required'), v => /.+@.+\..+/.test(v) || t('auth.invalidEmail')]"
              :disable="!!editingUser"
            />

            <q-input
              v-if="!editingUser"
              v-model="userForm.password"
              :label="t('auth.password') + ' *'"
              :type="showPassword ? 'text' : 'password'"
              outlined
              :rules="[v => !!v || t('common.required'), v => v.length >= 6 || t('users.passwordMinLength')]"
            >
              <template v-slot:append>
                <q-icon
                  :name="showPassword ? 'visibility' : 'visibility_off'"
                  class="cursor-pointer"
                  @click="showPassword = !showPassword"
                />
              </template>
            </q-input>

            <q-input
              v-model="userForm.phone"
              :label="t('auth.phone')"
              outlined
            />

            <q-input
              v-model="userForm.streetAddress"
              :label="t('users.address')"
              outlined
            />

            <q-select
              v-if="authStore.isAdmin"
              v-model="userForm.role"
              :options="roleOptions"
              :label="t('users.role') + ' *'"
              outlined
              emit-value
              map-options
            />

            <q-card v-if="editingUser" flat bordered class="q-mt-md">
              <q-card-section>
                <div class="text-subtitle2 q-mb-sm">{{ t('users.changePassword') }}</div>
                <q-btn 
                  outline 
                  color="primary" 
                  :label="t('users.changePassword')" 
                  @click="openChangePasswordDialog"
                  no-caps
                />
              </q-card-section>
            </q-card>

            <div class="row justify-end q-mt-md">
              <q-btn 
                flat 
                :label="t('common.cancel')" 
                v-close-popup 
                class="q-mr-sm"
              />
              <q-btn 
                color="primary" 
                :label="t('common.save')" 
                type="submit" 
                :loading="userStore.loading"
                no-caps
              />
            </div>
          </q-form>
        </q-card-section>
      </q-card>
    </q-dialog>

    <!-- Change Password Dialog -->
    <q-dialog v-model="showPasswordDialog" persistent>
      <q-card style="min-width: 350px">
        <q-card-section>
          <div class="text-h6">{{ t('users.changePassword') }}</div>
        </q-card-section>

        <q-card-section>
          <q-form @submit="changePassword" class="q-gutter-md">
            <q-input
              v-if="editingUser?.id === authStore.user?.id"
              v-model="passwordForm.oldPassword"
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
              v-model="passwordForm.newPassword"
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
              v-model="passwordForm.confirmPassword"
              :label="t('users.confirmNewPassword') + ' *'"
              :type="showConfirmPassword ? 'text' : 'password'"
              outlined
              :rules="[
                v => !!v || t('common.required'),
                v => v === passwordForm.newPassword || t('users.passwordsMatch')
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
              <q-btn flat :label="t('common.cancel')" @click="showPasswordDialog = false" />
              <q-btn color="primary" :label="t('common.save')" type="submit" :loading="userStore.loading" no-caps />
            </q-card-actions>
          </q-form>
        </q-card-section>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useUserStore } from '../stores/user'
import { useAuthStore, UserRole } from '../stores/auth.ts'

const $q = useQuasar()
const userStore = useUserStore()
const authStore = useAuthStore()

const users = computed(() => userStore.users)
const filter = ref('')
const showDialog = ref(false)
const showPasswordDialog = ref(false)
const editingUser = ref<typeof userStore.users[0] | null>(null)

const showPassword = ref(false)
const showOldPassword = ref(false)
const showNewPassword = ref(false)
const showConfirmPassword = ref(false)

const userForm = ref({
  email: '',
  fullName: '',
  phone: '',
  password: '',
  role: UserRole.Neighbor,
  streetAddress: ''
})

const passwordForm = ref({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const roleOptions = [
  { label: 'Administrador', value: UserRole.Admin },
  { label: 'Encargado', value: UserRole.Manager },
  { label: 'Guardia', value: UserRole.Guard },
  { label: 'Vecino', value: UserRole.Neighbor },
  { label: 'Acceso Restringido', value: UserRole.RestrictedAccess }
]

const t = (key: string) => {
  const translations: Record<string, string> = {
    'users.title': 'Gestión de Usuarios',
    'users.newUser': 'Nuevo Usuario',
    'users.editUser': 'Editar Usuario',
    'users.address': 'Dirección',
    'users.role': 'Rol',
    'users.status': 'Estado',
    'users.actions': 'Acciones',
    'users.active': 'Activo',
    'users.inactive': 'Inactivo',
    'users.noUsers': 'No hay usuarios registrados',
    'users.changePassword': 'Cambiar Contraseña',
    'users.oldPassword': 'Contraseña Anterior',
    'users.newPassword': 'Nueva Contraseña',
    'users.confirmNewPassword': 'Confirmar Contraseña',
    'users.passwordMinLength': 'Mínimo 6 caracteres',
    'users.passwordsMatch': 'Las contraseñas no coinciden',
    'auth.email': 'Correo Electrónico',
    'auth.fullName': 'Nombre Completo',
    'auth.phone': 'Teléfono',
    'auth.password': 'Contraseña',
    'auth.invalidEmail': 'Correo inválido',
    'common.search': 'Buscar',
    'common.edit': 'Editar',
    'common.delete': 'Eliminar',
    'common.save': 'Guardar',
    'common.cancel': 'Cancelar',
    'common.required': 'Campo requerido',
    'common.error': 'Ha ocurrido un error',
    'common.success': 'Operación exitosa',
    'common.confirmDelete': 'Confirmar Eliminación',
    'common.deleteMessage': '¿Está seguro de eliminar a {item}?'
  }
  return translations[key] || key
}

const columns = computed(() => [
  { name: 'avatar', label: '', field: 'photoUrl', align: 'center' as const },
  { name: 'fullName', label: t('auth.fullName'), field: 'fullName', sortable: true, align: 'left' as const },
  { name: 'phone', label: t('auth.phone'), field: 'phone', align: 'left' as const },
  { name: 'role', label: t('users.role'), field: 'role', sortable: true, align: 'center' as const },
  { name: 'credibilityLevel', label: 'Credibilidad', field: 'credibilityLevel', sortable: true, align: 'center' as const },
  { name: 'status', label: t('users.status'), field: 'status', sortable: true, align: 'center' as const },
  { name: 'actions', label: t('users.actions'), field: 'id', align: 'center' as const }
])

function getInitials(name: string): string {
  return name
    .split(' ')
    .map(n => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2)
}

function getCredibilityColor(level: number): string {
  if (level >= 4) return 'positive'
  if (level >= 2) return 'warning'
  return 'negative'
}

function canEdit(user: typeof userStore.users[0]): boolean {
  if (authStore.isAdmin) return true
  if (authStore.isManager) return true
  if (authStore.user?.id === user.id) return true
  return false
}

function canDelete(user: typeof userStore.users[0]): boolean {
  if (!authStore.isAdmin) return false
  if (user.id === authStore.user?.id) return false
  return true
}

function openCreateDialog() {
  editingUser.value = null
  userForm.value = {
    email: '',
    fullName: '',
    phone: '',
    password: '',
    role: UserRole.Neighbor,
    streetAddress: ''
  }
  showDialog.value = true
}

function editUser(user: typeof userStore.users[0]) {
  editingUser.value = user
  userForm.value = {
    email: user.email,
    fullName: user.fullName,
    phone: user.phone || '',
    password: '',
    role: user.role,
    streetAddress: user.streetAddress || ''
  }
  showDialog.value = true
}

function openChangePasswordDialog() {
  passwordForm.value = {
    oldPassword: '',
    newPassword: '',
    confirmPassword: ''
  }
  showPasswordDialog.value = true
}

async function saveUser() {
  if (!editingUser.value) {
    const result = await userStore.create({
      email: userForm.value.email,
      password: userForm.value.password,
      fullName: userForm.value.fullName,
      phone: userForm.value.phone || undefined,
      role: userForm.value.role,
      streetAddress: userForm.value.streetAddress || undefined
    })

    if (result) {
      $q.notify({ type: 'positive', message: t('common.success') })
      showDialog.value = false
      await userStore.fetchAll()
    } else {
      $q.notify({ type: 'negative', message: userStore.error || t('common.error') })
    }
  } else {
    const result = await userStore.update(editingUser.value.id, {
      fullName: userForm.value.fullName,
      phone: userForm.value.phone || undefined,
      streetAddress: userForm.value.streetAddress || undefined,
      role: authStore.isAdmin ? userForm.value.role : undefined
    })

    if (result) {
      $q.notify({ type: 'positive', message: t('common.success') })
      showDialog.value = false
      await userStore.fetchAll()
    } else {
      $q.notify({ type: 'negative', message: userStore.error || t('common.error') })
    }
  }
}

async function changePassword() {
  const result = await userStore.changePassword(
    editingUser.value!.id,
    passwordForm.value.oldPassword,
    passwordForm.value.newPassword
  )

  if (result.success) {
    $q.notify({ type: 'positive', message: result.message })
    showPasswordDialog.value = false
  } else {
    $q.notify({ type: 'negative', message: result.message })
  }
}

function confirmDelete(user: typeof userStore.users[0]) {
  $q.dialog({
    title: t('common.confirmDelete'),
    message: t('common.deleteMessage').replace('{item}', user.fullName),
    cancel: true,
    persistent: true
  }).onOk(async () => {
    const result = await userStore.remove(user.id)
    if (result) {
      $q.notify({ type: 'positive', message: t('common.success') })
    } else {
      $q.notify({ type: 'negative', message: userStore.error || t('common.error') })
    }
  })
}

onMounted(async () => {
  await userStore.fetchAll()
})
</script>
