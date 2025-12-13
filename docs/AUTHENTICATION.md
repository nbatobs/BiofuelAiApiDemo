# Azure AD B2C Authentication Setup

This API uses Azure AD B2C as the Identity Provider (IDP) for authentication and authorization.

## Configuration Steps

### 1. Azure B2C Setup

Before running the API, you need to configure your Azure AD B2C tenant:

1. **Create an Azure AD B2C Tenant** (if not already done)
   - Go to Azure Portal → Create a resource → Azure AD B2C
   - Follow the wizard to create your tenant

2. **Register the API Application**
   - In your B2C tenant, go to "App registrations" → "New registration"
   - Name: `BiofuelAiApi`
   - Supported account types: "Accounts in this organizational directory only"
   - Register the application

3. **Configure API Permissions & Scopes**
   - Go to "Expose an API" → "Add a scope"
   - Application ID URI: `https://<your-tenant-name>.onmicrosoft.com/biofuel-api`
   - Scope name: `access_as_user`
   - Admin consent display name: "Access BiofuelAI API"
   - Save the scope

4. **Create User Flows (Policies)**
   - Go to "User flows" → "New user flow"
   - Create the following flows:
     - **Sign up and sign in**: `B2C_1_susi`
     - **Password reset**: `B2C_1_reset`
     - **Profile editing**: `B2C_1_edit_profile`

5. **Configure Custom Attributes** (Optional)
   - Go to "User attributes" → Add custom attribute
   - Add `UserRole` attribute to store user roles
   - Include this in your user flows

### 2. Update appsettings.json

Update the `AzureAdB2C` section in both `appsettings.json` and `appsettings.Development.json`:

```json
{
  "AzureAdB2C": {
    "Instance": "https://<your-tenant-name>.b2clogin.com",
    "Domain": "<your-tenant-name>.onmicrosoft.com",
    "TenantId": "<your-tenant-id>",
    "ClientId": "<your-api-client-id>",
    "SignUpSignInPolicyId": "B2C_1_susi",
    "ResetPasswordPolicyId": "B2C_1_reset",
    "EditProfilePolicyId": "B2C_1_edit_profile",
    "Scopes": "https://<your-tenant-name>.onmicrosoft.com/biofuel-api/access_as_user"
  }
}
```

Replace:
- `<your-tenant-name>`: Your B2C tenant name (e.g., `contosob2c`)
- `<your-tenant-id>`: Your tenant ID (GUID)
- `<your-api-client-id>`: The Application (client) ID from step 2

### 3. Frontend/Client Application Setup

Your client application (web app, mobile app) needs to:

1. **Register as a separate application** in Azure B2C
   - Go to "App registrations" → "New registration"
   - Name: `BiofuelAiClient`
   - Configure redirect URIs for your client app
   - Configure API permissions to access the API scope

2. **Implement authentication flow**
   - Use MSAL (Microsoft Authentication Library) for your platform
   - Authenticate users through Azure B2C user flows
   - Obtain access tokens for the API
   - Include tokens in API requests: `Authorization: Bearer <token>`

## API Usage

### Authentication Flow

1. **User Registration** (via Azure B2C)
   - User signs up through B2C user flow
   - After successful registration in B2C, call `/api/auth/register` to sync user to local database

2. **User Login** (via Azure B2C)
   - User logs in through B2C user flow
   - B2C returns an access token
   - Call `/api/auth/me` to get user info from local database

3. **API Requests**
   - Include the B2C token in the Authorization header
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
curl -X GET "https://localhost:7141/api/auth/me" \
  -H "Authorization: Bearer <your-token>"

# Register a new user
curl -X POST "https://localhost:7141/api/auth/register" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-token>" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!",
    "confirmPassword": "Password123!",
    "firstName": "John",
    "lastName": "Doe",
    "companyId": null,
    "userRole": 2,
    "isIndividual": true
  }'
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

## Next Steps

After authentication is working:

1. Implement additional controllers (Companies, Sites, Uploads)
2. Add site-specific authorization checks
3. Implement data validation and business logic
4. Set up proper CORS policy for your frontend
5. Configure logging and monitoring
6. Update Microsoft.Identity.Web to resolve security advisory
