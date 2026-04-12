import { defineStore } from 'pinia'
import { api } from '@/boot/api'
import { Alert, CreateAlertRequest } from '@/types/models'
import { AlertStatusLabel } from '@/utils/appEnums'

interface AlertState {
  alerts: Alert[]
  currentAlert: Alert | null
  loading: boolean
  error: string | null
}

export const useAlertStore = defineStore('alert', {
  state: (): AlertState => ({
    alerts: [],
    currentAlert: null,
    loading: false,
    error: null
  }),

  actions: {
    async fetchByCondominium(condominiumId: string): Promise<Alert[]> {
      this.loading = true
      try {
        const response = await api.get(`/condominiums/${condominiumId}/alerts`)
        this.alerts = response.data
        return this.alerts
      } catch (error: any) {
        this.error = error.message
        return []
      } finally {
        this.loading = false
      }
    },

    async fetchActive(condominiumId: string): Promise<Alert[]> {
      this.loading = true
      try {
        const response = await api.get(`/condominiums/${condominiumId}/alerts/active`)
        this.alerts = response.data
        return this.alerts
      } catch (error: any) {
        this.error = error.message
        return []
      } finally {
        this.loading = false
      }
    },

    async create(condominiumId: string, data: CreateAlertRequest): Promise<string | null> {
      this.loading = true
      try {
        const response = await api.post(`/condominiums/${condominiumId}/alerts`, data)
        return response.data.id
      } catch (error: any) {
        this.error = error.message
        return null
      } finally {
        this.loading = false
      }
    },

    async panicButton(condominiumId: string, data: CreateAlertRequest): Promise<string | null> {
      this.loading = true
      try {
        const response = await api.post(`/condominiums/${condominiumId}/alerts/panic`, data)
        return response.data.id
      } catch (error: any) {
        this.error = error.message
        return null
      } finally {
        this.loading = false
      }
    },

    async approve(alertId: string): Promise<boolean> {
      try {
        await api.post(`/alerts/${alertId}/approve`)
        return true
      } catch (error: any) {
        return false
      }
    },

    async resend(alertId: string): Promise<boolean> {
      try {
        await api.post(`/alerts/${alertId}/resend`)
        return true
      } catch (error: any) {
        return false
      }
    },

    async acknowledge(alertId: string): Promise<boolean> {
      try {
        await api.post(`/alerts/${alertId}/acknowledge`)
        return true
      } catch (error: any) {
        return false
      }
    },

    async resolve(alertId: string): Promise<boolean> {
      try {
        await api.post(`/alerts/${alertId}/resolve`)
        return true
      } catch (error: any) {
        return false
      }
    },

    async remove(alertId: string): Promise<boolean> {
      try {
        await api.delete(`/alerts/${alertId}`)
        return true
      } catch (error: any) {
        return false
      }
    },

    getStatusLabel(status: number): string {
      return AlertStatusLabel(status)
    }
  }
})
