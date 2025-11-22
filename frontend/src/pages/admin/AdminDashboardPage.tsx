import React from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '@/contexts/AuthContext'
import { Users, MapPin, Calendar, Plus, Settings, Activity, LogOut, Home } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import Button from '@/components/ui/Button'

const AdminDashboardPage: React.FC = () => {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/admin/login')
  }

  const stats = [
    {
      title: 'Total Saints',
      value: '142',
      change: '+5 this month',
      icon: Users,
      color: 'text-blue-600',
      bgColor: 'bg-blue-100',
    },
    {
      title: 'Active Schedules',
      value: '38',
      change: '+12 this week',
      icon: Calendar,
      color: 'text-green-600',
      bgColor: 'bg-green-100',
    },
    {
      title: 'Locations',
      value: '85',
      change: '+8 this month',
      icon: MapPin,
      color: 'text-purple-600',
      bgColor: 'bg-purple-100',
    },
    {
      title: 'Admin Users',
      value: '12',
      change: 'No change',
      icon: Settings,
      color: 'text-gray-600',
      bgColor: 'bg-gray-100',
    },
  ]

  const recentActivity = [
    {
      id: 1,
      action: 'Added new schedule',
      entity: 'Acharya XYZ in Mumbai',
      time: '2 hours ago',
      user: 'Admin1'
    },
    {
      id: 2,
      action: 'Updated saint profile',
      entity: 'Muni ABC',
      time: '5 hours ago',
      user: 'Admin2'
    },
    {
      id: 3,
      action: 'Added new location',
      entity: 'Jain Temple, Delhi',
      time: '1 day ago',
      user: 'Admin1'
    },
    {
      id: 4,
      action: 'Resolved schedule conflict',
      entity: 'Acharya PQR schedule',
      time: '2 days ago',
      user: 'SuperAdmin'
    },
  ]

  const quickActions = [
    {
      title: 'Add New Saint',
      description: 'Add a new saint to the system',
      icon: Users,
      href: '/admin/saints/new',
      color: 'bg-blue-600',
    },
    {
      title: 'Add Schedule',
      description: 'Create a new saint schedule',
      icon: Calendar,
      href: '/admin/schedules/new',
      color: 'bg-green-600',
    },
    {
      title: 'Manage Locations',
      description: 'Add or update temple locations',
      icon: MapPin,
      href: '/admin/locations',
      color: 'bg-purple-600',
    },
    {
      title: 'View Activity Logs',
      description: 'Monitor system activities',
      icon: Activity,
      href: '/admin/logs',
      color: 'bg-gray-600',
    },
  ]

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center">
              <Link to="/admin/dashboard" className="flex items-center space-x-2">
                <div className="w-8 h-8 bg-primary-600 rounded-lg flex items-center justify-center">
                  <span className="text-white font-bold text-lg">JM</span>
                </div>
                <span className="text-xl font-bold text-gray-900">Admin Panel</span>
              </Link>
            </div>

            <div className="flex items-center space-x-4">
              <span className="text-sm text-gray-600">
                Welcome, {user?.username}
              </span>
              <Button
                variant="outline"
                onClick={() => navigate('/')}
                className="flex items-center gap-2"
              >
                <Home className="w-4 h-4" />
                Main Site
              </Button>
              <Button
                variant="outline"
                onClick={handleLogout}
                className="flex items-center gap-2"
              >
                <LogOut className="w-4 h-4" />
                Logout
              </Button>
            </div>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Page Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
          <p className="text-gray-600">
            Manage saints, schedules, and locations for the Jain Munis platform
          </p>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          {stats.map((stat, index) => (
            <Card key={index}>
              <CardContent className="p-6">
                <div className="flex items-center">
                  <div className={`p-2 rounded-lg ${stat.bgColor} mr-4`}>
                    <stat.icon className={`w-6 h-6 ${stat.color}`} />
                  </div>
                  <div>
                    <p className="text-sm font-medium text-gray-600">{stat.title}</p>
                    <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
                    <p className="text-xs text-gray-500">{stat.change}</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Quick Actions */}
          <div className="lg:col-span-2">
            <Card>
              <CardHeader>
                <CardTitle>Quick Actions</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {quickActions.map((action, index) => (
                    <Button
                      key={index}
                      variant="outline"
                      onClick={() => navigate(action.href)}
                      className="h-auto p-4 flex flex-col items-start text-left"
                    >
                      <div className={`w-8 h-8 rounded-lg ${action.color} flex items-center justify-center mb-3`}>
                        <action.icon className="w-4 h-4 text-white" />
                      </div>
                      <div className="font-medium text-gray-900">{action.title}</div>
                      <div className="text-sm text-gray-600 mt-1">{action.description}</div>
                    </Button>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Recent Activity */}
          <div>
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Activity className="w-5 h-5" />
                  Recent Activity
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {recentActivity.map((activity) => (
                    <div key={activity.id} className="pb-4 border-b border-gray-100 last:border-0">
                      <p className="text-sm font-medium text-gray-900">{activity.action}</p>
                      <p className="text-sm text-gray-600">{activity.entity}</p>
                      <p className="text-xs text-gray-500 mt-1">
                        {activity.time} by {activity.user}
                      </p>
                    </div>
                  ))}
                </div>
                <Button
                  variant="outline"
                  className="w-full mt-4"
                  onClick={() => navigate('/admin/logs')}
                >
                  View All Activity
                </Button>
              </CardContent>
            </Card>
          </div>
        </div>

        {/* Upcoming Schedules */}
        <div className="mt-8">
          <Card>
            <CardHeader>
              <CardTitle>Schedules Needing Attention</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium text-yellow-900">Saint: Acharya PQR</p>
                      <p className="text-yellow-700 text-sm">Location: Pune • Ends Tomorrow</p>
                    </div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => navigate('/admin/schedules')}
                    >
                      View
                    </Button>
                  </div>
                </div>

                <div className="p-4 bg-orange-50 border border-orange-200 rounded-lg">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium text-orange-900">Saint: Muni LMN</p>
                      <p className="text-orange-700 text-sm">Location: Surat • No Contact Info</p>
                    </div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => navigate('/admin/schedules')}
                    >
                      Update
                    </Button>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </main>
    </div>
  )
}

export default AdminDashboardPage