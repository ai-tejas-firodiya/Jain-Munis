import React from 'react'
import { Link } from 'react-router-dom'
import { Search, MapPin, Calendar, Users, ArrowRight, Star } from 'lucide-react'

const HomePage: React.FC = () => {
  return (
    <div className="min-h-screen">
      {/* Hero Section */}
      <section className="bg-gradient-to-br from-primary-50 to-secondary-50 py-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h1 className="text-4xl md:text-6xl font-bold text-gray-900 mb-6">
              Track Jain Saints
              <span className="block text-primary-600">Near You</span>
            </h1>
            <p className="text-xl text-gray-600 mb-8 max-w-3xl mx-auto">
              Connect with spiritual leaders in your community. Find where saints are staying,
              their schedules, and plan your visits to receive blessings and guidance.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link
                to="/saints"
                className="inline-flex items-center justify-center px-8 py-3 bg-primary-600 text-white font-medium rounded-lg hover:bg-primary-700 transition-colors"
              >
                <Users className="w-5 h-5 mr-2" />
                Browse All Saints
              </Link>
              <Link
                to="/locations"
                className="inline-flex items-center justify-center px-8 py-3 bg-white text-primary-600 font-medium rounded-lg border border-primary-200 hover:bg-primary-50 transition-colors"
              >
                <MapPin className="w-5 h-5 mr-2" />
                Search by Location
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Features */}
      <section className="py-20 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 mb-4">
              How It Works
            </h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Simple and intuitive ways to stay connected with Jain spiritual leaders
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div className="text-center">
              <div className="w-16 h-16 bg-primary-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <Search className="w-8 h-8 text-primary-600" />
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-2">
                Search & Discover
              </h3>
              <p className="text-gray-600">
                Find saints by name, search by location, or browse all available spiritual leaders
              </p>
            </div>

            <div className="text-center">
              <div className="w-16 h-16 bg-primary-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <Calendar className="w-8 h-8 text-primary-600" />
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-2">
                View Schedules
              </h3>
              <p className="text-gray-600">
                Check current locations and upcoming schedules to plan your visits accordingly
              </p>
            </div>

            <div className="text-center">
              <div className="w-16 h-16 bg-primary-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <MapPin className="w-8 h-8 text-primary-600" />
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-2">
                Visit & Connect
              </h3>
              <p className="text-gray-600">
                Get directions, contact information, and visit saints in your area
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Quick Stats */}
      <section className="py-20 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 mb-4">
              Growing Community
            </h2>
            <p className="text-lg text-gray-600">
              Join thousands of devotees staying connected with their spiritual guides
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
            <div className="text-center">
              <div className="text-3xl md:text-4xl font-bold text-primary-600 mb-2">150+</div>
              <p className="text-gray-600">Saints Listed</p>
            </div>
            <div className="text-center">
              <div className="text-3xl md:text-4xl font-bold text-primary-600 mb-2">50+</div>
              <p className="text-gray-600">Cities Covered</p>
            </div>
            <div className="text-center">
              <div className="text-3xl md:text-4xl font-bold text-primary-600 mb-2">500+</div>
              <p className="text-gray-600">Active Schedules</p>
            </div>
            <div className="text-center">
              <div className="text-3xl md:text-4xl font-bold text-primary-600 mb-2">10K+</div>
              <p className="text-gray-600">Devotees Connected</p>
            </div>
          </div>
        </div>
      </section>

      {/* Popular Cities */}
      <section className="py-20 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 mb-4">
              Popular Cities
            </h2>
            <p className="text-lg text-gray-600">
              Find saints in major Jain community centers across India
            </p>
          </div>

          <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-4">
            {[
              'Mumbai', 'Delhi', 'Bangalore', 'Ahmedabad',
              'Kolkata', 'Chennai', 'Pune', 'Jaipur',
              'Surat', 'Lucknow', 'Nagpur', 'Indore'
            ].map((city) => (
              <Link
                key={city}
                to={`/locations?city=${encodeURIComponent(city)}`}
                className="block p-4 text-center border border-gray-200 rounded-lg hover:border-primary-300 hover:bg-primary-50 transition-colors"
              >
                <MapPin className="w-6 h-6 text-primary-600 mx-auto mb-2" />
                <span className="text-sm font-medium text-gray-900">{city}</span>
              </Link>
            ))}
          </div>
        </div>
      </section>

      {/* Call to Action */}
      <section className="py-20 bg-gradient-to-r from-primary-600 to-secondary-600">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-3xl md:text-4xl font-bold text-white mb-4">
            Stay Connected with Your Spiritual Journey
          </h2>
          <p className="text-xl text-primary-100 mb-8">
            Get notified about saint visits, spiritual discourses, and community events in your area
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link
              to="/saints"
              className="inline-flex items-center justify-center px-8 py-3 bg-white text-primary-600 font-medium rounded-lg hover:bg-gray-50 transition-colors"
            >
              Start Exploring
              <ArrowRight className="w-5 h-5 ml-2" />
            </Link>
            <Link
              to="/map"
              className="inline-flex items-center justify-center px-8 py-3 bg-transparent text-white font-medium rounded-lg border border-white hover:bg-white hover:text-primary-600 transition-colors"
            >
              View on Map
            </Link>
          </div>
        </div>
      </section>
    </div>
  )
}

export default HomePage