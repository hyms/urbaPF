<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">{{ t('posts.title') }}</div>
      <q-space />
      <q-btn 
        color="primary" 
        icon="add" 
        :label="t('posts.newPost')" 
        @click="openCreateDialog"
        no-caps
        :disable="!canCreatePost"
      >
        <q-tooltip v-if="!canCreatePost">{{ t('posts.validationRequired') }}</q-tooltip>
      </q-btn>
    </div>

    <!-- Filters -->
    <q-card flat bordered class="q-mb-md">
      <q-card-section class="q-py-sm">
        <div class="row q-col-gutter-md items-center">
          <div class="col-12 col-md-6">
            <q-input
              v-model="search"
              dense
              outlined
              :placeholder="t('common.search')"
              clearable
            >
              <template v-slot:prepend>
                <q-icon name="search" />
              </template>
            </q-input>
          </div>
          <div class="col-12 col-md-6">
            <q-select
              v-model="filterStatus"
              :options="statusOptions"
              dense
              outlined
              :label="t('posts.status')"
              emit-value
              map-options
              clearable
            />
          </div>
          <div class="col-12">
            <q-btn
              flat
              color="primary"
              :label="t('common.clearFilters')"
              @click="clearFilters"
              no-caps
              class="full-width"
            />
          </div>
        </div>
      </q-card-section>
    </q-card>

    <!-- Posts List -->
    <q-card flat bordered>
      <q-list separator>
        <template v-if="loading">
          <q-item v-for="n in 3" :key="n">
            <q-item-section avatar>
              <q-skeleton type="QAvatar" />
            </q-item-section>
            <q-item-section>
              <q-item-label>
                <q-skeleton type="text" width="60%" />
              </q-item-label>
              <q-item-label caption>
                <q-skeleton type="text" width="40%" />
              </q-item-label>
            </q-item-section>
          </q-item>
        </template>

        <template v-else-if="filteredPosts.length === 0">
          <div class="full-width column items-center q-pa-xl">
            <q-icon name="article" size="64px" color="grey-4" />
            <div class="text-h6 text-grey-6 q-mt-md">{{ t('posts.noPosts') }}</div>
            <q-btn
              v-if="canCreatePost"
              color="primary"
              :label="t('posts.createFirst')"
              @click="openCreateDialog"
              no-caps
              class="q-mt-md"
            />
          </div>
        </template>

        <PostItem
          v-else
          v-for="post in filteredPosts"
          :key="post.id"
          :post="post"
          @click="viewPost"
          @approve="approvePost"
          @reject="rejectPost"
          @edit="editPost"
          @delete="confirmDelete"
        />
      </q-list>
    </q-card>

    <!-- Create/Edit Dialog -->
    <q-dialog v-model="showDialog" persistent>
      <q-card style="min-width: 500px; max-width: 90vw;">
        <q-card-section class="row items-center q-pb-none">
          <div class="text-h6">{{ editingPost ? t('posts.editPost') : t('posts.newPost') }}</div>
          <q-space />
          <q-btn icon="close" flat round dense v-close-popup />
        </q-card-section>

        <q-card-section>
          <q-form @submit="savePost" class="q-gutter-md">
            <q-input
              v-model="postForm.title"
              :label="t('posts.title') + ' *'"
              outlined
              :rules="[v => !!v || t('common.required')]"
            />

            <q-input
              v-model="postForm.content"
              :label="t('posts.content') + ' *'"
              type="textarea"
              outlined
              rows="4"
              :rules="[v => !!v || t('common.required')]"
            />
            <div class="row q-col-gutter-md">
              <div class="col-6">
                <q-toggle
                  v-model="postForm.isPinned"
                  :label="t('posts.pinned')"
                  color="orange"
                />
              </div>
              <div class="col-6">
                <q-toggle
                  v-model="postForm.isAnnouncement"
                  :label="t('posts.announcement')"
                  color="green"
                />
              </div>
            </div>

            <div class="row justify-end q-mt-md">
              <q-btn
                flat
                :label="t('common.cancel')"
                v-close-popup
                @click="closeDialog"
                class="q-mr-sm"
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
    </q-dialog>

    <!-- View Post Dialog -->
    <q-dialog v-model="showViewDialog">
      <q-card style="min-width: 400px; max-width: 90vw;">
        <q-card-section class="row items-center q-pb-none">
          <div class="text-h6">{{ viewingPost?.title }}</div>
          <q-space />
          <q-btn icon="close" flat round dense v-close-popup />
        </q-card-section>

        <q-card-section>
          <div class="q-mb-md">
            <q-chip
              v-if="viewingPost?.isPinned"
              color="orange"
              text-color="white"
              size="sm"
              icon="push_pin"
            >
              {{ t('posts.pinned') }}
            </q-chip>
            <q-chip
              v-if="viewingPost?.isAnnouncement"
              color="green"
              text-color="white"
              size="sm"
              icon="campaign"
            >
              {{ t('posts.announcement') }}
            </q-chip>
            <q-chip
              :color="PostStatusColor(viewingPost?.status || 0)"
              text-color="white"
              size="sm"
            >
              {{ PostStatusLabel(viewingPost?.status || 0) }}
            </q-chip>
          </div>

          <div class="text-body1 q-mb-md" style="white-space: pre-wrap;">
            {{ viewingPost?.content }}
          </div>

          <div class="text-caption text-grey-6">
            <q-icon name="person" class="q-mr-xs" />
            {{ t('posts.by') }}: {{ viewingPost?.authorName }}
            <span class="q-mx-sm">|</span>
            <q-icon name="visibility" class="q-mr-xs" />
            {{ viewingPost?.viewCount }} {{ t('posts.views') }}
            <span class="q-mx-sm">|</span>
            <q-icon name="schedule" class="q-mr-xs" />
            {{ formatDateTime(viewingPost?.createdAt) }}
          </div>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat :label="t('common.close')" v-close-popup />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useI18n } from 'vue-i18n'
import { usePostStore } from '@/stores/post'
import { useCondominiumStore } from '@/stores/condominium'
import { useAuthStore } from '@/stores/auth'
import { Post, CreatePostRequest, UpdatePostRequest } from '@/types/models'


const $q = useQuasar()
const { t } = useI18n()
const postStore = usePostStore()
const condoStore = useCondominiumStore()
const authStore = useAuthStore()


const loading = computed(() => postStore.loading)
const posts = computed(() => postStore.posts)

const search = ref('')

const filterStatus = ref<number | null>(null)

const showDialog = ref(false)
const showViewDialog = ref(false)
const editingPost = ref<Post | null>(null)
const viewingPost = ref<Post | null>(null)

const postForm = ref({
  title: '',
  content: '',
  isPinned: false,
  isAnnouncement: false
})

const canCreatePost = computed(() => {
  return authStore.isAdmin || authStore.isManager || (authStore.isNeighbor && authStore.isValidated)
})

const filteredPosts = computed(() => {
  let result = posts.value

  if (search.value) {
    const searchLower = search.value.toLowerCase()
    result = result.filter(p =>
      p.title.toLowerCase().includes(searchLower) ||
      p.content.toLowerCase().includes(searchLower) ||
      p.authorName?.toLowerCase().includes(searchLower)
    )
  }

  if (filterStatus.value !== null) {
    result = result.filter(p => p.status === filterStatus.value)
  }

  return result
})

function clearFilters() {
  search.value = ''
  filterStatus.value = null
}

function formatDateTime(date: string | undefined): string {
  if (!date) return ''
  return new Date(date).toLocaleString('es-BO', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

async function loadPosts() {
  const condos = await condoStore.fetchAll()
  if (condos.length > 0) {
    await postStore.fetchByCondominium(condos[0].id)
  }
}

function openCreateDialog() {
  editingPost.value = null
  postForm.value = {
    title: '',
    content: '',
    isPinned: false,
    isAnnouncement: false
  }
  showDialog.value = true
}

function editPost(post: Post) {
  editingPost.value = post
  postForm.value = {
    title: post.title,
    content: post.content,
    isPinned: post.isPinned,
    isAnnouncement: post.isAnnouncement
  }
  showDialog.value = true
}

function closeDialog() {
  showDialog.value = false
  editingPost.value = null
}

async function savePost() {
  const condos = await condoStore.fetchAll()
  if (condos.length === 0) {
    $q.notify({ type: 'negative', message: t('common.error') })
    return
  }

  const condominiumId = condos[0].id

  if (editingPost.value) {
    const data: UpdatePostRequest = {
      title: postForm.value.title,
      content: postForm.value.content,
      isPinned: postForm.value.isPinned,
      isAnnouncement: postForm.value.isAnnouncement
    }
    const success = await postStore.update(editingPost.value.id, data)
    if (success) {
      $q.notify({ type: 'positive', message: t('posts.updateSuccess') })
      closeDialog()
      await loadPosts()
    } else {
      $q.notify({ type: 'negative', message: postStore.error || t('common.error') })
    }
  } else {
    const data: CreatePostRequest = {
      title: postForm.value.title,
      content: postForm.value.content,
      isPinned: postForm.value.isPinned,
      isAnnouncement: postForm.value.isAnnouncement
    }
    const id = await postStore.create(condominiumId, data)
    if (id) {
      $q.notify({ type: 'positive', message: t('posts.createSuccess') })
      closeDialog()
      await loadPosts()
    } else {
      $q.notify({ type: 'negative', message: postStore.error || t('common.error') })
    }
  }
}

function viewPost(post: Post) {
  viewingPost.value = post
  showViewDialog.value = true
}

async function approvePost(post: Post) {
  const success = await postStore.update(post.id, { status: 1 } as UpdatePostRequest)
  if (success) {
    $q.notify({ type: 'positive', message: t('posts.approveSuccess') })
    await loadPosts()
  } else {
    $q.notify({ type: 'negative', message: t('common.error') })
  }
}

async function rejectPost(post: Post) {
  const success = await postStore.update(post.id, { status: 2 } as UpdatePostRequest)
  if (success) {
    $q.notify({ type: 'positive', message: t('posts.rejectSuccess') })
    await loadPosts()
  } else {
    $q.notify({ type: 'negative', message: t('common.error') })
  }
}

function confirmDelete(post: Post) {
  $q.dialog({
    title: t('common.confirmDelete'),
    message: t('common.deleteMessage').replace('{item}', post.title),
    cancel: true,
    persistent: true
  }).onOk(async () => {
    const success = await postStore.remove(post.id)
    if (success) {
      $q.notify({ type: 'positive', message: t('common.success') })
      await loadPosts()
    } else {
      $q.notify({ type: 'negative', message: t('common.error') })
    }
  })
}

onMounted(async () => {
  await loadPosts()
})
</script>
