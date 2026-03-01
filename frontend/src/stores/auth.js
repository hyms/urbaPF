import { defineStore } from 'pinia'
import { api } from '../boot/api'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: JSON.parse(localStorage.getItem('user') || 'null'),
    token: localStorage.getItem('token') || null,
    loading: false,
    error: null
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
    currentUser: (state) => state.user,
    isAdmin: (state) => state.user?.role === 4,
    isManager: (state) => state.user?.role === 3,
    isNeighbor: (state) => state.user?.role === 2,
    isGuard: (state) => state.user?.role === 1
  },

  actions: {
    async login(email, password) {
      this.loading = true
      this.error = null
      try {
        const response = await api.post('/auth/login', { email, password })
        this.token = response.data.token
        localStorage.setItem('token', this.token)
        
        const payload = JSON.parse(atob(this.token.split('.')[1]))
        this.user = {
          id: payload.id,
          email: payload.email,
          role: parseInt(payload.role),
          fullName: payload.fullName
        }
        localStorage.setItem('user', JSON.stringify(this.user))
        
        return true
      } catch (error) {
        this.error = error.response?.data?.message || 'Error al iniciar sesión'
        return false
      } finally {
        this.loading = false
      }
    },

    async register(email, password, fullName, phone) {
      this.loading = true
      this.error = null
      try {
        await api.post('/auth/register', { email, password, fullName, phone })
        return true
      } catch (error) {
        this.error = error.response?.data?.error || 'Error al registrarse'
        return false
      } finally {
        this.loading = false
      }
    },

    logout() {
      this.token = null
      this.user = null
      localStorage.removeItem('token')
      localStorage.removeItem('user')
    }
  }
})
