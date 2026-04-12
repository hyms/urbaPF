<template>
  <q-item clickable v-ripple @click="$emit('click', post)">
    <q-item-section>
      <q-item-label class="row items-center q-gutter-xs">
        <q-chip
          v-if="post.isPinned"
          color="orange"
          text-color="white"
          size="sm"
          icon="push_pin"
          class="q-mr-xs"
        >
          {{ t('posts.pinned') }}
        </q-chip>
        <q-chip
          v-if="post.isAnnouncement"
          color="green"
          text-color="white"
          size="sm"
          icon="campaign"
          class="q-mr-xs"
        >
          {{ t('posts.announcement') }}
        </q-chip>
        <q-chip
          v-if="post.status === 0"
          color="warning"
          text-color="white"
          size="sm"
        >
          {{ t('posts.pending') }}
        </q-chip>
        <q-chip
          v-if="post.status === 2"
          color="negative"
          text-color="white"
          size="sm"
        >
          {{ t('posts.rejected') }}
        </q-chip>
        <span class="text-weight-medium">{{ post.title }}</span>
      </q-item-label>
      <q-item-label caption lines="2">{{ post.content }}</q-item-label>
      <q-item-label caption>
        <div class="row items-center q-gutter-x-sm">
          <span>{{ post.authorName }}</span>
          <span>•</span>
          <span>{{ formatRelativeTime(post.created_at) }}</span>
          <span v-if="post.viewCount">•</span>
          <span v-if="post.viewCount">
            <q-icon name="visibility" size="14px" class="q-mr-xs" />
            {{ post.viewCount }}
          </span>
        </div>
      </q-item-label>
    </q-item-section>

    <q-item-section side>
      <div class="column items-end">
        <q-btn
          v-if="canModerate"
          flat
          dense
          round
          icon="more_vert"
          class="q-mt-xs"
          aria-label="Más acciones de publicación"
          @click.stop
        >
          <q-menu>
            <q-list style="min-width: 150px">
              <q-item
                v-if="post.status === 0"
                clickable
                v-close-popup
                @click.stop="$emit('approve', post)"
              >
                <q-item-section avatar>
                  <q-icon name="check_circle" color="positive" />
                </q-item-section>
                <q-item-section>{{ t('posts.approve') }}</q-item-section>
              </q-item>
                <q-item
                  v-if="post.status === 0"
                  clickable
                  v-close-popup
                  @click.stop="$emit('reject', post)"
                >
                  <q-item-section avatar>
                    <q-icon name="cancel" color="negative" />
                  </q-item-section>
                  <q-item-section>{{ t('posts.reject') }}</q-item-section>
                </q-item>
              <q-item
                v-if="canEdit"
                clickable
                v-close-popup
                @click.stop="$emit('edit', post)"
              >
                <q-item-section avatar>
                  <q-icon name="edit" />
                </q-item-section>
                <q-item-section>{{ t('common.edit') }}</q-item-section>
              </q-item>
              <q-item
                v-if="canDelete"
                clickable
                v-close-popup
                @click.stop="$emit('delete', post)"
              >
                <q-item-section avatar>
                  <q-icon name="delete" color="negative" />
                </q-item-section>
                <q-item-section>{{ t('common.delete') }}</q-item-section>
              </q-item>
            </q-list>
          </q-menu>
        </q-btn>
      </div>
    </q-item-section>
  </q-item>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { Post } from '@/types/models'
import { useI18n } from 'vue-i18n' 

const props = defineProps<{
  post: Post
}>()

defineEmits<{
  (e: 'click', post: Post): void
  (e: 'approve', post: Post): void
  (e: 'reject', post: Post): void
  (e: 'edit', post: Post): void
  (e: 'delete', post: Post): void
}>()

const authStore = useAuthStore()
const { t } = useI18n() 


const canModerate = computed(() => {
  return authStore.isAdmin || authStore.isManager
})

const canEdit = computed(() => {
  if (authStore.isAdmin || authStore.isManager) return true
  if (authStore.user?.id === props.post.authorId) return true
  return false
})

const canDelete = computed(() => {
  return authStore.isAdmin
})

function formatRelativeTime(dateString: string): string {
  const date = new Date(dateString)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMs / 3600000)
  const diffDays = Math.floor(diffMs / 86400000)

  if (diffMins < 1) return 'ahora'
  if (diffMins < 60) return `hace ${diffMins} min`
  if (diffHours < 24) return `hace ${diffHours} h`
  if (diffDays < 7) return `hace ${diffDays} días`

  return date.toLocaleDateString('es-BO', {
    day: 'numeric',
    month: 'short'
  })
}
</script>
