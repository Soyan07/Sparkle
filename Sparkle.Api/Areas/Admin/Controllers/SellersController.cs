using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sparkle.Domain.Sellers;
using Sparkle.Infrastructure;

namespace Sparkle.Api.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SellersController : Controller
{
    private readonly ApplicationDbContext _db;

    public SellersController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var sellers = await _db.Sellers
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        foreach (var seller in sellers)
        {
            seller.TotalProducts = await _db.Products.CountAsync(p => p.SellerId == seller.Id);
            seller.TotalSales = await _db.Orders
                .Where(o => o.Items.Any(i => i.ProductVariant != null && i.ProductVariant.Product.SellerId == seller.Id))
                .CountAsync();
        }

        return View(sellers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var seller = await _db.Sellers.FirstOrDefaultAsync(v => v.Id == id);
        if (seller == null)
        {
            return NotFound();
        }

        var products = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Where(p => p.SellerId == seller.Id)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        ViewBag.Seller = seller;
        return View(products);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int sellerId)
    {
        var seller = await _db.Sellers.FirstOrDefaultAsync(v => v.Id == sellerId);
        if (seller == null)
        {
            return Json(new { success = false, message = "Seller not found" });
        }

        seller.Status = seller.Status == SellerStatus.Active 
            ? SellerStatus.Pending 
            : SellerStatus.Active;

        await _db.SaveChangesAsync();

        return Json(new { success = true, status = seller.Status.ToString(), isActive = seller.Status == SellerStatus.Active });
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int sellerId)
    {
        var seller = await _db.Sellers.FirstOrDefaultAsync(v => v.Id == sellerId);
        if (seller == null)
        {
            return Json(new { success = false, message = "Seller not found" });
        }

        seller.Status = SellerStatus.Active;
        seller.ApprovedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> Suspend(int sellerId)
    {
        var seller = await _db.Sellers.FirstOrDefaultAsync(v => v.Id == sellerId);
        if (seller == null)
        {
            return Json(new { success = false, message = "Seller not found" });
        }

        seller.Status = SellerStatus.Suspended;
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }
}
