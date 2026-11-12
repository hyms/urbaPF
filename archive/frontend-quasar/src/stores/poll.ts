import { defineStore } from 'pinia'
import { api } from '@/boot/api'
import { Poll, CreatePollRequest, UpdatePollRequest, VoteResult } from '@/types/models'
import { PollStatusLabel, PollTypeLabel } from '../utils/appEnums'

interface PollState {
  polls: Poll[]
  currentPoll: Poll | null
  loading: boolean
  error: string | null
}

export const usePollStore = defineStore('poll', {
  state: (): PollState => ({
    polls: [],
    currentPoll: null,
    loading: false,
    error: null
  }),

  actions: {
    async fetchByCondominium(condominiumId: string): Promise<Poll[]> {
      this.loading = true
      try {
        const response = await api.get<Poll[]>(`/condominiums/${condominiumId}/polls`)
        this.polls = response.data
        return this.polls
      } catch (error: any) {
        this.error = error.message || 'Error fetching polls'
        return []
      } finally {
        this.loading = false
      }
    },

    async fetchById(id: string): Promise<Poll | null> {
      this.loading = true
      try {
        const response = await api.get<Poll>(`/polls/${id}`)
        this.currentPoll = response.data
        return this.currentPoll
      } catch (error: any) {
        this.error = error.message || 'Error fetching poll'
        return null
      } finally {
        this.loading = false
      }
    },

    async create(condominiumId: string, data: CreatePollRequest): Promise<string | null> {
      this.loading = true
      try {
        const response = await api.post<{ id: string }>(`/condominiums/${condominiumId}/polls`, data)
        return response.data.id
      } catch (error: any) {
        this.error = error.message || 'Error creating poll'
        return null
      } finally {
        this.loading = false
      }
    },

    async update(id: string, data: UpdatePollRequest): Promise<boolean> {
      try {
        await api.put(`/polls/${id}`, data)
        return true
      } catch (error: any) {
        this.error = error.message || 'Error updating poll'
        return false
      }
    },

    async vote(pollId: string, optionIndex: number): Promise<any> {
      try {
        const response = await api.post(`/polls/${pollId}/vote`, { optionIndex })
        return response.data
      } catch (error: any) {
        this.error = error.message || 'Error voting'
        return null
      }
    },

    async getResults(pollId: string): Promise<VoteResult | null> {
      try {
        const response = await api.get<VoteResult>(`/polls/${pollId}/votes`)
        return response.data
      } catch (error: any) {
        this.error = error.message || 'Error fetching results'
        return null
      }
    },

    async remove(id: string): Promise<boolean> {
      try {
        await api.delete(`/polls/${id}`)
        return true
      } catch (error: any) {
        this.error = error.message || 'Error deleting poll'
        return false
      }
    },

    getStatusLabel(status: number): string {
      return PollStatusLabel(status)
    },

    getTypeLabel(type: number): string {
      return PollTypeLabel(type)
    }
  }
})
