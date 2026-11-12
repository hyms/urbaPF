import { defineStore } from 'pinia'
import { api } from '../boot/api'

export interface Notification {
  id: string
  type: string
  title: string
  message: string
  read: boolean
  createdAt: string
  data?: Record<string, string>
}

export const useNotificationStore = defineStore('notification', {
  state: () => ({
    notifications: [] as Notification[],
    loading: false,
    unreadCount: 0
  }),

  getters: {
    unreadNotifications: (state) => state.notifications.filter(n => !n.read),
    hasUnread: (state) => state.unreadCount > 0
  },

  actions: {
    async fetchNotifications() {
      this.loading = true
      try {
        const response = await api.get<Notification[]>('/api/notifications')
        this.notifications = response.data
        this.unreadCount = response.data.filter(n => !n.read).length
      } catch (error) {
        console.error('Error fetching notifications:', error)
      } finally {
        this.loading = false
      }
    },

    async markAsRead(notificationId: string) {
      try {
        await api.put(`/api/notifications/${notificationId}/read`)
        const notification = this.notifications.find(n => n.id === notificationId)
        if (notification && !notification.read) {
          notification.read = true
          this.unreadCount = Math.max(0, this.unreadCount - 1)
        }
      } catch (error) {
        console.error('Error marking notification as read:', error)
      }
    },

    async markAllAsRead() {
      try {
        await api.put('/api/notifications/read-all')
        this.notifications.forEach(n => n.read = true)
        this.unreadCount = 0
      } catch (error) {
        console.error('Error marking all notifications as read:', error)
      }
    },

    addNotification(notification: Notification) {
      this.notifications.unshift(notification)
      if (!notification.read) {
        this.unreadCount++
      }
    },

    clearNotifications() {
      this.notifications = []
      this.unreadCount = 0
    }
  }
})