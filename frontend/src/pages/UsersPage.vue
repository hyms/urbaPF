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

    <!-- Diálogo para cambiar contraseña -->
    <q-dialog v-model="showChangePasswordDialog" persistent>
      <q-card style="min-width: 400px">
        <q-card-section>
          <div class="text-h6">{{ t('users.changePassword') }}</div>
        </q-card-section>

        <q-card-section>
          <q-form class="q-gutter-md" @submit="changePasswordUser">
            <!-- Contraseña antigua (solo para Neighbor editando su propio perfil) -->
            <q-input
              v-if="authStore.isNeighbor && editingUser?.id === authStore.currentUser?.id"
              v-model="passwordForm.oldPassword"
              :label="t('users.oldPassword')"
              type="password"
              outlined
              :rules="[v => !!v || t('common.required')]"
            />

            <!-- Nueva contraseña -->
            <q-input
              v-model="passwordForm.newPassword"
              :label="t('users.newPassword')"
              type="password"
              outlined
              :rules="[
                v => !!v || t('common.required'),
                v => v.length >= 6 || t('users.passwordMinLength')
              ]"
            />

            <!-- Confirmar nueva contraseña -->
            <q-input
              v-model="passwordForm.confirmPassword"
              :label="t('users.confirmNewPassword')"
              type="password"
              outlined
              :rules="[
                v => !!v || t('common.required'),
                v => v === passwordForm.newPassword || t('users.passwordsMatch')
              ]"
            />

            <q-card-actions align="right">
              <q-btn flat :label="t('common.cancel')" @click="showChangePasswordDialog = false" />
              <q-btn color="primary" :label="t('common.save')" type="submit" :loading="loading" />
            </q-card-actions>
          </q-form>
        </q-card-section>
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
    address: user.streetAddress || '', // Mapear a address
    password: '', // No precargar la contraseña
    photo: null, // Resetear archivo de foto
    photoUrl: user.photoUrl || '' // Cargar URL de foto existente
  }
  showCreateDialog.value = true
}

async function saveUser() {
  loading.value = true
  try {
    let finalPhotoUrl = userForm.value.photoUrl; // Mantener la URL existente por defecto

    // 1. Si hay una nueva foto, subirla primero
    if (userForm.value.photo) {
      const uploadResult = await userStore.uploadPhoto(editingUser.value.id, userForm.value.photo);
      if (!uploadResult.success) {
        $q.notify({ type: 'negative', message: uploadResult.message });
        return; // Detener si la subida falla
      }
      finalPhotoUrl = uploadResult.photoUrl; // Usar la URL de la foto recién subida
    }

    // 2. Preparar los datos para la actualización del usuario
    const updateData = {
      fullName: userForm.value.fullName,
      phone: userForm.value.phone,
      address: userForm.value.address, // Esto se mapea a streetAddress en el store
      photoUrl: finalPhotoUrl // Usar la URL final de la foto
    };

    // Para Admin, también permite actualizar el rol
    if (authStore.isAdmin) {
      updateData.role = userForm.value.role;
      updateData.email = userForm.value.email; // Admin puede editar el email
    }

    // 3. Realizar la actualización del usuario
    if (editingUser.value) {
      const updateResult = await userStore.update(editingUser.value.id, updateData);
      if (!updateResult) {
        $q.notify({ type: 'negative', message: userStore.error || t('common.error') });
        return;
      }
      $q.notify({ type: 'positive', message: t('common.success') });
    } else {
      // Lógica para crear un nuevo usuario (ya existente, solo asegurar que PhotoUrl y Address se pasan)
      const createData = {
        email: userForm.value.email,
        password: userForm.value.password,
        fullName: userForm.value.fullName,
        phone: userForm.value.phone,
        role: userForm.value.role,
        streetAddress: userForm.value.address, // Asegurarse de pasar la dirección
        photoUrl: finalPhotoUrl // Asegurarse de pasar la URL de la foto (si hay)
      };
      const createResult = await userStore.create(createData);
      if (!createResult) {
        $q.notify({ type: 'negative', message: userStore.error || t('common.error') });
        return;
      }
      $q.notify({ type: 'positive', message: t('common.success') });
    }

    showCreateDialog.value = false
    users.value = await userStore.fetchAll() // Refrescar la lista de usuarios
  } catch (e) {
    console.error('Error al guardar usuario:', e);
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    loading.value = false
    editingUser.value = null
    userForm.value.photo = null; // Limpiar el archivo de foto después de guardar
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

function removePhoto() {
  userForm.value.photo = null;
  userForm.value.photoUrl = '';
  // TODO: Si fuera necesario, se podría llamar a un endpoint para eliminar la foto del almacenamiento
}

function openChangePasswordDialog(userId) {
  editingUser.value = users.value.find(u => u.id === userId); // Asegurarse de tener el usuario correcto
  passwordForm.value = {
    oldPassword: '',
    newPassword: '',
    confirmPassword: ''
  };
  showChangePasswordDialog.value = true;
}

async function changePasswordUser() {
  loading.value = true;
  try {
    if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
      $q.notify({ type: 'negative', message: t('users.passwordsMatch') });
      return;
    }

    const userIdToUpdate = editingUser.value.id; // Puede ser el propio usuario o uno que el admin edita
    const isOwner = authStore.isNeighbor && userIdToUpdate === authStore.currentUser?.id;

    // Si no es admin y es el propio usuario, se requiere la contraseña antigua
    const oldPassword = isOwner ? passwordForm.value.oldPassword : '';
    const newPassword = passwordForm.value.newPassword;

    const result = await userStore.changePassword(userIdToUpdate, oldPassword, newPassword);

    if (result.success) {
      $q.notify({ type: 'positive', message: result.message });
      showChangePasswordDialog.value = false;
      passwordForm.value = { oldPassword: '', newPassword: '', confirmPassword: '' }; // Limpiar formulario
    } else {
      $q.notify({ type: 'negative', message: result.message });
    }
  } catch (e) {
    console.error('Error al cambiar contraseña:', e);
    $q.notify({ type: 'negative', message: t('common.error') });
  } finally {
    loading.value = false;
  }
}

function getRoleLabel(role) {
  return userStore.getRoleLabel(role)
}

function getRoleColor(role) {
  const colors = { 0: 'purple', 1: 'blue', 2: 'grey' }
  return colors[role] || 'grey'
}
</script>
