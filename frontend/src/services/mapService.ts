import type { Map as LeafletMap, TileLayer, Marker, Popup, LatLng } from 'leaflet'

interface MapConfig {
  accessToken: string
  styleId?: string
  center?: [number, number]
  zoom?: number
}

interface MapMarker {
  id: string
  position: [number, number]
  title: string
  description?: string
  type: 'saint' | 'temple' | 'current' | 'upcoming'
  iconUrl?: string
  data?: any
}

interface GeocodingResult {
  lat: number
  lng: number
  display_name: string
  address: string
  country?: string
  state?: string
  city?: string
  postcode?: string
}

class MapService {
  private map: LeafletMap | null = null
  private markers: Map<string, Marker> = new Map()
  private tileLayer: TileLayer | null = null
  private config: MapConfig

  constructor(config: MapConfig) {
    this.config = config
  }

  async initializeMap(container: HTMLElement): Promise<LeafletMap> {
    try {
      // Dynamically import Leaflet to avoid SSR issues
      const L = await import('leaflet')
      const map = L.map(container, {
        center: this.config.center || [20.5937, 78.9629], // Default: India
        zoom: this.config.zoom || 5,
        zoomControl: true,
        attributionControl: true
      })

      // Add Mapbox tile layer
      this.tileLayer = L.tileLayer(
        `https://api.mapbox.com/styles/v1/${this.config.styleId}/tiles/{z}/{x}/{y}?access_token=${this.config.accessToken}`,
        {
          attribution: '© Mapbox © OpenStreetMap contributors',
          maxZoom: 19
        }
      )

      this.tileLayer.addTo(map)
      this.map = map

      return map
    } catch (error) {
      console.error('Failed to initialize map:', error)
      throw new Error('Map initialization failed')
    }
  }

  addMarker(markerData: MapMarker): string {
    if (!this.map) {
      console.warn('Map not initialized')
      return ''
    }

    // Dynamically import Leaflet for icons
    const L = require('leaflet')

    // Create custom icon based on marker type
    let icon: any

    switch (markerData.type) {
      case 'current':
        icon = L.divIcon({
          className: 'custom-div-icon',
          html: `<div class="marker-icon marker-current">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <circle cx="12" cy="12" r="10" fill="#10B981" />
              <path d="M12 2C13.1046 2 14 2.89543 14 4V7.5C14 8.10457 13.1046 9 12 9S10 8.10457 10 7.5V4C10 2.89543 10.8954 2 12 2Z" fill="white"/>
            </svg>
          </div>`,
          iconSize: [32, 32],
          iconAnchor: [16, 32],
          popupAnchor: [0, -32]
        })
        break

      case 'upcoming':
        icon = L.divIcon({
          className: 'custom-div-icon',
          html: `<div class="marker-icon marker-upcoming">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <circle cx="12" cy="12" r="10" fill="#F59E0B" />
              <path d="M10 8H14M12 2C13.1046 2 14 2.89543 14 4V7.5C14 8.10457 13.1046 9 12 9S10 8.10457 10 7.5V4C10 2.89543 10.8954 2 12 2Z" fill="white"/>
            </svg>
          </div>`,
          iconSize: [32, 32],
          iconAnchor: [16, 32],
          popupAnchor: [0, -32]
        })
        break

      case 'temple':
        icon = L.divIcon({
          className: 'custom-div-icon',
          html: `<div class="marker-icon marker-temple">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <circle cx="12" cy="12" r="10" fill="#8B5CF6" />
              <path d="M4 12H8M12 4V8M16 12H20M12 16V20" stroke="white" stroke-width="2" stroke-linecap="round"/>
            </svg>
          </div>`,
          iconSize: [32, 32],
          iconAnchor: [16, 32],
          popupAnchor: [0, -32]
        })
        break

      default:
        icon = L.divIcon({
          className: 'custom-div-icon',
          html: `<div class="marker-icon marker-default">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <circle cx="12" cy="12" r="10" fill="#6B7280" />
              <path d="M12 6C13.1046 6 14 6.89543 14 8V10C14 11.1046 13.1046 12 12 12S10 11.1046 10 10V8C10 6.89543 10.8954 6 12 6Z" fill="white"/>
            </svg>
          </div>`,
          iconSize: [32, 32],
          iconAnchor: [16, 32],
          popupAnchor: [0, -32]
        })
    }

    const marker = L.marker([markerData.position[0], markerData.position[1]], { icon })
      .addTo(this.map!)

    // Add popup
    if (markerData.description) {
      const popup = L.popup()
        .setContent(`
          <div class="map-popup">
            <h3 class="map-popup-title">${markerData.title}</h3>
            <p class="map-popup-description">${markerData.description}</p>
          </div>
        `)

      marker.bindPopup(popup)
    }

    // Store marker reference
    this.markers.set(markerData.id, marker)

    return markerData.id
  }

  removeMarker(markerId: string): void {
    const marker = this.markers.get(markerId)
    if (marker && this.map) {
      this.map.removeLayer(marker)
      this.markers.delete(markerId)
    }
  }

  updateMarker(markerId: string, markerData: Partial<MapMarker>): void {
    this.removeMarker(markerId)
    if (markerData.position) {
      this.addMarker({ ...markerData, id: markerId } as MapMarker)
    }
  }

  clearAllMarkers(): void {
    this.markers.forEach((marker) => {
      if (this.map) {
        this.map.removeLayer(marker)
      }
    })
    this.markers.clear()
  }

  setView(position: [number, number], zoom: number): void {
    if (this.map) {
      this.map.setView(position, zoom)
    }
  }

  fitBounds(bounds: [[number, number], [number, number]]): void {
    if (this.map) {
      this.map.fitBounds(bounds, { padding: [50, 50] })
    }
  }

  getUserLocation(): Promise<GeocodingResult | null> {
    return new Promise((resolve) => {
      if (!navigator.geolocation) {
        resolve(null)
        return
      }

      navigator.geolocation.getCurrentPosition(
        (position) => {
          resolve({
            lat: position.coords.latitude,
            lng: position.coords.longitude,
            display_name: 'Your Location'
          })
        },
        (error) => {
          console.error('Geolocation error:', error)
          resolve(null)
        },
        {
          enableHighAccuracy: true,
          timeout: 10000,
          maximumAge: 300000
        }
      )
    })
  }

  async geocodeAddress(address: string): Promise<GeocodingResult | null> {
    try {
      const response = await fetch(
        `https://api.mapbox.com/geocoding/v5/mapbox.places/${encodeURIComponent(address)}.json?access_token=${this.config.accessToken}&limit=1`
      )

      if (!response.ok) {
        throw new Error('Geocoding request failed')
      }

      const data = await response.json()

      if (data.features && data.features.length > 0) {
        const feature = data.features[0]
        const center = feature.center

        return {
          lat: center[1],
          lng: center[0],
          display_name: feature.place_name,
          address: feature.place_name,
          country: feature.context?.find((c: any) => c.id?.startsWith('country'))?.short_name,
          state: feature.context?.find((c: any) => c.id?.startsWith('region'))?.short_name,
          city: feature.context?.find((c: any) => c.id?.startsWith('place'))?.short_name,
          postcode: feature.context?.find((c: any) => c.id?.startsWith('postcode'))?.short_name
        }
      }

      return null
    } catch (error) {
      console.error('Geocoding error:', error)
      return null
    }
  }

  async reverseGeocode(lat: number, lng: number): Promise<GeocodingResult | null> {
    try {
      const response = await fetch(
        `https://api.mapbox.com/geocoding/v5/mapbox.places/${lng},${lat}.json?access_token=${this.config.accessToken}`
      )

      if (!response.ok) {
        throw new Error('Reverse geocoding request failed')
      }

      const data = await response.json()

      if (data.features && data.features.length > 0) {
        const feature = data.features[0]

        return {
          lat,
          lng,
          display_name: feature.place_name,
          address: feature.place_name,
          country: feature.context?.find((c: any) => c.id?.startsWith('country'))?.short_name,
          state: feature.context?.find((c: any) => c.id?.startsWith('region'))?.short_name,
          city: feature.context?.find((c: any) => c.id?.startsWith('place'))?.short_name,
          postcode: feature.context?.find((c: any) => c.id?.startsWith('postcode'))?.short_name
        }
      }

      return null
    } catch (error) {
      console.error('Reverse geocoding error:', error)
      return null
    }
  }

  calculateDistance(pos1: [number, number], pos2: [number, number]): number {
    const R = 6371 // Earth's radius in kilometers
    const dLat = this.toRadians(pos2[0] - pos1[0])
    const dLon = this.toRadians(pos2[1] - pos1[1])
    const a =
      Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.cos(this.toRadians(pos1[0])) * Math.cos(this.toRadians(pos2[0])) *
      Math.sin(dLon / 2) * Math.sin(dLon / 2)
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a))
    return R * c
  }

  private toRadians(degrees: number): number {
    return degrees * (Math.PI / 180)
  }

  destroy(): void {
    if (this.map) {
      this.map.remove()
      this.map = null
    }
    this.markers.clear()
  }
}

export default MapService
export type { MapConfig, MapMarker, GeocodingResult }