<template>
  <q-card :style="$q.screen.lt.sm ? 'width: 100%; max-width: 100%;' : 'min-width: 500px; max-width: 90vw;'">
    <q-card-section class="row items-center q-pb-none">
      <div class="text-h6">{{ editingPost ? t('posts.editPost') : t('posts.newPost') }}</div>
      <q-space />
      <q-btn icon="close" flat round dense v-close-popup @click="$emit('cancel')" />
    </q-card-section>

    <q-card-section>
      <q-form @submit="onSubmit" class="q-gutter-md">
        <q-input
          v-model="form.title"
          :label="t('posts.title') + ' *'"
          outlined
          :rules="[v => !!v || t('common.required')]"
        />

        <q-input
          v-model="form.content"
          :label="t('posts.content') + ' *'"
          type="textarea"
          outlined
          rows="4"
          :rules="[v => !!v || t('common.required')]"
        />
        <div class="row q-col-gutter-md">
          <div class="col-6">
            <q-toggle
              v-model="form.isPinned"
              :label="t('posts.pinned')"
              color="orange"
            />
          </div>
          <div class="col-6">
            <q-toggle
              v-model="form.isAnnouncement"
              :label="t('posts.announcement')"
              color="green"
            />
          </div>
        </div>

        <div class="row justify-end q-mt-md">
          <q-btn
            flat
            :label="t('common.cancel')"
            @click="$emit('cancel')"
            class="q-mr-sm"
            :disable="loading"
          />
          <q-btn
            color="primary"
            :label="editingPost ? t('common.save') : t('common.create')"
            type="submit"
            :loading="loading"
            no-caps
          />
        </div>
      </q-form>
    </q-card-section>
  </q-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { Post } from '@/types/models'

const props = defineProps<{
  post?: Post | null
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'submit', form: { title: string; content: string; isPinned: boolean; isAnnouncement: boolean }): void
  (e: 'cancel'): void
}>()

const { t } = useI18n()

const editingPost = ref<Post | null>(null)

const form = ref({
  title: '',
  content: '',
  isPinned: false,
  isAnnouncement: false
})

watch(() => props.post, (newVal) => {
  if (newVal) {
    editingPost.value = newVal
    form.value = {
      title: newVal.title,
      content: newVal.content,
      isPinned: newVal.isPinned,
      isAnnouncement: newVal.isAnnouncement
    }
  } else {
    editingPost.value = null
    form.value = {
      title: '',
      content: '',
      isPinned: false,
      isAnnouncement: false
    }
  }
}, { immediate: true })

const onSubmit = () => {
  emit('submit', form.value)
}
</script>