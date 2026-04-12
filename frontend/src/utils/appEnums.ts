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

export const PollTypeLabel = (type: number): string => {
  const labels: Record<number, string> = {
    1: 'Opción Única',
    2: 'Opción Múltiple',
    3: 'Sí/No',
    4: 'Calificación'
  }
  return labels[type] || 'Desconocido'
}

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

export const IncidentTypeLabel = (type: number): string => {
  const labels: Record<number, string> = {
    1: 'Mantenimiento',
    2: 'Seguridad',
    3: 'Limpieza',
    4: 'Infraestructura',
    5: 'Otro'
  }
  return labels[type] || 'Otro'
}

export const IncidentTypeIcon = (type: number): string => {
  const icons: Record<number, string> = {
    1: 'build',
    2: 'security',
    3: 'cleaning_services',
    4: 'infrastructure',
    5: 'more_horiz'
  }
  return icons[type] || 'report_problem'
}

export const IncidentPriorityLabel = (priority: number): string => {
  const labels: Record<number, string> = {
    1: 'Baja',
    2: 'Media',
    3: 'Alta',
    4: 'Urgente'
  }
  return labels[priority] || 'Media'
}

export const IncidentPriorityColor = (priority: number): string => {
  const colors: Record<number, string> = {
    1: 'green',
    2: 'yellow',
    3: 'orange',
    4: 'red'
  }
  return colors[priority] || 'yellow'
}

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

export const AlertStatusLabel = (status: number): string => {
  const labels: Record<number, string> = {
    1: 'Pendiente',
    2: 'En Camino',
    3: 'Llegó',
    4: 'Completada',
    5: 'Cancelada'
  }
  return labels[status] || 'Desconocido'
}

export const AlertStatusColor = (status: number): string => {
  const colors: Record<number, string> = {
    1: 'red',
    2: 'blue',
    3: 'orange',
    4: 'green',
    5: 'grey'
  }
  return colors[status] || 'grey'
}

export const AlertTypeLabel = (type: number): string => {
  const labels: Record<number, string> = {
    1: 'Emergencia',
    2: 'Robo',
    3: 'Incendio',
    4: 'Médica',
    5: 'Otro'
  }
  return labels[type] || 'Desconocido'
}

export const AlertTypeIcon = (type: number): string => {
  const icons: Record<number, string> = {
    1: 'emergency',
    2: 'warning',
    3: 'local_fire_department',
    4: 'medical_services',
    5: 'more_horiz'
  }
  return icons[type] || 'notification_important'
}

export enum UserRole {
  Restricted = 0,
  Guard = 1,
  Neighbor = 2,
  Manager = 3,
  Administrator = 4
}

export function UserRoleColor(role: UserRole): string {
  const colors: Record<UserRole, string> = {
    [UserRole.Restricted]: 'grey',
    [UserRole.Guard]: 'deep-purple',
    [UserRole.Neighbor]: 'blue',
    [UserRole.Manager]: 'orange',
    [UserRole.Administrator]: 'red'
  }
  return colors[role] || 'grey'
}

export function UserRoleLabel(role: UserRole): string {
  const labels: Record<UserRole, string> = {
    [UserRole.Restricted]: 'Acceso Restringido',
    [UserRole.Guard]: 'Guardia',
    [UserRole.Neighbor]: 'Vecino',
    [UserRole.Manager]: 'Administrador',
    [UserRole.Administrator]: 'Admin'
  }
  return labels[role] || 'Desconocido'
}

export function getCredibilityColor(level: number): string {
  if (level >= 4) return 'positive'
  if (level >= 2) return 'warning'
  return 'negative'
}