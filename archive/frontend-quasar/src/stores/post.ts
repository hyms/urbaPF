import { defineStore } from 'pinia'
import { api } from '@/boot/api'
import { Post, CreatePostRequest, UpdatePostRequest, Comment } from '@/types/models'
import { PostCategoryLabel, PostStatusLabel } from '@/utils/appEnums'


interface PostState {
  posts: Post[]
  currentPost: Post | null
  loading: boolean
  error: string | null
}

export const usePostStore = defineStore('post', {
  state: (): PostState => ({
    posts: [],
    currentPost: null,
    loading: false,
    error: null
  }),

  actions: {
    async fetchByCondominium(condominiumId: string): Promise<Post[]> {
      this.loading = true
      try {
        const response = await api.get<Post[]>(`/condominiums/${condominiumId}/posts`)
        this.posts = response.data
        return this.posts
      } catch (error: any) {
        this.error = error.message || 'Error fetching posts'
        return []
      } finally {
        this.loading = false
      }
    },

    async fetchById(id: string): Promise<Post | null> {
      this.loading = true
      try {
        const response = await api.get<Post>(`/posts/${id}`)
        this.currentPost = response.data
        return this.currentPost
      } catch (error: any) {
        this.error = error.message || 'Error fetching post'
        return null
      } finally {
        this.loading = false
      }
    },

    async create(condominiumId: string, data: CreatePostRequest): Promise<string | null> {
      this.loading = true
      try {
        const response = await api.post<{ id: string }>(`/condominiums/${condominiumId}/posts`, data)
        return response.data.id
      } catch (error: any) {
        this.error = error.message || 'Error creating post'
        return null
      } finally {
        this.loading = false
      }
    },

    async update(id: string, data: UpdatePostRequest): Promise<boolean> {
      try {
        await api.put(`/posts/${id}`, data)
        return true
      } catch (error: any) {
        this.error = error.message || 'Error updating post'
        return false
      }
    },

    async remove(id: string): Promise<boolean> {
      try {
        await api.delete(`/posts/${id}`)
        return true
      } catch (error: any) {
        this.error = error.message || 'Error deleting post'
        return false
      }
    },

    async getComments(postId: string): Promise<Comment[]> {
      try {
        const response = await api.get<Comment[]>(`/posts/${postId}/comments`)
        return response.data
      } catch (error: any) {
        this.error = error.message || 'Error fetching comments'
        return []
      }
    },

    async addComment(postId: string, content: string, parentCommentId?: string): Promise<string | null> {
      try {
        const response = await api.post<{ id: string }>(`/posts/${postId}/comments`, {
          content,
          parentCommentId
        })
        return response.data.id
      } catch (error: any) {
        this.error = error.message || 'Error adding comment'
        return null
      }
    },



    getStatusLabel(status: number): string {
      return PostStatusLabel(status)
    },

    getCategoryLabel(category: number): string {
      return PostCategoryLabel(category)
    }
  }
})
