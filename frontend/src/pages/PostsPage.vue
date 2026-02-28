<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">Publicaciones</div>
      <q-space />
      <q-btn color="primary" icon="add" label="Nueva" @click="showCreateDialog = true" />
    </div>

    <q-card>
      <q-list separator>
        <q-item v-for="post in posts" :key="post.id" clickable v-ripple @click="viewPost(post)">
          <q-item-section avatar>
            <q-avatar :color="getCategoryColor(post.category)" text-color="white">
              <q-icon :name="getCategoryIcon(post.category)" />
            </q-avatar>
          </q-item-section>
          <q-item-section>
            <q-item-label>
              <q-chip v-if="post.isPinned" color="orange" text-color="white" size="sm" class="q-mr-sm">
                Fijado
              </q-chip>
              <q-chip v-if="post.isAnnouncement" color="green" text-color="white" size="sm" class="q-mr-sm">
                Anuncio
              </q-chip>
              {{ post.title }}
            </q-item-label>
            <q-item-label caption lines="1">{{ post.content }}</q-item-label>
            <q-item-label caption>
              {{ post.authorName }} | {{ formatDate(post.createdAt) }} | {{ post.viewCount }} vistas
            </q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-chip :color="getCategoryColor(post.category)" text-color="white" size="sm">
              {{ getCategoryLabel(post.category) }}
            </q-chip>
          </q-item-section>
        </q-item>
        <q-item v-if="posts.length === 0">
          <q-item-section class="text-grey text-center">
            No hay publicaciones
          </q-item-section>
        </q-item>
      </q-list>
    </q-card>

    <q-dialog v-model="showCreateDialog" persistent>
      <q-card style="min-width: 500px">
        <q-card-section>
          <div class="text-h6">Nueva Publicación</div>
        </q-card-section>

        <q-card-section>
          <q-form class="q-gutter-md">
            <q-input v-model="newPost.title" label="Título" outlined :rules="[v => !!v || 'Requerido']" />
            <q-input v-model="newPost.content" label="Contenido" type="textarea" outlined rows="4" :rules="[v => !!v || 'Requerido']" />
            <q-select v-model="newPost.category" :options="categoryOptions" label="Categoría" outlined emit-value map-options />
            <q-toggle v-model="newPost.isPinned" label="Fijar publicación" />
            <q-toggle v-model="newPost.isAnnouncement" label="Marcar como anuncio" />
          </q-form>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat label="Cancelar" v-close-popup />
          <q-btn color="primary" label="Publicar" @click="createPost" :loading="loading" />
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

const $q = useQuasar()
const postStore = usePostStore()
const condoStore = useCondominiumStore()

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
      $q.notify({ type: 'positive', message: 'Publicación creada' })
      showCreateDialog.value = false
      posts.value = await postStore.fetchByCondominium(condos[0].id)
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: 'Error al crear' })
  } finally {
    loading.value = false
  }
}

function viewPost(post) {
  $q.dialog({
    title: post.title,
    message: `${post.content}\n\nPor: ${post.authorName}\nVistas: ${post.viewCount}`,
    ok: 'Cerrar'
  })
}

function getCategoryLabel(category) {
  const labels = { 1: 'General', 2: 'Evento', 3: 'Anuncio', 4: 'Discusión' }
  return labels[category] || 'General'
}

function getCategoryColor(category) {
  const colors = { 1: 'grey', 2: 'blue', 3: 'green', 4: 'purple' }
  return colors[category] || 'grey'
}

function getCategoryIcon(category) {
  const icons = { 1: 'article', 2: 'event', 3: 'campaign', 4: 'forum' }
  return icons[category] || 'article'
}

function formatDate(date) {
  return new Date(date).toLocaleDateString('es-ES')
}
</script>
