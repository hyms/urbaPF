export interface User {
  id: string
  email: string
  fullName: string
  phone?: string
  photoUrl?: string
  role: number
  credibilityLevel: number
  status: number
  condominiumId?: string
  lotNumber?: string
  streetAddress?: string
  isValidated: boolean
  managerVotes: number
  createdAt: string
  lastLoginAt?: string
}

export interface Alert {
  id: string
  condominiumId: string
  creatorId: string
  type: number
  title: string
  description?: string
  lat?: number
  lng?: number
  status: number
  reputationLevel: number
  needsApproval: boolean
  createdAt: string
}

export interface CreateAlertRequest {
  type: number
  title: string
  description?: string
  lat?: number
  lng?: number
}

export interface Post {
  id: string
  condominiumId?: string
  authorId: string
  authorName?: string
  title: string
  content: string
  category?: number
  isPinned?: boolean
  isAnnouncement?: boolean
  status: number
  createdAt: string
  updatedAt?: string
  viewCount?: number
}

export interface CreatePostRequest {
  title: string
  content: string
  category: number
  isPinned: boolean
  isAnnouncement: boolean
}

export interface UpdatePostRequest {
  title?: string
  content?: string
  category?: number
  isPinned?: boolean
  isAnnouncement?: boolean
  status?: number
}

export interface Comment {
  id: string
  postId: string
  authorId: string
  authorName?: string
  content: string
  parentCommentId?: string
  createdAt: string
  credibilityLevel?: number
}

export interface Incident {
  id: string
  condominiumId: string
  reporterId: string
  title: string
  description?: string
  type: number
  priority: number
  status: number
  location?: string
  addressReference?: string
  media?: any
  resolutionNotes?: string
  resolvedAt?: string
  closedAt?: string
  createdAt: string
  updatedAt?: string
  deletedAt?: string
}

export interface CreateIncidentRequest {
  title: string
  description?: string
  type: number
  location?: string
  addressReference?: string
  media?: any
}

export interface UpdateIncidentRequest {
  title: string
  description?: string
  type: number
  location?: string
  addressReference?: string
  media?: any
}

export interface Condominium {
  id: string
  name: string
  address: string
  logoUrl?: string
  description?: string
  rules?: string
  monthlyFee: number
  currency: string
  isActive: boolean
  createdAt: string
  updatedAt?: string
  deletedAt?: string
}

export interface CreateCondominiumRequest {
  name: string
  address: string
  logoUrl?: string
  description?: string
  rules?: string
  monthlyFee: number
  currency: string
}

export interface UpdateCondominiumRequest {
  name?: string
  address?: string
  logoUrl?: string
  description?: string
  rules?: string
  monthlyFee?: number
  currency?: string
  isActive?: boolean
}

export interface Poll {
  id: string
  condominiumId: string
  title: string
  description?: string
  options: string
  pollType: number
  startsAt: string
  endsAt: string
  requiresJustification: boolean
  minRoleToVote: number
  status: number
  createdAt: string
  createdById: string
  createdByName?: string
}

export interface CreatePollRequest {
  title: string
  description?: string
  options: string
  pollType: number
  startsAt: string
  endsAt: string
  requiresJustification: boolean
  minRoleToVote: number
}

export interface UpdatePollRequest {
  title?: string
  description?: string
  options?: string
  startsAt?: string
  endsAt?: string
  status?: number
}

export interface VoteResult {
  votes: PollVote[]
  results: Record<number, number>
}

export interface PollVote {
  id: string
  pollId: string
  userId: string
  userName?: string
  optionIndex: number
  digitalSignature: string
  votedAt: string
}

export interface PollResults {
  results: number[]
  votersCount: number
  signature?: string
}
