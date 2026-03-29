/// <reference types="vite/client" />

declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<{}, {}, any>
  export default component
}

interface ImportMetaEnv {
  readonly VITE_API_URL: string
  readonly VITE_GOOGLE_MAPS_API_KEY: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}

declare module '../utils/format' {
  export function formatDate(date: string | Date): string
  export function formatDateTime(date: string | Date): string
  export function formatRelativeTime(date: string | Date): string
}

declare module '../composables/useI18n' {
  export function useI18n(): any
}
