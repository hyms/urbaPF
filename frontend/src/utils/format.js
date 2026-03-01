export function formatDate(date) {
  if (!date) return ''
  return new Date(date).toLocaleDateString('es-ES', {
    day: 'numeric',
    month: 'short',
    year: 'numeric'
  })
}

// Categorías de Publicaciones
export function getCategoryLabel(category) {
  const labels = { 1: 'General', 2: 'Evento', 3: 'Anuncio', 4: 'Discusión' }
  return labels[category] || 'General'
}

export function getCategoryColor(category) {
  const colors = { 1: 'grey', 2: 'blue', 3: 'green', 4: 'purple' }
  return colors[category] || 'grey'
}

export function getCategoryIcon(category) {
  const icons = { 1: 'article', 2: 'event', 3: 'campaign', 4: 'forum' }
  return icons[category] || 'article'
}

// Prioridad de Incidentes
export function getPriorityLabel(priority) {
  const labels = { 1: 'Baja', 2: 'Media', 3: 'Alta', 4: 'Urgente' }
  return labels[priority] || 'Baja'
}

export function getPriorityColor(priority) {
  const colors = { 1: 'green', 2: 'yellow', 3: 'orange', 4: 'red' }
  return colors[priority] || 'green'
}

// Estado de Incidentes
export function getIncidentStatusLabel(status) {
  const labels = {
    1: 'Reportado',
    2: 'En proceso',
    3: 'Pendiente',
    4: 'Resuelto',
    5: 'Cerrado',
    6: 'Cancelado'
  }
  return labels[status] || 'Desconocido'
}

export function getIncidentStatusColor(status) {
  const colors = { 1: 'blue', 2: 'orange', 3: 'yellow', 4: 'green', 5: 'grey', 6: 'red' }
  return colors[status] || 'grey'
}
