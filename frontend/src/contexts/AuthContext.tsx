import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react'
import { User, LoginRequest } from '@/types/api'
import { authApi } from '@/services/api'
import { useToast } from '@/components/ui/Toast'

interface AuthContextType {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (credentials: LoginRequest) => Promise<boolean>
  logout: () => void
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export const useAuth = () => {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}

interface AuthProviderProps {
  children: ReactNode
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const { addToast } = useToast()

  const isAuthenticated = !!user

  useEffect(() => {
    // Check for existing auth token and user on mount
    const token = localStorage.getItem('authToken')
    const savedUser = localStorage.getItem('user')

    if (token && savedUser) {
      try {
        const parsedUser = JSON.parse(savedUser)
        setUser(parsedUser)
      } catch (error) {
        // Invalid user data, clear localStorage
        localStorage.removeItem('authToken')
        localStorage.removeItem('user')
      }
    }

    setIsLoading(false)
  }, [])

  const login = async (credentials: LoginRequest): Promise<boolean> => {
    try {
      const response = await authApi.login(credentials)

      if (response.success && response.data) {
        const { token, user: userData } = response.data

        // Store token and user data
        localStorage.setItem('authToken', token)
        localStorage.setItem('user', JSON.stringify(userData))

        setUser(userData)

        addToast({
          type: 'success',
          title: 'Login successful',
          message: `Welcome back, ${userData.username}!`
        })

        return true
      } else {
        addToast({
          type: 'error',
          title: 'Login failed',
          message: response.error?.message || 'Invalid credentials'
        })
        return false
      }
    } catch (error: any) {
      addToast({
        type: 'error',
        title: 'Login failed',
        message: error.error?.message || 'An unexpected error occurred'
      })
      return false
    }
  }

  const logout = async () => {
    try {
      // Call logout API (optional, as we're using JWT)
      await authApi.logout()
    } catch (error) {
      // Continue with logout even if API call fails
      console.error('Logout API call failed:', error)
    }

    // Clear local storage and state
    localStorage.removeItem('authToken')
    localStorage.removeItem('user')
    setUser(null)

    addToast({
      type: 'info',
      title: 'Logged out',
      message: 'You have been successfully logged out'
    })
  }

  const value: AuthContextType = {
    user,
    isAuthenticated,
    isLoading,
    login,
    logout,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}