import React, { useEffect, useRef, useState } from 'react'
import { LoadingSpinner } from './Loading'
import Button from './Button'
import MapService, { type MapMarker, type MapConfig } from '@/services/mapService'

interface MapViewProps {
  saints?: Array<{
    id: string
    name: string
    title?: string
    currentLocation?: {
      name: string
      address: string
      city: string
      state?: string
      latitude: number
      longitude: number
    }
    upcomingSchedules?: Array<{
      location: {
        name: string
        city: string
        state?: string
        latitude: number
        longitude: number
      }
      startDate: string
      endDate: string
    }>
  }>
  onMarkerClick?: (markerId: string, marker: MapMarker) => void
  className?: string
  height?: string
  center?: [number, number]
  zoom?: number
  showUserLocation?: boolean
  userLocation?: { lat: number; lng: number }
}

const MapView: React.FC<MapViewProps> = ({
  saints = [],
  onMarkerClick,
  className = '',
  height = '400px',
  center = [20.5937, 78.9629], // India center
  zoom = 5,
  showUserLocation = false,
  userLocation
}) => {
  const mapRef = useRef<HTMLDivElement>(null)
  const mapServiceRef = useRef<MapService | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Mapbox configuration - you should get this from environment variables
  const mapConfig: MapConfig = {
    accessToken: 'pk.eyJ1IjoiMzI5OTQzNjYwZDUwYTZiIjoiYWZkY2UyMDEzNzUyNDRlNWNhMSIsImEiOiJjYTVjOTc0OTgwYzUwMDAwN2I2In0.NFEd3YjJLc0QxMTdDM3MXZ4MnJ2OTg',
    styleId: 'mapbox/streets-v12',
    center,
    zoom
  }

  useEffect(() => {
    if (!mapRef.current) return

    const initializeMap = async () => {
      try {
        setIsLoading(true)
        setError(null)

        const L = await import('leaflet')

        // Load Leaflet CSS
        const link = document.createElement('link')
        link.rel = 'stylesheet'
        link.href = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'
        document.head.appendChild(link)

        // Initialize map service
        const mapService = new MapService(mapConfig)
        mapServiceRef.current = mapService

        // Initialize map
        const map = await mapService.initializeMap(mapRef.current)

        // Add markers for saints
        addSaintMarkers(mapService, saints)

        // Add user location marker if provided
        if (showUserLocation && userLocation) {
          addUserLocationMarker(mapService, userLocation)
        }

        setIsLoading(false)
      } catch (err) {
        console.error('Failed to initialize map:', err)
        setError('Failed to load map. Please try again later.')
        setIsLoading(false)
      }
    }

    initializeMap()

    // Cleanup
    return () => {
      if (mapServiceRef.current) {
        mapServiceRef.current.destroy()
        mapServiceRef.current = null
      }
    }
  }, [saints, center, zoom, showUserLocation, userLocation])

  // Update markers when saints data changes
  useEffect(() => {
    if (mapServiceRef.current && saints) {
      mapServiceRef.current.clearAllMarkers()
      addSaintMarkers(mapServiceRef.current, saints)
    }
  }, [saints])

  const addSaintMarkers = (mapService: MapService, saints: MapViewProps['saints']) => {
    saints.forEach((saint) => {
      if (saint.currentLocation) {
        const markerId = `current-${saint.id}`
        mapService.addMarker({
          id: markerId,
          position: [saint.currentLocation.latitude, saint.currentLocation.longitude],
          title: `${saint.title ? `${saint.title} ` : ''}${saint.name}`,
          description: `
            <strong>Currently at:</strong> ${saint.currentLocation.name}<br>
            <strong>Address:</strong> ${saint.currentLocation.address}<br>
            <strong>City:</strong> ${saint.currentLocation.city}${saint.currentLocation.state ? `, ${saint.currentLocation.state}` : ''}
          `,
          type: 'current',
          data: saint
        })
      }

      // Add markers for upcoming schedules
      saint.upcomingSchedules?.forEach((schedule, index) => {
        const markerId = `upcoming-${saint.id}-${index}`
        mapService.addMarker({
          id: markerId,
          position: [schedule.location.latitude, schedule.location.longitude],
          title: `${saint.name} - Upcoming Visit`,
          description: `
            <strong>Location:</strong> ${schedule.location.name}<br>
            <strong>City:</strong> ${schedule.location.city}<br>
            <strong>Visit:</strong> ${new Date(schedule.startDate).toLocaleDateString()} - ${new Date(schedule.endDate).toLocaleDateString()}
          `,
          type: 'upcoming',
          data: { saint, schedule }
        })
      })
    })
  }

  const addUserLocationMarker = (mapService: MapService, location: { lat: number; lng: number }) => {
    mapService.addMarker({
      id: 'user-location',
      position: [location.lat, location.lng],
      title: 'Your Location',
      description: 'This is your current location',
      type: 'temple'
    })
  }

  const handleLocateMe = async () => {
    if (!mapServiceRef.current) return

    try {
      const userLocation = await mapServiceRef.current.getUserLocation()
      if (userLocation) {
        mapServiceRef.current.setView([userLocation.lat, userLocation.lng], 13)

        // Add or update user location marker
        mapServiceRef.current.removeMarker('user-location')
        addUserLocationMarker(mapServiceRef.current, userLocation)
      } else {
        // Fallback to India center if geolocation fails
        mapServiceRef.current.setView([20.5937, 78.9629], 5)
      }
    } catch (error) {
      console.error('Failed to get user location:', error)
    }
  }

  return (
    <div className={`map-container ${className}`} style={{ height }}>
      {isLoading && (
        <div className="flex items-center justify-center h-full bg-gray-100">
          <LoadingSpinner size="lg" />
        </div>
      )}

      {error && (
        <div className="flex items-center justify-center h-full bg-red-50">
          <div className="text-center p-6">
            <h3 className="text-lg font-medium text-red-900 mb-2">Map Error</h3>
            <p className="text-red-600 mb-4">{error}</p>
            <Button onClick={() => window.location.reload()}>
              Try Again
            </Button>
          </div>
        </div>
      )}

      <div ref={mapRef} className="w-full h-full" />

      {showUserLocation && (
        <div className="absolute top-4 right-4 z-10">
          <Button
            onClick={handleLocateMe}
            size="sm"
            variant="outline"
            className="bg-white shadow-lg"
          >
            Locate Me
          </Button>
        </div>
      )}
    </div>
  )
}

export default MapView