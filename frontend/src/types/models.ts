export interface Post {
  id: string
  condominiumId?: string
  authorId: string
  authorName?: string
  title: string
  content: string
  category: number
  isPinned: boolean
  isAnnouncement: boolean
  status: number
  createdAt: string
  updatedAt?: string
  viewCount?: number
}

export interface Incident {
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

export interface Alert {
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

export interface Poll {
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
