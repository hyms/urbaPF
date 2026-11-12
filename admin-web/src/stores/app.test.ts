import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it } from 'vitest'
import { useAppStore } from './app'

describe('app store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('tracks API availability', () => {
    const store = useAppStore()
    expect(store.apiOnline).toBeNull()
    store.setApiOnline(true)
    expect(store.apiOnline).toBe(true)
  })
})