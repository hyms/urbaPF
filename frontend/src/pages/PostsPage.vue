<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">{{ t('posts.title') }}</div>
      <q-space />
      <q-btn color="primary" icon="add" :label="t('common.create')" @click="showCreateDialog = true" />
    </div>

    <q-card>
      <q-list separator>
        <PostItem v-for="post in posts" :key="post.id" :post="post" @click="viewPost" />
        <q-item v-if="posts.length === 0">
          <q-item-section class="text-grey text-center">
            {{ t('posts.noPosts') }}
          </q-item-section>
        </q-item>
      </q-list>
    </q-card>

    <q-dialog v-model="showCreateDialog" persistent>
      <q-card style="min-width: 500px">
        <q-card-section>
          <div class="text-h6">{{ t('posts.newPost') }}</div>
        </q-card-section>

        <q-card-section>
          <q-form class="q-gutter-md">
            <q-input v-model="newPost.title" label="Título" outlined :rules="[v => !!v || t('common.required')]" />
            <q-input v-model="newPost.content" :label="t('posts.content')" type="textarea" outlined rows="4" :rules="[v => !!v || t('common.required')]" />
            <q-select v-model="newPost.category" :options="categoryOptions" :label="t('posts.category')" outlined emit-value map-options />
            <q-toggle v-model="newPost.isPinned" :label="t('posts.pinned')" />
            <q-toggle v-model="newPost.isAnnouncement" :label="t('posts.announcement')" />
          </q-form>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat :label="t('common.cancel')" v-close-popup />
          <q-btn color="primary" :label="t('common.create')" @click="createPost" :loading="loading" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { usePostStore } from '../stores/post'
import { useCondominiumStore } from '../stores/condominium'
import { useI18n } from '../composables/useI18n'
import PostItem from '../components/PostItem.vue'

const $q = useQuasar()
const postStore = usePostStore()
const condoStore = useCondominiumStore()
const { t } = useI18n()

const posts = ref([])
const showCreateDialog = ref(false)
const loading = ref(false)

const newPost = ref({
  title: '',
  content: '',
  category: 1,
  isPinned: false,
  isAnnouncement: false
})

const categoryOptions = [
  { label: 'General', value: 1 },
  { label: 'Evento', value: 2 },
  { label: 'Anuncio', value: 3 },
  { label: 'Discusión', value: 4 }
]

onMounted(async () => {
  const condos = await condoStore.fetchAll()
  if (condos.length > 0) {
    posts.value = await postStore.fetchByCondominium(condos[0].id)
  }
})

async function createPost() {
  loading.value = true
  try {
    const condos = await condoStore.fetchAll()
    if (condos.length > 0) {
      await postStore.create(condos[0].id, newPost.value)
      $q.notify({ type: 'positive', message: t('posts.createSuccess') })
      showCreateDialog.value = false
      posts.value = await postStore.fetchByCondominium(condos[0].id)
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: t('posts.createError') })
  } finally {
    loading.value = false
  }
}

function viewPost(post) {
  $q.dialog({
    title: post.title,
    message: `${post.content}\n\n${t('posts.by')}: ${post.authorName}\n${t('posts.views')}: ${post.viewCount}`,
    ok: t('common.yes')
  })
}
</script>
