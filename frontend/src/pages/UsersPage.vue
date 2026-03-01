<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">{{ t('users.title') }}</div>
      <q-space />
      <q-btn color="primary" icon="add" :label="t('users.newUser')" @click="showCreateDialog = true" v-if="authStore.isAdmin" />
    </div>

    <q-card>
      <q-table
        :rows="users"
        :columns="columns"
        row-key="id"
        :loading="loading"
        :filter="filter"
      >
        <template v-slot:top-right>
          <q-input borderless dense debounce="300" v-model="filter" :placeholder="t('common.search')">
            <template v-slot:append>
              <q-icon name="search" />
            </template>
          </q-input>
        </template>

        <template v-slot:body-cell-role="props">
          <q-td :props="props">
            <q-chip :color="getRoleColor(props.row.role)" text-color="white" size="sm">
              {{ getRoleLabel(props.row.role) }}
            </q-chip>
          </q-td>
        </template>

        <template v-slot:body-cell-status="props">
          <q-td :props="props">
            <q-chip :color="props.row.status === 1 ? 'green' : 'red'" text-color="white" size="sm">
              {{ props.row.status === 1 ? t('users.active') : t('users.inactive') }}
            </q-chip>
          </q-td>
        </template>

        <template v-slot:body-cell-actions="props">
          <q-td :props="props">
            <q-btn flat round dense icon="edit" @click="editUser(props.row)" v-if="authStore.isAdmin || authStore.isManager || (authStore.isNeighbor && authStore.currentUser.id === props.row.id)" />
            <q-btn flat round dense icon="delete" color="negative" @click="deleteUser(props.row)" v-if="authStore.isAdmin" />
          </q-td>
        </template>
      </q-table>
    </q-card>

    <q-dialog v-model="showCreateDialog" persistent>
      <q-card style="min-width: 400px">
        <q-card-section>
          <div class="text-h6">{{ editingUser ? t('users.editUser') : t('users.newUser') }}</div>
        </q-card-section>

        <q-card-section>
  <q-form class="q-gutter-md">
    <!-- Email - solo visible y deshabilitado si se está editando -->
    <q-input
      v-model="userForm.email"
      :label="t('auth.email')"
      type="email"
      outlined
      :rules="[v => !!v || t('common.required')]"
      :disable="!!editingUser || !authStore.isAdmin"
      v-if="editingUser || authStore.isAdmin"
    />

    <!-- Full Name -->
    <q-input
      v-model="userForm.fullName"
      :label="t('auth.fullName')"
      outlined
      :rules="[v => !!v || t('common.required')]"
      :disable="!authStore.isAdmin && !authStore.isManager && !(authStore.isNeighbor && editingUser?.id === authStore.currentUser?.id)"
    />

    <!-- Phone -->
    <q-input
      v-model="userForm.phone"
      :label="t('auth.phone')"
      outlined
      :disable="!authStore.isAdmin && !authStore.isManager && !(authStore.isNeighbor && editingUser?.id === authStore.currentUser?.id)"
    />

    <!-- Address (StreetAddress) + Google Maps -->
    <q-input
      v-model="userForm.address"
      :label="t('users.address')"
      outlined
      v-if="authStore.isAdmin || (authStore.isNeighbor && editingUser?.id === authStore.currentUser?.id)"
    />
    <div v-if="mapUrl" style="width: 100%; height: 200px; margin-top: 10px;">
      <iframe
        width="100%"
        height="100%"
        style="border:0"
        :src="mapUrl"
        allowfullscreen
        loading="lazy"
        referrerpolicy="no-referrer-when-downgrade"
      ></iframe>
    </div>

    <!-- Photo Upload and Preview -->
    <q-card v-if="authStore.isAdmin || (authStore.isNeighbor && editingUser?.id === authStore.currentUser?.id)">
      <q-card-section>
        <div class="text-subtitle1">{{ t('users.photo') }}</div>
        <div class="row items-center justify-center q-gutter-md q-mt-sm">
          <q-avatar size="100px" font-size="52px" color="primary" text-color="white" icon="person" v-if="!userForm.photoUrl && !userForm.photo" />
          <q-img :src="userForm.photoUrl || (userForm.photo ? URL.createObjectURL(userForm.photo) : null)"
                 v-if="userForm.photoUrl || userForm.photo"
                 style="height: 100px; max-width: 100px; border-radius: 50%;">
            <template v-slot:loading>
              <q-spinner-oval color="white" />
            </template>
          </q-img>
        </div>
        <q-file v-model="userForm.photo" :label="t('users.uploadPhoto')" outlined accept="image/*" class="q-mt-md" />
        <q-btn v-if="userForm.photoUrl" flat dense icon="delete" color="negative" @click="removePhoto" :label="t('users.removePhoto')" />
      </q-card-section>
    </q-card>


    <!-- Password - Botón para cambiar contraseña (para Neighbor editando su perfil) -->
    <q-btn
      v-if="!editingUser && authStore.isAdmin"
      @click="showCreateDialog = true"
      color="primary"
      :label="t('users.changePassword')"
      class="q-mt-md"
    />
    <q-btn
      v-if="editingUser && (authStore.isAdmin || (authStore.isNeighbor && editingUser.id === authStore.currentUser?.id))"
      @click="openChangePasswordDialog(editingUser.id)"
      color="primary"
      :label="t('users.changePassword')"
      class="q-mt-md"
    />


    <!-- Role -->
    <q-select
      v-model="userForm.role"
      :options="roleOptions"
      :label="t('users.role')"
      outlined
      emit-value
      map-options
      :disable="!authStore.isAdmin"
      v-if="authStore.isAdmin"
    />
  </q-form>
</q-card-section>

        <q-card-actions align="right">
          <q-btn flat :label="t('common.cancel')" v-close-popup />
          <q-btn color="primary" :label="t('common.save')" @click="saveUser" :loading="loading" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { useQuasar } from 'quasar'
import { useUserStore } from '../stores/user'
import { useAuthStore } from '../stores/auth'
import { useI18n } from '../composables/useI18n'

// Role mapping for display and backend consistency
const roleMapping = {
  4: 'Administrator',
  3: 'Manager',
  1: 'Guard',
  2: 'Neighbor'
}

const roleReverseMapping = {
  'Administrator': 4,
  'Manager': 3,
  'Guard': 1,
  'Neighbor': 2
}

const $q = useQuasar()
const userStore = useUserStore()
const authStore = useAuthStore()
const { t } = useI18n()

const googleMapsApiKey = import.meta.env.VITE_GOOGLE_MAPS_API_KEY; // Leer del .env
const mapUrl = ref(''); // Para la URL del iframe de Google Maps


const users = ref([])
const loading = ref(false)
const filter = ref('')
const showCreateDialog = ref(false)
const editingUser = ref(null)

const showChangePasswordDialog = ref(false); // Nuevo: Diálogo de cambio de contraseña
const passwordForm = ref({ // Nuevo: Formulario para cambio de contraseña
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
});

const userForm = ref({
  email: '',
  fullName: '',
  phone: '',
  role: 2, // Default to Neighbor
  address: '', // Nuevo campo para la dirección
  password: '', // Solo para creación o restablecimiento por admin
  photo: null, // Objeto File para la carga
  photoUrl: '' // URL de la foto existente
})

const roleOptions = [
  { label: t('roles.administrator'), value: 4 },
  { label: t('roles.manager'), value: 3 },
  { label: t('roles.guard'), value: 1 },
  { label: t('roles.neighbor'), value: 2 }
]

const columns = computed(() => [
  { name: 'fullName', label: t('users.name'), field: 'fullName', sortable: true, align: 'left' },
  { name: 'email', label: t('auth.email'), field: 'email', sortable: true, align: 'left' },
  { name: 'phone', label: t('auth.phone'), field: 'phone', align: 'left' },
  { name: 'address', label: t('users.address'), field: 'streetAddress', align: 'left' }, // Actualizado
  { name: 'role', label: t('users.role'), field: 'role', align: 'center' },
  { name: 'status', label: t('users.status'), field: 'status', align: 'center' },
  { name: 'actions', label: t('users.actions'), align: 'center' }
])

watch(() => userForm.value.address, (newAddress) => {
  if (newAddress && googleMapsApiKey) {
    mapUrl.value = `https://www.google.com/maps/embed/v1/place?key=${googleMapsApiKey}&q=${encodeURIComponent(newAddress)}`;
  } else {
    mapUrl.value = '';
  }
});

onMounted(async () => {
  users.value = await userStore.fetchAll()
})

function editUser(user) {
  editingUser.value = user
  userForm.value = {
    email: user.email,
    fullName: user.fullName,
    phone: user.phone || '',
    role: user.role,
    lotNumber: user.lotNumber || '',
    password: ''
  }
  showCreateDialog.value = true
}

async function saveUser() {
  loading.value = true
  try {
    if (editingUser.value) {
      await userStore.update(editingUser.value.id, userForm.value)
      $q.notify({ type: 'positive', message: t('common.success') })
    } else {
      await userStore.create(userForm.value)
      $q.notify({ type: 'positive', message: t('common.success') })
    }
    showCreateDialog.value = false
    users.value = await userStore.fetchAll()
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    loading.value = false
    editingUser.value = null
  }
}

async function deleteUser(user) {
  $q.dialog({
    title: t('common.confirmDelete'),
    message: t('common.deleteMessage').replace('{item}', user.fullName),
    cancel: true
  }).onOk(async () => {
    await userStore.remove(user.id)
    users.value = await userStore.fetchAll()
    $q.notify({ type: 'positive', message: t('common.success') })
  })
}

function getRoleLabel(role) {
  return userStore.getRoleLabel(role)
}

function getRoleColor(role) {
  const colors = { 0: 'purple', 1: 'blue', 2: 'grey' }
  return colors[role] || 'grey'
}
</script>
