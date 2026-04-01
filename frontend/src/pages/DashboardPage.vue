<template>
  <q-page class="q-pa-md">
    <div class="text-h4 q-mb-md">{{ t('dashboard.title') }}</div>

    <q-tabs v-model="activeTab" class="q-mb-md" align="left" dense>
      <q-tab name="overview" :label="t('dashboard.overview')" />
      <q-tab name="myactivity" :label="t('dashboard.myActivity')" />
      <q-tab v-if="authStore.isManager" name="security" :label="t('dashboard.securityMap')" />
      <q-tab v-if="authStore.isManager" name="approvals" :label="t('dashboard.pendingApprovals')">
        <q-badge v-if="pendingCount > 0" color="red" floating>{{ pendingCount }}</q-badge>
      </q-tab>
    </q-tabs>

    <q-tab-panels v-model="activeTab" animated>
      <!-- Overview Tab -->
      <q-tab-panel name="overview" class="q-pa-none">
        <div class="row q-col-gutter-md q-mb-md">
          <div class="col-12 col-sm-6 col-md-3">
            <q-card class="bg-primary text-white">
              <q-card-section>
                <div class="text-h6">
                  <q-icon name="home_work" class="q-mr-sm" />
                  {{ t('common.condominiums') }}
                </div>
                <div class="text-h3">{{ stats.condominiums }}</div>
              </q-card-section>
            </q-card>
          </div>

          <div class="col-12 col-sm-6 col-md-3">
            <q-card class="bg-orange text-white">
              <q-card-section>
                <div class="text-h6">
                  <q-icon name="warning" class="q-mr-sm" />
                  {{ t('common.incidents') }}
                </div>
                <div class="text-h3">{{ stats.incidents }}</div>
              </q-card-section>
            </q-card>
          </div>

          <div class="col-12 col-sm-6 col-md-3">
            <q-card class="bg-purple text-white">
              <q-card-section>
                <div class="text-h6">
                  <q-icon name="poll" class="q-mr-sm" />
                  {{ t('common.polls') }}
                </div>
                <div class="text-h3">{{ stats.polls }}</div>
              </q-card-section>
            </q-card>
          </div>

          <div class="col-12 col-sm-6 col-md-3">
            <q-card class="bg-red text-white">
              <q-card-section>
                <div class="text-h6">
                  <q-icon name="notifications_active" class="q-mr-sm" />
                  {{ t('common.alerts') }}
                </div>
                <div class="text-h3">{{ stats.alerts }}</div>
              </q-card-section>
            </q-card>
          </div>
        </div>

        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-6">
            <q-card>
              <q-card-section>
                <div class="text-h6">{{ t('dashboard.recentPosts') }}</div>
              </q-card-section>
              <q-separator />
              <q-list separator>
                <PostItem v-for="post in recentPosts" :key="post.id" :post="post" />
                <q-item v-if="recentPosts.length === 0">
                  <q-item-section class="text-grey">
                    {{ t('dashboard.noPosts') }}
                  </q-item-section>
                </q-item>
              </q-list>
            </q-card>
          </div>

          <div class="col-12 col-md-6">
            <q-card>
              <q-card-section>
                <div class="text-h6">{{ t('dashboard.recentIncidents') }}</div>
              </q-card-section>
              <q-separator />
              <q-list separator>
                <IncidentItem v-for="incident in recentIncidents" :key="incident.id" :incident="incident" />
                <q-item v-if="recentIncidents.length === 0">
                  <q-item-section class="text-grey">
                    {{ t('dashboard.noIncidents') }}
                  </q-item-section>
                </q-item>
              </q-list>
            </q-card>
          </div>
        </div>
      </q-tab-panel>

      <!-- My Activity Tab -->
      <q-tab-panel name="myactivity" class="q-pa-none">
        <q-tabs v-model="myActivityTab" class="q-mb-md" align="left" dense>
          <q-tab name="posts" :label="t('common.posts')" />
          <q-tab name="incidents" :label="t('common.incidents')" />
          <q-tab name="alerts" :label="t('common.alerts')" />
          <q-tab name="polls" :label="t('common.polls')" />
        </q-tabs>

        <q-tab-panels v-model="myActivityTab" animated>
          <q-tab-panel name="posts" class="q-pa-none">
            <div class="row q-col-gutter-md">
              <div v-for="post in myPosts" :key="post.id" class="col-12 col-md-6">
                <PostItem :post="post" @click="() => {}" />
              </div>
              <div v-if="myPosts.length === 0" class="col-12 text-center text-grey q-pa-xl">
                {{ t('dashboard.noMyPosts') }}
              </div>
            </div>
          </q-tab-panel>

          <q-tab-panel name="incidents" class="q-pa-none">
            <div class="row q-col-gutter-md">
              <div v-for="incident in myIncidents" :key="incident.id" class="col-12 col-md-6">
                <IncidentItem :incident="incident" />
              </div>
              <div v-if="myIncidents.length === 0" class="col-12 text-center text-grey q-pa-xl">
                {{ t('dashboard.noMyIncidents') }}
              </div>
            </div>
          </q-tab-panel>

          <q-tab-panel name="alerts" class="q-pa-none">
            <div class="row q-col-gutter-md">
              <div v-for="alert in myAlerts" :key="alert.id" class="col-12 col-md-6">
                <AlertItem :alert="alert" />
              </div>
              <div v-if="myAlerts.length === 0" class="col-12 text-center text-grey q-pa-xl">
                {{ t('dashboard.noMyAlerts') }}
              </div>
            </div>
          </q-tab-panel>

          <q-tab-panel name="polls" class="q-pa-none">
            <div class="row q-col-gutter-md">
              <div v-for="poll in myPolls" :key="poll.id" class="col-12 col-md-6">
                <PollItem :poll="poll" :can-manage="false" :has-voted="false" />
              </div>
              <div v-if="myPolls.length === 0" class="col-12 text-center text-grey q-pa-xl">
                {{ t('dashboard.noMyPolls') }}
              </div>
            </div>
          </q-tab-panel>
        </q-tab-panels>
      </q-tab-panel>

      <!-- Security Map Tab (Admin/Manager only) -->
      <q-tab-panel name="security" class="q-pa-none">
        <q-card>
          <q-card-section>
            <div class="text-h6">{{ t('dashboard.securityMap') }}</div>
          </q-card-section>
          <q-separator />
          <SecurityMap
            :incidents="mapIncidents"
            :alerts="mapAlerts"
            @marker-click="handleMarkerClick"
          />
        </q-card>

        <div class="row q-col-gutter-md q-mt-md">
          <div class="col-12 col-md-6">
            <q-card>
              <q-card-section>
                <div class="text-h6">{{ t('dashboard.incidentWorkflow') }}</div>
              </q-card-section>
              <q-separator />
              <q-list separator>
                <q-item v-for="incident in recentIncidents" :key="incident.id">
                  <q-item-section>
                    <q-item-label>{{ incident.title }}</q-item-label>
                    <q-item-label caption>{{ incidentStore.getStatusLabel(incident.status) }}</q-item-label>
                  </q-item-section>
                  <q-item-section side>
                    <q-select
                      v-model="incident.status"
                      :options="statusOptions"
                      dense
                      borderless
                      @update:model-value="(val) => updateIncidentStatus(incident.id, val)"
                    />
                  </q-item-section>
                </q-item>
                <q-item v-if="recentIncidents.length === 0">
                  <q-item-section class="text-grey">No hay incidentes</q-item-section>
                </q-item>
              </q-list>
            </q-card>
          </div>

          <div class="col-12 col-md-6">
            <q-card>
              <q-card-section>
                <div class="text-h6">{{ t('dashboard.activeAlerts') }}</div>
              </q-card-section>
              <q-separator />
              <q-list separator>
                <q-item v-for="alert in activeAlerts" :key="alert.id">
                  <q-item-section>
                    <q-item-label>{{ alert.title }}</q-item-label>
                    <q-item-label caption>{{ alertStore.getStatusLabel(alert.status) }}</q-item-label>
                  </q-item-section>
                  <q-item-section side>
                    <q-btn
                      v-if="alert.status === 1 && authStore.isManager"
                      flat
                      color="positive"
                      :label="t('common.approve')"
                      @click="approveAlert(alert.id)"
                    />
                  </q-item-section>
                </q-item>
                <q-item v-if="activeAlerts.length === 0">
                  <q-item-section class="text-grey">No hay alertas activas</q-item-section>
                </q-item>
              </q-list>
            </q-card>
          </div>
        </div>
      </q-tab-panel>

      <!-- Pending Approvals Tab (Admin/Manager only) -->
      <q-tab-panel name="approvals" class="q-pa-none">
        <q-tabs v-model="approvalTab" class="q-mb-md" align="left" dense>
          <q-tab name="posts">
            <q-badge v-if="pendingPosts.length > 0" color="orange">{{ pendingPosts.length }}</q-badge>
            {{ t('common.posts') }}
          </q-tab>
          <q-tab name="polls">
            <q-badge v-if="pendingPolls.length > 0" color="orange">{{ pendingPolls.length }}</q-badge>
            {{ t('common.polls') }}
          </q-tab>
          <q-tab name="incidents">
            <q-badge v-if="pendingIncidents.length > 0" color="orange">{{ pendingIncidents.length }}</q-badge>
            {{ t('common.incidents') }}
          </q-tab>
          <q-tab name="alerts">
            <q-badge v-if="pendingAlerts.length > 0" color="orange">{{ pendingAlerts.length }}</q-badge>
            {{ t('common.alerts') }}
          </q-tab>
        </q-tabs>

        <q-tab-panels v-model="approvalTab" animated>
          <q-tab-panel name="posts" class="q-pa-none">
            <div class="row q-col-gutter-md">
              <div v-for="post in pendingPosts" :key="post.id" class="col-12 col-md-6">
                <q-card>
                  <q-card-section>
                    <div class="text-subtitle1">{{ post.title }}</div>
                    <div class="text-caption">{{ post.content?.substring(0, 100) }}...</div>
                    <div class="text-caption text-grey">{{ post.authorName }} - {{ post.createdAt }}</div>
                  </q-card-section>
                  <q-card-actions align="right">
                    <q-btn flat color="negative" :label="t('common.reject')" @click="rejectPost(post.id)" />
                    <q-btn color="positive" :label="t('common.approve')" @click="approvePost(post.id)" />
                  </q-card-actions>
                </q-card>
              </div>
              <div v-if="pendingPosts.length === 0" class="col-12 text-center text-grey q-pa-xl">
                No hay publicaciones pendientes
              </div>
            </div>
          </q-tab-panel>

          <q-tab-panel name="polls" class="q-pa-none">
            <div class="row q-col-gutter-md">
              <div v-for="poll in pendingPolls" :key="poll.id" class="col-12 col-md-6">
                <q-card>
                  <q-card-section>
                    <div class="text-subtitle1">{{ poll.title }}</div>
                    <div class="text-caption">{{ poll.description?.substring(0, 100) }}...</div>
                    <div class="text-caption text-grey">{{ poll.createdByName }} - {{ poll.createdAt }}</div>
                  </q-card-section>
                  <q-card-actions align="right">
                    <q-btn flat color="negative" :label="t('common.reject')" @click="rejectPoll(poll.id)" />
                    <q-btn color="positive" :label="t('common.approve')" @click="approvePoll(poll.id)" />
                  </q-card-actions>
                </q-card>
              </div>
              <div v-if="pendingPolls.length === 0" class="col-12 text-center text-grey q-pa-xl">
                No hay votaciones pendientes
              </div>
            </div>
          </q-tab-panel>

          <q-tab-panel name="incidents" class="q-pa-none">
            <div class="row q-col-gutter-md">
              <div v-for="incident in pendingIncidents" :key="incident.id" class="col-12 col-md-6">
                <IncidentItem :incident="incident" />
              </div>
              <div v-if="pendingIncidents.length === 0" class="col-12 text-center text-grey q-pa-xl">
                No hay incidentes pendientes
              </div>
            </div>
          </q-tab-panel>

          <q-tab-panel name="alerts" class="q-pa-none">
            <div class="row q-col-gutter-md">
              <div v-for="alert in pendingAlerts" :key="alert.id" class="col-12 col-md-6">
                <AlertItem :alert="alert" />
                <q-card-actions align="right">
                  <q-btn flat color="negative" :label="t('common.reject')" @click="rejectAlert(alert.id)" />
                  <q-btn color="positive" :label="t('common.approve')" @click="approveAlert(alert.id)" />
                </q-card-actions>
              </div>
              <div v-if="pendingAlerts.length === 0" class="col-12 text-center text-grey q-pa-xl">
                No hay alertas pendientes
              </div>
            </div>
          </q-tab-panel>
        </q-tab-panels>
      </q-tab-panel>
    </q-tab-panels>
  </q-page>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useCondominiumStore } from '../stores/condominium'
import { usePostStore } from '../stores/post'
import { useIncidentStore } from '../stores/incident'
import { usePollStore } from '../stores/poll'
import { useAlertStore } from '../stores/alert'
import { Post, Incident, Alert, Poll } from '../types/models'
import { useAuthStore } from '../stores/auth'
import { useI18n } from '../composables/useI18n'
import PostItem from '../components/PostItem.vue'
import IncidentItem from '../components/IncidentItem.vue'
import AlertItem from '../components/AlertItem.vue'
import PollItem from '../components/PollItem.vue'
import SecurityMap from '../components/SecurityMap.vue'

// (I will rename the local interfaces or use the imported types directly)

const $q = useQuasar()
const condoStore = useCondominiumStore()
const postStore = usePostStore()
const incidentStore = useIncidentStore()
const pollStore = usePollStore()
const alertStore = useAlertStore()
const authStore = useAuthStore()
// eslint-disable-next-line @typescript-eslint/no-unused-vars
const { t } = useI18n()

const activeTab = ref('overview')
const myActivityTab = ref('posts')
const approvalTab = ref('posts')

const stats = ref({
  condominiums: 0,
  incidents: 0,
  polls: 0,
  alerts: 0
})

interface Post {
  id: string
  title: string
  content: string
  authorId: string
  authorName?: string
  status: number
  createdAt: string
  category: number
  isPinned: boolean
  isAnnouncement: boolean
}

interface Incident {
  id: string
  title: string
  description?: string
  status: number
  priority: number
  lat?: number
  lng?: number
  createdById: string
  createdAt: string
  type: number
}

interface Alert {
  id: string
  title: string
  description?: string
  status: number
  lat?: number
  lng?: number
  createdById: string
  createdAt: string
  type: number
}

interface Poll {
  id: string
  title: string
  description?: string
  status: number
  createdById: string
  createdByName?: string
  createdAt: string
  options: string
  startsAt: string
  endsAt: string
  pollType: number
}

const recentPosts = ref<Post[]>([])
const recentIncidents = ref<Incident[]>([])
const allPosts = ref<Post[]>([])
const allIncidents = ref<Incident[]>([])
const allAlerts = ref<Alert[]>([])
const allPolls = ref<Poll[]>([])

const myPosts = computed(() => allPosts.value.filter(p => p.authorId === authStore.user?.id))
const myIncidents = computed(() => allIncidents.value.filter(i => i.createdById === authStore.user?.id))
const myAlerts = computed(() => allAlerts.value.filter(a => a.createdById === authStore.user?.id))
const myPolls = computed(() => allPolls.value.filter(p => p.createdById === authStore.user?.id))

const pendingPosts = computed(() => allPosts.value.filter(p => p.status === 1))
const pendingPolls = computed(() => allPolls.value.filter(p => p.status === 1))
const pendingIncidents = computed(() => allIncidents.value.filter(i => i.status === 1))
const pendingAlerts = computed(() => allAlerts.value.filter(a => a.status === 1))

const pendingCount = computed(() => pendingPosts.value.length + pendingPolls.value.length + pendingIncidents.value.length + pendingAlerts.value.length)

const activeAlerts = computed(() => allAlerts.value.filter(a => a.status === 1 || a.status === 2))

const mapIncidents = computed(() => recentIncidents.value
  .filter(i => i.lat != null && i.lng != null && i.type != null)
  .map(i => ({
    id: i.id,
    type: 'incident' as const,
    title: i.title,
    description: i.description,
    lat: i.lat!,
    lng: i.lng!,
    status: i.status,
    priority: i.priority,
    created_at: i.createdAt
  })))

const mapAlerts = computed(() => activeAlerts.value
  .filter(a => a.lat != null && a.lng != null)
  .map(a => ({
    id: a.id,
    type: 'alert' as const,
    title: a.title,
    description: a.description,
    lat: a.lat!,
    lng: a.lng!,
    status: a.status,
    priority: 4,
    created_at: a.createdAt
  })))

const statusOptions = [
  { label: 'Reportado', value: 1 },
  { label: 'En proceso', value: 2 },
  { label: 'Pendiente', value: 3 },
  { label: 'Resuelto', value: 4 },
  { label: 'Cerrado', value: 5 },
  { label: 'Cancelado', value: 6 }
]

onMounted(async () => {
  const condos = await condoStore.fetchAll()
  stats.value.condominiums = condos.length

  if (condos.length > 0) {
    const condoId = condos[0].id

    allPosts.value = await postStore.fetchByCondominium(condoId)
    recentPosts.value = allPosts.value.slice(0, 5)

    allIncidents.value = await incidentStore.fetchByCondominium(condoId)
    recentIncidents.value = allIncidents.value.slice(0, 5)

    allPolls.value = await pollStore.fetchByCondominium(condoId)
    allAlerts.value = await alertStore.fetchByCondominium(condoId)

    stats.value.polls = allPolls.value.length
    stats.value.alerts = allAlerts.value.length
    stats.value.incidents = allIncidents.value.length
  }
})

function handleMarkerClick(item: any) {
  console.log('Marker clicked:', item)
}

async function updateIncidentStatus(id: string, status: any) {
  await incidentStore.updateStatus(id, status.value)
  $q.notify({ type: 'positive', message: 'Estado actualizado' })
}

async function approveAlert(alertId: string) {
  await alertStore.approve(alertId)
  $q.notify({ type: 'positive', message: 'Alerta aprobada' })
  await refreshData()
}

async function rejectAlert(alertId: string) {
  await alertStore.remove(alertId)
  $q.notify({ type: 'info', message: 'Alerta rechazada' })
  await refreshData()
}

async function approvePost(postId: string) {
  await postStore.update(postId, { status: 2 })
  $q.notify({ type: 'positive', message: 'Publicación aprobada' })
  await refreshData()
}

async function rejectPost(postId: string) {
  await postStore.remove(postId)
  $q.notify({ type: 'info', message: 'Publicación rechazada' })
  await refreshData()
}

async function approvePoll(pollId: string) {
  await pollStore.update(pollId, { status: 2 })
  $q.notify({ type: 'positive', message: 'Votación aprobada' })
  await refreshData()
}

async function rejectPoll(pollId: string) {
  await pollStore.remove(pollId)
  $q.notify({ type: 'info', message: 'Votación rechazada' })
  await refreshData()
}

async function refreshData() {
  const condos = await condoStore.fetchAll()
  if (condos.length > 0) {
    const condoId = condos[0].id
    allPosts.value = await postStore.fetchByCondominium(condoId)
    allIncidents.value = await incidentStore.fetchByCondominium(condoId)
    allPolls.value = await pollStore.fetchByCondominium(condoId)
    allAlerts.value = await alertStore.fetchByCondominium(condoId)
    recentPosts.value = allPosts.value.slice(0, 5)
    recentIncidents.value = allIncidents.value.slice(0, 5)
  }
}
</script>
