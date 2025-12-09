# Known Issues and Configuration Notes

## CORS Configuration Issue (Pre-existing)

### Issue
The CORS domain URL in `Program.cs` (line 45) contains a malformed double 'https://' prefix:

```csharp
var blazorDomain = "https://sfmb-ui.https://goldfish-app-j6a9p.ondigitalocean.app/";
```

### Impact
This will cause CORS configuration to fail when the application attempts to handle cross-origin requests.

### Recommended Fix
Update the URL to remove the duplicate protocol prefix. Choose one of these options:

**Option 1** (if subdomain is needed):
```csharp
var blazorDomain = "https://sfmb-ui.goldfish-app-j6a9p.ondigitalocean.app/";
```

**Option 2** (if no subdomain):
```csharp
var blazorDomain = "https://goldfish-app-j6a9p.ondigitalocean.app/";
```

### Location
File: `BlazorApp.UI/Program.cs`
Line: 45

---

## Other Configuration Considerations

### HTTPS Redirection
HTTPS redirection is currently commented out in `Program.cs`:

```csharp
//app.UseHttpsRedirection();
```

**Recommendation**: Enable HTTPS redirection in production environments for security.

### Data Protection Key Storage
Keys are persisted to the filesystem at `/app/data/protection`. Ensure this directory:
- Has appropriate permissions
- Is backed up regularly
- Is not accessible from outside the container

### API URL Configuration
The application requires an API URL to be configured either through:
- Environment variable: `API_URL`
- Connection string in `appsettings.json`: `ConnectionStrings:ApiBaseUrl`

Ensure the correct backend API URL is configured before deployment.
