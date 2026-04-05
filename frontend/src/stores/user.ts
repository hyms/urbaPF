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
        return this.users
      } catch (error: any) {
        this.error = error.message
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
        return this.currentUser
      } catch (error: any) {
        this.error = error.message
        return null
      } finally {
        this.loading = false
      }
    },

    async create(data: any): Promise<string | null> {
      this.loading = true
      try {
        const response = await api.post('/users', data)
        return response.data.id
      } catch (error: any) {
        this.error = error.message
        return null
      } finally {
        this.loading = false
      }
    },

    async update(id: string, data: any): Promise<boolean> {
      this.loading = true
      try {
        const updateData = {
          fullName: data.fullName,
          phone: data.phone,
          role: data.role,
          streetAddress: data.address,
          photoUrl: data.photoUrl
        };
        await api.put(`/users/${id}`, updateData);
        if (this.currentUser && this.currentUser.id === id) {
          this.currentUser = { ...this.currentUser, ...updateData };
        }
        return true;
      } catch (error: any) {
        this.error = error.response?.data?.message || 'Error al actualizar usuario';
        return false;
      } finally {
        this.loading = false;
      }
    },

    async changePassword(id: string, oldPassword: string, newPassword: string): Promise<{ success: boolean; message: string }> {
      this.loading = true;
      this.error = null;
      try {
        await api.put(`/users/${id}/password`, { oldPassword, newPassword });
        return { success: true, message: 'Contraseña actualizada con éxito' };
      } catch (error: any) {
        this.error = error.response?.data?.message || 'Error al cambiar contraseña';
        return { success: false, message: this.error };
      } finally {
        this.loading = false;
      }
    },

    async uploadPhoto(id: string, file: File): Promise<{ success: boolean; photoUrl?: string; message: string }> {
      this.loading = true;
      this.error = null;
      try {
        const formData = new FormData();
        formData.append('file', file);
        const response = await api.post(`/users/${id}/photo`, formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        });
        const photoUrl = response.data.photoUrl;
        if (this.currentUser && this.currentUser.id === id) {
          this.currentUser.photoUrl = photoUrl;
        }
        return { success: true, photoUrl: photoUrl, message: 'Foto de perfil actualizada' };
      } catch (error: any) {
        this.error = error.response?.data?.message || 'Error al subir la foto';
        return { success: false, message: this.error };
      } finally {
        this.loading = false;
      }
    },

    async remove(id: string): Promise<boolean> {
      try {
        await api.delete(`/users/${id}`)
        return true
      } catch (error: any) {
        return false
      }
    },

    getRoleLabel(role: number): string {
      const labels: Record<number, string> = {
        0: 'Restricted',
        1: 'Guard',
        2: 'Neighbor',
        3: 'Manager',
        4: 'Administrator'
      };
      return labels[role] || 'Desconocido';
    },

    getRoleColor(role: number): string {
      const colors: Record<number, string> = {
        0: 'grey', // Restricted
        1: 'blue-grey', // Guard
        2: 'primary', // Neighbor
        3: 'teal', // Manager
        4: 'purple' // Administrator
      };
      return colors[role] || 'grey';
    }
  }
})
