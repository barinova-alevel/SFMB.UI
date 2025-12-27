# Technology Stack

This document provides a comprehensive overview of all technologies, frameworks, libraries, and tools used in the Self Finance Manager Blazor (SFMB) application.

## Overview

SFMB is a personal finance management application built using modern .NET technologies with a focus on server-side rendering, security, and ease of deployment.

## Table of Contents
- [Frontend Technologies](#frontend-technologies)
- [Backend Technologies](#backend-technologies)
- [Authentication & Authorization](#authentication--authorization)
- [Database](#database)
- [Infrastructure & Deployment](#infrastructure--deployment)
- [Development Tools](#development-tools)
- [NuGet Packages](#nuget-packages)
- [Architecture Pattern](#architecture-pattern)

## Frontend Technologies

### Blazor Server (.NET 9.0)
- **Version**: .NET 9.0
- **Type**: Server-side Blazor with Interactive Server Components
- **Purpose**: Core UI framework for building interactive web applications
- **Key Features Used**:
  - Interactive Server Render Mode
  - Razor Components
  - Component-based architecture
  - Real-time updates via SignalR
  - Server-side state management

### Bootstrap 5
- **Purpose**: CSS framework for responsive design and UI components
- **Integration**: Local static files in `wwwroot/lib/bootstrap`
- **Features Used**:
  - Responsive grid system
  - Form controls and validation styling
  - Utility classes
  - Modal dialogs
  - Navigation components
  - Card components
  - RTL (Right-to-Left) support

### Bootstrap Icons
- **Version**: 1.11.1
- **Source**: CDN (jsdelivr.net)
- **Purpose**: Icon library for UI elements
- **Integration**: Loaded via CDN in App.razor

### CSS
- **Custom Styles**: `app.css` for application-specific styling
- **Component Styles**: Scoped CSS files (e.g., `OperationTypes.razor.css`, `PeriodReport.razor.css`)
- **Blazor Generated**: Auto-generated component styles (`BlazorApp.UI.styles.css`)

## Backend Technologies

### ASP.NET Core 9.0
- **Framework**: ASP.NET Core Web SDK
- **Version**: .NET 9.0
- **Runtime**: ASP.NET Core Runtime
- **Purpose**: Web server and application host

### C# Language
- **Version**: C# 12 (with .NET 9.0)
- **Features Used**:
  - Nullable reference types (`<Nullable>enable</Nullable>`)
  - Implicit usings (`<ImplicitUsings>enable</ImplicitUsings>`)
  - Async/await patterns
  - Record types
  - Modern C# syntax

### HTTP Client
- **Implementation**: `IHttpClientFactory`
- **Purpose**: Communication with backend API (WebApiForAz)
- **Configuration**: Named client "Api" with configurable base address

## Authentication & Authorization

### ASP.NET Core Identity Infrastructure
- **Authentication Scheme**: Cookie-based authentication
- **Package**: `Microsoft.AspNetCore.Authentication.Cookies` v2.2.0
- **Authorization**: `Microsoft.AspNetCore.Components.Authorization` v9.0.0
- **Session Storage**: Protected session storage with encryption
- **Key Features**:
  - User login and registration
  - Password management (forgot password)
  - Route-level authorization
  - Claims-based authentication
  - Cascading authentication state

### Custom Authentication Components
- **AuthenticationStateProvider**: Custom implementation for Blazor
- **AuthService**: Service layer for authentication operations
- **AuthStore**: State management for authentication data
- **Protected Session Storage**: Encrypted user data storage

### Data Protection
- **Package**: ASP.NET Core Data Protection
- **Purpose**: Encrypt session data and cookies
- **Key Storage**:
  - Production: `/app/data/protection`
  - Development: Local application data folder
- **Application Name**: "goldfish-app"

## Database

### PostgreSQL
- **Type**: Relational database
- **Purpose**: Backend data storage (handled by WebApiForAz API)
- **Access**: Via REST API calls (not direct database access from UI)
- **Entity Framework Core**: v9.0.8 (for potential future direct database access)

## Infrastructure & Deployment

### Docker
- **Base Image (Build)**: `mcr.microsoft.com/dotnet/sdk:9.0`
- **Base Image (Runtime)**: `mcr.microsoft.com/dotnet/aspnet:9.0`
- **Multi-stage Build**: Yes (build stage + runtime stage)
- **Exposed Port**: 8080
- **Container Structure**:
  - Build stage: Restores and publishes the application
  - Runtime stage: Minimal runtime image with published application

### DigitalOcean
- **Platform**: Cloud hosting platform
- **Deployment**: Containerized application deployment
- **Domain**: goldfish-app-j6a9p.ondigitalocean.app

### Environment Configuration
- **Configuration System**: ASP.NET Core Configuration
- **Settings Files**:
  - `appsettings.json` - Base configuration
  - `appsettings.Development.json` - Development overrides
- **Environment Variables**:
  - `API_URL` - Backend API base URL
  - Standard ASP.NET Core environment variables

## Development Tools

### .NET SDK
- **Version**: 9.0
- **Tools**:
  - `dotnet build` - Build the application
  - `dotnet run` - Run the application locally
  - `dotnet restore` - Restore NuGet packages
  - `dotnet publish` - Publish for deployment

### Build System
- **Build Tool**: MSBuild (via .NET SDK)
- **Project File**: `BlazorApp.UI.csproj`
- **Solution File**: `SFMB.UI.sln`
- **Output**: Self-contained web application

### Launch Settings
- **Configuration**: `launchSettings.json`
- **Profiles**: Development profiles with different settings
- **Default Port**: 7132 (HTTPS), 5000 (HTTP)

## NuGet Packages

### Core Packages
1. **Microsoft.AspNetCore.Authentication.Cookies** (v2.2.0)
   - Cookie-based authentication support
   - Session management

2. **Microsoft.AspNetCore.Components.Authorization** (v9.0.0)
   - Authorization infrastructure for Blazor
   - AuthorizeView component
   - Route-level authorization

3. **Microsoft.EntityFrameworkCore** (v9.0.8)
   - ORM framework (prepared for future use)
   - Database context management

### Implicit Packages
These packages are automatically included with the `Microsoft.NET.Sdk.Web` SDK:
- Microsoft.AspNetCore.Components
- Microsoft.AspNetCore.Components.Web
- Microsoft.AspNetCore.Components.Forms
- Microsoft.AspNetCore.Razor
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging
- Microsoft.Extensions.Http

## Architecture Pattern

### Application Architecture
- **Pattern**: Component-based architecture with service layer
- **UI Layer**: Razor components (Pages and Components)
- **Service Layer**: Authentication services, HTTP client services
- **Data Layer**: DTOs (Data Transfer Objects) and Models

### Project Structure
```
SFMB.UI/
├── BlazorApp.UI/                    # Main application project
│   ├── Auth/                        # Authentication infrastructure
│   │   ├── CustomAuthenticationStateProvider.cs
│   │   ├── AuthStore.cs
│   │   ├── Models/                  # Auth-related models
│   │   └── Services/                # Auth services
│   ├── Components/
│   │   ├── App.razor                # Root component
│   │   ├── Routes.razor             # Routing configuration
│   │   ├── Pages/                   # Page components
│   │   ├── Layout/                  # Layout components
│   │   └── Components/              # Reusable UI components
│   ├── Dtos/                        # Data transfer objects
│   ├── Models/                      # Domain models
│   ├── Properties/                  # Launch settings
│   ├── wwwroot/                     # Static assets
│   │   ├── lib/                     # Third-party libraries
│   │   ├── app.css                  # Application styles
│   │   └── favicon.png              # Application icon
│   ├── Program.cs                   # Application entry point
│   └── appsettings.json             # Configuration
├── Dockerfile                        # Docker container definition
├── SFMB.UI.sln                      # Visual Studio solution
└── README.md                        # Project documentation
```

### Design Patterns Used
1. **Dependency Injection**: Services registered and injected throughout the application
2. **Repository Pattern**: Separation of data access logic
3. **Service Layer Pattern**: Business logic encapsulated in services
4. **Component Pattern**: Reusable UI components
5. **Authentication State Pattern**: Centralized authentication state management

## API Integration

### Backend API (WebApiForAz)
- **Communication**: RESTful HTTP calls
- **Format**: JSON
- **Client**: Named HttpClient with configured base address
- **Endpoints**:
  - Authentication: `/api/auth/*`
  - Operations: `/api/operations/*`
  - Operation Types: `/api/operationtypes/*`
  - Reports: `/api/reports/*`

## Localization

### Culture Configuration
- **Default Culture**: Ukrainian (uk-UA)
- **UI Culture**: Ukrainian (uk-UA)
- **Configuration**: Set in Program.cs startup

## Security Features

### Implemented Security Measures
1. **HTTPS Support**: HTTPS redirection available (currently disabled for development)
2. **CORS Policy**: Configured for specific domain origin
3. **Cookie Security**: Secure cookie configuration with 24-hour max age
4. **Data Protection**: Encrypted session storage
5. **Antiforgery**: Antiforgery token validation
6. **Authorization**: Route-level access control
7. **HSTS**: HTTP Strict Transport Security (enabled in production)

### Security Packages
- ASP.NET Core Data Protection (built-in)
- Cookie Authentication
- Authorization middleware

## Performance Considerations

### Optimization Features
1. **Server-side Rendering**: Reduced client-side processing
2. **Static Asset Mapping**: Efficient asset delivery
3. **Minimal JavaScript**: Only Blazor Server framework JavaScript required (no custom JavaScript needed)
4. **Component Scoping**: Isolated component styles
5. **SignalR**: Efficient real-time communication

## Future Considerations

### Potential Enhancements
1. **Blazor WebAssembly**: Consider hybrid or WASM-only mode for offline support
2. **Progressive Web App (PWA)**: Add PWA capabilities
3. **Advanced Caching**: Implement response caching
4. **CDN Integration**: Use CDN for static assets
5. **Health Checks**: Add health check endpoints
6. **Logging**: Implement structured logging (Serilog, NLog)
7. **Monitoring**: Add Application Insights or similar
8. **Testing**: Add unit and integration tests

## Version Information

| Component | Version |
|-----------|---------|
| .NET | 9.0 |
| C# | 12 |
| ASP.NET Core | 9.0 |
| Blazor | 9.0 (Server) |
| Bootstrap | 5.x |
| Bootstrap Icons | 1.11.1 |
| Entity Framework Core | 9.0.8 |
| PostgreSQL | Latest (via API) |

## Documentation References

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [Bootstrap Documentation](https://getbootstrap.com/docs/)
- [.NET 9.0 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)

## Related Documents

- [README.md](README.md) - Project overview and getting started guide
- [AUTHENTICATION_GUIDE.md](AUTHENTICATION_GUIDE.md) - Detailed authentication implementation guide
- [KNOWN_ISSUES.md](KNOWN_ISSUES.md) - Known issues and configuration notes

---

**Last Updated**: December 2024  
**Maintained by**: Development Team
