import axios, { AxiosResponse } from 'axios'
import type {
  ApiResponse,
  Saint,
  Location,
  Schedule,
  CreateSaintRequest,
  UpdateSaintRequest,
  CreateLocationRequest,
  UpdateLocationRequest,
  CreateScheduleRequest,
  UpdateScheduleRequest,
  LoginRequest,
  LoginResponse,
  User,
  SearchParams,
  PaginationDto
} from '@/types/api'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

// Create axios instance
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor to add auth token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token expired or invalid
      localStorage.removeItem('authToken')
      localStorage.removeItem('user')
      window.location.href = '/admin/login'
    }
    return Promise.reject(error)
  }
)

// Generic API request function
async function apiRequest<T>(url: string, options?: RequestInit): Promise<ApiResponse<T>> {
  try {
    const response: AxiosResponse<ApiResponse<T>> = await api({
      url,
      ...options,
    })
    return response.data
  } catch (error: any) {
    if (error.response?.data) {
      throw error.response.data
    }
    throw {
      success: false,
      error: {
        code: 'NETWORK_ERROR',
        message: error.message || 'An unexpected error occurred',
      },
      timestamp: new Date().toISOString(),
    }
  }
}

// Saints API
export const saintsApi = {
  getAll: async (params?: {
    page?: number
    limit?: number
    search?: string
    city?: string
    isActive?: boolean
  }): Promise<ApiResponse<{ saints: Saint[] } & PaginationDto>> => {
    const searchParams = new URLSearchParams()
    if (params?.page) searchParams.set('page', params.page.toString())
    if (params?.limit) searchParams.set('limit', params.limit.toString())
    if (params?.search) searchParams.set('search', params.search)
    if (params?.city) searchParams.set('city', params.city)
    if (params?.isActive !== undefined) searchParams.set('isActive', params.isActive.toString())

    const url = `/saints${searchParams.toString() ? `?${searchParams.toString()}` : ''}`
    return apiRequest(url)
  },

  getById: async (id: string): Promise<ApiResponse<Saint>> => {
    return apiRequest(`/saints/${id}`)
  },

  create: async (data: CreateSaintRequest): Promise<ApiResponse<Saint>> => {
    return apiRequest('/saints', {
      method: 'POST',
      data: JSON.stringify(data),
    })
  },

  update: async (id: string, data: UpdateSaintRequest): Promise<ApiResponse<Saint>> => {
    return apiRequest(`/saints/${id}`, {
      method: 'PUT',
      data: JSON.stringify(data),
    })
  },

  delete: async (id: string): Promise<ApiResponse<object>> => {
    return apiRequest(`/saints/${id}`, {
      method: 'DELETE',
    })
  },

  getByCity: async (city: string): Promise<ApiResponse<Saint[]>> => {
    return apiRequest(`/saints/city/${encodeURIComponent(city)}`)
  },

  getNearby: async (latitude: number, longitude: number, radiusKm = 50): Promise<ApiResponse<Saint[]>> => {
    return apiRequest(`/saints/nearby?latitude=${latitude}&longitude=${longitude}&radiusKm=${radiusKm}`)
  },

  updatePhoto: async (id: string, photoFile: File): Promise<ApiResponse<{ photoUrl: string }>> => {
    const formData = new FormData()
    formData.append('photo', photoFile)

    return apiRequest(`/saints/${id}/photo`, {
      method: 'POST',
      data: formData,
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
  },
}

// Locations API
export const locationsApi = {
  getAll: async (params?: {
    page?: number
    limit?: number
    search?: string
    city?: string
    state?: string
  }): Promise<ApiResponse<{ locations: Location[] } & PaginationDto>> => {
    const searchParams = new URLSearchParams()
    if (params?.page) searchParams.set('page', params.page.toString())
    if (params?.limit) searchParams.set('limit', params.limit.toString())
    if (params?.search) searchParams.set('search', params.search)
    if (params?.city) searchParams.set('city', params.city)
    if (params?.state) searchParams.set('state', params.state)

    const url = `/locations${searchParams.toString() ? `?${searchParams.toString()}` : ''}`
    return apiRequest(url)
  },

  getById: async (id: string): Promise<ApiResponse<Location>> => {
    return apiRequest(`/locations/${id}`)
  },

  create: async (data: CreateLocationRequest): Promise<ApiResponse<Location>> => {
    return apiRequest('/locations', {
      method: 'POST',
      data: JSON.stringify(data),
    })
  },

  update: async (id: string, data: UpdateLocationRequest): Promise<ApiResponse<Location>> => {
    return apiRequest(`/locations/${id}`, {
      method: 'PUT',
      data: JSON.stringify(data),
    })
  },

  delete: async (id: string): Promise<ApiResponse<object>> => {
    return apiRequest(`/locations/${id}`, {
      method: 'DELETE',
    })
  },

  getCities: async (query?: string): Promise<ApiResponse<string[]>> => {
    const url = query ? `/locations/cities?query=${encodeURIComponent(query)}` : '/locations/cities'
    return apiRequest(url)
  },

  getByCity: async (city: string): Promise<ApiResponse<Location[]>> => {
    return apiRequest(`/locations/city/${encodeURIComponent(city)}`)
  },
}

// Schedules API
export const schedulesApi = {
  getAll: async (params?: {
    page?: number
    limit?: number
    saintId?: string
    city?: string
    dateFrom?: Date
    dateTo?: Date
  }): Promise<ApiResponse<{ schedules: Schedule[] } & PaginationDto>> => {
    const searchParams = new URLSearchParams()
    if (params?.page) searchParams.set('page', params.page.toString())
    if (params?.limit) searchParams.set('limit', params.limit.toString())
    if (params?.saintId) searchParams.set('saintId', params.saintId)
    if (params?.city) searchParams.set('city', params.city)
    if (params?.dateFrom) searchParams.set('dateFrom', params.dateFrom.toISOString())
    if (params?.dateTo) searchParams.set('dateTo', params.dateTo.toISOString())

    const url = `/schedules${searchParams.toString() ? `?${searchParams.toString()}` : ''}`
    return apiRequest(url)
  },

  getById: async (id: string): Promise<ApiResponse<Schedule>> => {
    return apiRequest(`/schedules/${id}`)
  },

  create: async (data: CreateScheduleRequest): Promise<ApiResponse<Schedule>> => {
    return apiRequest('/schedules', {
      method: 'POST',
      data: JSON.stringify(data),
    })
  },

  update: async (id: string, data: UpdateScheduleRequest): Promise<ApiResponse<Schedule>> => {
    return apiRequest(`/schedules/${id}`, {
      method: 'PUT',
      data: JSON.stringify(data),
    })
  },

  delete: async (id: string): Promise<ApiResponse<object>> => {
    return apiRequest(`/schedules/${id}`, {
      method: 'DELETE',
    })
  },

  getCurrent: async (params?: { city?: string; saintId?: string }): Promise<ApiResponse<Schedule[]>> => {
    const searchParams = new URLSearchParams()
    if (params?.city) searchParams.set('city', params.city)
    if (params?.saintId) searchParams.set('saintId', params.saintId)

    const url = `/schedules/current${searchParams.toString() ? `?${searchParams.toString()}` : ''}`
    return apiRequest(url)
  },

  getUpcoming: async (params?: {
    city?: string
    saintId?: string
    daysAhead?: number
  }): Promise<ApiResponse<Schedule[]>> => {
    const searchParams = new URLSearchParams()
    if (params?.city) searchParams.set('city', params.city)
    if (params?.saintId) searchParams.set('saintId', params.saintId)
    if (params?.daysAhead) searchParams.set('daysAhead', params.daysAhead.toString())

    const url = `/schedules/upcoming${searchParams.toString() ? `?${searchParams.toString()}` : ''}`
    return apiRequest(url)
  },

  getBySaint: async (saintId: string): Promise<ApiResponse<Schedule[]>> => {
    return apiRequest(`/schedules/saint/${saintId}`)
  },

  checkConflicts: async (params: {
    saintId: string
    startDate: string
    endDate: string
    excludeScheduleId?: string
  }): Promise<ApiResponse<Schedule[]>> => {
    const searchParams = new URLSearchParams({
      saintId: params.saintId,
      startDate: params.startDate,
      endDate: params.endDate,
    })
    if (params.excludeScheduleId) {
      searchParams.set('excludeScheduleId', params.excludeScheduleId)
    }

    return apiRequest(`/schedules/overlap?${searchParams.toString()}`)
  },
}

// Auth API
export const authApi = {
  login: async (credentials: LoginRequest): Promise<ApiResponse<LoginResponse>> => {
    return apiRequest('/auth/login', {
      method: 'POST',
      data: JSON.stringify(credentials),
    })
  },

  logout: async (): Promise<ApiResponse<object>> => {
    return apiRequest('/auth/logout', {
      method: 'POST',
    })
  },

  getProfile: async (): Promise<ApiResponse<User>> => {
    return apiRequest('/auth/profile')
  },
}

// Search API
export const searchApi = {
  getNearby: async (params: {
    latitude: number
    longitude: number
    radiusKm?: number
    currentOnly?: boolean
  }): Promise<ApiResponse<Schedule[]>> => {
    const searchParams = new URLSearchParams({
      latitude: params.latitude.toString(),
      longitude: params.longitude.toString(),
    })
    if (params.radiusKm) searchParams.set('radiusKm', params.radiusKm.toString())
    if (params.currentOnly) searchParams.set('currentOnly', params.currentOnly.toString())

    return apiRequest(`/search/nearby?${searchParams.toString()}`)
  },

  getCities: async (query?: string): Promise<ApiResponse<string[]>> => {
    const url = query ? `/search/cities?query=${encodeURIComponent(query)}` : '/search/cities'
    return apiRequest(url)
  },

  getSuggestions: async (query: string): Promise<ApiResponse<any>> => {
    return apiRequest(`/search/suggestions?query=${encodeURIComponent(query)}`)
  },

  advanced: async (params: {
    search?: string
    city?: string
    state?: string
    dateFrom?: Date
    dateTo?: Date
    currentOnly?: boolean
    page?: number
    limit?: number
  }): Promise<ApiResponse<any>> => {
    const searchParams = new URLSearchParams()
    if (params.search) searchParams.set('search', params.search)
    if (params.city) searchParams.set('city', params.city)
    if (params.state) searchParams.set('state', params.state)
    if (params.dateFrom) searchParams.set('dateFrom', params.dateFrom.toISOString())
    if (params.dateTo) searchParams.set('dateTo', params.dateTo.toISOString())
    if (params.currentOnly) searchParams.set('currentOnly', params.currentOnly.toString())
    if (params.page) searchParams.set('page', params.page.toString())
    if (params.limit) searchParams.set('limit', params.limit.toString())

    const url = `/search/advanced${searchParams.toString() ? `?${searchParams.toString()}` : ''}`
    return apiRequest(url)
  },
}

export default api