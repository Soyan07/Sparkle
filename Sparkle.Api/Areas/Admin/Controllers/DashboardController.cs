using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sparkle.Api.Models;
using Sparkle.Domain.Orders;
using Sparkle.Infrastructure;

namespace Sparkle.Api.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var weekAgo = now.AddDays(-7);

        // Optimize category counts query
        var categoryCounts = await _db.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .GroupBy(p => p.Category.Slug)
            .Select(g => new { Slug = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Slug, x => x.Count);

        var stats = new DashboardStats
        {
            TotalUsers = await _db.Users.CountAsync(u => !u.IsSeller),
            NewUsersToday = await _db.Users.CountAsync(u => u.Id != null && !u.IsSeller), // Simplified
            TotalSellers = await _db.Sellers.CountAsync(),
            ActiveSellers = await _db.Sellers.CountAsync(v => v.Status == Domain.Sellers.SellerStatus.Active),
            PendingSellers = await _db.Sellers.CountAsync(v => v.Status == Domain.Sellers.SellerStatus.Pending),
            
            TotalProducts = await _db.Products.CountAsync(),
            ActiveProducts = await _db.Products.CountAsync(p => p.IsActive),
            TotalCategories = await _db.Categories.CountAsync(),
            
            TotalOrders = await _db.Orders.CountAsync(),
            PendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
            ProcessingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Processing),
            DeliveredOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Delivered),
            TodayOrders = await _db.Orders.CountAsync(o => o.OrderDate >= today && o.OrderDate < today.AddDays(1)),
            WeekOrders = await _db.Orders.CountAsync(o => o.OrderDate >= weekAgo),
            
            TotalRevenue = await _db.Orders.Where(o => o.Status == OrderStatus.Delivered).SumAsync(o => (decimal?)o.Total) ?? 0m,
            TodayRevenue = await _db.Orders.Where(o => o.OrderDate >= today && o.OrderDate < today.AddDays(1) && o.Status == OrderStatus.Delivered).SumAsync(o => (decimal?)o.Total) ?? 0m,
            MonthRevenue = await _db.Orders.Where(o => o.OrderDate >= monthStart && o.Status == OrderStatus.Delivered).SumAsync(o => (decimal?)o.Total) ?? 0m,
            
            RecentOrders = await _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new AdminRecentOrder
                {
                    OrderId = o.Id,
                    CustomerName = o.User != null ? (o.User.FullName ?? o.User.Email ?? "Guest") : "Guest",
                    Date = o.CreatedAt,
                    Total = o.Total,
                    Status = o.Status.ToString()
                }).ToListAsync(),
            
            TopSellers = await _db.Sellers
                .Select(v => new TopSeller
                {
                    SellerId = v.Id,
                    ShopName = v.ShopName,
                    TotalProducts = _db.Products.Count(p => p.SellerId == v.Id),
                    TotalRevenue = _db.OrderItems
                        .Where(oi => oi.ProductVariant != null && oi.ProductVariant.Product != null && oi.ProductVariant.Product.SellerId == v.Id)
                        .Sum(oi => (decimal?)oi.LineTotal) ?? 0m
                })
                .OrderByDescending(tv => tv.TotalRevenue)
                .Take(5)
                .ToListAsync(),

            CategoryHighlights = CategoryHighlightProvider.All
                .Select(h => new CategoryHighlightStats
                {
                    Highlight = h,
                    ProductCount = categoryCounts.GetValueOrDefault(h.Slug, 0)
                })
                .ToList()
        };

        stats.AverageOrderValue = stats.TotalOrders > 0 ? stats.TotalRevenue / stats.TotalOrders : 0m;

        return View(stats);
    }

    public class DashboardStats
    {
        public int TotalUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int TotalSellers { get; set; }
        public int ActiveSellers { get; set; }
        public int PendingSellers { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int TodayOrders { get; set; }
        public int WeekOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<AdminRecentOrder> RecentOrders { get; set; } = new();
        public List<TopSeller> TopSellers { get; set; } = new();
        public List<CategoryHighlightStats> CategoryHighlights { get; set; } = new();
    }

    public class AdminRecentOrder
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class TopSeller
    {
        public int SellerId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public decimal TotalRevenue { get; set; }
    }
    
    public class CategoryHighlightStats
    {
        public CategoryHighlight Highlight { get; set; } = default!;
        public int ProductCount { get; set; }
    }
}
