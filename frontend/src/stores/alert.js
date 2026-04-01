import { defineStore } from 'pinia'
import { api } from '../boot/api'
import { AlertStatusLabel, AlertTypeLabel } from '../utils/appEnums'

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

    async fetchActive(condominiumId) {
      this.loading = true
      try {
        const response = await api.get(`/condominiums/${condominiumId}/alerts/active`)
        this.alerts = response.data
        return this.alerts
      } catch (error) {
        this.error = error.message
        return []
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

    async panicButton(condominiumId, data) {
      this.loading = true
      try {
        const response = await api.post(`/condominiums/${condominiumId}/alerts/panic`, data)
        return response.data.id
      } catch (error) {
        this.error = error.message
        return null
      } finally {
        this.loading = false
      }
    },

    async approve(alertId) {
      try {
        await api.post(`/alerts/${alertId}/approve`)
        return true
      } catch (error) {
        return false
      }
    },

    async resend(alertId) {
      try {
        await api.post(`/alerts/${alertId}/resend`)
        return true
      } catch (error) {
        return false
      }
    },

    async acknowledge(alertId) {
      try {
        await api.post(`/alerts/${alertId}/acknowledge`)
        return true
      } catch (error) {
        return false
      }
    },

    async resolve(alertId, notes) {
      try {
        await api.post(`/alerts/${alertId}/resolve`)
        return true
      } catch (error) {
        return false
      }
    },

    async remove(alertId) {
      try {
        await api.delete(`/alerts/${alertId}`)
        return true
      } catch (error) {
        return false
      }
    },

    getStatusLabel(status) {
      return AlertStatusLabel(status)
    },

    getTypeLabel(type) {
      return AlertTypeLabel(type)
    }
  }
})
