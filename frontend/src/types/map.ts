export interface MapItem {
  id: string
  type: 'incident' | 'alert'
  title: string
  description?: string
  lat?: number
  lng?: number
  status: number
  priority: number
  created_at: string
  address_reference?: string
}
