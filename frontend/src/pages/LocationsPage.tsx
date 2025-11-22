import React, { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Search, MapPin, Calendar, Users } from 'lucide-react'
import { locationsApi } from '@/services/api'
import { Location } from '@/types/api'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import { LoadingSpinner } from '@/components/ui/Loading'
import Button from '@/components/ui/Button'
import { formatDate } from '@/lib/utils'

const LocationsPage: React.FC = () => {
  const [searchQuery, setSearchQuery] = useState('')
  const [selectedCity, setSelectedCity] = useState('')
  const [currentPage, setCurrentPage] = useState(1)

  const {
    data: response,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['locations', currentPage, searchQuery, selectedCity],
    queryFn: () => locationsApi.getAll({
      page: currentPage,
      limit: 20,
      search: searchQuery || undefined,
      city: selectedCity || undefined,
    }),
  })

  const locations = response?.data || []
  const pagination = response?.pagination

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    setCurrentPage(1)
  }

  const handleCityFilter = (city: string) => {
    setSelectedCity(city === selectedCity ? '' : city)
    setCurrentPage(1)
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Locations</h1>
          <p className="text-gray-600">
            Find temples and spiritual centers where Jain saints are staying
          </p>
        </div>

        {/* Search and Filters */}
        <div className="bg-white rounded-lg border border-gray-200 p-6 mb-8">
          {/* Search Bar */}
          <form onSubmit={handleSearch} className="flex gap-4 mb-4">
            <div className="flex-1 relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Search by location name or address..."
                className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              />
            </div>
            <Button type="submit" className="flex-shrink-0">
              Search
            </Button>
          </form>

          {/* City Filters */}
          <div className="border-t border-gray-200 pt-4">
            <h3 className="text-sm font-medium text-gray-700 mb-2">Filter by City</h3>
            <div className="flex flex-wrap gap-2">
              {['Mumbai', 'Delhi', 'Bangalore', 'Ahmedabad', 'Kolkata', 'Chennai', 'Pune', 'Jaipur', 'Surat', 'Lucknow'].map((city) => (
                <button
                  key={city}
                  onClick={() => handleCityFilter(city)}
                  className={`px-3 py-1 rounded-full text-sm font-medium transition-colors ${
                    selectedCity === city
                      ? 'bg-primary-600 text-white'
                      : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                  }`}
                >
                  {city}
                </button>
              ))}
            </div>
          </div>
        </div>

        {/* Loading State */}
        {isLoading && (
          <div className="flex justify-center py-12">
            <LoadingSpinner size="lg" />
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-6 mb-8">
            <h3 className="text-red-800 font-medium mb-2">Unable to load locations</h3>
            <p className="text-red-600">
              {error instanceof Error ? error.message : 'An unexpected error occurred'}
            </p>
            <Button
              onClick={() => window.location.reload()}
              variant="outline"
              className="mt-4"
            >
              Try Again
            </Button>
          </div>
        )}

        {/* Locations Grid */}
        {!isLoading && !error && locations.length > 0 && (
          <>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
              {locations.map((location: Location) => (
                <Card key={location.id} className="hover:shadow-md transition-shadow">
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <MapPin className="w-5 h-5 text-primary-600" />
                      {location.name}
                    </CardTitle>
                    <p className="text-sm text-gray-600">
                      {location.city}
                      {location.state && `, ${location.state}`}
                    </p>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-4">
                      {/* Address */}
                      <div>
                        <p className="text-gray-700">{location.address}</p>
                      </div>

                      {/* Contact */}
                      {location.contactPhone && (
                        <div>
                          <span className="text-sm font-medium text-gray-700">Contact: </span>
                          <span className="text-sm text-gray-600">{location.contactPhone}</span>
                        </div>
                      )}

                      {/* Current Saints */}
                      {location.schedules.length > 0 && (
                        <div>
                          <h4 className="text-sm font-medium text-gray-700 mb-2">Saints Currently Here:</h4>
                          <div className="space-y-2">
                            {location.schedules
                              .filter(schedule => schedule.isCurrent)
                              .map((schedule) => (
                                <div key={schedule.id} className="p-2 bg-green-50 border border-green-200 rounded">
                                  <p className="text-sm font-medium text-green-900">
                                    {schedule.saint?.name}
                                  </p>
                                  <p className="text-xs text-green-700">
                                    {schedule.saint?.title && `${schedule.saint.title} • `}
                                    {formatDate(schedule.startDate)} - {formatDate(schedule.endDate)}
                                  </p>
                                  {schedule.purpose && (
                                    <p className="text-xs text-green-600 mt-1">{schedule.purpose}</p>
                                  )}
                                </div>
                              ))}
                          </div>
                        </div>
                      )}

                      {/* Upcoming Visits */}
                      {location.schedules.some(schedule => schedule.isUpcoming) && (
                        <div>
                          <h4 className="text-sm font-medium text-gray-700 mb-2">Upcoming Visits:</h4>
                          <div className="space-y-2">
                            {location.schedules
                              .filter(schedule => schedule.isUpcoming)
                              .slice(0, 2)
                              .map((schedule) => (
                                <div key={schedule.id} className="text-sm text-gray-600">
                                  <span className="font-medium">{schedule.saint?.name}</span>
                                  <span className="text-gray-500 ml-1">
                                    ({formatDate(schedule.startDate)})
                                  </span>
                                </div>
                              ))}
                          </div>
                        </div>
                      )}
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>

            {/* Pagination */}
            {pagination && pagination.totalPages > 1 && (
              <div className="flex justify-center">
                <div className="flex items-center space-x-2">
                  <Button
                    variant="outline"
                    onClick={() => setCurrentPage(prev => Math.max(1, prev - 1))}
                    disabled={currentPage === 1}
                  >
                    Previous
                  </Button>
                  <span className="text-sm text-gray-600">
                    Page {currentPage} of {pagination.totalPages}
                  </span>
                  <Button
                    variant="outline"
                    onClick={() => setCurrentPage(prev => Math.min(pagination.totalPages, prev + 1))}
                    disabled={currentPage === pagination.totalPages}
                  >
                    Next
                  </Button>
                </div>
              </div>
            )}
          </>
        )}

        {/* Empty State */}
        {!isLoading && !error && locations.length === 0 && (
          <div className="text-center py-12">
            <MapPin className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">No locations found</h3>
            <p className="text-gray-600 mb-4">
              {searchQuery || selectedCity
                ? 'Try adjusting your search or filters'
                : 'No locations are currently listed in the system'}
            </p>
            {(searchQuery || selectedCity) && (
              <Button
                variant="outline"
                onClick={() => {
                  setSearchQuery('')
                  setSelectedCity('')
                  setCurrentPage(1)
                }}
              >
                Clear Filters
              </Button>
            )}
          </div>
        )}
      </div>
    </div>
  )
}

export default LocationsPage