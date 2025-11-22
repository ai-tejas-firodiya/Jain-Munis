# Jain Munis Tracking App

A comprehensive web application for tracking Jain saints (munis) and their schedules, allowing devotees to find where saints are currently staying and plan their visits.

## 🙏 Overview

The Jain Munis Tracking App is a modern, mobile-responsive platform designed to serve the Jain community by providing real-time information about:
- Current locations of Jain saints and spiritual leaders
- Upcoming schedules and visits to different cities
- Temple locations and contact information
- Administrative dashboard for schedule management

## 🏗️ Architecture

### Backend (ASP.NET Core Web API)
- **Framework**: .NET 8 with ASP.NET Core Web API
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT tokens with ASP.NET Core Identity
- **Caching**: Redis for performance optimization
- **File Storage**: Local/Cloud storage for saint photos
- **Logging**: Serilog for structured logging

### Frontend (React + TypeScript)
- **Framework**: Vite + React 18 with TypeScript
- **UI Library**: Tailwind CSS with custom components
- **State Management**: React Query (TanStack Query) for API state
- **Routing**: React Router v6
- **HTTP Client**: Axios with interceptors
- **Build Tool**: Vite with optimized bundling

## 🚀 Features

### For Devotees (Public Interface)
- **Saint Discovery**: Browse and search for Jain saints by name, location, or spiritual lineage
- **Real-time Schedules**: View current locations and upcoming visits
- **Location-based Search**: Find saints in specific cities or nearby areas
- **Interactive Map**: Visual representation of saint locations (placeholder for mapping service)
- **Detailed Profiles**: Complete saint information with photos and background
- **Mobile Responsive**: Optimized for mobile devices with PWA capabilities

### For Administrators (Admin Panel)
- **Secure Authentication**: Role-based access with JWT tokens
- **Saint Management**: Add, edit, and manage saint profiles
- **Schedule Management**: Create and manage saint schedules with conflict detection
- **Location Management**: Add and manage temple locations
- **Activity Logging**: Complete audit trail of all changes
- **User Management**: Multi-admin support with granular permissions
- **Dashboard Overview**: Real-time statistics and system health

## 📋 Core Features Implementation Status

### ✅ Completed Features

#### Backend API
- [x] Complete ASP.NET Core Web API setup
- [x] SQL Server database with Entity Framework Core
- [x] JWT authentication with ASP.NET Core Identity
- [x] CRUD operations for Saints, Locations, Schedules
- [x] Admin user management with role-based access
- [x] Search and filtering capabilities
- [x] Activity logging system
- [x] Database migrations with seed data
- [x] API documentation with Swagger/OpenAPI

#### Frontend Application
- [x] Vite + React + TypeScript setup
- [x] Responsive design with Tailwind CSS
- [x] Core UI components (Button, Card, Input, etc.)
- [x] Authentication context and protected routes
- [x] Saint listing and profile pages
- [x] Location-based search functionality
- [x] Admin dashboard with authentication
- [x] API service layer with error handling
- [x] Toast notifications system

### 🚧 Features In Progress
- [ ] Saint and schedule management forms (admin)
- [ ] File upload for saint photos
- [ ] Advanced map integration (Mapbox/Google Maps)

### 📅 Planned Features
- [ ] Push notifications for schedule updates
- [ ] Email notification system
- [ ] WhatsApp Business API integration
- [ ] Advanced analytics dashboard
- [ ] Multi-language support (Hindi, Gujarati)
- [ ] Mobile app (React Native)

## 🛠️ Technical Implementation

### Database Schema

The application uses a well-structured SQL Server database with the following main entities:

- **Saints**: Profiles of Jain saints with photos, lineage, and contact information
- **Locations**: Temples and spiritual centers with addresses and coordinates
- **Schedules**: Saint visit schedules with dates, purposes, and contact details
- **AdminUsers**: System administrators with role-based permissions
- **ActivityLogs**: Complete audit trail of all system changes
- **NotificationSubscriptions**: User preferences for notifications

### API Endpoints

#### Public Endpoints (No Authentication)
```
GET /api/saints              # List all saints with filtering
GET /api/saints/{id}         # Get saint details
GET /api/locations           # List locations with search
GET /api/schedules/current   # Current saint schedules
GET /api/schedules/upcoming  # Upcoming schedules
GET /api/search/cities       # City autocomplete
```

#### Admin Endpoints (Authentication Required)
```
POST /api/auth/login          # Admin authentication
GET /api/auth/profile         # Current user profile
POST /api/admin/saints        # Create saint
PUT /api/admin/saints/{id}   # Update saint
POST /api/admin/schedules     # Create schedule
PUT /api/admin/schedules/{id} # Update schedule
GET /api/admin/activity-logs  # Activity logs
```

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+ and npm
- SQL Server (or Docker)
- Redis (optional, for caching)

### Backend Setup

1. **Clone the repository**
```bash
git clone <repository-url>
cd Jain-Munis
```

2. **Configure the database**
```bash
cd JainMunis.API
# Update connection string in appsettings.json
dotnet ef database update
```

3. **Run the backend**
```bash
dotnet run
```

The API will be available at `http://localhost:5000`

### Frontend Setup

1. **Install dependencies**
```bash
cd frontend
npm install
```

2. **Configure environment variables**
```bash
cp .env.example .env
# Update API URL if needed
```

3. **Run the frontend**
```bash
npm run dev
```

The frontend will be available at `http://localhost:5173`

### Docker Setup (Optional)

1. **Using Docker Compose**
```bash
docker-compose up -d
```

This will start:
- SQL Server on port 1433
- Redis on port 6379
- Backend API on port 5000
- Frontend on port 5173

## 🔐 Default Credentials

**Admin Login:**
- Username: `admin@jainmunis.app`
- Password: `Admin@123`

## 📱 Mobile Responsiveness

The application is designed mobile-first with:
- Responsive breakpoints: 320px (mobile), 768px (tablet), 1024px (desktop)
- Touch-friendly interaction targets (minimum 44px)
- Optimized images and performance
- Progressive Web App (PWA) capabilities

## 🔒 Security Features

- JWT token-based authentication
- Role-based access control (RBAC)
- Input validation and sanitization
- SQL injection prevention
- XSS protection
- CORS configuration
- Rate limiting on sensitive operations
- Activity logging and audit trails

## 🧪 Testing

The application includes:
- Comprehensive API documentation with Swagger
- Seed data for testing
- Demo credentials for admin access
- Error handling and validation
- Mobile responsiveness testing

## 📈 Performance Optimizations

- **Backend**: Response caching, connection pooling, optimized queries
- **Frontend**: Code splitting, lazy loading, image optimization, bundle analysis
- **Database**: Strategic indexing, query optimization, read replicas support

## 🚀 Deployment

### Production Environment Variables
```bash
# Backend
ConnectionStrings__DefaultConnection=YourProductionConnectionString
JwtSettings__SecretKey=YourStrongProductionSecret
Redis=YourRedisConnectionString
FileStorage__Provider=AzureBlobStorage

# Frontend
VITE_API_URL=https://your-api-domain.com/api
```

### Deployment Options
- **Backend**: Azure App Service, AWS EC2, Railway
- **Database**: Azure SQL Database, AWS RDS
- **Frontend**: Vercel, Netlify, Azure Static Web Apps
- **Storage**: Azure Blob Storage, AWS S3
- **Monitoring**: Azure Monitor, Sentry

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Built for the Jain community to enhance spiritual connectivity
- Inspired by the need to organize and track saint movements
- Designed with modern web technologies for accessibility and performance
- Community-driven development approach

## 📞 Support

For support, questions, or suggestions:
- Email: info@jainmunis.app
- Create an issue in the GitHub repository
- Contact the development team

---

**Made with ❤️ for the Jain Community**