import { defineStore } from 'pinia'
import { api } from '../boot/api'

export const usePollStore = defineStore('poll', {
  state: () => ({
    polls: [],
    currentPoll: null,
    loading: false,
    error: null
  }),

  actions: {
    async fetchByCondominium(condominiumId) {
      this.loading = true
      try {
        const response = await api.get(`/condominiums/${condominiumId}/polls`)
        this.polls = response.data
        return this.polls
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
        const response = await api.get(`/polls/${id}`)
        this.currentPoll = response.data
        return this.currentPoll
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
        const response = await api.post(`/condominiums/${condominiumId}/polls`, data)
        return response.data.id
      } catch (error) {
        this.error = error.message
        return null
      } finally {
        this.loading = false
      }
    },

    async update(id, data) {
      try {
        await api.put(`/polls/${id}`, data)
        return true
      } catch (error) {
        return false
      }
    },

    async vote(pollId, optionIndex) {
      try {
        const response = await api.post(`/polls/${pollId}/vote`, { optionIndex })
        return response.data
      } catch (error) {
        return null
      }
    },

    async getResults(pollId) {
      try {
        const response = await api.get(`/polls/${pollId}/votes`)
        return response.data
      } catch (error) {
        return null
      }
    },

    async remove(id) {
      try {
        await api.delete(`/polls/${id}`)
        return true
      } catch (error) {
        return false
      }
    },

    getStatusLabel(status) {
      const labels = {
        1: 'Borrador',
        2: 'Activa',
        3: 'Cerrada',
        4: 'Cancelada'
      }
      return labels[status] || 'Desconocido'
    }
  }
})
