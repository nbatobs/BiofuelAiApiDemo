# Microsoft Entra External ID Authentication Setup

This API uses Microsoft Entra External ID (formerly Azure AD B2C) as the Identity Provider (IDP) for authentication and authorization.

## Configuration Steps

### 1. Microsoft Entra External ID Setup

Before running the API, you need to configure your Microsoft Entra External ID tenant:

1. **Create a Microsoft Entra External ID Tenant** (if not already done)
   - Go to Azure Portal → Microsoft Entra External ID
   - Create your external tenant

2. **Register the API Application**
   - In your External ID tenant, go to "App registrations" → "New registration"
   - Name: `BiofuelAiApi`
   - Supported account types: "Accounts in this organizational directory only"
   - Register the application

3. **Configure API Permissions & Scopes**
   - Go to "Expose an API" → "Add a scope"
   - Application ID URI: `https://<your-tenant-name>.onmicrosoft.com/biofuel-api`
   - Scope name: `access_as_user`
   - Admin consent display name: "Access BiofuelAI API"
   - Save the scope

4. **Create User Flows**
   - Go to "User flows" → "New user flow"
   - Create sign-up and sign-in flow: `B2C_1_signupsignin`
   - Configure identity providers and user attributes

### 2. Update appsettings.json

Update the `BackendApi` section in both `appsettings.json` and `appsettings.Development.json`:

```json
{
  "BackendApi": {
    "Instance": "https://<your-tenant-name>.ciamlogin.com",
    "Domain": "<your-tenant-name>.onmicrosoft.com",
    "TenantId": "<your-tenant-id>",
    "ClientId": "<your-api-client-id>",
    "SignUpSignInPolicyId": "B2C_1_signupsignin",
    "Scopes": "https://<your-tenant-name>.onmicrosoft.com/biofuel-api/access_as_user"
  }
}
```

Replace:
- `<your-tenant-name>`: Your External ID tenant name
- `<your-tenant-id>`: Your tenant ID (GUID)
- `<your-api-client-id>`: The Application (client) ID from step 2

**Note:** Microsoft Entra External ID uses `.ciamlogin.com` domain instead of the legacy `.b2clogin.com`

### 3. Frontend/Client Application Setup

Your client application (web app, mobile app) needs to:

1. **Register as a separate application** in Microsoft Entra External ID
   - Go to "App registrations" → "New registration"
   - Name: `BiofuelAiClient`
   - Configure redirect URIs for your client app
   - Configure API permissions to access the API scope

2. **Implement authentication flow**
   - Use MSAL (Microsoft Authentication Library) for your platform
   - Authenticate users through External ID user flows
   - Obtain access tokens for the API
   - Include tokens in API requests: `Authorization: Bearer <token>`

## API Usage

### Authentication Flow

1. **User Registration** (via Microsoft Entra External ID)
   - User signs up through External ID user flow
   - After successful registration, user can log in

2. **User Login** (via Microsoft Entra External ID)
   - User logs in through External ID user flow
   - External ID returns an access token
   - Call `/api/users/me` to sync user to local database

3. **API Requests**
   - Include the External ID token in the Authorization header
   - Example: `Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGc...`

### Authorization Attributes

#### `[RequireUserRole]` - For system-wide role checks

```csharp
[HttpGet]
[RequireUserRole(UserRole.Admin, UserRole.Manager)]
public async Task<ActionResult> GetSensitiveData()
{
    // Only Admin and Manager users can access this endpoint
}
```

#### `[RequireSiteRole]` - For site-specific role checks

```csharp
[HttpPost("sites/{siteId}/data")]
[RequireSiteRole(SiteRole.Operator, SiteRole.SiteAdmin)]
public async Task<ActionResult> UploadSiteData(int siteId)
{
    // Check site-specific access
    var userId = int.Parse(User.FindFirst("userId")?.Value);
    var hasAccess = await _siteAuthService.HasSiteRoleAsync(
        userId, siteId, SiteRole.Operator, SiteRole.SiteAdmin);
    
    if (!hasAccess)
        return Forbid();
    
    // Process upload
}
```

### Available Endpoints

#### Auth Controller (`/api/auth`)

- **POST /api/auth/register** - Register user in local DB (after B2C registration)
- **GET /api/auth/me** - Get current user info
- **POST /api/auth/validate** - Validate token and update last login
- **GET /api/auth/health** - Health check (no auth required)

## Testing Authentication

### Using Swagger UI

1. Run the API: `dotnet run`
2. Navigate to: `https://localhost:7141/swagger`
3. Click "Authorize" button
4. Enter: `Bearer <your-token-from-b2c>`
5. Click "Authorize" and close the dialog
6. All authenticated endpoints will now include the token

### Using curl

```bash
# Get current user info
curl -X GET "https://localhost:7141/api/users/me" \
  -H "Authorization: Bearer <your-token>"
```

## User Roles

### UserRole (System-wide)
- **SuperUser** (0) - Full system access
- **Admin** (1) - Company-wide administration
- **Manager** (2) - Management functions
- **User** (3) - Standard user access
- **Viewer** (4) - Read-only access

### SiteRole (Site-specific)
- **Owner** (0) - Full site control
- **SiteAdmin** (1) - Site administration
- **Operator** (2) - Operational access
- **Analyst** (3) - Data analysis access
- **Viewer** (4) - Read-only access

## Security Notes

⚠️ **Important Security Considerations:**

1. **HTTPS Only** - Always use HTTPS in production
2. **Token Validation** - Tokens are validated against Azure B2C
3. **Role Claims** - Roles stored in custom B2C attribute `extension_UserRole`
4. **CORS Configuration** - Update CORS policy for production (currently allows all)
5. **Vulnerability Warning** - Microsoft.Identity.Web 3.3.0 has a known vulnerability
   - Update to latest version when available
   - Monitor: https://github.com/advisories/GHSA-rpq8-q44m-2rpg

## Troubleshooting

### Common Issues

1. **401 Unauthorized**
   - Check token is valid and not expired
   - Verify B2C configuration in appsettings.json
   - Ensure token includes required claims

2. **403 Forbidden**
   - User doesn't have required role
   - Check UserRole claim in token
   - Verify user exists in local database

3. **Token validation errors**
   - Verify `Instance`, `Domain`, and `TenantId` in config
   - Check that policy name matches user flow name
   - Ensure API scope is included in token

1. **401 Unauthorized**
   - Check token is valid and not expired
   - Verify External ID configuration in appsettings.json
   - Ensure token includes required claims

2. **403 Forbidden**
   - User doesn't have required role
   - Check user exists in local database
   - Verify site access in UserSiteAccess table

3. **Token validation errors**
   - Verify `Instance`, `Domain`, and `TenantId` in config
   - Check that user flow name matches configuration
   - Ensure API scope is included in token
   - Verify using `.ciamlogin.com` domain (not `.b2clogin.com`)