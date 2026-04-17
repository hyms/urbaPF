import { boot } from 'quasar/wrappers'
import { useAuthStore } from '../stores/auth'
import { useCondominiumStore } from '../stores/condominium'

export default boot(async ({ store }) => {
  const authStore = useAuthStore(store)
  authStore.loadUserFromStorage()

  const condoStore = useCondominiumStore(store)
  await condoStore.fetchAll()
  condoStore.loadCurrentCondominiumFromStorage()
})