import { defineStore } from 'pinia'
import { api } from '../boot/api'

export const useUserStore = defineStore('user', {
  state: () => ({
    users: [],
    currentUser: null,
    loading: false,
    error: null
  }),

  actions: {
    async fetchAll() {
      this.loading = true
      try {
        const response = await api.get('/users')
        this.users = response.data
        return this.users
      } catch (error) {
        this.error = error.message
        return []
      } finally {
        this.loading = false
      }
    },

    async fetchById(id) {
      this.loading = true
      try {
        const response = await api.get(`/users/${id}`)
        this.currentUser = response.data
        return this.currentUser
      } catch (error) {
        this.error = error.message
        return null
      } finally {
        this.loading = false
      }
    },

    async update(id, data) {
      this.loading = true
      try {
        await api.put(`/users/${id}`, data)
        return true
      } catch (error) {
        this.error = error.message
        return false
      } finally {
        this.loading = false
      }
    },

    async remove(id) {
      try {
        await api.delete(`/users/${id}`)
        return true
      } catch (error) {
        return false
      }
    },

    getRoleLabel(role) {
      const labels = {
        0: 'Administrador',
        1: 'Gerente',
        2: 'Residente'
      }
      return labels[role] || 'Desconocido'
    }
  }
})
