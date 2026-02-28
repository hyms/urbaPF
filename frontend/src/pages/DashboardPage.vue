<template>
  <q-page class="q-pa-md">
    <div class="text-h4 q-mb-md">Dashboard</div>

    <div class="row q-col-gutter-md q-mb-md">
      <div class="col-12 col-sm-6 col-md-3">
        <q-card class="bg-primary text-white">
          <q-card-section>
            <div class="text-h6">
              <q-icon name="home_work" class="q-mr-sm" />
              Condominios
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
              Incidentes
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
              Votaciones
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
              Alertas
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
            <div class="text-h6">Últimas Publicaciones</div>
          </q-card-section>
          <q-separator />
          <q-list separator>
            <q-item v-for="post in recentPosts" :key="post.id" clickable v-ripple>
              <q-item-section>
                <q-item-label>{{ post.title }}</q-item-label>
                <q-item-label caption>{{ post.authorName }} - {{ formatDate(post.createdAt) }}</q-item-label>
              </q-item-section>
              <q-item-section side>
                <q-chip :color="getCategoryColor(post.category)" text-color="white" size="sm">
                  {{ getCategoryLabel(post.category) }}
                </q-chip>
              </q-item-section>
            </q-item>
            <q-item v-if="recentPosts.length === 0">
              <q-item-section class="text-grey">
                No hay publicaciones recientes
              </q-item-section>
            </q-item>
          </q-list>
        </q-card>
      </div>

      <div class="col-12 col-md-6">
        <q-card>
          <q-card-section>
            <div class="text-h6">Incidentes Recientes</div>
          </q-card-section>
          <q-separator />
          <q-list separator>
            <q-item v-for="incident in recentIncidents" :key="incident.id" clickable v-ripple>
              <q-item-section>
                <q-item-label>{{ incident.title }}</q-item-label>
                <q-item-label caption>{{ incident.reportedByName }} - {{ formatDate(incident.createdAt) }}</q-item-label>
              </q-item-section>
              <q-item-section side>
                <q-chip :color="getPriorityColor(incident.priority)" text-color="white" size="sm">
                  {{ getPriorityLabel(incident.priority) }}
                </q-chip>
              </q-item-section>
            </q-item>
            <q-item v-if="recentIncidents.length === 0">
              <q-item-section class="text-grey">
                No hay incidentes recientes
              </q-item-section>
            </q-item>
          </q-list>
        </q-card>
      </div>
    </div>
  </q-page>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useCondominiumStore } from '../stores/condominium'
import { usePostStore } from '../stores/post'
import { useIncidentStore } from '../stores/incident'
import { usePollStore } from '../stores/poll'
import { useAlertStore } from '../stores/alert'

const condoStore = useCondominiumStore()
const postStore = usePostStore()
const incidentStore = useIncidentStore()
const pollStore = usePollStore()
const alertStore = useAlertStore()

const stats = ref({
  condominiums: 0,
  incidents: 0,
  polls: 0,
  alerts: 0
})

const recentPosts = ref([])
const recentIncidents = ref([])

onMounted(async () => {
  const condos = await condoStore.fetchAll()
  stats.value.condominiums = condos.length

  if (condos.length > 0) {
    const condoId = condos[0].id
    recentPosts.value = await postStore.fetchByCondominium(condoId)
    recentIncidents.value = await incidentStore.fetchByCondominium(condoId)
    const polls = await pollStore.fetchByCondominium(condoId)
    const alerts = await alertStore.fetchByCondominium(condoId)
    stats.value.polls = polls.length
    stats.value.alerts = alerts.length
    stats.value.incidents = recentIncidents.value.length
  }
})

function formatDate(date) {
  return new Date(date).toLocaleDateString('es-ES', {
    day: 'numeric',
    month: 'short',
    year: 'numeric'
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

function getPriorityLabel(priority) {
  const labels = { 1: 'Baja', 2: 'Media', 3: 'Alta', 4: 'Urgente' }
  return labels[priority] || 'Baja'
}

function getPriorityColor(priority) {
  const colors = { 1: 'green', 2: 'yellow', 3: 'orange', 4: 'red' }
  return colors[priority] || 'green'
}
</script>
