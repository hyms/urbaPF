import { setActivePinia, createPinia } from 'pinia'
import { describe, it, expect, beforeEach, vi } from 'vitest'

const { mockApi, mockApiGet, mockApiPost, mockApiPut, mockApiDelete } = vi.hoisted(() => ({
  mockApiGet: vi.fn(),
  mockApiPost: vi.fn(),
  mockApiPut: vi.fn(),
  mockApiDelete: vi.fn(),
  mockApi: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn()
  }
}))

vi.mock('../boot/api', () => ({
  api: {
    get: mockApiGet,
    post: mockApiPost,
    put: mockApiPut,
    delete: mockApiDelete
  }
}))

vi.mock('../utils/appEnums', () => ({
  PostCategoryLabel: (category: number) => {
    const labels: Record<number, string> = { 1: 'General', 2: 'Anuncio', 10: 'Eventos' }
    return labels[category] || 'General'
  },
  PostStatusLabel: (status: number) => {
    const labels: Record<number, string> = { 0: 'Pendiente', 1: 'Aprobado', 2: 'Rechazado' }
    return labels[status] || 'Desconocido'
  }
}))

import { usePostStore } from './post'

describe('post store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('should have initial state', () => {
    const store = usePostStore()
    expect(store.posts).toEqual([])
    expect(store.currentPost).toBeNull()
    expect(store.loading).toBe(false)
    expect(store.error).toBeNull()
  })

  it('should get category label', () => {
    const store = usePostStore()
    expect(store.getCategoryLabel(1)).toBe('General')
    expect(store.getCategoryLabel(2)).toBe('Anuncio')
  })

  it('should get status label', () => {
    const store = usePostStore()
    expect(store.getStatusLabel(0)).toBe('Pendiente')
    expect(store.getStatusLabel(1)).toBe('Aprobado')
    expect(store.getStatusLabel(2)).toBe('Rechazado')
  })
})
