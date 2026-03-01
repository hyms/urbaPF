<template>
  <q-item clickable v-ripple @click="$emit('click', post)">
    <q-item-section avatar>
      <q-avatar :color="getCategoryColor(post.category)" text-color="white">
        <q-icon :name="getCategoryIcon(post.category)" />
      </q-avatar>
    </q-item-section>
    <q-item-section>
      <q-item-label>
        <q-chip v-if="post.isPinned" color="orange" text-color="white" size="sm" class="q-mr-sm">
          {{ t('posts.pinned') }}
        </q-chip>
        <q-chip v-if="post.isAnnouncement" color="green" text-color="white" size="sm" class="q-mr-sm">
          {{ t('posts.announcement') }}
        </q-chip>
        {{ post.title }}
      </q-item-label>
      <q-item-label caption lines="1">{{ post.content }}</q-item-label>
      <q-item-label caption>
        {{ post.authorName }} | {{ formatDate(post.createdAt) }}
        <span v-if="post.viewCount">| {{ post.viewCount }} {{ t('posts.views') }}</span>
      </q-item-label>
    </q-item-section>
    <q-item-section side>
      <q-chip :color="getCategoryColor(post.category)" text-color="white" size="sm">
        {{ getCategoryLabel(post.category) }}
      </q-chip>
    </q-item-section>
  </q-item>
</template>

<script setup>
import { formatDate, getCategoryLabel, getCategoryColor, getCategoryIcon } from '../utils/format'
import { useI18n } from '../composables/useI18n'

const props = defineProps({
  post: {
    type: Object,
    required: true
  }
})

const { t } = useI18n()
</script>
