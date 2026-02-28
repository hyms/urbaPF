import { defineStore } from 'pinia'
import { api } from '../boot/api'

export const useCondominiumStore = defineStore('condominium', {
  state: () => ({
    currentCondominium: null,
    condominiums: [],
    loading: false,
    error: null
  }),

  actions: {
    async fetchAll() {
      this.loading = true
      try {
        const response = await api.get('/condominiums')
        this.condominiums = response.data
        return this.condominiums
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
        const response = await api.get(`/condominiums/${id}`)
        this.currentCondominium = response.data
        return this.currentCondominium
      } catch (error) {
        this.error = error.message
        return null
      } finally {
        this.loading = false
      }
    },

    async create(data) {
      this.loading = true
      try {
        const response = await api.post('/condominiums', data)
        return response.data.id
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
        await api.put(`/condominiums/${id}`, data)
        return true
      } catch (error) {
        this.error = error.message
        return false
      } finally {
        this.loading = false
      }
    },

    async remove(id) {
      this.loading = true
      try {
        await api.delete(`/condominiums/${id}`)
        return true
      } catch (error) {
        this.error = error.message
        return false
      } finally {
        this.loading = false
      }
    }
  }
})
