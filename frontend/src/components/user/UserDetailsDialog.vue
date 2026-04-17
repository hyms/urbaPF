<template>
  <q-card style="min-width: 350px">
    <q-card-section class="row items-center q-pb-none">
      <div class="text-h6">{{ t('directory.userDetails') }}</div>
      <q-space />
      <q-btn icon="close" flat round dense v-close-popup />
    </q-card-section>

    <q-card-section>
      <div class="column q-gutter-md">
        <div class="row justify-center q-mb-md">
          <q-avatar size="100px" color="primary" text-color="white">
            <q-img v-if="user?.photoUrl" :src="user.photoUrl" />
            <span v-else class="text-h5">{{ getInitials(user?.fullName || '') }}</span>
          </q-avatar>
        </div>

        <div class="text-center text-weight-bold text-h6">{{ user?.fullName }}</div>
        <div class="text-center text-caption text-grey-6">{{ user?.email }}</div>

        <q-separator class="q-my-sm" />

        <div class="row q-col-gutter-sm">
          <div class="col-6">
            <div class="text-caption text-grey">{{ t('auth.phone') }}</div>
            <div class="text-body1">{{ user?.phone || '-' }}</div>
          </div>
          <div class="col-6">
            <div class="text-caption text-grey">{{ t('directory.mza') }}/{{ t('directory.lot') }}</div>
            <div class="text-body1">{{ user?.lotNumber || '-' }}</div>
          </div>
        </div>

        <div>
          <div class="text-caption text-grey">{{ t('auth.address') }}</div>
          <div class="text-body1">{{ user?.streetAddress || '-' }}</div>
        </div>

        <div v-if="showManagerDetails">
          <q-separator class="q-my-sm" />
          <div class="text-subtitle2 text-primary q-mb-sm">{{ t('directory.managerDetails') }}</div>
          
          <div class="row q-col-gutter-sm">
            <div class="col-6">
              <div class="text-caption text-grey">{{ t('users.credibilityLevel') }}</div>
              <q-badge :color="getCredibilityColor(user?.credibilityLevel || 1)">
                {{ user?.credibilityLevel }}/5
              </q-badge>
            </div>
            <div class="col-6">
              <div class="text-caption text-grey">{{ t('users.role') }}</div>
              <q-chip :color="UserRoleColor(user?.role || 0)" text-color="white" size="sm">
                {{ UserRoleLabel(user?.role || 0) }}
              </q-chip>
            </div>
          </div>

          <div class="row q-col-gutter-sm q-mt-sm">
            <div class="col-6">
              <div class="text-caption text-grey">{{ t('users.status') }}</div>
              <div class="text-body1">{{ user?.status === 1 ? t('users.active') : t('users.inactive') }}</div>
            </div>
            <div class="col-6">
              <div class="text-caption text-grey">{{ t('users.votes') }}</div>
              <div class="text-body1">{{ user?.managerVotes || 0 }}</div>
            </div>
          </div>

          <div class="row q-col-gutter-sm q-mt-sm">
            <div class="col-6">
              <div class="text-caption text-grey">{{ t('users.createdAt') }}</div>
              <div class="text-body2">{{ formatDate(user?.createdAt) }}</div>
            </div>
            <div class="col-6">
              <div class="text-caption text-grey">{{ t('users.lastLogin') }}</div>
              <div class="text-body2">{{ formatDate(user?.lastLoginAt) }}</div>
            </div>
          </div>
        </div>
      </div>
    </q-card-section>

    <q-card-actions align="right">
      <q-btn flat :label="t('common.close')" v-close-popup />
    </q-card-actions>
  </q-card>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from '@/composables/useI18n'
import { useAuthStore } from '@/stores/auth'
import { getInitials, formatDate } from '@/utils/format'
import { getCredibilityColor, UserRoleLabel, UserRoleColor } from '@/utils/appEnums'
import { User } from '@/types/models'

interface Props {
  user: User | null
}

const props = defineProps<Props>()
const emit = defineEmits(['close'])

const authStore = useAuthStore()
const { t } = useI18n()

const showManagerDetails = computed(() => authStore.isManager || authStore.isAdmin)
</script>