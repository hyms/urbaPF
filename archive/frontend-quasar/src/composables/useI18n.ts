import { ref } from 'vue'
import es from '@/i18n/es'

const currentLocale = ref('es')
const messages: Record<string, any> = { es }

export function useI18n() {
  const t = (key: string): string => {
    const keys = key.split('.')
    let value = messages[currentLocale.value]
    for (const k of keys) {
      if (value && value[k]) {
        value = value[k]
      } else {
        return key
      }
    }
    return typeof value === 'string' ? value : key
  }

  return { t, locale: currentLocale }
}
