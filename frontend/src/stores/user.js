import { defineStore } from 'pinia'
import { api } from '../boot/api'

export const useUserStore = defineStore('user', {
  state: () => ({
    users: [],
    currentUser: null,
    loading: false,
    error: null
  }),

  actions: {
    async fetchAll() {
      this.loading = true
      try {
        const response = await api.get('/users')
        this.users = response.data
        return this.users
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
        const response = await api.get(`/users/${id}`)
        this.currentUser = response.data
        return this.currentUser
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
        const response = await api.post('/users', data)
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
        const updateData = {
          fullName: data.fullName,
          phone: data.phone,
          role: data.role,
          streetAddress: data.address, // Mapear 'address' del frontend a 'streetAddress' del backend
          photoUrl: data.photoUrl
        };
        await api.put(`/users/${id}`, updateData);
        // Actualizar el currentUser si el id coincide
        if (this.currentUser && this.currentUser.id === id) {
          this.currentUser = { ...this.currentUser, ...updateData };
        }
        return true;
      } catch (error) {
        this.error = error.response?.data?.message || 'Error al actualizar usuario';
        return false;
      } finally {
        this.loading = false;
      }
    },

    async changePassword(id, oldPassword, newPassword) {
      this.loading = true;
      this.error = null;
      try {
        await api.put(`/users/${id}/password`, { oldPassword, newPassword });
        return { success: true, message: 'Contraseña actualizada con éxito' };
      } catch (error) {
        this.error = error.response?.data?.message || 'Error al cambiar contraseña';
        return { success: false, message: this.error };
      } finally {
        this.loading = false;
      }
    },

    async uploadPhoto(id, file) {
      this.loading = true;
      this.error = null;
      try {
        const formData = new FormData();
        formData.append('file', file);
        const response = await api.post(`/users/${id}/photo`, formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        });
        const photoUrl = response.data.photoUrl;
        // Actualizar el currentUser si el id coincide
        if (this.currentUser && this.currentUser.id === id) {
          this.currentUser.photoUrl = photoUrl;
        }
        return { success: true, photoUrl: photoUrl, message: 'Foto de perfil actualizada' };
      } catch (error) {
        this.error = error.response?.data?.message || 'Error al subir la foto';
        return { success: false, message: this.error };
      } finally {
        this.loading = false;
      }
    },

    async remove(id) {
      try {
        await api.delete(`/users/${id}`)
        return true
      } catch (error) {
        return false
      }
    },

    getRoleLabel(role) {
      const labels = {
        0: 'Restricted',
        1: 'Guard',
        2: 'Neighbor',
        3: 'Manager',
        4: 'Administrator'
      };
      return labels[role] || 'Desconocido';
    }
  }
})
