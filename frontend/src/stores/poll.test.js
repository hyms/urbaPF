import { setActivePinia, createPinia } from 'pinia'
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'

const { mockApiPost, mockApiGet, mockApiDelete, mockApiPut } = vi.hoisted(() => ({
  mockApiPost: vi.fn(),
  mockApiGet: vi.fn(),
  mockApiDelete: vi.fn(),
  mockApiPut: vi.fn()
}))

vi.mock('../boot/api', () => ({
  api: {
    post: mockApiPost,
    get: mockApiGet,
    delete: mockApiDelete,
    put: mockApiPut
  }
}))

import { usePollStore } from './poll'

describe('poll store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    localStorage.clear()
    vi.clearAllMocks()
  })

  afterEach(() => {
    vi.resetAllMocks()
  })

  it('should have initial state', () => {
    const store = usePollStore()
    expect(store.polls).toEqual([])
    expect(store.currentPoll).toBeNull()
    expect(store.loading).toBe(false)
    expect(store.error).toBeNull()
  })

  it('should fetch polls by condominium', async () => {
    const store = usePollStore()
    const mockPolls = [
      {
        id: '1',
        condominiumId: 'condo-1',
        title: 'Test Poll 1',
        description: 'Description 1',
        options: '["Option1", "Option2"]',
        pollType: 1,
        startsAt: '2026-01-01T00:00:00Z',
        endsAt: '2026-01-07T00:00:00Z',
        requiresJustification: false,
        minRoleToVote: 2,
        status: 3,
        createdAt: '2026-01-01T00:00:00Z',
        createdById: 'user-1',
        createdByName: 'Test User'
      }
    ]

    mockApiGet.mockResolvedValueOnce({ data: mockPolls })

    const result = await store.fetchByCondominium('condo-1')

    expect(store.polls).toEqual(mockPolls)
    expect(store.loading).toBe(false)
    expect(mockApiGet).toHaveBeenCalledWith('/condominiums/condo-1/polls')
  })

  it('should fetch poll by id', async () => {
    const store = usePollStore()
    const mockPoll = {
      id: '1',
      condominiumId: 'condo-1',
      title: 'Test Poll',
      description: 'Test Description',
      options: '["Option1", "Option2"]',
      pollType: 1,
      startsAt: '2026-01-01T00:00:00Z',
      endsAt: '2026-01-07T00:00:00Z',
      requiresJustification: false,
      minRoleToVote: 2,
      status: 3,
      createdAt: '2026-01-01T00:00:00Z',
      createdById: 'user-1',
      createdByName: 'Test User'
    }

    mockApiGet.mockResolvedValueOnce({ data: mockPoll })

    const result = await store.fetchById('1')

    expect(store.currentPoll).toEqual(mockPoll)
    expect(mockApiGet).toHaveBeenCalledWith('/polls/1')
  })

  it('should create a poll', async () => {
    const store = usePollStore()
    const pollData = {
      title: 'New Poll',
      description: 'New Description',
      options: '["A", "B", "C"]',
      pollType: 1,
      startsAt: '2026-02-01T00:00:00Z',
      endsAt: '2026-02-07T00:00:00Z',
      requiresJustification: false,
      minRoleToVote: 2
    }

    mockApiPost.mockResolvedValueOnce({ data: { id: 'new-poll-id', status: 2 } })

    const result = await store.create('condo-1', pollData)

    expect(result).toBe('new-poll-id')
    expect(mockApiPost).toHaveBeenCalledWith('/condominiums/condo-1/polls', pollData)
  })

  it('should update a poll', async () => {
    const store = usePollStore()
    const updateData = { title: 'Updated Title' }

    mockApiPut.mockResolvedValueOnce({ data: { message: 'Votación actualizada' } })

    const result = await store.update('1', updateData)

    expect(result).toBe(true)
    expect(mockApiPut).toHaveBeenCalledWith('/polls/1', updateData)
  })

  it('should vote on a poll', async () => {
    const store = usePollStore()

    mockApiPost.mockResolvedValueOnce({ data: { message: 'Voto registrado' } })

    const result = await store.vote('1', 0)

    expect(result).toEqual({ message: 'Voto registrado' })
    expect(mockApiPost).toHaveBeenCalledWith('/polls/1/vote', { optionIndex: 0 })
  })

  it('should get poll results', async () => {
    const store = usePollStore()
    const mockResults = {
      votes: [
        {
          id: 'vote-1',
          pollId: '1',
          userId: 'user-1',
          userName: 'Test User',
          optionIndex: 0,
          digitalSignature: 'sig123',
          votedAt: '2026-01-02T00:00:00Z'
        }
      ],
      results: { 0: 1, 1: 0 }
    }

    mockApiGet.mockResolvedValueOnce({ data: mockResults })

    const result = await store.getResults('1')

    expect(result).toEqual(mockResults)
    expect(mockApiGet).toHaveBeenCalledWith('/polls/1/votes')
  })

  it('should delete a poll', async () => {
    const store = usePollStore()

    mockApiDelete.mockResolvedValueOnce({ data: { message: 'Votación eliminada' } })

    const result = await store.remove('1')

    expect(result).toBe(true)
    expect(mockApiDelete).toHaveBeenCalledWith('/polls/1')
  })

  it('should handle errors in fetchByCondominium', async () => {
    const store = usePollStore()

    mockApiGet.mockRejectedValueOnce(new Error('Network error'))

    const result = await store.fetchByCondominium('condo-1')

    expect(result).toEqual([])
    expect(store.error).toBe('Network error')
    expect(store.loading).toBe(false)
  })

  it('should get status label', () => {
    const store = usePollStore()
    expect(store.getStatusLabel(1)).toBe('Borrador')
    expect(store.getStatusLabel(2)).toBe('Programada')
    expect(store.getStatusLabel(3)).toBe('Activa')
    expect(store.getStatusLabel(4)).toBe('Cerrada')
    expect(store.getStatusLabel(5)).toBe('Cancelada')
  })

  it('should get type label', () => {
    const store = usePollStore()
    expect(store.getTypeLabel(1)).toBe('Opción Única')
    expect(store.getTypeLabel(2)).toBe('Opción Múltiple')
    expect(store.getTypeLabel(3)).toBe('Sí/No')
    expect(store.getTypeLabel(4)).toBe('Calificación')
  })
})
