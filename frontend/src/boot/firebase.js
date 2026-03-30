import { boot } from 'quasar/wrappers'
import { useAuthStore } from '../stores/auth.ts'

export default boot(async ({ store }) => {
  const authStore = useAuthStore(store)
  
  const updateToken = async (token) => {
    if (authStore.isAuthenticated && token) {
      await authStore.updateFcmToken(token)
    }
  }

  if ('serviceWorker' in navigator && 'PushManager' in window) {
    try {
      const registration = await navigator.serviceWorker.ready
      
      const existingSubscription = await registration.pushManager.getSubscription()
      if (existingSubscription) {
        const token = existingSubscription.endpoint
        await updateToken(token)
        return
      }

      const subscription = await registration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: urlBase64ToUint8Array('')
      })
      
      const token = subscription.endpoint
      await updateToken(token)
      
    } catch (error) {
      console.log('Push notification subscription failed:', error)
    }
  } else {
    console.log('Push notifications not supported')
  }
})

function urlBase64ToUint8Array(base64String) {
  const padding = '='.repeat((4 - base64String.length % 4) % 4)
  const base64 = (base64String + padding)
    .replace(/-/g, '+')
    .replace(/_/g, '/')
  
  const rawData = window.atob(base64)
  const outputArray = new Uint8Array(rawData.length)
  
  for (let i = 0; i < rawData.length; ++i) {
    outputArray[i] = rawData.charCodeAt(i)
  }
  return outputArray
}
