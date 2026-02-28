import { defineStore } from 'pinia'
import { api } from '../boot/api'

export const usePostStore = defineStore('post', {
  state: () => ({
    posts: [],
    currentPost: null,
    loading: false,
    error: null
  }),

  actions: {
    async fetchByCondominium(condominiumId) {
      this.loading = true
      try {
        const response = await api.get(`/condominiums/${condominiumId}/posts`)
        this.posts = response.data
        return this.posts
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
        const response = await api.get(`/posts/${id}`)
        this.currentPost = response.data
        return this.currentPost
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
        const response = await api.post(`/condominiums/${condominiumId}/posts`, data)
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
        await api.put(`/posts/${id}`, data)
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
        await api.delete(`/posts/${id}`)
        return true
      } catch (error) {
        this.error = error.message
        return false
      } finally {
        this.loading = false
      }
    },

    async getComments(postId) {
      try {
        const response = await api.get(`/posts/${postId}/comments`)
        return response.data
      } catch (error) {
        return []
      }
    },

    async addComment(postId, content, parentCommentId = null) {
      try {
        const response = await api.post(`/posts/${postId}/comments`, {
          content,
          parentCommentId
        })
        return response.data.id
      } catch (error) {
        return null
      }
    }
  }
})
