import { boot } from 'quasar/wrappers'
import { useAuthStore } from '../stores/auth.ts'

export default boot(({ store }) => {
  const authStore = useAuthStore(store)
  authStore.loadUserFromStorage()
})
