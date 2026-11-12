import { defineStore } from 'pinia'
import { api } from '@/boot/api'
import { Condominium, CreateCondominiumRequest, UpdateCondominiumRequest } from '@/types/models'

interface CondominiumState {
  condominiums: Condominium[]
  currentCondominium: Condominium | null
  loading: boolean
  error: string | null
}

export const useCondominiumStore = defineStore('condominium', {
  state: (): CondominiumState => ({
    condominiums: [],
    currentCondominium: null,
    loading: false,
    error: null
  }),

  actions: {
    async fetchAll(): Promise<Condominium[]> {
      this.loading = true
      try {
        const response = await api.get<Condominium[]>('/condominiums')
        this.condominiums = response.data
        return this.condominiums
      } catch (error: any) {
        this.error = error.message
        return []
      } finally {
        this.loading = false
      }
    },

    async fetchById(id: string): Promise<Condominium | null> {
      this.loading = true
      try {
        const response = await api.get<Condominium>(`/condominiums/${id}`)
        this.currentCondominium = response.data
        return this.currentCondominium
      } catch (error: any) {
        this.error = error.message
        return null
      } finally {
        this.loading = false
      }
    },

    async create(data: CreateCondominiumRequest): Promise<string | null> {
      this.loading = true
      try {
        const response = await api.post<{ id: string }>('/condominiums', data)
        return response.data.id
      } catch (error: any) {
        this.error = error.message
        return null
      } finally {
        this.loading = false
      }
    },

    async update(id: string, data: UpdateCondominiumRequest): Promise<boolean> {
      this.loading = true
      try {
        await api.put(`/condominiums/${id}`, data)
        return true
      } catch (error: any) {
        this.error = error.message
        return false
      } finally {
        this.loading = false
      }
    },

    async remove(id: string): Promise<boolean> {
      this.loading = true
      try {
        await api.delete(`/condominiums/${id}`)
        return true
      } catch (error: any) {
        this.error = error.message
        return false
      } finally {
        this.loading = false
      }
    },

    async setCurrentCondominium(id: string): Promise<void> {
      localStorage.setItem('currentCondoId', id)
      await this.fetchById(id)
    },

    loadCurrentCondominiumFromStorage(): void {
      const id = localStorage.getItem('currentCondoId')
      if (id && this.condominiums.length > 0) {
        this.currentCondominium = this.condominiums.find(c => c.id === id) || null
      }
    }
  }
})
