import React from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ArrowLeft, MapPin, Calendar, Users, Phone, Mail, Star } from 'lucide-react'
import { saintsApi } from '@/services/api'
import { Saint } from '@/types/api'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import { LoadingSpinner } from '@/components/ui/Loading'
import Button from '@/components/ui/Button'
import { formatDate, formatDateRange, formatPhoneNumber } from '@/lib/utils'

const SaintProfilePage: React.FC = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()

  const {
    data: saint,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['saint', id],
    queryFn: () => saintsApi.getById(id!),
    enabled: !!id,
  })

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <LoadingSpinner size="lg" />
      </div>
    )
  }

  if (error || !saint?.data) {
    return (
      <div className="min-h-screen bg-gray-50">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="bg-red-50 border border-red-200 rounded-lg p-6">
            <h3 className="text-red-800 font-medium mb-2">Saint not found</h3>
            <p className="text-red-600 mb-4">
              The saint you're looking for doesn't exist or has been removed.
            </p>
            <Button onClick={() => navigate('/saints')}>
              Back to Saints
            </Button>
          </div>
        </div>
      </div>
    )
  }

  const saintData: Saint = saint.data

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Back Button */}
        <Button
          variant="outline"
          onClick={() => navigate(-1)}
          className="mb-6"
        >
          <ArrowLeft className="w-4 h-4 mr-2" />
          Back
        </Button>

        {/* Saint Header */}
        <Card className="mb-8">
          <CardContent className="p-6">
            <div className="flex flex-col md:flex-row items-start md:items-center gap-6">
              {/* Profile Photo */}
              <div className="w-32 h-32 bg-gray-200 rounded-full flex items-center justify-center flex-shrink-0">
                {saintData.photoUrl ? (
                  <img
                    src={saintData.photoUrl}
                    alt={saintData.name}
                    className="w-full h-full rounded-full object-cover"
                  />
                ) : (
                  <Users className="w-16 h-16 text-gray-400" />
                )}
              </div>

              {/* Saint Info */}
              <div className="flex-1">
                <h1 className="text-3xl font-bold text-gray-900 mb-2">{saintData.name}</h1>
                {saintData.title && (
                  <p className="text-xl text-primary-600 font-medium mb-2">{saintData.title}</p>
                )}
                {saintData.spiritualLineage && (
                  <p className="text-gray-600 mb-4">{saintData.spiritualLineage}</p>
                )}
                {saintData.bio && (
                  <p className="text-gray-700 leading-relaxed">{saintData.bio}</p>
                )}
              </div>

              {/* Contact Info */}
              <div className="flex flex-col space-y-2">
                {saintData.phone && (
                  <div className="flex items-center gap-2 text-gray-600">
                    <Phone className="w-4 h-4" />
                    <span>{formatPhoneNumber(saintData.phone)}</span>
                  </div>
                )}
                {saintData.email && (
                  <div className="flex items-center gap-2 text-gray-600">
                    <Mail className="w-4 h-4" />
                    <span>{saintData.email}</span>
                  </div>
                )}
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Current Location */}
          {saintData.currentSchedule && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <MapPin className="w-5 h-5 text-green-600" />
                  Current Location
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="p-4 bg-green-50 border border-green-200 rounded-lg">
                    <h3 className="font-medium text-green-900 mb-2">
                      {saintData.currentSchedule.location?.name}
                    </h3>
                    <p className="text-green-800 mb-1">
                      {saintData.currentSchedule.location?.address}
                    </p>
                    <p className="text-green-700 text-sm">
                      {saintData.currentSchedule.location?.city}
                      {saintData.currentSchedule.location?.state && `, ${saintData.currentSchedule.location?.state}`}
                      {saintData.currentSchedule.location?.country && `, ${saintData.currentSchedule.location?.country}`}
                    </p>
                    <p className="text-green-600 text-sm mt-2">
                      {formatDateRange(saintData.currentSchedule.startDate, saintData.currentSchedule.endDate)}
                    </p>
                    {saintData.currentSchedule.purpose && (
                      <p className="text-green-700 text-sm mt-1">
                        <span className="font-medium">Purpose:</span> {saintData.currentSchedule.purpose}
                      </p>
                    )}
                    {saintData.currentSchedule.contactPerson && (
                      <p className="text-green-700 text-sm mt-1">
                        <span className="font-medium">Contact:</span> {saintData.currentSchedule.contactPerson}
                      </p>
                    )}
                    {saintData.currentSchedule.contactPhone && (
                      <p className="text-green-700 text-sm">
                        <span className="font-medium">Phone:</span> {formatPhoneNumber(saintData.currentSchedule.contactPhone)}
                      </p>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Upcoming Visits */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Calendar className="w-5 h-5 text-primary-600" />
                Upcoming Visits
              </CardTitle>
            </CardHeader>
            <CardContent>
              {saintData.upcomingSchedules.length > 0 ? (
                <div className="space-y-4">
                  {saintData.upcomingSchedules.map((schedule) => (
                    <div key={schedule.id} className="p-4 border border-gray-200 rounded-lg">
                      <h3 className="font-medium text-gray-900 mb-1">
                        {schedule.location?.name}
                      </h3>
                      <p className="text-gray-600 text-sm mb-1">
                        {schedule.location?.city}
                        {schedule.location?.state && `, ${schedule.location?.state}`}
                      </p>
                      <p className="text-primary-600 text-sm font-medium">
                        {formatDateRange(schedule.startDate, schedule.endDate)}
                      </p>
                      {schedule.purpose && (
                        <p className="text-gray-700 text-sm mt-1">
                          <span className="font-medium">Purpose:</span> {schedule.purpose}
                        </p>
                      )}
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-gray-500 text-center py-8">
                  No upcoming visits scheduled
                </p>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Notes Section */}
        {saintData.currentSchedule?.notes && (
          <Card className="mt-8">
            <CardHeader>
              <CardTitle>Information for Devotees</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-gray-700 leading-relaxed">{saintData.currentSchedule.notes}</p>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  )
}

export default SaintProfilePage