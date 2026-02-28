<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">Usuarios</div>
      <q-space />
      <q-btn color="primary" icon="add" label="Nuevo Usuario" @click="showCreateDialog = true" />
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
          <q-input borderless dense debounce="300" v-model="filter" placeholder="Buscar">
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
              {{ props.row.status === 1 ? 'Activo' : 'Inactivo' }}
            </q-chip>
          </q-td>
        </template>

        <template v-slot:body-cell-actions="props">
          <q-td :props="props">
            <q-btn flat round dense icon="edit" @click="editUser(props.row)" />
            <q-btn flat round dense icon="delete" color="negative" @click="deleteUser(props.row)" />
          </q-td>
        </template>
      </q-table>
    </q-card>

    <q-dialog v-model="showCreateDialog" persistent>
      <q-card style="min-width: 400px">
        <q-card-section>
          <div class="text-h6">{{ editingUser ? 'Editar' : 'Nuevo' }} Usuario</div>
        </q-card-section>

        <q-card-section>
          <q-form class="q-gutter-md">
            <q-input v-model="userForm.email" label="Email" type="email" outlined :rules="[v => !!v || 'Requerido']" :disable="!!editingUser" />
            <q-input v-model="userForm.fullName" label="Nombre completo" outlined :rules="[v => !!v || 'Requerido']" />
            <q-input v-model="userForm.phone" label="Teléfono" outlined />
            <q-select v-model="userForm.role" :options="roleOptions" label="Rol" outlined emit-value map-points :disable="!authStore.isAdmin" />
            <q-input v-model="userForm.lotNumber" label="Número de lote" outlined />
            <q-input v-if="!editingUser" v-model="userForm.password" label="Contraseña" type="password" outlined :rules="[v => !editingUser || v.length >= 6 || 'Mínimo 6 caracteres']" />
          </q-form>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat label="Cancelar" v-close-popup />
          <q-btn color="primary" label="Guardar" @click="saveUser" :loading="loading" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useUserStore } from '../stores/user'
import { useAuthStore } from '../stores/auth'

const $q = useQuasar()
const userStore = useUserStore()
const authStore = useAuthStore()

const users = ref([])
const loading = ref(false)
const filter = ref('')
const showCreateDialog = ref(false)
const editingUser = ref(null)

const userForm = ref({
  email: '',
  fullName: '',
  phone: '',
  role: 2,
  lotNumber: '',
  password: ''
})

const roleOptions = [
  { label: 'Administrador', value: 0 },
  { label: 'Gerente', value: 1 },
  { label: 'Residente', value: 2 }
]

const columns = [
  { name: 'fullName', label: 'Nombre', field: 'fullName', sortable: true, align: 'left' },
  { name: 'email', label: 'Email', field: 'email', sortable: true, align: 'left' },
  { name: 'phone', label: 'Teléfono', field: 'phone', align: 'left' },
  { name: 'lotNumber', label: 'Lote', field: 'lotNumber', align: 'left' },
  { name: 'role', label: 'Rol', field: 'role', align: 'center' },
  { name: 'status', label: 'Estado', field: 'status', align: 'center' },
  { name: 'actions', label: 'Acciones', align: 'center' }
]

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
      $q.notify({ type: 'positive', message: 'Usuario actualizado' })
    } else {
      $q.notify({ type: 'warning', message: 'Registro no implementado en API' })
    }
    showCreateDialog.value = false
    users.value = await userStore.fetchAll()
  } catch (e) {
    $q.notify({ type: 'negative', message: 'Error al guardar' })
  } finally {
    loading.value = false
    editingUser.value = null
  }
}

async function deleteUser(user) {
  $q.dialog({
    title: 'Confirmar eliminación',
    message: `¿Eliminar usuario ${user.fullName}?`,
    cancel: true
  }).onOk(async () => {
    await userStore.remove(user.id)
    users.value = await userStore.fetchAll()
    $q.notify({ type: 'positive', message: 'Usuario eliminado' })
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
