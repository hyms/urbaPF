import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useAppStore = defineStore('app', () => {
  const locale = ref('es')
  const apiOnline = ref<boolean | null>(null)

  function setApiOnline(value: boolean) {
    apiOnline.value = value
  }

  return {
    locale,
    apiOnline,
    setApiOnline,
  }
})