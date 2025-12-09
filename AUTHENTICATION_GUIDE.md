# Authentication & Authorization Implementation

## Overview
This document describes the authentication and authorization features added to the Self Finance Manager Blazor (SFMB) application. The implementation follows .NET best practices and security patterns for Blazor Server applications.

## Table of Contents
- [Features Implemented](#features-implemented)
- [Architecture](#architecture)
- [Files Added](#files-added)
- [Files Modified](#files-modified)
- [Dependencies Added](#dependencies-added)
- [Configuration](#configuration)
- [Usage](#usage)
- [Security Considerations](#security-considerations)
- [API Requirements](#api-requirements)
- [Testing](#testing)

## Features Implemented

### 1. User Authentication
- **Login Page** (`/login`) - User authentication with email and password
- **Registration Page** (`/register`) - New user account creation with validation
- **Forgot Password** (`/forgot-password`) - Password reset functionality
- **Logout Functionality** - Secure session termination

### 2. Authorization
- Protected routes requiring authentication for:
  - Operations (`/operations`)
  - Operation Types (`/operation-types`)
  - Daily Report (`/daily-report`)
  - Period Report (`/period-report`)
- Public access for:
  - Home page (`/`)
  - Login page (`/login`)
  - Registration page (`/register`)
  - Forgot Password page (`/forgot-password`)

### 3. User Interface Enhancements
- User information display in the main layout (username)
- Logout button in the navigation bar
- Automatic redirect to login for unauthorized access
- Login/Signup buttons for anonymous users

## Architecture

### Authentication Flow
1. **User Login/Registration**: User submits credentials through login or registration form
2. **API Communication**: `AuthService` sends request to the backend API
3. **Session Storage**: On successful authentication, user information is stored in `ProtectedSessionStorage`
4. **Authentication State**: `CustomAuthenticationStateProvider` manages authentication state
5. **Protected Routes**: `AuthorizeRouteView` checks authentication status and redirects if needed

### Key Components

#### Authentication State Provider
`CustomAuthenticationStateProvider` extends `AuthenticationStateProvider` to manage user authentication state throughout the application.

#### Authentication Service
`AuthService` implements `IAuthService` to handle all authentication-related API calls:
- Login
- Registration
- Password reset
- Logout
- Get current user

#### Models
- `LoginRequest` - Login credentials
- `RegisterRequest` - New user registration data
- `ForgotPasswordRequest` - Password reset request
- `UserInfo` - Current user information
- `AuthResponse` - API response wrapper

## Files Added

### Authentication Infrastructure
```
BlazorApp.UI/
├── Auth/
│   ├── CustomAuthenticationStateProvider.cs
│   ├── Models/
│   │   ├── AuthResponse.cs
│   │   ├── ForgotPasswordRequest.cs
│   │   ├── LoginRequest.cs
│   │   ├── RegisterRequest.cs
│   │   └── UserInfo.cs
│   └── Services/
│       ├── AuthService.cs
│       └── IAuthService.cs
```

### UI Pages
```
BlazorApp.UI/Components/Pages/
├── Login.razor
├── Register.razor
└── ForgotPassword.razor
```

### Components
```
BlazorApp.UI/Components/Components/
└── RedirectToLogin.razor
```

## Files Modified

### Core Application Files
1. **Program.cs**
   - Added authentication and authorization services
   - Registered `CustomAuthenticationStateProvider`
   - Registered `IAuthService` and `AuthService`
   - Added authentication middleware

2. **Routes.razor**
   - Wrapped router in `CascadingAuthenticationState`
   - Changed from `RouteView` to `AuthorizeRouteView`
   - Added redirect to login for unauthorized access

3. **_Imports.razor**
   - Added `Microsoft.AspNetCore.Components.Authorization`
   - Added `Microsoft.AspNetCore.Authorization`

4. **MainLayout.razor**
   - Added `AuthorizeView` component
   - Display user information when authenticated
   - Added logout button
   - Show login button for anonymous users

### Protected Pages
Added `@attribute [Authorize]` directive to:
- `Operations.razor`
- `OperationTypes.razor`
- `DailyReport.razor`
- `PeriodReport.razor`

### Public Pages
Added `@attribute [AllowAnonymous]` directive to:
- `Home.razor`
- `Login.razor`
- `Register.razor`
- `ForgotPassword.razor`

## Dependencies Added

### NuGet Packages
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.Cookies" Version="2.2.0" />
<PackageReference Include="Microsoft.AspNetCore.Components.Authorization" Version="9.0.0" />
```

## Configuration

### Program.cs Services
```csharp
// Authentication and authorization
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Custom authentication services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
```

### Middleware Pipeline
```csharp
app.UseAuthentication();
app.UseAuthorization();
```

## Usage

### Login
1. Navigate to `/login`
2. Enter email and password
3. Click "Login" button
4. On success, redirected to home page with authenticated session

### Registration
1. Navigate to `/register`
2. Fill in:
   - Name
   - Email
   - Password
   - Confirm Password
3. Click "Create Account" button
4. On success, automatically logged in and redirected to home page

### Forgot Password
1. Navigate to `/forgot-password`
2. Enter registered email address
3. Click "Send Reset Instructions"
4. Check email for password reset link (handled by backend API)

### Logout
1. Click "Logout" button in the top navigation bar
2. Session cleared and redirected to login page

## Security Considerations

### Implemented Security Measures

1. **Protected Session Storage**
   - User credentials stored using `ProtectedSessionStorage`
   - Data encrypted at rest

2. **Form Validation**
   - Client-side validation using Data Annotations
   - Email format validation
   - Password minimum length (6 characters)
   - Password confirmation matching

3. **Route Protection**
   - Automatic redirect to login for protected routes
   - `[Authorize]` attribute on protected pages
   - `[AllowAnonymous]` attribute on public pages

4. **HTTPS Consideration**
   - HTTPS redirection commented but prepared in code
   - Should be enabled in production: `app.UseHttpsRedirection();`

5. **CORS Policy**
   - Configured for specific domain
   - Restricts unauthorized cross-origin requests

### Recommendations for Production

1. **Enable HTTPS**
   ```csharp
   app.UseHttpsRedirection(); // Uncomment in Program.cs
   ```

2. **Stronger Password Policy**
   - Increase minimum password length to 8+ characters
   - Require password complexity (uppercase, lowercase, numbers, special characters)

3. **Rate Limiting**
   - Implement rate limiting on login and registration endpoints
   - Prevent brute force attacks

4. **Multi-Factor Authentication (MFA)**
   - Consider adding 2FA/MFA for enhanced security

5. **Session Timeout**
   - Configure appropriate session timeout duration
   - Implement automatic logout on inactivity

6. **JWT Tokens (Optional Enhancement)**
   - Consider using JWT tokens instead of session storage
   - Better for scalability and stateless authentication

## API Requirements

The frontend expects the following API endpoints to be implemented on the backend:

### Authentication Endpoints

#### POST /api/auth/login
**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response (Success - 200 OK):**
```json
{
  "success": true,
  "message": "Login successful",
  "user": {
    "userId": "123",
    "email": "user@example.com",
    "name": "John Doe",
    "token": "jwt-token-here"
  }
}
```

**Response (Failure - 400/401):**
```json
{
  "success": false,
  "message": "Invalid credentials"
}
```

#### POST /api/auth/register
**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "password123",
  "confirmPassword": "password123",
  "name": "John Doe"
}
```

**Response (Success - 200 OK):**
```json
{
  "success": true,
  "message": "Registration successful",
  "user": {
    "userId": "123",
    "email": "user@example.com",
    "name": "John Doe",
    "token": "jwt-token-here"
  }
}
```

**Response (Failure - 400):**
```json
{
  "success": false,
  "message": "Email already exists"
}
```

#### POST /api/auth/forgot-password
**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response (Success - 200 OK):**
```json
{
  "success": true,
  "message": "Password reset instructions sent to email"
}
```

**Response (Failure - 400):**
```json
{
  "success": false,
  "message": "Email not found"
}
```

### Backend Implementation Notes

1. **Password Hashing**: Use strong hashing algorithms (bcrypt, Argon2)
2. **Token Generation**: Generate secure JWT tokens with appropriate expiration
3. **Email Service**: Implement email service for password reset functionality
4. **Database**: Store user credentials securely in PostgreSQL database
5. **Validation**: Server-side validation for all inputs

### Example Backend Model (C#)
```csharp
public class User
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
```

## Testing

### Manual Testing Checklist

#### Authentication Flow
- [ ] Test login with valid credentials
- [ ] Test login with invalid credentials
- [ ] Test registration with valid data
- [ ] Test registration with existing email
- [ ] Test registration with mismatched passwords
- [ ] Test forgot password with valid email
- [ ] Test forgot password with invalid email
- [ ] Test logout functionality

#### Authorization Flow
- [ ] Access protected routes without authentication (should redirect to login)
- [ ] Access protected routes with authentication (should allow access)
- [ ] Access public routes without authentication (should allow access)
- [ ] Verify user info displayed in navigation bar
- [ ] Verify logout clears session

#### Form Validation
- [ ] Test empty form submission
- [ ] Test invalid email format
- [ ] Test password too short
- [ ] Test password mismatch on registration

### Integration Testing
To fully test the authentication system, you need:
1. Backend API running with authentication endpoints implemented
2. PostgreSQL database configured
3. SMTP server configured for password reset emails

### Testing Without Backend
For frontend development without backend, you can modify `AuthService.cs` to return mock responses:

```csharp
// Temporary mock implementation for testing
public async Task<AuthResponse> LoginAsync(LoginRequest request)
{
    await Task.Delay(1000); // Simulate API delay
    
    if (request.Email == "test@example.com" && request.Password == "password123")
    {
        var user = new UserInfo
        {
            UserId = "123",
            Email = request.Email,
            Name = "Test User",
            Token = "mock-token"
        };
        
        await _sessionStorage.SetAsync(UserSessionKey, user);
        
        return new AuthResponse
        {
            Success = true,
            Message = "Login successful",
            User = user
        };
    }
    
    return new AuthResponse
    {
        Success = false,
        Message = "Invalid credentials"
    };
}
```

## Summary

This implementation provides a complete authentication and authorization system for the SFMB Blazor application, following .NET best practices and security patterns. The system is ready for integration with the backend API and can be easily extended with additional features such as role-based authorization, multi-factor authentication, or OAuth providers.

## Next Steps

1. **Backend Implementation**: Implement the required API endpoints
2. **Database Schema**: Create user tables in PostgreSQL
3. **Email Service**: Configure SMTP for password reset emails
4. **Production Deployment**: Enable HTTPS and configure production settings
5. **Enhanced Security**: Implement rate limiting and stronger password policies
6. **User Management**: Add user profile management features
7. **Role-Based Access**: Implement role-based authorization if needed

## Support and Documentation

For questions or issues related to the authentication implementation:
- Review ASP.NET Core Authentication documentation: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/
- Review Blazor Security documentation: https://docs.microsoft.com/en-us/aspnet/core/blazor/security/

---

**Author**: GitHub Copilot
**Date**: December 2024
**Version**: 1.0
