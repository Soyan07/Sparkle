using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sparkle.Domain.Users;
using Sparkle.Infrastructure;
using Sparkle.Domain.Identity;
using System.ComponentModel.DataAnnotations;

namespace Sparkle.Api.Controllers;

[Authorize]
[Route("profile")]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(
        UserManager<ApplicationUser> userManager, 
        ApplicationDbContext context,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _context = context;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        // Fetch dashboard stats
        var recentOrders = await _context.Orders
            .Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .ToListAsync();

        var model = new ProfileViewModel
        {
            FullName = user.FullName ?? "",
            Email = user.Email ?? "",
            ContactPhone = user.ContactPhone,
            Address = user.Address,
            DateOfBirth = user.DateOfBirth,
            ProfilePhotoPath = user.ProfilePhotoPath,
            RecentOrders = recentOrders,
            TotalOrders = await _context.Orders.CountAsync(o => o.UserId == user.Id),
            TotalSpent = await _context.Orders.Where(o => o.UserId == user.Id).SumAsync(o => o.TotalAmount)
        };

        return View(model);
    }

    [HttpGet("security")]
    public async Task<IActionResult> Security()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        return View(new SecurityViewModel { 
            Email = user.Email,
            HasPassword = await _userManager.HasPasswordAsync(user),
            TwoFactorEnabled = user.TwoFactorEnabled
        });
    }

    [HttpGet("addresses")]
    public async Task<IActionResult> Addresses()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var addresses = await _context.UserAddresses
            .Where(a => a.UserId == user.Id && a.IsActive)
            .OrderByDescending(a => a.IsDefault)
            .ToListAsync();

        return View(addresses);
    }

    [HttpGet("addresses/create")]
    public IActionResult CreateAddress()
    {
        return View(new UserAddress { Country = "Bangladesh" });
    }

    [HttpPost("addresses/create")]
    public async Task<IActionResult> CreateAddress(UserAddress model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        if (ModelState.IsValid)
        {
            model.UserId = user.Id;
            model.IsActive = true;
            
            // If this is the first address, make it default
            if (!await _context.UserAddresses.AnyAsync(a => a.UserId == user.Id && a.IsActive))
            {
                model.IsDefault = true;
            }

            if (model.IsDefault)
            {
                // Unset other defaults
                var defaults = await _context.UserAddresses
                    .Where(a => a.UserId == user.Id && a.IsDefault)
                    .ToListAsync();
                defaults.ForEach(a => a.IsDefault = false);
            }

            _context.UserAddresses.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Address added successfully";
            return RedirectToAction(nameof(Addresses));
        }
        return View(model);
    }

    [HttpGet("payment-methods")]
    public IActionResult PaymentMethods()
    {
        // Placeholder for Payment Methods
        return View();
    }

    [HttpGet("orders")]
    public async Task<IActionResult> Orders()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var orders = await _context.Orders
            .Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.OrderDate)
            .Take(10)
            .ToListAsync();

        return View(orders);
    }
    
    [HttpGet("more")]
    public IActionResult More()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Update(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        // Update user properties
        user.FullName = model.FullName;
        user.ContactPhone = model.ContactPhone;
        user.Address = model.Address;
        user.DateOfBirth = model.DateOfBirth;

        // Handle photo upload
        if (model.ProfilePhoto != null && model.ProfilePhoto.Length > 0)
        {
            // Delete old photo if exists
            if (!string.IsNullOrEmpty(user.ProfilePhotoPath))
            {
                var oldPhotoPath = Path.Combine(_environment.WebRootPath, user.ProfilePhotoPath.TrimStart('/'));
                if (System.IO.File.Exists(oldPhotoPath))
                {
                    System.IO.File.Delete(oldPhotoPath);
                }
            }

            // Save new photo
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(model.ProfilePhoto.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProfilePhoto.CopyToAsync(stream);
            }

            user.ProfilePhotoPath = $"/uploads/profiles/{fileName}";
        }

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Index");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View("Index", model);
    }

    [HttpPost("delete-photo")]
    public async Task<IActionResult> DeletePhoto()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        if (!string.IsNullOrEmpty(user.ProfilePhotoPath))
        {
            var photoPath = Path.Combine(_environment.WebRootPath, user.ProfilePhotoPath.TrimStart('/'));
            if (System.IO.File.Exists(photoPath))
            {
                System.IO.File.Delete(photoPath);
            }

            user.ProfilePhotoPath = null;
            await _userManager.UpdateAsync(user);
        }

        return Json(new { success = true });
    }

    public class ProfileViewModel
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? ContactPhone { get; set; }

        public string? Address { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public IFormFile? ProfilePhoto { get; set; }

        public string? ProfilePhotoPath { get; set; }
        
        // Dashboard Stats
        public List<Sparkle.Domain.Orders.Order> RecentOrders { get; set; } = new();
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class SecurityViewModel
    {
        public string? Email { get; set; }
        public bool HasPassword { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }
}
