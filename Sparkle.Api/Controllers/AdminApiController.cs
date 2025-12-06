using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sparkle.Domain.Identity;

namespace Sparkle.Api.Controllers;

[Route("api/admin")]
[ApiController]
public class AdminApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminApiController> _logger;

    public AdminApiController(
        UserManager<ApplicationUser> userManager,
        ILogger<AdminApiController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Update user password - FOR ADMIN USE ONLY
    /// POST /api/admin/update-password
    /// Body: { "email": "user@example.com", "newPassword": "password123" }
    /// </summary>
    [HttpPost("update-password")]
    [AllowAnonymous] // Remove this in production and add proper authorization
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        _logger.LogInformation($"Password update requested for: {request.Email}");

        var user = await _userManager.FindByEmailAsync(request.Email);
        
        if (user == null)
        {
            _logger.LogWarning($"User not found: {request.Email}");
            return NotFound(new { error = $"User not found with email: {request.Email}" });
        }

        // Remove old password
        await _userManager.RemovePasswordAsync(user);
        
        // Add new password
        var result = await _userManager.AddPasswordAsync(user, request.NewPassword);

        if (result.Succeeded)
        {
            _logger.LogInformation($"Password updated successfully for: {request.Email}");
            return Ok(new 
            { 
                success = true,
                message = "Password updated successfully",
                email = request.Email,
                userId = user.Id,
                userName = user.UserName,
                fullName = user.FullName
            });
        }

        _logger.LogError($"Failed to update password for: {request.Email}");
        return BadRequest(new 
        { 
            success = false,
            error = "Failed to update password",
            errors = result.Errors.Select(e => e.Description).ToArray()
        });
    }
}

public class UpdatePasswordRequest
{
    public string Email { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}
