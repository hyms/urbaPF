import { defineStore } from 'pinia'
import { api } from '../boot/api'

export const useIncidentStore = defineStore('incident', {
  state: () => ({
    incidents: [],
    currentIncident: null,
    loading: false,
    error: null
  }),

  actions: {
    async fetchByCondominium(condominiumId, status = null) {
      this.loading = true
      try {
        const url = status 
          ? `/condominiums/${condominiumId}/incidents?status=${status}`
          : `/condominiums/${condominiumId}/incidents`
        const response = await api.get(url)
        this.incidents = response.data
        return this.incidents
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
        const response = await api.get(`/incidents/${id}`)
        this.currentIncident = response.data
        return this.currentIncident
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
        const response = await api.post(`/condominiums/${condominiumId}/incidents`, data)
        return response.data.id
      } catch (error) {
        this.error = error.message
        return null
      } finally {
        this.loading = false
      }
    },

    async updateStatus(id, status, resolutionNotes = null) {
      try {
        await api.put(`/incidents/${id}/status`, { status, resolutionNotes })
        return true
      } catch (error) {
        return false
      }
    },

    async update(id, data) {
      this.loading = true
      try {
        await api.put(`/incidents/${id}`, data)
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
        await api.delete(`/incidents/${id}`)
        return true
      } catch (error) {
        return false
      }
    },

    getStatusLabel(status) {
      const labels = {
        1: 'Reportado',
        2: 'En proceso',
        3: 'Pendiente',
        4: 'Resuelto',
        5: 'Cerrado',
        6: 'Cancelado'
      }
      return labels[status] || 'Desconocido'
    },

    getPriorityLabel(priority) {
      const labels = {
        1: 'Baja',
        2: 'Media',
        3: 'Alta',
        4: 'Urgente'
      }
      return labels[priority] || 'Desconocido'
    }
  }
})
