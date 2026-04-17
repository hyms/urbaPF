<template>
  <q-page class="q-pa-md">
    <div class="text-h4 q-mb-md">{{ t('directory.title') }}</div>

    <div v-if="!condoStore.currentCondominium" class="text-center q-pa-xl">
      <q-icon name="group" size="64px" color="grey-4" />
      <div class="text-h6 text-grey-6 q-mt-md">{{ t('directory.noCondominium') }}</div>
    </div>

    <q-card v-else flat bordered>
      <q-table
        :rows="userStore.neighbors"
        :columns="columns"
        row-key="id"
        :loading="userStore.loading"
        :pagination="{ rowsPerPage: 15 }"
        flat
      >
        <template v-slot:body-cell-avatar="props">
          <q-td :props="props">
              <q-avatar size="40px" color="primary" text-color="white">
                <q-img v-if="props.row.photoUrl" :src="props.row.photoUrl" loading="lazy" />
                <span v-else>{{ getInitials(props.row.fullName) }}</span>
              </q-avatar>
          </q-td>
        </template>

        <template v-slot:body-cell-fullName="props">
          <q-td :props="props">
            <div class="text-weight-medium">{{ props.row.fullName }}</div>
            <div v-if="props.row.lotNumber" class="text-caption text-grey-6">
              {{ t('directory.mza') }}/{{ t('directory.lot') }}: {{ props.row.lotNumber }}
            </div>
          </q-td>
        </template>

        <template v-slot:body-cell-address="props">
          <q-td :props="props">
            {{ props.row.streetAddress || '-' }}
          </q-td>
        </template>

        <template v-slot:body-cell-phone="props">
          <q-td :props="props">
            <span v-if="authStore.isManager || authStore.isAdmin">
              {{ props.row.phone || '-' }}
            </span>
            <span v-else class="text-grey-5">{{ t('directory.hidden') }}</span>
          </q-td>
        </template>

        <template v-slot:body-cell-actions="props">
          <q-td :props="props">
            <q-btn 
              flat 
              round 
              dense 
              icon="visibility" 
              @click="viewDetails(props.row)"
            >
              <q-tooltip>{{ t('common.view') }}</q-tooltip>
            </q-btn>
          </q-td>
        </template>

        <template v-slot:no-data>
          <div class="full-width column items-center q-pa-lg">
            <q-icon name="group" size="64px" color="grey-4" />
            <div class="text-h6 text-grey-6 q-mt-md">{{ t('directory.noNeighbors') }}</div>
          </div>
        </template>
      </q-table>
    </q-card>

    <q-dialog v-model="showDetailsDialog">
      <UserDetailsDialog 
        :user="selectedUser" 
        @close="showDetailsDialog = false"
      />
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useUserStore } from '../stores/user'
import { useAuthStore } from '../stores/auth'
import { useCondominiumStore } from '../stores/condominium'
import { useI18n } from '../composables/useI18n'
import { getInitials } from '@/utils/format'
import { User } from '@/types/models'
import UserDetailsDialog from '../components/user/UserDetailsDialog.vue'

const userStore = useUserStore()
const authStore = useAuthStore()
const condoStore = useCondominiumStore()
const { t } = useI18n()

const showDetailsDialog = ref(false)
const selectedUser = ref<User | null>(null)

const columns = computed(() => [
  { name: 'avatar', label: '', field: 'photoUrl', align: 'center' as const },
  { name: 'fullName', label: t('auth.fullName'), field: 'fullName', sortable: true, align: 'left' as const },
  { name: 'address', label: t('auth.address'), field: 'streetAddress', align: 'left' as const },
  { name: 'phone', label: t('auth.phone'), field: 'phone', align: 'left' as const },
  { name: 'actions', label: t('users.actions'), field: 'id', align: 'center' as const }
])

async function viewDetails(user: User) {
  selectedUser.value = user
  if (authStore.isManager || authStore.isAdmin) {
    await userStore.fetchUserDetails(user.id)
    selectedUser.value = userStore.neighborDetails
  }
  showDetailsDialog.value = true
}

onMounted(async () => {
  if (condoStore.currentCondominium) {
    await userStore.fetchNeighbors(condoStore.currentCondominium.id)
  }
})
</script>