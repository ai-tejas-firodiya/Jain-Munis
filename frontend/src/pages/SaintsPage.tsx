import React, { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Search, Filter, MapPin, Calendar, Users } from 'lucide-react'
import { saintsApi } from '@/services/api'
import { Saint } from '@/types/api'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import { LoadingSpinner } from '@/components/ui/Loading'
import Button from '@/components/ui/Button'
import { formatDate } from '@/lib/utils'

const SaintsPage: React.FC = () => {
  const [searchQuery, setSearchQuery] = useState('')
  const [selectedCity, setSelectedCity] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [showFilters, setShowFilters] = useState(false)

  const {
    data: response,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['saints', currentPage, searchQuery, selectedCity],
    queryFn: () => saintsApi.getAll({
      page: currentPage,
      limit: 20,
      search: searchQuery || undefined,
      city: selectedCity || undefined,
      isActive: true,
    }),
  })

  const saints = response?.data || []
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
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Jain Saints</h1>
          <p className="text-gray-600">
            Discover and connect with Jain spiritual leaders in your community
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
                placeholder="Search by saint name, title, or spiritual lineage..."
                className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              />
            </div>
            <Button type="submit" className="flex-shrink-0">
              Search
            </Button>
            <Button
              type="button"
              variant="outline"
              onClick={() => setShowFilters(!showFilters)}
              className="flex items-center gap-2"
            >
              <Filter className="w-4 h-4" />
              Filters
            </Button>
          </form>

          {/* Filters */}
          {showFilters && (
            <div className="border-t border-gray-200 pt-4">
              <div className="mb-4">
                <h3 className="text-sm font-medium text-gray-700 mb-2">Filter by City</h3>
                <div className="flex flex-wrap gap-2">
                  {['Mumbai', 'Delhi', 'Bangalore', 'Ahmedabad', 'Kolkata', 'Chennai', 'Pune', 'Jaipur'].map((city) => (
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
          )}
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
            <h3 className="text-red-800 font-medium mb-2">Unable to load saints</h3>
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

        {/* Saints Grid */}
        {!isLoading && !error && saints.length > 0 && (
          <>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
              {saints.map((saint: Saint) => (
                <Card key={saint.id} className="hover:shadow-md transition-shadow">
                  <CardHeader className="pb-4">
                    <div className="flex items-start space-x-4">
                      <div className="w-16 h-16 bg-gray-200 rounded-full flex items-center justify-center flex-shrink-0">
                        {saint.photoUrl ? (
                          <img
                            src={saint.photoUrl}
                            alt={saint.name}
                            className="w-full h-full rounded-full object-cover"
                          />
                        ) : (
                          <Users className="w-8 h-8 text-gray-400" />
                        )}
                      </div>
                      <div className="flex-1 min-w-0">
                        <CardTitle className="text-lg text-gray-900">{saint.name}</CardTitle>
                        {saint.title && (
                          <p className="text-sm text-primary-600 font-medium">{saint.title}</p>
                        )}
                        {saint.spiritualLineage && (
                          <p className="text-sm text-gray-500 mt-1 line-clamp-2">
                            {saint.spiritualLineage}
                          </p>
                        )}
                      </div>
                    </div>
                  </CardHeader>
                  <CardContent className="pt-0">
                    {/* Current Location */}
                    {saint.currentSchedule && (
                      <div className="mb-4 p-3 bg-green-50 border border-green-200 rounded-lg">
                        <div className="flex items-center gap-2 text-green-800 mb-1">
                          <MapPin className="w-4 h-4" />
                          <span className="font-medium text-sm">Currently at</span>
                        </div>
                        <p className="text-green-700 text-sm font-medium">
                          {saint.currentSchedule.location?.name}
                        </p>
                        <p className="text-green-600 text-sm">
                          {saint.currentSchedule.location?.city}
                          {saint.currentSchedule.location?.state && `, ${saint.currentSchedule.location?.state}`}
                        </p>
                        <p className="text-green-600 text-xs mt-1">
                          {formatDate(saint.currentSchedule.startDate)} - {formatDate(saint.currentSchedule.endDate)}
                        </p>
                      </div>
                    )}

                    {/* Upcoming Visits */}
                    {saint.upcomingSchedules.length > 0 && (
                      <div className="mb-4">
                        <div className="flex items-center gap-2 text-gray-700 mb-2">
                          <Calendar className="w-4 h-4" />
                          <span className="font-medium text-sm">Upcoming Visits</span>
                        </div>
                        <div className="space-y-1">
                          {saint.upcomingSchedules.slice(0, 2).map((schedule) => (
                            <div key={schedule.id} className="text-sm text-gray-600">
                              <span className="font-medium">{schedule.location?.city}</span>
                              <span className="text-gray-500 ml-1">
                                ({formatDate(schedule.startDate)})
                              </span>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}

                    {/* View Details Button */}
                    <Button
                      variant="outline"
                      className="w-full"
                      onClick={() => window.location.href = `/saints/${saint.id}`}
                    >
                      View Details
                    </Button>
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
        {!isLoading && !error && saints.length === 0 && (
          <div className="text-center py-12">
            <Users className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">No saints found</h3>
            <p className="text-gray-600 mb-4">
              {searchQuery || selectedCity
                ? 'Try adjusting your search or filters'
                : 'No saints are currently listed in the system'}
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

export default SaintsPage