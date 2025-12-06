using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sparkle.Domain.Identity;

namespace Sparkle.Api.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IWebHostEnvironment environment,
        ILogger<AuthController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model, string loginType = "user")
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials or account inactive.");
            return View(model);
        }

        // Check if login type matches user's actual role
        var isSeller = await _userManager.IsInRoleAsync(user, "Seller");
        var isUser = await _userManager.IsInRoleAsync(user, "User");
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        // Validate login type matches account type
        if (loginType == "seller" && !isSeller)
        {
            ModelState.AddModelError(string.Empty, "This account is not registered as a seller. Please login as a user or register as a seller.");
            return View(model);
        }

        if (loginType == "user" && !isUser)
        {
            if (isSeller)
            {
                ModelState.AddModelError(string.Empty, "This is a seller account. Please switch to 'Seller' login type.");
            }
            else if (isAdmin)
            {
                ModelState.AddModelError(string.Empty, "Admin accounts must use the admin login page.");
            }
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        // Redirect to appropriate dashboard based on role
        if (isSeller)
        {
            return Redirect("/Seller/Dashboard");
        }

        return Redirect("/"); // Redirect to Homepage (Product Dashboard)
    }

    [HttpGet("admin-login")]
    [AllowAnonymous]
    public IActionResult AdminLogin(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("admin-login")]
    [AllowAnonymous]
    public async Task<IActionResult> AdminLogin(LoginViewModel model)
    {
        if (model == null)
        {
            return View(new LoginViewModel());
        }

        _logger.LogInformation("========== ADMIN LOGIN ATTEMPT ==========");
        _logger.LogInformation($"Email: {model?.Email ?? "NULL"}");
        _logger.LogInformation($"Password Length: {model?.Password?.Length ?? 0}");
        _logger.LogInformation($"RememberMe: {model?.RememberMe ?? false}");
        _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState is INVALID. Errors:");
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key]?.Errors;
                if (errors != null && errors.Count > 0)
                {
                    foreach (var error in errors)
                    {
                        _logger.LogWarning($"  [{key}]: {error.ErrorMessage}");
                    }
                }
            }
            return View(model);
        }

        _logger.LogInformation("ModelState is valid. Looking up user...");
        var user = await _userManager.FindByEmailAsync(model?.Email ?? "");
        
        _logger.LogInformation($"User found: {user != null}");
        if (user == null)
        {
            _logger.LogWarning($"No user found with email: {model?.Email ?? "unknown"}");
            ModelState.AddModelError(string.Empty, "Invalid admin credentials or account inactive.");
            return View(model);
        }

        _logger.LogInformation($"User ID: {user.Id}");
        _logger.LogInformation($"User.IsActive: {user.IsActive}");
        
        if (!user.IsActive)
        {
            _logger.LogWarning($"User account is INACTIVE: {model?.Email ?? "unknown"}");
            ModelState.AddModelError(string.Empty, "Invalid admin credentials or account inactive.");
            return View(model);
        }

        // Check if user is admin
        _logger.LogInformation("Checking if user has Admin role...");
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        _logger.LogInformation($"Is Admin: {isAdmin}");
        
        if (!isAdmin)
        {
            _logger.LogWarning($"User {model?.Email ?? "unknown"} is NOT an admin");
            ModelState.AddModelError(string.Empty, "Access denied. Admin credentials required.");
            return View(model);
        }

        _logger.LogInformation("Attempting password sign-in...");
        var result = await _signInManager.PasswordSignInAsync(user, model?.Password ?? string.Empty, model?.RememberMe ?? false, lockoutOnFailure: false);
        
        _logger.LogInformation($"Sign-in result - Succeeded: {result.Succeeded}");
        _logger.LogInformation($"Sign-in result - IsLockedOut: {result.IsLockedOut}");
        _logger.LogInformation($"Sign-in result - IsNotAllowed: {result.IsNotAllowed}");
        _logger.LogInformation($"Sign-in result - RequiresTwoFactor: {result.RequiresTwoFactor}");
        
        if (!result.Succeeded)
        {
            _logger.LogError($"Password sign-in FAILED for user: {model?.Email ?? "unknown"}");
            ModelState.AddModelError(string.Empty, "Invalid admin credentials.");
            return View(model);
        }

        _logger.LogInformation("Password sign-in SUCCEEDED!");
        
        if (!string.IsNullOrEmpty(model?.ReturnUrl) && Url.IsLocalUrl(model?.ReturnUrl))
        {
            _logger.LogInformation($"Redirecting to ReturnUrl: {model.ReturnUrl}");
            return Redirect(model.ReturnUrl);
        }

        _logger.LogInformation("Redirecting to /Admin/Dashboard");
        return Redirect("/Admin/Dashboard");
    }

    [HttpPost("external-login")]
    [AllowAnonymous]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        // Check if the provider is supported and configured
        if (provider == "Google")
        {
            var googleConfig = HttpContext.RequestServices.GetService<Services.IGoogleAuthConfigurationService>();
            if (googleConfig?.IsGoogleAuthConfigured != true)
            {
                ModelState.AddModelError(string.Empty, "Google login is currently not configured. Please contact the administrator or use email/password login.");
                return RedirectToAction(nameof(Login));
            }
        }

        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet("external-login-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            _logger.LogWarning($"Error from external provider: {remoteError}");
            ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
            return RedirectToAction(nameof(Login));
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogError("External login info was null. This usually means the user cancelled the flow, or the Google Client ID/Secret is mismatched/invalid.");
            ModelState.AddModelError(string.Empty, "Failed to retrieve information from the external login provider. Please try again or use email/password login.");
            return RedirectToAction(nameof(Login));
        }

        _logger.LogInformation($"External login attempt from provider: {info.LoginProvider}");
        _logger.LogInformation($"Provider Key: {info.ProviderKey}");

        // Sign in the user with this external login provider if the user already has a login
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        
        if (result.Succeeded)
        {
            _logger.LogInformation($"User successfully logged in via {info.LoginProvider}");
            return LocalRedirect(returnUrl ?? "/");
        }
        
        if (result.IsLockedOut)
        {
             _logger.LogWarning($"User account is locked out for {info.LoginProvider}");
             return RedirectToAction(nameof(Login));
        }

        // If the user does not have an account, create one
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(ClaimTypes.Name);
        _logger.LogInformation($"New external login user detected. Email: {email}, Name: {name}");

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning($"Email not received from {info.LoginProvider}");
            ModelState.AddModelError(string.Empty, $"Email not received from {info.LoginProvider}. Please ensure you've granted email permission.");
            return RedirectToAction(nameof(Login));
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Create new user
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email.Split('@')[0],
                IsActive = true,
                ContactPhone = "",
                Address = "",
                DateOfBirth = DateTime.Now.AddYears(-20) // Default age
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                _logger.LogError($"Failed to create user account for {email}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                ModelState.AddModelError(string.Empty, "Failed to create user account. Please contact support.");
                return RedirectToAction(nameof(Login));
            }

            // Assign User role (Google login is for users only, not vendors or admins)
            await _userManager.AddToRoleAsync(user, "User");
            _logger.LogInformation($"Created new user account for {email} via {info.LoginProvider}");
        }

        // Add external login to user
        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (addLoginResult.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation($"Successfully linked {info.LoginProvider} to user {email}");
            return LocalRedirect(returnUrl ?? "/");
        }

        _logger.LogError($"Failed to link external login for {email}: {string.Join(", ", addLoginResult.Errors.Select(e => e.Description))}");
        ModelState.AddModelError(string.Empty, "Failed to link external login. Please try again or contact support.");
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("register")]
    [AllowAnonymous]
    public IActionResult RegisterUser()
    {
        return View(new RegisterUserViewModel());
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterUser(RegisterUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Handle NID file uploads (optional)
        string? nidFrontPath = null;
        string? nidBackPath = null;

        if (model.NationalIdFront?.Length > 0)
        {
            nidFrontPath = await SaveNidFileAsync(model.NationalIdFront, "front");
        }

        if (model.NationalIdBack?.Length > 0)
        {
            nidBackPath = await SaveNidFileAsync(model.NationalIdBack, "back");
        }

        var composedAddress = string.IsNullOrWhiteSpace(model.Location)
            ? (model.Address ?? "")
            : $"{model.Location} | {model.Address ?? ""}";

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            IsActive = true,
            ContactPhone = model.ContactPhone,
            Address = string.IsNullOrWhiteSpace(composedAddress) ? "" : composedAddress,
            DateOfBirth = model.DateOfBirth,
            NationalIdFrontPath = nidFrontPath,
            NationalIdBackPath = nidBackPath
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        await EnsureRoleExists("User");
        await _userManager.AddToRoleAsync(user, "User");

        await _signInManager.SignInAsync(user, isPersistent: true);
        return Redirect("/");
    }

    [HttpGet("register-seller")]
    [AllowAnonymous]
    public IActionResult RegisterSeller()
    {
        return View(new RegisterSellerViewModel());
    }

    [HttpPost("register-seller")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterSeller(RegisterSellerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var sellerProfileLines = new List<string>();
        if (!string.IsNullOrWhiteSpace(model.BusinessName))
        {
            sellerProfileLines.Add($"Business: {model.BusinessName}");
        }
        if (!string.IsNullOrWhiteSpace(model.BusinessCategory))
        {
            sellerProfileLines.Add($"Category: {model.BusinessCategory}");
        }
        if (!string.IsNullOrWhiteSpace(model.BusinessRegistrationNumber))
        {
            sellerProfileLines.Add($"Reg#: {model.BusinessRegistrationNumber}");
        }
        if (!string.IsNullOrWhiteSpace(model.BusinessWebsite))
        {
            sellerProfileLines.Add($"Website: {model.BusinessWebsite}");
        }
        sellerProfileLines.Add($"Address: {model.Address}");

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            IsSeller = true,
            IsActive = true,
            ContactPhone = model.ContactPhone,
            Address = string.Join(" | ", sellerProfileLines)
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        await EnsureRoleExists("Seller");
        await _userManager.AddToRoleAsync(user, "Seller");

        // Optionally create Seller profile here (admin can later approve)

        await _signInManager.SignInAsync(user, isPersistent: true);
        return Redirect("/Seller/Dashboard");
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/");
    }

    private async Task EnsureRoleExists(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
        }
    }

    private async Task<string> SaveNidFileAsync(IFormFile file, string side)
    {
        var uploadsRoot = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "nid");
        Directory.CreateDirectory(uploadsRoot);

        var safeFileName = Path.GetFileNameWithoutExtension(file.FileName);
        var extension = Path.GetExtension(file.FileName);
        var uniqueName = $"nid_{side}_{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsRoot, uniqueName);

        using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative path for storing in database
        var relativePath = $"/uploads/nid/{uniqueName}";
        return relativePath;
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }

    public class RegisterUserViewModel
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Compare("Password"), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Extra information collected during signup
        [Required]
        [Display(Name = "Contact Phone")]
        [Phone]
        public string ContactPhone { get; set; } = string.Empty;

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "City / Division")]
        public string? Location { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Upload NID (Front)")]
        public IFormFile? NationalIdFront { get; set; }

        [Display(Name = "Upload NID (Back)")]
        public IFormFile? NationalIdBack { get; set; }
    }

    public class RegisterSellerViewModel
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Compare("Password"), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Business Phone")]
        [Phone]
        public string ContactPhone { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Business Address")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Business / Brand Name")]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Business Category")]
        public string BusinessCategory { get; set; } = string.Empty;

        [Display(Name = "Trade License / Registration #")]
        public string? BusinessRegistrationNumber { get; set; }

        [Display(Name = "Website / Facebook Page")]
        [Url]
        public string? BusinessWebsite { get; set; }
    }
}
