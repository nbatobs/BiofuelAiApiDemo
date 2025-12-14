# Microsoft Entra External ID Integration - Refactored

## What Changed

### Removed (Old Approach ❌)
- **AuthController** - No login/register endpoints needed
- **IAuthService/AuthService** - Microsoft Entra External ID handles authentication
- **Login/Register DTOs** - Not needed with External ID flow

### Added (Correct Approach ✅)
- **IUserService/UserService** - Lightweight service for user lookup/creation from JWT claims
- **UsersController** - Single `/api/users/me` endpoint
- **User Model Updates**:
  - `IdpSub` - Maps to External ID `sub` claim (unique identifier)
  - `IdpIssuer` - Maps to `iss` claim (issuer identifier)
  - `Name` - User's display name from External ID
  - `UpdatedAt` - Track user info updates

## How It Works

### Authentication Flow

```
1. Frontend redirects to Microsoft Entra External ID
   ↓
2. User authenticates with External ID
   ↓
3. External ID returns JWT token to frontend
   ↓
4. Frontend includes JWT in API requests
   Authorization: Bearer <token>
   ↓
5. API validates JWT (Microsoft.Identity.Web)
   ↓
6. API extracts claims (sub, iss, email, name)
   ↓
7. API looks up/creates user in local DB
```

### Key Endpoints

**`GET /api/users/me`**
- Extracts External ID claims from JWT
- Creates user in DB if doesn't exist
- Updates user info if changed
- Returns local user record

**Sample Controllers**
- `/api/sample/*` - Demonstrates authorization patterns
- Shows how to get current user ID
- Shows role-based and site-based authorization

### User Lookup Pattern

All controllers should use this pattern to get the current user:

```csharp
private async Task<(int? userId, string? error)> GetCurrentUserIdAsync()
{
    var idpSub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
        ?? User.FindFirst("sub")?.Value
        ?? User.FindFirst("oid")?.Value;
    
    if (string.IsNullOrEmpty(idpSub))
        return (null, "User identifier not found in token");

    var idpIssuer = User.FindFirst("iss")?.Value;
    if (string.IsNullOrEmpty(idpIssuer))
        return (null, "Issuer not found in token");

    var user = await _userService.GetUserByIdpSubAsync(idpSub, idpIssuer);
    if (user == null)
        return (null, "User not found. Call /api/users/me first.");

## Database Migration Needed

Run this to add the new User fields:

```bash
cd src/Data
dotnet ef migrations add AddIdpFieldsToUser --startup-project ../Api
dotnet ef database update --startup-project ../Api
```

## Configuration

In `appsettings.json`:

```json
{
  "BackendApi": {
    "Instance": "https://<tenant>.ciamlogin.com/",
    "Domain": "<tenant>.onmicrosoft.com",
    "TenantId": "<guid>",
    "ClientId": "<guid>",
    "SignUpSignInPolicyId": "B2C_1_signupsignin"
  }
}
``` "ClientId": "<guid>",
    "SignUpSignInPolicyId": "B2C_1_susi"
  }
}
```

## Frontend Integration

1. Use MSAL.js (or MSAL for your platform)
2. Authenticate with Azure B2C
3. Get access token from B2C
4. Call `/api/users/me` to sync user to DB
5. Include token in all subsequent requests

```javascript
// Example with MSAL.js
const token = await msalInstance.acquireTokenSilent({
  scopes: ["https://<tenant>.onmicrosoft.com/<app>/access_as_user"]
});

// First request - creates user in DB
const user = await fetch('/api/users/me', {
  headers: { 'Authorization': `Bearer ${token.accessToken}` }
});

// Subsequent requests
const data = await fetch('/api/sites/123/data', {
  headers: { 'Authorization': `Bearer ${token.accessToken}` }
});
```

## Authorization

### System-wide Roles
```csharp
[RequireUserRole(UserRole.Admin, UserRole.Manager)]
public async Task<IActionResult> AdminEndpoint() { }
```

### Site-specific Roles
```csharp
[RequireSiteRole(SiteRole.Operator, SiteRole.SiteAdmin)]
public async Task<IActionResult> SiteEndpoint(int siteId)
{
    var (userId, error) = await GetCurrentUserIdAsync();
    if (error != null) return Unauthorized(error);
    
    var hasAccess = await _siteAuthService.HasSiteRoleAsync(
        userId.Value, siteId, SiteRole.Operator, SiteRole.SiteAdmin);
    
    if (!hasAccess) return Forbid();
    // ... proceed
}
```

## Benefits of This Approach

✅ **No password management** - Microsoft Entra External ID handles it
✅ **Auto user sync** - Users created on first API call
✅ **Claims-based** - All user info in JWT
✅ **Scalable** - No session state needed
✅ **Secure** - Microsoft-managed identity platform
✅ **Simple** - Fewer endpoints and less code
✅ **IDP-agnostic** - Uses standard `sub` and `iss` claims, can switch IDPs if needed
## Next Steps

1. Run the database migration
2. Configure Microsoft Entra External ID tenant settings
3. Test `/api/users/me` endpoint
4. Implement remaining controllers (Companies, Sites, Uploads)
5. Add proper CORS policy for your frontend domaines, Uploads)
5. Add proper CORS policy for your frontend domain
