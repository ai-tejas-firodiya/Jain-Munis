import React, { useState } from 'react'
import { Search, MapPin, Navigation } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import { LoadingSpinner } from '@/components/ui/Loading'
import Button from '@/components/ui/Button'

const MapPage: React.FC = () => {
  const [searchQuery, setSearchQuery] = useState('')
  const [userLocation, setUserLocation] = useState<{ lat: number; lng: number } | null>(null)
  const [isLocating, setIsLocating] = useState(false)

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    // TODO: Implement map search functionality
  }

  const handleGetCurrentLocation = () => {
    if (navigator.geolocation) {
      setIsLocating(true)
      navigator.geolocation.getCurrentPosition(
        (position) => {
          setUserLocation({
            lat: position.coords.latitude,
            lng: position.coords.longitude
          })
          setIsLocating(false)
        },
        (error) => {
          console.error('Error getting location:', error)
          setIsLocating(false)
        }
      )
    } else {
      alert('Geolocation is not supported by this browser')
    }
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Map View</h1>
          <p className="text-gray-600">
            Find Jain saints and spiritual centers near you on an interactive map
          </p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
          {/* Search and Filters Sidebar */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-lg border border-gray-200 p-6 mb-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Search</h3>

              {/* Search Bar */}
              <form onSubmit={handleSearch} className="mb-4">
                <div className="relative">
                  <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                  <input
                    type="text"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    placeholder="Search locations..."
                    className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  />
                </div>
                <Button type="submit" className="w-full mt-2">
                  Search
                </Button>
              </form>

              {/* Current Location */}
              <div className="border-t border-gray-200 pt-4">
                <Button
                  variant="outline"
                  onClick={handleGetCurrentLocation}
                  disabled={isLocating}
                  className="w-full"
                >
                  {isLocating ? (
                    <>
                      <LoadingSpinner size="sm" className="mr-2" />
                      Getting location...
                    </>
                  ) : (
                    <>
                      <Navigation className="w-4 h-4 mr-2" />
                      Use My Location
                    </>
                  )}
                </Button>
              </div>
            </div>

            {/* Legend */}
            <div className="bg-white rounded-lg border border-gray-200 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Legend</h3>
              <div className="space-y-2">
                <div className="flex items-center gap-2">
                  <div className="w-4 h-4 bg-green-600 rounded-full"></div>
                  <span className="text-sm text-gray-700">Saints currently here</span>
                </div>
                <div className="flex items-center gap-2">
                  <div className="w-4 h-4 bg-blue-600 rounded-full"></div>
                  <span className="text-sm text-gray-700">Upcoming visits</span>
                </div>
                <div className="flex items-center gap-2">
                  <div className="w-4 h-4 bg-gray-400 rounded-full"></div>
                  <span className="text-sm text-gray-700">All locations</span>
                </div>
              </div>
            </div>
          </div>

          {/* Map Area */}
          <div className="lg:col-span-3">
            <Card className="h-[600px]">
              <CardContent className="p-0 h-full">
                {/* Placeholder for Map */}
                <div className="w-full h-full bg-gray-100 flex items-center justify-center">
                  <div className="text-center">
                    <MapPin className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                    <h3 className="text-lg font-semibold text-gray-900 mb-2">Interactive Map</h3>
                    <p className="text-gray-600 mb-4">
                      Map functionality will be implemented with a mapping service like Mapbox or Google Maps
                    </p>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
                      <div className="bg-white p-3 rounded-lg border border-gray-200">
                        <div className="font-medium text-gray-900 mb-1">Current Locations</div>
                        <div className="text-2xl font-bold text-green-600">12</div>
                        <div className="text-gray-600">Saints available</div>
                      </div>
                      <div className="bg-white p-3 rounded-lg border border-gray-200">
                        <div className="font-medium text-gray-900 mb-1">Total Locations</div>
                        <div className="text-2xl font-bold text-blue-600">45</div>
                        <div className="text-gray-600">Spiritual centers</div>
                      </div>
                      <div className="bg-white p-3 rounded-lg border border-gray-200">
                        <div className="font-medium text-gray-900 mb-1">Cities Covered</div>
                        <div className="text-2xl font-bold text-purple-600">28</div>
                        <div className="text-gray-600">Across India</div>
                      </div>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>

        {/* Information Cards */}
        <div className="mt-8 grid grid-cols-1 md:grid-cols-3 gap-6">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Real-time Updates</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-gray-600">
                Get the latest information about saint locations and schedules updated in real-time.
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Detailed Information</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-gray-600">
                Click on any location pin to see details about the saint, schedule, and contact information.
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Mobile Friendly</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-gray-600">
                Works seamlessly on mobile devices with touch gestures and GPS integration.
              </p>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}

export default MapPage