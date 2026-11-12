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
              :color="UserRoleColor(props.row.role)" 
              text-color="white" 
              size="sm"
              :label="UserRoleLabel(props.row.role)"
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
      <UserFormDialog
        :user="editingUser"
        :loading="userStore.loading"
        @submit="saveUser"
        @cancel="showDialog = false"
      />
    </q-dialog>

    <!-- Change Password Dialog -->
    <q-dialog v-model="showPasswordDialog" persistent>
      <ChangePasswordDialog
        :userId="editingUser!.id"
        :isAdmin="authStore.isAdmin"
        :isCurrentUser="editingUser!.id === authStore.user?.id"
        :loading="userStore.loading"
        @submit="changePassword"
        @cancel="showPasswordDialog = false"
      />
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useUserStore } from '../stores/user'
import { useAuthStore } from '../stores/auth'
import { useI18n } from '../composables/useI18n'
import { getInitials } from '@/utils/format'
import { getCredibilityColor, UserRole, UserRoleLabel, UserRoleColor } from '@/utils/appEnums'
import UserFormDialog from '../components/user/UserFormDialog.vue'
import ChangePasswordDialog from '../components/user/ChangePasswordDialog.vue'

const $q = useQuasar()
const userStore = useUserStore()
const authStore = useAuthStore()
const { t } = useI18n()

const users = computed(() => userStore.users)
const filter = ref('')
const showDialog = ref(false)
const showPasswordDialog = ref(false)
const editingUser = ref<typeof userStore.users[0] | null>(null)

const columns = computed(() => [
  { name: 'avatar', label: '', field: 'photoUrl', align: 'center' as const },
  { name: 'fullName', label: t('auth.fullName'), field: 'fullName', sortable: true, align: 'left' as const },
  { name: 'phone', label: t('auth.phone'), field: 'phone', align: 'left' as const },
  { name: 'role', label: t('users.role'), field: 'role', sortable: true, align: 'center' as const },
  { name: 'credibilityLevel', label: t('users.credibilityLevel'), field: 'credibilityLevel', sortable: true, align: 'center' as const },
  { name: 'status', label: t('users.status'), field: 'status', sortable: true, align: 'center' as const },
  { name: 'actions', label: t('users.actions'), field: 'id', align: 'center' as const }
])

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
  showDialog.value = true
}

function editUser(user: typeof userStore.users[0]) {
  editingUser.value = user
  showDialog.value = true
}

function openChangePasswordDialog() {
  showPasswordDialog.value = true
}

async function saveUser(formData: any) {
  if (!editingUser.value) {
    const result = await userStore.create({
      email: formData.email,
      password: formData.password,
      fullName: formData.fullName,
      phone: formData.phone || undefined,
      role: formData.role,
      streetAddress: formData.streetAddress || undefined
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
      fullName: formData.fullName,
      phone: formData.phone || undefined,
      streetAddress: formData.streetAddress || undefined,
      role: authStore.isAdmin ? formData.role : undefined
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

async function changePassword(formData: { oldPassword?: string; newPassword: string; confirmPassword: string }) {
  const success = await userStore.changePassword(
    editingUser.value!.id,
    { oldPassword: formData.oldPassword || '', newPassword: formData.newPassword }
  )

  if (success) {
    $q.notify({ type: 'positive', message: t('settings.passwordChangeSuccess') })
    showPasswordDialog.value = false
  } else {
    $q.notify({ type: 'negative', message: userStore.error || t('settings.passwordChangeError') })
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