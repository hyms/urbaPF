import { defineStore } from 'pinia'
import { api } from '../boot/api'

export const useAlertStore = defineStore('alert', {
  state: () => ({
    alerts: [],
    currentAlert: null,
    loading: false,
    error: null
  }),

  actions: {
    async fetchByCondominium(condominiumId) {
      this.loading = true
      try {
        const response = await api.get(`/condominiums/${condominiumId}/alerts`)
        this.alerts = response.data
        return this.alerts
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
        const response = await api.get(`/alerts/${id}`)
        this.currentAlert = response.data
        return this.currentAlert
      } catch (error) {
        this.error = error.message
        return null
      } finally {
        this.loading = false
      }
    },

    async create(condominiumId, data) {
      this.loading = true
      try {
        const response = await api.post(`/condominiums/${condominiumId}/alerts`, data)
        return response.data.id
      } catch (error) {
        this.error = error.message
        return null
      } finally {
        this.loading = false
      }
    },

    async updateStatus(id, status) {
      try {
        await api.put(`/alerts/${id}/status`, { status })
        return true
      } catch (error) {
        return false
      }
    },

    async remove(id) {
      try {
        await api.delete(`/alerts/${id}`)
        return true
      } catch (error) {
        return false
      }
    },

    getStatusLabel(status) {
      const labels = {
        1: 'Activa',
        2: 'Acknowledged',
        3: 'En camino',
        4: 'Llegó',
        5: 'Completada',
        6: 'Cancelada'
      }
      return labels[status] || 'Desconocido'
    },

    getTypeLabel(type) {
      const labels = {
        1: 'Emergencia',
        2: 'Información',
        3: 'Advertencia',
        4: 'Otro'
      }
      return labels[type] || 'Desconocido'
    }
  }
})
