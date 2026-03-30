<template>
  <div class="security-map" style="height: 400px; width: 100%">
    <div ref="mapContainer" style="height: 100%; width: 100%"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

interface MapItem {
  id: string
  type: 'incident' | 'alert'
  title: string
  description?: string
  lat: number
  lng: number
  status: number
  priority: number
}

const props = defineProps<{
  incidents: MapItem[]
  alerts: MapItem[]
  center?: [number, number]
  zoom?: number
}>()

const emit = defineEmits<{
  (e: 'marker-click', item: MapItem): void
}>()

const mapContainer = ref<HTMLElement | null>(null)
let map: L.Map | null = null
let markersLayer: L.LayerGroup | null = null

const defaultCenter: [number, number] = props.center || [-17.8146, -63.1561]
const defaultZoom = props.zoom || 15

function getIncidentIcon(priority: number, status: number) {
  const color = priority === 4 ? 'red' : priority === 3 ? 'orange' : priority === 2 ? 'blue' : 'green'
  const iconColor = status >= 4 ? '#9e9e9e' : color
  
  return L.divIcon({
    className: 'custom-marker',
    html: `<div style="
      background-color: ${iconColor};
      width: 24px;
      height: 24px;
      border-radius: 50%;
      border: 2px solid white;
      box-shadow: 0 2px 4px rgba(0,0,0,0.3);
      display: flex;
      align-items: center;
      justify-content: center;
    "><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="white" width="14" height="14"><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"/></svg></div>`,
    iconSize: [24, 24],
    iconAnchor: [12, 12]
  })
}

function getAlertIcon(status: number) {
  const color = status === 1 ? 'red' : status >= 3 ? 'orange' : 'blue'
  
  return L.divIcon({
    className: 'custom-marker',
    html: `<div style="
      background-color: ${color};
      width: 32px;
      height: 32px;
      border-radius: 50%;
      border: 3px solid white;
      box-shadow: 0 2px 8px rgba(0,0,0,0.4);
      display: flex;
      align-items: center;
      justify-content: center;
      animation: pulse 2s infinite;
    "><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="white" width="18" height="18"><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"/></svg></div>`,
    iconSize: [32, 32],
    iconAnchor: [16, 16]
  })
}

function initMap() {
  if (!mapContainer.value || map) return

  map = L.map(mapContainer.value).setView(defaultCenter, defaultZoom)

  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
  }).addTo(map)

  markersLayer = L.layerGroup().addTo(map)
  
  updateMarkers()
}

function updateMarkers() {
  if (!map || !markersLayer) return

  markersLayer.clearLayers()

  props.incidents.forEach(item => {
    if (item.lat && item.lng) {
      const marker = L.marker([item.lat, item.lng], {
        icon: getIncidentIcon(item.priority, item.status)
      })

      marker.bindPopup(`
        <div style="min-width: 150px">
          <strong>${item.title}</strong><br/>
          <span style="color: #666">Incidente</span><br/>
          ${item.description || ''}
        </div>
      `)

      marker.on('click', () => emit('marker-click', item))
      markersLayer!.addLayer(marker)
    }
  })

  props.alerts.forEach(item => {
    if (item.lat && item.lng) {
      const marker = L.marker([item.lat, item.lng], {
        icon: getAlertIcon(item.status)
      })

      marker.bindPopup(`
        <div style="min-width: 150px">
          <strong style="color: red">${item.title}</strong><br/>
          <span style="color: #666">Alerta de Emergencia</span><br/>
          ${item.description || ''}
        </div>
      `)

      marker.on('click', () => emit('marker-click', item))
      markersLayer!.addLayer(marker)
    }
  })

  if (props.incidents.length > 0 || props.alerts.length > 0) {
    const group = L.featureGroup(markersLayer.getLayers() as L.Marker[])
    map.fitBounds(group.getBounds().pad(0.1))
  }
}

function centerMap(lat: number, lng: number, zoom?: number) {
  if (!map) return
  map.setView([lat, lng], zoom || defaultZoom)
}

watch(() => [props.incidents, props.alerts], () => {
  updateMarkers()
}, { deep: true })

onMounted(() => {
  initMap()
})

defineExpose({
  centerMap,
  updateMarkers
})
</script>

<style>
@keyframes pulse {
  0% { transform: scale(1); box-shadow: 0 0 0 0 rgba(244, 67, 54, 0.7); }
  70% { transform: scale(1.1); box-shadow: 0 0 0 10px rgba(244, 67, 54, 0); }
  100% { transform: scale(1); box-shadow: 0 0 0 0 rgba(244, 67, 54, 0); }
}

.custom-marker {
  background: transparent;
  border: none;
}
</style>
