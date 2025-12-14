using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get current user information. This endpoint automatically creates a user record
    /// in the local database if one doesn't exist for the authenticated user.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        // Extract IDP subject (sub claim)
        var idpSub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("oid")?.Value;
        
        if (string.IsNullOrEmpty(idpSub))
        {
            return Unauthorized(new { message = "User identifier (sub) not found in token" });
        }

        // Extract IDP issuer
        var idpIssuer = User.FindFirst("iss")?.Value;
        if (string.IsNullOrEmpty(idpIssuer))
        {
            return Unauthorized(new { message = "Issuer (iss) not found in token" });
        }

        // Extract email from claims
        var email = User.FindFirst(ClaimTypes.Email)?.Value 
            ?? User.FindFirst("emails")?.Value 
            ?? User.FindFirst("email")?.Value
            ?? User.FindFirst("preferred_username")?.Value
            ?? "";
        
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { message = "Email claim not found in token" });
        }

        // Extract name from claims
        var name = User.FindFirst(ClaimTypes.Name)?.Value 
            ?? User.FindFirst("name")?.Value 
            ?? User.FindFirst("given_name")?.Value;

        // Get or create user in local database
        var user = await _userService.GetOrCreateUserFromClaimsAsync(idpSub, idpIssuer, email, name);

        return Ok(new
        {
            user.Id,
            user.Email,
            user.Name,
            user.Role,
            user.CompanyId,
            CompanyName = user.Company?.Name,
            user.IsIndividual,
            user.CreatedAt,
            user.LastLogin
        });
    }

    /// <summary>
    /// Health check endpoint for user service.
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new 
        { 
            status = "healthy", 
            service = "users",
            timestamp = DateTime.UtcNow 
        });
    }
}
