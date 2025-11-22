import { Routes, Route } from 'react-router-dom'
import Header from '@/components/layout/Header'
import Footer from '@/components/layout/Footer'
import HomePage from '@/pages/HomePage'
import SaintsPage from '@/pages/SaintsPage'
import SaintProfilePage from '@/pages/SaintProfilePage'
import LocationsPage from '@/pages/LocationsPage'
import MapPage from '@/pages/MapPage'
import AdminLoginPage from '@/pages/admin/AdminLoginPage'
import AdminDashboardPage from '@/pages/admin/AdminDashboardPage'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { AuthProvider } from '@/contexts/AuthContext'
import { ToastContainer } from '@/components/ui/Toast'

function App() {
  return (
    <AuthProvider>
      <div className="min-h-screen flex flex-col bg-gray-50">
        <Header />
        <main className="flex-1">
          <Routes>
            {/* Public routes */}
            <Route path="/" element={<HomePage />} />
            <Route path="/saints" element={<SaintsPage />} />
            <Route path="/saints/:id" element={<SaintProfilePage />} />
            <Route path="/locations" element={<LocationsPage />} />
            <Route path="/locations/:city" element={<LocationsPage />} />
            <Route path="/map" element={<MapPage />} />

            {/* Admin routes */}
            <Route path="/admin/login" element={<AdminLoginPage />} />
            <Route
              path="/admin/dashboard"
              element={
                <ProtectedRoute>
                  <AdminDashboardPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/admin/*"
              element={
                <ProtectedRoute>
                  <div className="container mx-auto px-4 py-8">
                    <h1 className="text-2xl font-bold text-gray-900">Admin Panel</h1>
                    <p className="text-gray-600">Admin functionality will be implemented here</p>
                  </div>
                </ProtectedRoute>
              }
            />

            {/* Fallback */}
            <Route
              path="*"
              element={
                <div className="container mx-auto px-4 py-8 text-center">
                  <h1 className="text-2xl font-bold text-gray-900 mb-4">Page Not Found</h1>
                  <p className="text-gray-600">The page you're looking for doesn't exist.</p>
                </div>
              }
            />
          </Routes>
        </main>
        <Footer />
        <ToastContainer />
      </div>
    </AuthProvider>
  )
}

export default App