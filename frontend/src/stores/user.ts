import { defineStore } from 'pinia'
import { api } from '../boot/api.ts'

export interface User {
  id: string
  email: string
  fullName: string
  phone?: string
  role: number
  credibilityLevel: number
  status: number
  condominiumId?: string
  lotNumber?: string
  streetAddress?: string
  photoUrl?: string
  createdAt: string
  lastLoginAt?: string
  isValidated: boolean
  managerVotes: number
}

export interface CreateUserRequest {
  email: string
  password: string
  fullName: string
  phone?: string
  role?: number
  streetAddress?: string
  photoUrl?: string
}

export interface UpdateUserRequest {
  fullName?: string
  phone?: string
  streetAddress?: string
  photoUrl?: string
  role?: number
}

export const useUserStore = defineStore('user', {
  state: () => ({
    users: [] as User[],
    currentUser: null as User | null,
    loading: false,
    error: null as string | null
  }),

  getters: {
    getUserById: (state) => (id: string) => state.users.find(u => u.id === id)
  },

  actions: {
    async fetchAll(): Promise<User[]> {
      this.loading = true
      this.error = null
      try {
        const response = await api.get<User[]>('/users')
        this.users = response.data
        return this.users
      } catch (error: any) {
        this.error = error.response?.data?.error || 'Error al cargar usuarios'
        return []
      } finally {
        this.loading = false
      }
    },

    async fetchById(id: string): Promise<User | null> {
      this.loading = true
      this.error = null
      try {
        const response = await api.get<User>(`/users/${id}`)
        this.currentUser = response.data
        return this.currentUser
      } catch (error: any) {
        this.error = error.response?.data?.error || 'Error al cargar usuario'
        return null
      } finally {
        this.loading = false
      }
    },

    async create(data: CreateUserRequest): Promise<string | null> {
      this.loading = true
      this.error = null
      try {
        const response = await api.post<{ id: string }>('/users', data)
        return response.data.id
      } catch (error: any) {
        this.error = error.response?.data?.error || 'Error al crear usuario'
        return null
      } finally {
        this.loading = false
      }
    },

    async update(id: string, data: UpdateUserRequest): Promise<boolean> {
      this.loading = true
      this.error = null
      try {
        await api.put(`/users/${id}`, data)
        return true
      } catch (error: any) {
        this.error = error.response?.data?.error || 'Error al actualizar usuario'
        return false
      } finally {
        this.loading = false
      }
    },

    async remove(id: string): Promise<boolean> {
      this.loading = true
      this.error = null
      try {
        await api.delete(`/users/${id}`)
        this.users = this.users.filter(u => u.id !== id)
        return true
      } catch (error: any) {
        this.error = error.response?.data?.error || 'Error al eliminar usuario'
        return false
      } finally {
        this.loading = false
      }
    },

    async uploadPhoto(userId: string, file: File): Promise<{ success: boolean; photoUrl: string; message?: string }> {
      try {
        const formData = new FormData()
        formData.append('file', file)
        
        const response = await api.post<{ message: string; userId: string }>(
          `/users/${userId}/photo`,
          formData,
          {
            headers: {
              'Content-Type': 'multipart/form-data'
            }
          }
        )
        return { success: true, photoUrl: response.data.message }
      } catch (error: any) {
        return { success: false, photoUrl: '', message: error.response?.data?.error || 'Error al subir foto' }
      }
    },

    async changePassword(userId: string, oldPassword: string, newPassword: string): Promise<{ success: boolean; message: string }> {
      try {
        await api.put(`/users/${userId}/password`, {
          oldPassword,
          newPassword
        })
        return { success: true, message: 'Contraseña actualizada' }
      } catch (error: any) {
        return { success: false, message: error.response?.data?.message || 'Error al cambiar contraseña' }
      }
    },

    getRoleLabel(role: number): string {
      const labels: Record<number, string> = {
        4: 'Administrador',
        3: 'Encargado',
        1: 'Guardia',
        2: 'Vecino',
        0: 'Acceso Restringido'
      }
      return labels[role] || 'Desconocido'
    },

    getRoleColor(role: number): string {
      const colors: Record<number, string> = {
        4: 'purple',
        3: 'blue',
        1: 'teal',
        2: 'grey',
        0: 'orange'
      }
      return colors[role] || 'grey'
    }
  }
})
