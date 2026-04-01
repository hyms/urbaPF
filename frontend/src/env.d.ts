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

declare module '../stores/alert' {
  export const useAlertStore: any
}

declare module '../stores/incident' {
  export const useIncidentStore: any
}

declare module '../stores/condominium' {
  export const useCondominiumStore: any
}

declare module '../stores/post' {
  export const usePostStore: any
  export type Post = any
  export type CreatePostRequest = any
  export type UpdatePostRequest = any
}

declare module '../stores/poll' {
  export const usePollStore: any
  export type Poll = any
  export type CreatePollRequest = any
  export type UpdatePollRequest = any
}

declare module '../composables/useI18n' {
  export function useI18n(): any
}

declare module '../utils/format' {
  export function formatRelativeTime(dateString: string): string
  export function formatDate(dateString: string): string
}

declare module '../stores/incident' {
  export const useIncidentStore: any
}

declare module '../stores/condominium' {
  export const useCondominiumStore: any
}
