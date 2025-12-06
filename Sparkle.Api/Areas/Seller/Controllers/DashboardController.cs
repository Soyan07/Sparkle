using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sparkle.Domain.Catalog;
using Sparkle.Domain.Orders;
using Sparkle.Infrastructure;
using Sparkle.Domain.Identity;

namespace Sparkle.Api.Areas.Seller.Controllers;

[Area("Seller")]
[Authorize(Roles = "Seller")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException();

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var seller = await _db.Sellers.FirstOrDefaultAsync(v => v.UserId == userId);
        
        if (seller == null)
        {
            return RedirectToAction("Setup");
        }

        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = now.AddDays(-7);
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var sellerOrderItems = _db.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.ProductVariant)
            .ThenInclude(pv => pv!.Product)
            .Where(oi => oi.ProductVariant != null && oi.ProductVariant.Product != null && oi.ProductVariant.Product!.SellerId == seller.Id);

        var stats = new SellerDashboardStats
        {
            ShopName = seller.ShopName,
            TotalProducts = await _db.Products.CountAsync(p => p.SellerId == seller.Id),
            ActiveProducts = await _db.Products.CountAsync(p => p.SellerId == seller.Id && p.IsActive),
            LowStockProducts = await _db.ProductVariants
                .Include(v => v.Product)
                .CountAsync(v => v.Product.SellerId == seller.Id && v.Stock <= 10),
            
            TotalOrders = await sellerOrderItems.Select(oi => oi.OrderId).Distinct().CountAsync(),
            PendingOrders = await sellerOrderItems.Where(oi => oi.Order.Status == OrderStatus.Pending).Select(oi => oi.OrderId).Distinct().CountAsync(),
            ProcessingOrders = await sellerOrderItems.Where(oi => oi.Order.Status == OrderStatus.Processing).Select(oi => oi.OrderId).Distinct().CountAsync(),
            CompletedOrders = await sellerOrderItems.Where(oi => oi.Order.Status == OrderStatus.Delivered).Select(oi => oi.OrderId).Distinct().CountAsync(),
            
            TotalRevenue = await sellerOrderItems.SumAsync(oi => (decimal?)oi.LineTotal) ?? 0m,
            TodayRevenue = await sellerOrderItems.Where(oi => oi.Order.OrderDate >= today && oi.Order.OrderDate < today.AddDays(1)).SumAsync(oi => (decimal?)oi.LineTotal) ?? 0m,
            MonthRevenue = await sellerOrderItems.Where(oi => oi.Order.OrderDate >= monthStart).SumAsync(oi => (decimal?)oi.LineTotal) ?? 0m,
            
            TodayOrders = await sellerOrderItems.Where(oi => oi.Order.OrderDate >= today && oi.Order.OrderDate < today.AddDays(1)).Select(oi => oi.OrderId).Distinct().CountAsync(),
            WeekOrders = await sellerOrderItems.Where(oi => oi.Order.OrderDate >= weekAgo).Select(oi => oi.OrderId).Distinct().CountAsync(),
            
            TotalCustomers = await sellerOrderItems.Select(oi => oi.Order.UserId).Distinct().CountAsync(),
            
            RecentOrders = await _db.Orders
                .Include(o => o.User)
                .Where(o => o.OrderItems.Any(oi => oi.ProductVariant != null && oi.ProductVariant.Product != null && oi.ProductVariant.Product!.SellerId == seller.Id))
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new RecentOrder
                {
                    OrderId = o.Id,
                    CustomerName = o.User != null ? (o.User.FullName ?? o.User.Email ?? "Guest") : "Guest",
                    Date = o.OrderDate,
                    Total = o.OrderItems.Where(oi => oi.ProductVariant != null && oi.ProductVariant.Product != null && oi.ProductVariant.Product!.SellerId == seller.Id).Sum(oi => oi.LineTotal),
                    Status = o.Status.ToString()
                }).ToListAsync(),
            
            TopProducts = await _db.OrderItems
                .Include(oi => oi.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                .Where(oi => oi.ProductVariant != null && oi.ProductVariant.Product != null && oi.ProductVariant.Product.SellerId == seller.Id)
                .GroupBy(oi => new { Id = oi.ProductVariant!.Product!.Id, Title = oi.ProductVariant.Product.Title })
                .Select(g => new TopProduct
                {
                    ProductId = g.Key.Id,
                    ProductName = g.Key.Title,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(oi => oi.LineTotal)
                })
                .OrderByDescending(tp => tp.Revenue)
                .Take(5)
                .ToListAsync()
        };

        stats.AverageOrderValue = stats.TotalOrders > 0 ? stats.TotalRevenue / stats.TotalOrders : 0m;

        return View(stats);
    }

    public async Task<IActionResult> Products()
    {
        var userId = GetUserId();
        var seller = await _db.Sellers.FirstOrDefaultAsync(v => v.UserId == userId);

        if (seller == null)
        {
            return RedirectToAction("Setup");
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

    public async Task<IActionResult> StoreSettings()
    {
        var userId = GetUserId();
        var seller = await _db.Sellers.FirstOrDefaultAsync(v => v.UserId == userId);

        if (seller == null)
        {
            return RedirectToAction("Setup");
        }

        return View(seller);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStoreSettings(Domain.Sellers.Seller model)
    {
        var userId = GetUserId();
        var seller = await _db.Sellers.FirstOrDefaultAsync(v => v.UserId == userId);

        if (seller == null)
        {
            return RedirectToAction("Setup");
        }

        seller.ShopName = model.ShopName;
        seller.ShopDescription = model.ShopDescription;
        seller.MobileNumber = model.MobileNumber;
        seller.BkashMerchantNumber = model.BkashMerchantNumber;
        seller.BusinessAddress = model.BusinessAddress;
        seller.City = model.City;
        seller.District = model.District;
        seller.StoreLogo = model.StoreLogo ?? seller.StoreLogo;
        seller.StoreBanner = model.StoreBanner ?? seller.StoreBanner;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Store settings updated successfully!";
        return RedirectToAction(nameof(StoreSettings));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleProductStatus(int productId)
    {
        var userId = GetUserId();
        var seller = await _db.Sellers.FirstOrDefaultAsync(v => v.UserId == userId);

        if (seller == null)
        {
            return Json(new { success = false, message = "Seller not found" });
        }

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.SellerId == seller.Id);
        if (product == null)
        {
            return Json(new { success = false, message = "Product not found" });
        }

        product.IsActive = !product.IsActive;
        await _db.SaveChangesAsync();

        return Json(new { success = true, isActive = product.IsActive });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStock(int variantId, int stock)
    {
        var userId = GetUserId();
        var seller = await _db.Sellers.FirstOrDefaultAsync(v => v.UserId == userId);

        if (seller == null)
        {
            return Json(new { success = false, message = "Seller not found" });
        }

        var variant = await _db.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == variantId && v.Product.SellerId == seller.Id);

        if (variant == null)
        {
            return Json(new { success = false, message = "Variant not found" });
        }

        variant.Stock = stock;
        await _db.SaveChangesAsync();

        return Json(new { success = true, stock = variant.Stock });
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePrice(int variantId, decimal price)
    {
        var userId = GetUserId();
        var seller = await _db.Sellers.FirstOrDefaultAsync(v => v.UserId == userId);

        if (seller == null)
        {
            return Json(new { success = false, message = "Seller not found" });
        }

        var variant = await _db.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == variantId && v.Product.SellerId == seller.Id);

        if (variant == null)
        {
            return Json(new { success = false, message = "Variant not found" });
        }

        variant.Price = price;
        await _db.SaveChangesAsync();

        return Json(new { success = true, price = variant.Price });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStoreStatus()
    {
        var userId = GetUserId();
        var seller = await _db.Sellers.FirstOrDefaultAsync(v => v.UserId == userId);

        if (seller == null)
        {
            return Json(new { success = false, message = "Seller not found" });
        }

        seller.Status = seller.Status == Domain.Sellers.SellerStatus.Active 
            ? Domain.Sellers.SellerStatus.Pending 
            : Domain.Sellers.SellerStatus.Active;

        await _db.SaveChangesAsync();

        return Json(new { success = true, status = seller.Status.ToString(), isActive = seller.Status == Domain.Sellers.SellerStatus.Active });
    }

    public class SellerDashboardStats
    {
        public string ShopName { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public int TodayOrders { get; set; }
        public int WeekOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalCustomers { get; set; }
        public List<RecentOrder> RecentOrders { get; set; } = new();
        public List<TopProduct> TopProducts { get; set; } = new();
    }

    public class RecentOrder
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class TopProduct
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
