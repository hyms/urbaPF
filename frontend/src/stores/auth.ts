import { defineStore } from 'pinia'
import { api } from '../boot/api.ts'

export interface User {
  id: string
  email: string
  fullName: string
  phone?: string
  role: number
  credibilityLevel: number
  status: number
  condominiumId?: string
  lotNumber?: string
  streetAddress?: string
  photoUrl?: string
  createdAt: string
  lastLoginAt?: string
  isValidated: boolean
  managerVotes: number
}

export const UserRole = {
  RestrictedAccess: 0,
  Neighbor: 2,
  Guard: 1,
  Manager: 3,
  Admin: 4
} as const

export interface AuthResponse {
  token: string
  refreshToken: string
  expiresAt: string
  user: User
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
  fullName: string
  phone?: string
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as User | null,
    token: localStorage.getItem('token') || null,
    refreshToken: localStorage.getItem('refreshToken') || null,
    expiresAt: localStorage.getItem('expiresAt') || null,
    loading: false,
    error: null as string | null
  }),

  getters: {
    isAuthenticated: (state) => !!state.token && !!state.user,
    isTokenExpired: (state) => {
      if (!state.expiresAt) return true
      return new Date(state.expiresAt) <= new Date()
    },
    userRole: (state) => state.user?.role ?? 0,
    isAdmin: (state) => state.user?.role === UserRole.Admin,
    isManager: (state) => state.user?.role === UserRole.Manager,
    isGuard: (state) => state.user?.role === UserRole.Guard,
    isNeighbor: (state) => state.user?.role === UserRole.Neighbor,
    isRestricted: (state) => state.user?.role === UserRole.RestrictedAccess,
    isValidated: (state) => state.user?.isValidated ?? false
  },

  actions: {
    async login(email: string, password: string): Promise<boolean> {
      this.loading = true
      this.error = null
      try {
        const response = await api.post<AuthResponse>('/auth/login', { email, password })
        this.setAuth(response.data)
        return true
      } catch (error: any) {
        this.error = error.response?.data?.error || 'Error al iniciar sesión'
        return false
      } finally {
        this.loading = false
      }
    },

    async register(data: RegisterRequest): Promise<boolean> {
      this.loading = true
      this.error = null
      try {
        await api.post('/auth/register', data)
        return true
      } catch (error: any) {
        this.error = error.response?.data?.error || 'Error al registrar usuario'
        return false
      } finally {
        this.loading = false
      }
    },

    async refreshAccessToken(): Promise<boolean> {
      if (!this.refreshToken) return false

      try {
        const response = await api.post<AuthResponse>('/auth/refresh', {
          refreshToken: this.refreshToken
        })
        this.setAuth(response.data)
        return true
      } catch (error) {
        this.logout()
        return false
      }
    },

    async logout(): Promise<void> {
      try {
        if (this.refreshToken) {
          await api.post('/auth/revoke', { refreshToken: this.refreshToken })
        }
      } catch {
      } finally {
        this.clearAuth()
      }
    },

    setAuth(auth: AuthResponse): void {
      this.token = auth.token
      this.refreshToken = auth.refreshToken
      this.expiresAt = auth.expiresAt
      this.user = auth.user

      localStorage.setItem('token', auth.token)
      localStorage.setItem('refreshToken', auth.refreshToken)
      localStorage.setItem('expiresAt', auth.expiresAt)
      localStorage.setItem('user', JSON.stringify(auth.user))
    },

    clearAuth(): void {
      this.token = null
      this.refreshToken = null
      this.expiresAt = null
      this.user = null

      localStorage.removeItem('token')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('expiresAt')
      localStorage.removeItem('user')
    },

    loadUserFromStorage(): void {
      const userStr = localStorage.getItem('user')
      if (userStr) {
        try {
          this.user = JSON.parse(userStr)
        } catch {
          this.clearAuth()
        }
      }
    }
  }
})
