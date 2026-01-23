# Self Finance Manager Blazor (SFMB)

UI part for WebApiForAz - A personal finance management application built with Blazor Server.

## Features

- **Self Finance Management**: Track your income and expenses
- **Operations Management**: Create, edit, and delete financial operations
- **Operation Types**: Manage categories for your transactions
- **Reporting**: Daily and period-based financial reports
- **Authentication & Authorization**: Secure user login, registration, and password recovery

## Recent Updates

### Authentication & Authorization System
Complete authentication and authorization system has been implemented:

- **User Authentication**: Secure login and registration
- **Password Management**: Forgot password functionality
- **Protected Routes**: Authorization required for financial operations and reports
- **Session Management**: Secure user session handling
- **User Interface**: Login/logout buttons, user info display

For detailed information about the authentication implementation, see [AUTHENTICATION_GUIDE.md](AUTHENTICATION_GUIDE.md).

## Technology Stack

- **Frontend**: Blazor Server (.NET 9.0)
- **Backend API**: .NET Core Web API
- **Database**: PostgreSQL
- **Hosting**: DigitalOcean
- **Authentication**: ASP.NET Core Identity with session-based authentication

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- PostgreSQL database
- Backend API running (WebApiForAz)

### Configuration

1. Set the API URL in environment variables or `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "ApiBaseUrl": "https://your-api-url.com"
     }
   }
   ```
   Or use environment variable: `API_URL`

2. Build and run the application:
   ```bash
   dotnet build
   dotnet run
   ```

### Authentication Setup

For the authentication system to work properly, the backend API must implement the following endpoints:

- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/forgot-password` - Password reset

See [AUTHENTICATION_GUIDE.md](AUTHENTICATION_GUIDE.md) for detailed API requirements and implementation guide.

## Project Structure

The project follows a **layered architecture pattern** for better separation of concerns, maintainability, and testability. For detailed information about the architecture, see [ARCHITECTURE.md](ARCHITECTURE.md).

```
SFMB.UI/
├── BlazorApp.UI/
│   ├── Application/              # Application services and business logic
│   │   ├── Interfaces/           # Service interfaces
│   │   └── Services/             # Service implementations
│   ├── Domain/                   # Core business models
│   │   └── Models/               # Domain entities
│   ├── Infrastructure/           # External services and infrastructure
│   │   ├── Auth/                 # Authentication infrastructure
│   │   ├── Dtos/                 # Data transfer objects
│   │   └── ApiClients/           # API client implementations
│   ├── Presentation/             # UI layer
│   │   ├── Components/           # Reusable UI components
│   │   ├── Layout/               # Layout components
│   │   └── Pages/                # Razor pages
│   └── Program.cs                # Application startup
├── ARCHITECTURE.md               # Detailed architecture documentation
├── AUTHENTICATION_GUIDE.md       # Detailed authentication documentation
└── README.md                     # This file
```

### Architecture Layers

1. **Presentation Layer**: User interface components and pages
2. **Application Layer**: Business logic and service orchestration
3. **Domain Layer**: Core business entities and models
4. **Infrastructure Layer**: External services, authentication, and DTOs

For more details on the architecture, see [ARCHITECTURE.md](ARCHITECTURE.md).

## Usage

### First Time Users

1. Navigate to the application URL
2. Click "Sign Up" to create an account
3. Fill in your details (name, email, password)
4. After registration, you'll be automatically logged in
5. Start managing your finances!

### Existing Users

1. Navigate to the application URL
2. Click "Login"
3. Enter your email and password
4. Access your financial operations and reports

### Forgot Password

1. Click "Forgot Password?" on the login page
2. Enter your email address
3. Check your email for password reset instructions

## Security

The application implements several security measures:

- Password validation (minimum 6 characters)
- Email format validation
- Protected session storage with encryption
- Route-level authorization
- CORS policy configuration
- HTTPS ready (to be enabled in production)

**Important**: Before deploying to production, ensure:
- HTTPS is enabled
- Strong password policies are enforced
- Rate limiting is implemented
- Database credentials are secured

## License

See LICENSE.txt for details.

## Support

For issues, questions, or contributions, please open an issue in the repository.
