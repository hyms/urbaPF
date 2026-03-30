<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">Dashboard de Seguridad</div>
      <q-space />
      <q-btn
        color="primary"
        icon="refresh"
        label="Actualizar"
        @click="refreshData"
        :loading="loading"
      />
    </div>

    <div class="row q-col-gutter-md">
      <div class="col-12 col-md-8">
        <q-card>
          <q-card-section>
            <div class="text-subtitle1 q-mb-sm">Mapa de Incidentes y Alertas</div>
            <SecurityMap
              ref="mapRef"
              :incidents="mapIncidents"
              :alerts="mapAlerts"
              @marker-click="handleMarkerClick"
            />
          </q-card-section>
        </q-card>

        <q-card class="q-mt-md">
          <q-card-section>
            <div class="row items-center q-mb-sm">
              <div class="text-subtitle1">Estadísticas</div>
            </div>
            <div class="row q-col-gutter-sm">
              <div class="col-6 col-sm-3">
                <q-card class="bg-red text-white">
                  <q-card-section class="text-center">
                    <div class="text-h4">{{ urgentCount }}</div>
                    <div class="text-caption">Urgentes</div>
                  </q-card-section>
                </q-card>
              </div>
              <div class="col-6 col-sm-3">
                <q-card class="bg-orange text-white">
                  <q-card-section class="text-center">
                    <div class="text-h4">{{ activeAlerts }}</div>
                    <div class="text-caption">Alertas Activas</div>
                  </q-card-section>
                </q-card>
              </div>
              <div class="col-6 col-sm-3">
                <q-card class="bg-blue text-white">
                  <q-card-section class="text-center">
                    <div class="text-h4">{{ activeIncidents }}</div>
                    <div class="text-caption">Incidentes Activos</div>
                  </q-card-section>
                </q-card>
              </div>
              <div class="col-6 col-sm-3">
                <q-card class="bg-green text-white">
                  <q-card-section class="text-center">
                    <div class="text-h4">{{ resolvedToday }}</div>
                    <div class="text-caption">Resueltos Hoy</div>
                  </q-card-section>
                </q-card>
              </div>
            </div>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-md-4">
        <q-tabs v-model="activeTab" dense class="text-grey" align="justify">
          <q-tab name="alerts" label="Alertas" />
          <q-tab name="incidents" label="Incidentes" />
        </q-tabs>

        <q-tab-panels v-model="activeTab" animated>
          <q-tab-panel name="alerts" class="q-pa-none">
            <q-list separator>
              <q-item
                v-for="alert in activeAlertsList"
                :key="alert.id"
                clickable
                @click="selectAlert(alert)"
                :class="selectedItem?.id === alert.id ? 'bg-blue-1' : ''"
              >
                <q-item-section avatar>
                  <q-icon name="warning" color="red" />
                </q-item-section>
                <q-item-section>
                  <q-item-label>{{ alert.title }}</q-item-label>
                  <q-item-label caption>
                    {{ formatDate(alert.created_at) }}
                  </q-item-label>
                </q-item-section>
                <q-item-section side>
                  <q-badge :color="getAlertStatusColor(alert.status)">
                    {{ getAlertStatusLabel(alert.status) }}
                  </q-badge>
                </q-item-section>
              </q-item>
              <q-item v-if="activeAlertsList.length === 0">
                <q-item-section class="text-grey text-center">
                  No hay alertas activas
                </q-item-section>
              </q-item>
            </q-list>
          </q-tab-panel>

          <q-tab-panel name="incidents" class="q-pa-none">
            <q-list separator>
              <q-item
                v-for="incident in activeIncidentsList"
                :key="incident.id"
                clickable
                @click="selectIncident(incident)"
                :class="selectedItem?.id === incident.id ? 'bg-blue-1' : ''"
              >
                <q-item-section avatar>
                  <q-icon :name="getIncidentIcon(incident.type)" :color="getPriorityColor(incident.priority)" />
                </q-item-section>
                <q-item-section>
                  <q-item-label>{{ incident.title }}</q-item-label>
                  <q-item-label caption>
                    {{ formatDate(incident.created_at) }}
                  </q-item-label>
                </q-item-section>
                <q-item-section side>
                  <q-badge :color="getPriorityColor(incident.priority)">
                    {{ getPriorityLabel(incident.priority) }}
                  </q-badge>
                </q-item-section>
              </q-item>
              <q-item v-if="activeIncidentsList.length === 0">
                <q-item-section class="text-grey text-center">
                  No hay incidentes activos
                </q-item-section>
              </q-item>
            </q-list>
          </q-tab-panel>
        </q-tab-panels>
      </div>
    </div>

    <q-dialog v-model="showDetailDialog">
      <q-card style="min-width: 350px" v-if="selectedItem">
        <q-card-section>
          <div class="text-h6">
            {{ selectedItem.type === 'alert' ? 'Alerta' : 'Incidente' }}: {{ selectedItem.title }}
          </div>
        </q-card-section>
        <q-card-section>
          <div v-if="selectedItem.description" class="q-mb-md">
            {{ selectedItem.description }}
          </div>
          <div class="text-caption text-grey">
            Creado: {{ formatDate(selectedItem.created_at) }}
          </div>
          <div v-if="selectedItem.address_reference" class="q-mt-sm">
            <strong>Ubicación:</strong> {{ selectedItem.address_reference }}
          </div>
        </q-card-section>
        <q-card-actions align="right">
          <q-btn flat label="Cerrar" v-close-popup />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useIncidentStore } from '../stores/incident'
import { useAlertStore } from '../stores/alert'
import { useCondominiumStore } from '../stores/condominium'
import SecurityMap from '../components/SecurityMap.vue'
import {
  IncidentTypeIcon,
  IncidentPriorityLabel,
  IncidentPriorityColor
} from '../utils/appEnums'

const $q = useQuasar()
const incidentStore = useIncidentStore()
const alertStore = useAlertStore()
const condoStore = useCondominiumStore()

const loading = ref(false)
const activeTab = ref('alerts')
const incidents = ref<any[]>([])
const alerts = ref<any[]>([])
const selectedItem = ref<any>(null)
const showDetailDialog = ref(false)
const mapRef = ref<any>(null)

interface MapItem {
  id: string
  type: 'incident' | 'alert'
  title: string
  description?: string
  lat: number
  lng: number
  status: number
  priority: number
  created_at: string
  address_reference?: string
}

const mapIncidents = computed<MapItem[]>(() => {
  return incidents.value
    .filter(i => i.location)
    .map(i => {
      const [lat, lng] = i.location.split(' ')
      return {
        id: i.id,
        type: 'incident' as const,
        title: i.title,
        description: i.description,
        lat: parseFloat(lat),
        lng: parseFloat(lng),
        status: i.status,
        priority: i.priority,
        created_at: i.created_at,
        address_reference: i.address_reference
      }
    })
})

const mapAlerts = computed<MapItem[]>(() => {
  return alerts.value
    .filter(a => a.location)
    .map(a => {
      const [lat, lng] = a.location.split(' ')
      return {
        id: a.id,
        type: 'alert' as const,
        title: a.title,
        description: a.description,
        lat: parseFloat(lat),
        lng: parseFloat(lng),
        status: a.status,
        priority: 4,
        created_at: a.created_at
      }
    })
})

const urgentCount = computed(() => {
  const urgentIncidents = incidents.value.filter(i => i.priority === 4 && i.status < 4).length
  const urgentAlerts = alerts.value.filter(a => a.status < 4).length
  return urgentIncidents + urgentAlerts
})

const activeAlerts = computed(() => alerts.value.filter(a => a.status < 4).length)
const activeIncidents = computed(() => incidents.value.filter(i => i.status < 4).length)

const resolvedToday = computed(() => {
  const today = new Date().toDateString()
  return incidents.value.filter(i => 
    (i.status === 4 || i.status === 5) && 
    i.resolved_at && 
    new Date(i.resolved_at).toDateString() === today
  ).length
})

const activeAlertsList = computed(() => alerts.value.filter(a => a.status < 4))
const activeIncidentsList = computed(() => incidents.value.filter(i => i.status < 4))

async function refreshData() {
  loading.value = true
  try {
    const condos = await condoStore.fetchAll()
    if (condos.length > 0) {
      const condoId = condos[0].id
      incidents.value = await incidentStore.fetchByCondominium(condoId)
      alerts.value = await alertStore.fetchByCondominium(condoId)
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: 'Error al cargar datos' })
  } finally {
    loading.value = false
  }
}

function selectAlert(alert: any) {
  selectedItem.value = { ...alert, type: 'alert' }
  showDetailDialog.value = true
  
  if (alert.location) {
    const [lat, lng] = alert.location.split(' ')
    mapRef.value?.centerMap(parseFloat(lat), parseFloat(lng))
  }
}

function selectIncident(incident: any) {
  selectedItem.value = { ...incident, type: 'incident' }
  showDetailDialog.value = true
  
  if (incident.location) {
    const [lat, lng] = incident.location.split(' ')
    mapRef.value?.centerMap(parseFloat(lat), parseFloat(lng))
  }
}

function handleMarkerClick(item: MapItem) {
  if (item.type === 'alert') {
    const alert = alerts.value.find(a => a.id === item.id)
    if (alert) selectAlert(alert)
  } else {
    const incident = incidents.value.find(i => i.id === item.id)
    if (incident) selectIncident(incident)
  }
}

function formatDate(dateStr: string) {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleDateString('es-BO', {
    day: '2-digit',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function getIncidentIcon(type: number) { return IncidentTypeIcon(type) }
function getPriorityLabel(priority: number) { return IncidentPriorityLabel(priority) }
function getPriorityColor(priority: number) { return IncidentPriorityColor(priority) }

function getAlertStatusColor(status: number) {
  const colors: Record<number, string> = { 1: 'red', 2: 'blue', 3: 'orange', 4: 'primary', 5: 'positive' }
  return colors[status] || 'grey'
}

function getAlertStatusLabel(status: number) {
  const labels: Record<number, string> = {
    1: 'Pendiente', 2: 'Aprobada', 3: 'Notificada', 4: 'En Proceso', 5: 'Resuelta'
  }
  return labels[status] || 'Desconocido'
}

onMounted(() => {
  refreshData()
})
</script>
