import { defineStore } from 'pinia'
import { api } from '@/boot/api'
import { User, CreateUserRequest, UpdateUserRequest, ChangePasswordRequest } from '@/types/models'

interface UserState {
  users: User[]
  currentUser: User | null
  neighbors: User[]
  neighborDetails: User | null
  loading: boolean
  error: string | null
}

export const useUserStore = defineStore('user', {
  state: (): UserState => ({
    users: [],
    currentUser: null,
    neighbors: [],
    neighborDetails: null,
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
    },

    async create(data: CreateUserRequest): Promise<string | null> {
      this.loading = true;
      try {
        const response = await api.post<{ id: string }>('/users', data);
        return response.data.id;
      } catch (error) {
        this.error = (error as Error).message;
        return null;
      } finally {
        this.loading = false;
      }
    },

    async update(id: string, data: UpdateUserRequest): Promise<boolean> {
      this.loading = true;
      try {
        await api.put(`/users/${id}`, data);
        return true;
      } catch (error) {
        this.error = (error as Error).message;
        return false;
      } finally {
        this.loading = false;
      }
    },

    async changePassword(id: string, data: ChangePasswordRequest): Promise<boolean> {
      this.loading = true;
      try {
        await api.patch(`/users/${id}/password`, data);
        return true;
      } catch (error) {
        this.error = (error as Error).message;
        return false;
      } finally {
        this.loading = false;
      }
    },

    async remove(id: string): Promise<boolean> {
      this.loading = true;
      try {
        await api.delete(`/users/${id}`);
        return true;
      } catch (error) {
        this.error = (error as Error).message;
        return false;
      } finally {
        this.loading = false;
      }
    },

    async fetchNeighbors(condominiumId: string): Promise<User[]> {
      this.loading = true
      try {
        const response = await api.get(`/condominiums/${condominiumId}/neighbors`)
        this.neighbors = response.data
        return response.data
      } catch (error) {
        this.error = (error as Error).message
        return []
      } finally {
        this.loading = false
      }
    },

    async fetchUserDetails(userId: string): Promise<User | null> {
      this.loading = true
      try {
        const response = await api.get(`/users/${userId}/details`)
        this.neighborDetails = response.data
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
