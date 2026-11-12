import { setActivePinia, createPinia } from 'pinia'
import { describe, it, expect, beforeEach, vi } from 'vitest'

const { mockApiGet, mockApiPost, mockApiPut, mockApiDelete } = vi.hoisted(() => ({
  mockApiGet: vi.fn(),
  mockApiPost: vi.fn(),
  mockApiPut: vi.fn(),
  mockApiDelete: vi.fn()
}))

vi.mock('../boot/api', () => ({
  api: {
    get: mockApiGet,
    post: mockApiPost,
    put: mockApiPut,
    delete: mockApiDelete
  }
}))

import { useUserStore } from './user'

describe('user store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('should have initial state', () => {
    const store = useUserStore()
    expect(store.users).toEqual([])
    expect(store.currentUser).toBeNull()
    expect(store.loading).toBe(false)
    expect(store.error).toBeNull()
  })
})
