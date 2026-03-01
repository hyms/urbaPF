import { ref } from 'vue'
import es from '../i18n/es'

const currentLocale = ref('es')
const messages = { es }

export function useI18n() {
  const t = (key) => {
    const keys = key.split('.')
    let value = messages[currentLocale.value]
    for (const k of keys) {
      if (value && value[k]) {
        value = value[k]
      } else {
        return key
      }
    }
    return value
  }

  return { t, locale: currentLocale }
}
