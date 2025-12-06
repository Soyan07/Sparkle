using Microsoft.AspNetCore.Identity;

namespace Sparkle.Domain.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public bool IsSeller { get; set; }
    public bool IsActive { get; set; } = true;

    // Extended profile information collected during registration
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }

    // National ID document uploads (stored as relative paths under wwwroot)
    public string? NationalIdFrontPath { get; set; }
    public string? NationalIdBackPath { get; set; }

    // Profile photo (stored as relative path under wwwroot)
    public string? ProfilePhotoPath { get; set; }
}

public class ApplicationRole : IdentityRole
{
}
