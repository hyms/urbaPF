// Categorías de Publicaciones
export const PostCategoryLabel = (category: number): string => {
  const labels: Record<number, string> = {
    1: 'General',
    2: 'Anuncio',
    3: 'Mantenimiento',
    4: 'Seguridad',
    5: 'Social',
    6: 'Queja',
    7: 'Sugerencia',
    8: 'Objetos Perdidos',
    9: 'En Venta',
    10: 'Eventos'
  }
  return labels[category] || 'General'
}

export const PostCategoryColor = (category: number): string => {
  const colors: Record<number, string> = {
    1: 'grey',
    2: 'green',
    3: 'blue',
    4: 'red',
    5: 'purple',
    6: 'orange',
    7: 'teal',
    8: 'brown',
    9: 'indigo',
    10: 'pink'
  }
  return colors[category] || 'grey'
}

export const PostCategoryIcon = (category: number): string => {
  const icons: Record<number, string> = {
    1: 'article',
    2: 'campaign',
    3: 'build',
    4: 'security',
    5: 'groups',
    6: 'complaint',
    7: 'lightbulb',
    8: 'find_in_page',
    9: 'sell',
    10: 'event'
  }
  return icons[category] || 'article'
}

// Estado de Publicaciones (Backend: 0=Pending, 1=Approved, 2=Rejected)
export const PostStatusLabel = (status: number): string => {
  const labels: Record<number, string> = {
    0: 'Pendiente',
    1: 'Aprobado',
    2: 'Rechazado'
  }
  return labels[status] || 'Desconocido'
}

export const PostStatusColor = (status: number): string => {
  const colors: Record<number, string> = {
    0: 'warning',
    1: 'positive',
    2: 'negative'
  }
  return colors[status] || 'grey'
}

// Prioridad de Incidentes
export const IncidentPriorityLabel = (priority: number): string => {
  const labels: Record<number, string> = {
    1: 'Baja',
    2: 'Media',
    3: 'Alta',
    4: 'Urgente'
  }
  return labels[priority] || 'Baja'
}

export const IncidentPriorityColor = (priority: number): string => {
  const colors: Record<number, string> = {
    1: 'green',
    2: 'yellow',
    3: 'orange',
    4: 'red'
  }
  return colors[priority] || 'green'
}

// Estado de Incidentes
export const IncidentStatusLabel = (status: number): string => {
  const labels: Record<number, string> = {
    1: 'Reportado',
    2: 'En Proceso',
    3: 'Pendiente',
    4: 'Resuelto',
    5: 'Cerrado',
    6: 'Cancelado'
  }
  return labels[status] || 'Desconocido'
}

export const IncidentStatusColor = (status: number): string => {
  const colors: Record<number, string> = {
    1: 'blue',
    2: 'orange',
    3: 'yellow',
    4: 'green',
    5: 'grey',
    6: 'red'
  }
  return colors[status] || 'grey'
}

// Tipo de Alertas
export const AlertTypeLabel = (type: number): string => {
  const labels: Record<number, string> = {
    1: 'En Camino',
    2: 'Emergencia',
    3: 'Check-In',
    4: 'Patrulla'
  }
  return labels[type] || 'Desconocido'
}

export const AlertTypeColor = (type: number): string => {
  const colors: Record<number, string> = {
    1: 'blue',
    2: 'red',
    3: 'orange',
    4: 'grey'
  }
  return colors[type] || 'grey'
}

export const AlertTypeIcon = (type: number): string => {
  const icons: Record<number, string> = {
    1: 'directions_car',
    2: 'emergency',
    3: 'check_circle',
    4: 'local_police'
  }
  return icons[type] || 'notifications'
}

// Estado de Alertas
export const AlertStatusLabel = (status: number): string => {
  const labels: Record<number, string> = {
    1: 'Pendiente',
    2: 'Acknowledged',
    3: 'En Camino',
    4: 'Llegó',
    5: 'Completada',
    6: 'Cancelada'
  }
  return labels[status] || 'Desconocido'
}

export const AlertStatusColor = (status: number): string => {
  const colors: Record<number, string> = {
    1: 'red',
    2: 'orange',
    3: 'blue',
    4: 'green',
    5: 'grey',
    6: 'black'
  }
  return colors[status] || 'grey'
}

// Tipo de Votaciones
export const PollTypeLabel = (type: number): string => {
  const labels: Record<number, string> = {
    1: 'Opción Única',
    2: 'Opción Múltiple',
    3: 'Sí/No',
    4: 'Calificación'
  }
  return labels[type] || 'Desconocido'
}

// Estado de Votaciones
export const PollStatusLabel = (status: number): string => {
  const labels: Record<number, string> = {
    1: 'Borrador',
    2: 'Programada',
    3: 'Activa',
    4: 'Cerrada',
    5: 'Cancelada',
    6: 'Pendiente Aprobación'
  }
  return labels[status] || 'Desconocido'
}

export const PollStatusColor = (status: number): string => {
  const colors: Record<number, string> = {
    1: 'grey',
    2: 'blue',
    3: 'green',
    4: 'red',
    5: 'orange',
    6: 'warning'
  }
  return colors[status] || 'grey'
}
