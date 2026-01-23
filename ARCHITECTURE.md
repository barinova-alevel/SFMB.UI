# Layered Architecture Documentation

## Overview

The SFMB.UI project follows a **layered architecture pattern** to ensure separation of concerns, maintainability, and testability. The architecture is organized into four distinct layers, each with specific responsibilities.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│  (Components, Pages, Layout - User Interface)               │
│  - Operations.razor, OperationTypes.razor                   │
│  - Login.razor, Register.razor                              │
│  - MainLayout.razor, NavMenu.razor                          │
└────────────────────┬────────────────────────────────────────┘
                     │ Depends on (Injects Services)
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│  (Services, Business Logic)                                 │
│  - IOperationService / OperationService                     │
│  - IOperationTypeService / OperationTypeService             │
└────────────────────┬────────────────────────────────────────┘
                     │ Uses Models & DTOs
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│  (Core Business Models)                                     │
│  - OperationModel, OperationTypeModel                       │
│  - DailyReportModel, PeriodReportModel                      │
└─────────────────────────────────────────────────────────────┘
                     ▲
                     │ Used by
                     │
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  (External Services, Auth, DTOs)                            │
│  - Auth (IAuthService, CustomAuthenticationStateProvider)   │
│  - DTOs (OperationDtoBlazor, etc.)                          │
│  - API Clients (Future)                                     │
└─────────────────────────────────────────────────────────────┘
```

## Architecture Layers

### 1. Presentation Layer (`Presentation/`)

**Responsibility**: User interface and user interaction handling

**Contains**:
- **Components/**: Reusable UI components (modals, shared components)
- **Pages/**: Blazor pages representing different views (Operations, Reports, Auth pages)
- **Layout/**: Layout components (MainLayout, NavMenu)

**Key Characteristics**:
- Contains Blazor Razor components (.razor files) and their code-behind (.razor.cs files)
- Handles user input validation and UI state management
- Injects and uses services from the Application layer
- Should NOT contain business logic or direct HTTP calls
- Namespace: `BlazorApp.UI.Presentation.*`

### 2. Application Layer (`Application/`)

**Responsibility**: Business logic and application services

**Contains**:
- **Interfaces/**: Service interfaces defining contracts
  - `IOperationService.cs`
  - `IOperationTypeService.cs`
- **Services/**: Service implementations containing business logic
  - `OperationService.cs`
  - `OperationTypeService.cs`

**Key Characteristics**:
- Orchestrates data flow between Presentation and Infrastructure layers
- Contains business rules and validation logic
- Makes HTTP calls to external APIs
- Implements service interfaces
- Namespace: `BlazorApp.UI.Application.*`

### 3. Domain Layer (`Domain/`)

**Responsibility**: Core business entities and models

**Contains**:
- **Models/**: Domain models representing core business entities
  - `OperationModel.cs`
  - `OperationTypeModel.cs`
  - `DailyReportModel.cs`
  - `PeriodReportModel.cs`

**Key Characteristics**:
- Contains data annotations for validation
- Pure domain models without infrastructure dependencies
- Shared across all layers
- Namespace: `BlazorApp.UI.Domain.*`

### 4. Infrastructure Layer (`Infrastructure/`)

**Responsibility**: External services, authentication, and data transfer objects

**Contains**:
- **Auth/**: Authentication and authorization infrastructure
  - **Models/**: Auth-related models (LoginRequest, RegisterRequest, UserInfo, etc.)
  - **Services/**: Auth service implementations (AuthService, CustomAuthenticationStateProvider)
- **Dtos/**: Data Transfer Objects for API communication
  - `OperationDtoBlazor.cs`
  - `OperationTypeDtoBlazor.cs`
  - `DailyReportDtoBlazor.cs`
  - `PeriodReportDtoBlazor.cs`
- **ApiClients/**: (Future) Dedicated API client classes

**Key Characteristics**:
- Handles external service integrations
- Authentication and session management
- DTOs for serialization/deserialization
- Infrastructure concerns (CORS, data protection, etc.)
- Namespace: `BlazorApp.UI.Infrastructure.*`

## Dependency Flow

```
Presentation Layer
      ↓ (depends on)
Application Layer
      ↓ (depends on)
Domain Layer
      ↑ (used by)
Infrastructure Layer
```

**Important Rules**:
1. Upper layers can depend on lower layers
2. Lower layers should NOT depend on upper layers
3. Domain layer is independent and has no dependencies
4. Infrastructure layer can access Domain layer but should not depend on Application or Presentation

## Service Registration

Services are registered in `Program.cs`:

```csharp
// Application Layer Services
builder.Services.AddScoped<IOperationService, OperationService>();
builder.Services.AddScoped<IOperationTypeService, OperationTypeService>();

// Infrastructure Layer Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
```

## Benefits of This Architecture

1. **Separation of Concerns**: Each layer has a single, well-defined responsibility
2. **Maintainability**: Changes in one layer have minimal impact on others
3. **Testability**: Services can be easily mocked and tested independently
4. **Scalability**: Easy to add new features following the established pattern
5. **Code Reusability**: Services can be reused across different pages/components
6. **Clear Dependencies**: Explicit service dependencies via dependency injection

## Usage Examples

### Injecting Services in Pages

```razor
@page "/operations"
@using BlazorApp.UI.Domain.Models
@using BlazorApp.UI.Application.Interfaces
@inject IOperationService OperationService
@inject IOperationTypeService OperationTypeService
```

### Using Services in Code-Behind

```csharp
namespace BlazorApp.UI.Presentation.Pages
{
    public partial class Operations
    {
        protected override async Task OnInitializedAsync()
        {
            operations = await OperationService.GetAllAsync();
            operationTypes = await OperationTypeService.GetAllAsync();
        }
        
        private async Task HandleSubmit()
        {
            if (isEditing)
            {
                await OperationService.UpdateAsync(currentOperation.OperationId, currentOperation);
            }
            else
            {
                await OperationService.CreateAsync(currentOperation);
            }
        }
    }
}
```

## File Organization

```
BlazorApp.UI/
├── Application/
│   ├── Interfaces/
│   │   ├── IOperationService.cs
│   │   └── IOperationTypeService.cs
│   └── Services/
│       ├── OperationService.cs
│       └── OperationTypeService.cs
├── Domain/
│   └── Models/
│       ├── OperationModel.cs
│       ├── OperationTypeModel.cs
│       ├── DailyReportModel.cs
│       └── PeriodReportModel.cs
├── Infrastructure/
│   ├── Auth/
│   │   ├── Models/
│   │   │   ├── LoginRequest.cs
│   │   │   ├── RegisterRequest.cs
│   │   │   └── UserInfo.cs
│   │   └── Services/
│   │       ├── IAuthService.cs
│   │       └── AuthService.cs
│   ├── ApiClients/
│   └── Dtos/
│       ├── OperationDtoBlazor.cs
│       └── OperationTypeDtoBlazor.cs
└── Presentation/
    ├── Components/
    │   ├── App.razor
    │   ├── Routes.razor
    │   ├── CreateEditModal.razor
    │   └── DeleteConfirmationModal.razor
    ├── Layout/
    │   ├── MainLayout.razor
    │   └── NavMenu.razor
    ├── Pages/
    │   ├── Operations.razor
    │   ├── OperationTypes.razor
    │   ├── DailyReport.razor
    │   ├── PeriodReport.razor
    │   └── Login.razor
    └── _Imports.razor
```

## Best Practices

1. **Keep layers independent**: Don't create circular dependencies
2. **Use interfaces**: Define service contracts in the Application layer
3. **Inject dependencies**: Use dependency injection instead of creating instances
4. **Keep Pages thin**: Move logic to services
5. **Use DTOs**: For API communication, separate from domain models
6. **Follow naming conventions**: Consistent naming across layers

## Future Enhancements

1. Add repository pattern for data access
2. Implement CQRS for complex operations
3. Add mapping layer (e.g., AutoMapper) between DTOs and Models
4. Introduce domain events for cross-cutting concerns
5. Add unit tests for each layer

## Known Technical Debt

While the architecture follows layered principles, there are a few areas identified for future improvement:

1. **Report Models**: `DailyReportModel` and `PeriodReportModel` in the Domain layer currently reference Infrastructure DTOs, which violates pure domain principles. These are being used as view models and should be moved to `Presentation/ViewModels` in a future refactoring.

2. **Report Services**: The `DailyReport` and `PeriodReport` pages currently use `HttpClientFactory` directly. These should be refactored to use dedicated report services (e.g., `IReportService`) in the Application layer.

3. **Request DTOs**: `PeriodReportRequest` is currently in the Pages namespace. Consider moving to a ViewModels folder in the Presentation layer.

These items represent technical debt that should be addressed in future iterations while maintaining the existing functionality.
