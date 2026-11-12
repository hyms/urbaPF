import { defineStore } from 'pinia'
import { api } from '@/boot/api'
import { Incident, CreateIncidentRequest, UpdateIncidentRequest } from '@/types/models'
import { IncidentStatusLabel, IncidentPriorityLabel } from '../utils/appEnums'

interface IncidentState {
  incidents: Incident[]
  currentIncident: Incident | null
  loading: boolean
  error: string | null
}

export const useIncidentStore = defineStore('incident', {
  state: (): IncidentState => ({
    incidents: [],
    currentIncident: null,
    loading: false,
    error: null
  }),

  actions: {
    async fetchByCondominium(condominiumId: string, status: number | null = null): Promise<Incident[]> {
      this.loading = true
      try {
        const url = status 
          ? `/condominiums/${condominiumId}/incidents?status=${status}`
          : `/condominiums/${condominiumId}/incidents`
        const response = await api.get<Incident[]>(url)
        this.incidents = response.data
        return this.incidents
      } catch (error: any) {
        this.error = error.message
        return []
      } finally {
        this.loading = false
      }
    },

    async fetchById(id: string): Promise<Incident | null> {
      this.loading = true
      try {
        const response = await api.get<Incident>(`/incidents/${id}`)
        this.currentIncident = response.data
        return this.currentIncident
      } catch (error: any) {
        this.error = error.message
        return null
      } finally {
        this.loading = false
      }
    },

    async create(condominiumId: string, data: CreateIncidentRequest): Promise<string | null> {
      this.loading = true
      try {
        const response = await api.post<{ id: string }>(`/condominiums/${condominiumId}/incidents`, data)
        return response.data.id
      } catch (error: any) {
        this.error = error.message
        return null
      } finally {
        this.loading = false
      }
    },

    async updateStatus(id: string, status: number, resolutionNotes: string | null = null): Promise<boolean> {
      try {
        await api.put(`/incidents/${id}/status`, { status, resolutionNotes })
        return true
      } catch (error: any) {
        return false
      }
    },

    async update(id: string, data: UpdateIncidentRequest): Promise<boolean> {
      this.loading = true
      try {
        await api.put(`/incidents/${id}`, data)
        return true
      } catch (error: any) {
        this.error = error.message
        return false
      } finally {
        this.loading = false
      }
    },

    async remove(id: string): Promise<boolean> {
      try {
        await api.delete(`/incidents/${id}`)
        return true
      } catch (error: any) {
        return false
      }
    },

    getStatusLabel(status: number): string {
      return IncidentStatusLabel(status)
    },

    getPriorityLabel(priority: number): string {
      return IncidentPriorityLabel(priority)
    }
  }
})
