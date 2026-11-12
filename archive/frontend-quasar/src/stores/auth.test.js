import { setActivePinia, createPinia } from 'pinia'
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'

const { mockApiPost, mockApiGet, mockApiDelete } = vi.hoisted(() => ({
  mockApiPost: vi.fn(),
  mockApiGet: vi.fn(),
  mockApiDelete: vi.fn()
}))

vi.mock('../boot/api', () => ({
  api: {
    post: mockApiPost,
    get: mockApiGet,
    delete: mockApiDelete
  }
}))

import { useAuthStore, UserRole } from './auth'

describe('auth store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    localStorage.clear()
    vi.clearAllMocks()
  })

  afterEach(() => {
    vi.resetAllMocks()
  })

  it('should have initial state', () => {
    const store = useAuthStore()
    expect(store.user).toBeNull()
    expect(store.token).toBeNull()
    expect(store.refreshToken).toBeNull()
    expect(store.loading).toBe(false)
    expect(store.error).toBeNull()
  })

  it('should calculate isAuthenticated correctly', () => {
    const store = useAuthStore()
    expect(store.isAuthenticated).toBe(false)
    
    store.token = 'test-token'
    expect(store.isAuthenticated).toBe(false)
    
    store.user = { id: '1', email: 'test@test.com', fullName: 'Test', role: 2, credibilityLevel: 3, status: 1, createdAt: '', isValidated: true, managerVotes: 0 }
    expect(store.isAuthenticated).toBe(true)
  })

  it('should calculate isTokenExpired correctly', () => {
    const store = useAuthStore()
    expect(store.isTokenExpired).toBe(true)
    
    store.expiresAt = new Date(Date.now() + 10000).toISOString()
    expect(store.isTokenExpired).toBe(false)
    
    store.expiresAt = new Date(Date.now() - 10000).toISOString()
    expect(store.isTokenExpired).toBe(true)
  })

  it('should calculate user role getters correctly', () => {
    const store = useAuthStore()
    store.user = { id: '1', email: 'test@test.com', fullName: 'Test', role: UserRole.Admin, credibilityLevel: 3, status: 1, createdAt: '', isValidated: true, managerVotes: 0 }
    
    expect(store.isAdmin).toBe(true)
    expect(store.isManager).toBe(false)
    expect(store.isNeighbor).toBe(false)
    expect(store.isGuard).toBe(false)
    expect(store.isRestricted).toBe(false)
  })

  it('should set auth data correctly', () => {
    const store = useAuthStore()
    const authData = {
      token: 'test-token',
      refreshToken: 'refresh-token',
      expiresAt: '2026-01-01T00:00:00Z',
      user: {
        id: '1',
        email: 'test@test.com',
        fullName: 'Test User',
        role: UserRole.Admin,
        credibilityLevel: 5,
        status: 1,
        createdAt: '2026-01-01',
        isValidated: true,
        managerVotes: 0
      }
    }

    store.setAuth(authData)

    expect(store.token).toBe('test-token')
    expect(store.refreshToken).toBe('refresh-token')
    expect(store.user).toEqual(authData.user)
    expect(localStorage.getItem('token')).toBe('test-token')
  })

  it('should clear auth data correctly', () => {
    const store = useAuthStore()
    store.setAuth({
      token: 'test-token',
      refreshToken: 'refresh-token',
      expiresAt: '2026-01-01T00:00:00Z',
      user: {
        id: '1',
        email: 'test@test.com',
        fullName: 'Test User',
        role: UserRole.Admin,
        credibilityLevel: 5,
        status: 1,
        createdAt: '2026-01-01',
        isValidated: true,
        managerVotes: 0
      }
    })

    store.clearAuth()

    expect(store.token).toBeNull()
    expect(store.refreshToken).toBeNull()
    expect(store.user).toBeNull()
    expect(localStorage.getItem('token')).toBeNull()
  })

  it('should load user from storage', () => {
    const store = useAuthStore()
    const userData = {
      id: '1',
      email: 'test@test.com',
      fullName: 'Test',
      role: 2,
      credibilityLevel: 3,
      status: 1,
      createdAt: '',
      isValidated: true,
      managerVotes: 0
    }
    localStorage.setItem('user', JSON.stringify(userData))

    store.loadUserFromStorage()

    expect(store.user).toEqual(userData)
  })

  it('should handle invalid user data in storage', () => {
    const store = useAuthStore()
    localStorage.setItem('user', 'invalid-json')

    store.loadUserFromStorage()

    expect(store.user).toBeNull()
  })
})
