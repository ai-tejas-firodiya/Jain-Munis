// API Response Types
export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: ErrorDetail
  pagination?: PaginationDto
  timestamp: string
}

export interface ErrorDetail {
  code: string
  message: string
  details?: any
}

export interface PaginationDto {
  page: number
  limit: number
  total: number
  totalPages: number
}

// Entity Types
export interface Saint {
  id: string
  name: string
  title?: string
  spiritualLineage?: string
  bio?: string
  photoUrl?: string
  isActive: boolean
  createdAt: string
  updatedAt: string
  currentSchedule?: Schedule
  upcomingSchedules: Schedule[]
}

export interface Location {
  id: string
  name: string
  address: string
  city: string
  state?: string
  postalCode?: string
  country: string
  latitude?: number
  longitude?: number
  contactPhone?: string
  createdAt: string
  schedules: Schedule[]
}

export interface Schedule {
  id: string
  saintId: string
  saint?: Saint
  locationId: string
  location?: Location
  startDate: string
  endDate: string
  purpose?: string
  notes?: string
  contactPerson?: string
  contactPhone?: string
  createdAt: string
  updatedAt: string
  isCurrent: boolean
  isUpcoming: boolean
}

// Request Types
export interface CreateSaintRequest {
  name: string
  title?: string
  spiritualLineage?: string
  bio?: string
  phone?: string
  email?: string
}

export interface UpdateSaintRequest {
  name?: string
  title?: string
  spiritualLineage?: string
  bio?: string
  phone?: string
  email?: string
  isActive?: boolean
}

export interface CreateLocationRequest {
  name: string
  address: string
  city: string
  state?: string
  postalCode?: string
  country?: string
  latitude?: number
  longitude?: number
  contactPhone?: string
}

export interface UpdateLocationRequest {
  name?: string
  address?: string
  city?: string
  state?: string
  postalCode?: string
  country?: string
  latitude?: number
  longitude?: number
  contactPhone?: string
}

export interface CreateScheduleRequest {
  saintId: string
  locationId: string
  startDate: string
  endDate: string
  purpose?: string
  notes?: string
  contactPerson?: string
  contactPhone?: string
}

export interface UpdateScheduleRequest {
  saintId?: string
  locationId?: string
  startDate?: string
  endDate?: string
  purpose?: string
  notes?: string
  contactPerson?: string
  contactPhone?: string
}

// Auth Types
export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResponse {
  token: string
  expiresAt: string
  user: User
}

export interface User {
  id: string
  username: string
  email: string
  role: string
  lastLogin?: string
  isActive: boolean
}

// Search Types
export interface SearchParams {
  search?: string
  city?: string
  state?: string
  isActive?: boolean
  dateFrom?: Date
  dateTo?: Date
  latitude?: number
  longitude?: number
  radiusKm?: number
  saintId?: string
  daysAhead?: number
}