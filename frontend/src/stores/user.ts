import { defineStore } from 'pinia'
import { api } from '@/boot/api'
import { User } from '@/types/models'

interface UserState {
  users: User[]
  currentUser: User | null
  loading: boolean
  error: string | null
}

export const useUserStore = defineStore('user', {
  state: (): UserState => ({
    users: [],
    currentUser: null,
    loading: false,
    error: null
  }),

  actions: {
    async fetchAll(): Promise<User[]> {
      this.loading = true
      try {
        const response = await api.get('/users')
        this.users = response.data
        return response.data
      } catch (error) {
        this.error = (error as Error).message
        return []
      } finally {
        this.loading = false
      }
    },

    async fetchById(id: string): Promise<User | null> {
      this.loading = true
      try {
        const response = await api.get(`/users/${id}`)
        this.currentUser = response.data
        return response.data
      } catch (error) {
        this.error = (error as Error).message
        return null
      } finally {
        this.loading = false
      }
    }
  }
})
