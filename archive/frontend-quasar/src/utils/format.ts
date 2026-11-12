export function formatDate(date: string | Date | undefined): string {
  if (!date) return ''
  return new Date(date).toLocaleDateString('es-ES', {
    day: 'numeric',
    month: 'short',
    year: 'numeric'
  })
}

export function getCategoryLabel(category: number): string {
  const labels: Record<number, string> = { 1: 'General', 2: 'Evento', 3: 'Anuncio', 4: 'Discusión' }
  return labels[category] || 'General'
}

export function getCategoryColor(category: number): string {
  const colors: Record<number, string> = { 1: 'grey', 2: 'blue', 3: 'green', 4: 'purple' }
  return colors[category] || 'grey'
}

export function getCategoryIcon(category: number): string {
  const icons: Record<number, string> = { 1: 'article', 2: 'event', 3: 'campaign', 4: 'forum' }
  return icons[category] || 'article'
}

export function getPriorityLabel(priority: number): string {
  const labels: Record<number, string> = { 1: 'Baja', 2: 'Media', 3: 'Alta', 4: 'Urgente' }
  return labels[priority] || 'Baja'
}

export function getPriorityColor(priority: number): string {
  const colors: Record<number, string> = { 1: 'green', 2: 'yellow', 3: 'orange', 4: 'red' }
  return colors[priority] || 'green'
}

export function getIncidentStatusLabel(status: number): string {
  const labels: Record<number, string> = {
    1: 'Reportado',
    2: 'En proceso',
    3: 'Pendiente',
    4: 'Resuelto',
    5: 'Cerrado',
    6: 'Cancelado'
  }
  return labels[status] || 'Desconocido'
}

export function getIncidentStatusColor(status: number): string {
  const colors: Record<number, string> = { 1: 'blue', 2: 'orange', 3: 'yellow', 4: 'green', 5: 'grey', 6: 'red' }
  return colors[status] || 'grey'
}

export function formatDateTime(date: string | Date | undefined): string {
  if (!date) return ''
  return new Date(date).toLocaleString('es-BO', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}



export function parseMedia(mediaJson: string) {
  try {
    return JSON.parse(mediaJson)
  } catch {
    return []
  }
}

export function getInitials(name: string): string {
  if (!name) return ''
  return name
    .split(' ')
    .map(n => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2)
}