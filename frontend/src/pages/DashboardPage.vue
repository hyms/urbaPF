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
        <div class="row q-col-gutter-md q-mb-md" v-if="stats.condominiums > 0 || stats.incidents > 0 || stats.polls > 0 || stats.alerts > 0">
          <div class="col-12 col-sm-6 col-lg-3">
            <StatsCard :title="t('common.condominiums')" :value="stats.condominiums" icon="home_work" bgColor="bg-primary" />
          </div>
          <div class="col-12 col-sm-6 col-lg-3">
            <StatsCard :title="t('common.incidents')" :value="stats.incidents" icon="warning" bgColor="bg-orange" />
          </div>
          <div class="col-12 col-sm-6 col-lg-3">
            <StatsCard :title="t('common.polls')" :value="stats.polls" icon="poll" bgColor="bg-purple" />
          </div>
          <div class="col-12 col-sm-6 col-lg-3">
            <StatsCard :title="t('common.alerts')" :value="stats.alerts" icon="notifications_active" bgColor="bg-red" />
          </div>
        </div>

        <template v-for="condo in allCondos" :key="condo.id">
          <div class="q-mb-md" v-if="condo.rules">
            <q-card>
              <q-card-section>
                <div class="text-h6">
                  <q-icon name="gavel" class="q-mr-sm" />
                  {{ t('settings.rules') }} - {{ condo.name }}
                </div>
              </q-card-section>
              <q-separator />
              <q-card-section>
                <div class="text-body2" style="white-space: pre-line;">{{ condo.rules }}</div>
              </q-card-section>
            </q-card>
          </div>
        </template>

        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-6 q-mb-md q-mb-md-0">
            <RecentList 
              :title="t('dashboard.recentPosts')" 
              :isEmpty="recentPosts.length === 0" 
              :emptyMessage="t('dashboard.noPosts')"
            >
              <template #items>
                <PostItem v-for="post in recentPosts" :key="post.id" :post="post" />
              </template>
            </RecentList>
          </div>

          <div class="col-12 col-md-6">
            <RecentList 
              :title="t('dashboard.recentIncidents')" 
              :isEmpty="recentIncidents.length === 0" 
              :emptyMessage="t('dashboard.noIncidents')"
            >
              <template #items>
                <IncidentItem v-for="incident in recentIncidents" :key="incident.id" :incident="incident" />
              </template>
            </RecentList>
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
            <div class="row q-col-gutter-md q-mb-md" v-if="myPosts.length > 0">
              <div v-for="post in myPosts" :key="post.id" class="col-12 col-md-6 q-mb-md">
                <PostItem :post="post" @click="() => {}" />
              </div>
            </div>
            <div v-else class="col-12 text-center text-grey q-pa-xl">
              {{ t('dashboard.noMyPosts') }}
            </div>
          </q-tab-panel>

          <q-tab-panel name="incidents" class="q-pa-none">
            <div class="row q-col-gutter-md q-mb-md" v-if="myIncidents.length > 0">
              <div v-for="incident in myIncidents" :key="incident.id" class="col-12 col-md-6 q-mb-md">
                <IncidentItem :incident="incident" />
              </div>
            </div>
            <div v-else class="col-12 text-center text-grey q-pa-xl">
              {{ t('dashboard.noMyIncidents') }}
            </div>
          </q-tab-panel>

          <q-tab-panel name="alerts" class="q-pa-none">
            <div class="row q-col-gutter-md q-mb-md" v-if="myAlerts.length > 0">
              <div v-for="alert in myAlerts" :key="alert.id" class="col-12 col-md-6 q-mb-md">
                <AlertItem :alert="alert" />
              </div>
            </div>
            <div v-else class="col-12 text-center text-grey q-pa-xl">
              {{ t('dashboard.noMyAlerts') }}
            </div>
          </q-tab-panel>

          <q-tab-panel name="polls" class="q-pa-none">
            <div class="row q-col-gutter-md q-mb-md" v-if="myPolls.length > 0">
              <div v-for="poll in myPolls" :key="poll.id" class="col-12 col-md-6 q-mb-md">
                <PollItem :poll="poll" :can-manage="false" :has-voted="false" />
              </div>
            </div>
            <div v-else class="col-12 text-center text-grey q-pa-xl">
              {{ t('dashboard.noMyPolls') }}
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
                     <div class="text-caption text-grey">{{ post.authorName }} - {{ post.created_at }}</div>
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
                    <div class="text-caption text-grey">{{ poll.createdByName }} - {{ poll.created_at }}</div>
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
import { useCondominiumStore } from '@/stores/condominium'
import { usePostStore } from '@/stores/post'
import { useIncidentStore } from '@/stores/incident'
import { usePollStore } from '@/stores/poll'
import { useAlertStore } from '@/stores/alert'
import { Post, Incident, Alert, Poll } from '@/types/models'
import { useAuthStore } from '@/stores/auth'
import { useI18n } from '@/composables/useI18n'
import PostItem from '../components/PostItem.vue'
import IncidentItem from '../components/IncidentItem.vue'
import AlertItem from '../components/alert/AlertItem.vue'
import PollItem from '../components/PollItem.vue'
import SecurityMap from '../components/SecurityMap.vue'
import StatsCard from '../components/dashboard/StatsCard.vue'
import RecentList from '../components/dashboard/RecentList.vue'

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

const currentCondo = computed(() => {
  const condos = condoStore.condominiums
  return condos && condos.length > 0 ? condos[0] : null
})

const allCondos = computed(() => condoStore.condominiums || [])

const stats = ref({
  condominiums: 0,
  incidents: 0,
  polls: 0,
  alerts: 0
})

// Removed local interfaces

const recentPosts = ref<Post[]>([])
const recentIncidents = ref<Incident[]>([])
const allPosts = ref<Post[]>([])
const allIncidents = ref<Incident[]>([])
const allAlerts = ref<Alert[]>([])
const allPolls = ref<Poll[]>([])

const myPosts = computed(() => allPosts.value.filter(p => p.authorId === authStore.user?.id))
const myIncidents = computed(() => allIncidents.value.filter(i => i.reporterId === authStore.user?.id))
const myAlerts = computed(() => allAlerts.value.filter(a => a.creatorId === authStore.user?.id))
const myPolls = computed(() => allPolls.value.filter(p => p.createdById === authStore.user?.id))

const pendingPosts = computed(() => allPosts.value.filter(p => p.status === 1))
const pendingPolls = computed(() => allPolls.value.filter(p => p.status === 1))
const pendingIncidents = computed(() => allIncidents.value.filter(i => i.status === 1))
const pendingAlerts = computed(() => allAlerts.value.filter(a => a.status === 1))

const pendingCount = computed(() => pendingPosts.value.length + pendingPolls.value.length + pendingIncidents.value.length + pendingAlerts.value.length)

const activeAlerts = computed(() => allAlerts.value.filter(a => a.status === 1 || a.status === 2))

const mapIncidents = computed(() => recentIncidents.value
  .filter(i => i.location && i.type != null)
  .map(i => ({
    id: i.id,
    type: 'incident' as const,
    title: i.title,
    description: i.description,
    lat: parseFloat(i.location!.split(' ')[0]),
    lng: parseFloat(i.location!.split(' ')[1]),
    status: i.status,
    priority: i.priority,
    created_at: i.created_at
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
    created_at: a.created_at
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
