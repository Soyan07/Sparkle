using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sparkle.Domain.Orders;
using Sparkle.Infrastructure;

namespace Sparkle.Api.Controllers;

[Authorize(Roles = "User")]
[Route("cart")]
public class CartController : Controller
{
    private readonly ApplicationDbContext _db;

    public CartController(ApplicationDbContext db)
    {
        _db = db;
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue(ClaimTypes.Name) ??
        throw new InvalidOperationException("User id not found in token");

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetUserId();
            var cart = await _db.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(v => v.Product)
                            .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var vm = new CartViewModel(cart ?? new Cart { UserId = userId });
            return View(vm);
        }
        catch (Exception)
        {
            TempData["Error"] = "Unable to load cart. Please try again.";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet("count")]
    [AllowAnonymous]
    public async Task<IActionResult> Count()
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            return Json(new { count = 0 });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { count = 0 });
        }

        var count = await _db.Carts
            .Where(c => c.UserId == userId)
            .SelectMany(c => c.Items)
            .SumAsync(i => (int?)i.Quantity) ?? 0;

        return Json(new { count });
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add(int productVariantId, int quantity = 1, string? returnUrl = null)
    {
        try
        {
            if (quantity <= 0) quantity = 1;
            if (quantity > 99) quantity = 99; // Max quantity limit
            
            var userId = GetUserId();

            var variant = await _db.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.Id == productVariantId);
                
            if (variant == null || !variant.Product.IsActive)
            {
                TempData["Error"] = "Product is not available";
                return Redirect(returnUrl ?? "/");
            }

            // Check stock availability
            if (variant.Stock < quantity)
            {
                TempData["Error"] = $"Only {variant.Stock} items available in stock";
                return Redirect(returnUrl ?? $"/Product/{variant.Product.Id}");
            }

            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
                
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }

            var existing = cart.Items.FirstOrDefault(i => i.ProductVariantId == productVariantId);
            if (existing != null)
            {
                // Check total quantity doesn't exceed stock
                var newQuantity = existing.Quantity + quantity;
                if (newQuantity > variant.Stock)
                {
                    TempData["Error"] = $"Cannot add more. Only {variant.Stock} items available";
                    return Redirect(returnUrl ?? "/cart");
                }
                existing.Quantity = newQuantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductVariantId = productVariantId,
                    Quantity = quantity,
                    UnitPrice = variant.Price
                });
            }

            await _db.SaveChangesAsync();
            
            TempData["Success"] = "Product added to cart successfully!";
            return Redirect(returnUrl ?? "/cart");
        }
        catch (Exception)
        {
            TempData["Error"] = "Unable to add product to cart. Please try again.";
            return Redirect(returnUrl ?? "/");
        }
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(int itemId, int quantity)
    {
        var userId = GetUserId();
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
        {
            return RedirectToAction(nameof(Index));
        }

        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("remove")]
    public async Task<IActionResult> Remove(int itemId)
    {
        var userId = GetUserId();
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart != null)
        {
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                cart.Items.Remove(item);
                await _db.SaveChangesAsync();
            }
        }
        return RedirectToAction(nameof(Index));
    }

    public class CartViewModel
    {
        public CartViewModel(Cart cart)
        {
            Cart = cart;
        }

        public Cart Cart { get; }
        public decimal Subtotal => Cart.Items.Sum(i => i.UnitPrice * i.Quantity);
    }
}
