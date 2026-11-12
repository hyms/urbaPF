import { LatLngExpression, MarkerOptions } from 'leaflet';

export interface MapItem {
  id: string;
  type: 'incident' | 'alert';
  title: string;
  description?: string;
  lat: number;
  lng: number;
  status: number;
  priority?: number;
  reputationLevel?: number;
  needsApproval?: boolean;
  createdAt?: string;
}


