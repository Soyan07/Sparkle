using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Sparkle.Api.Models;
using Sparkle.Domain.Sellers;
using Sparkle.Infrastructure;

namespace Sparkle.Api.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<HomeController> _logger;
    private readonly IDistributedCache _cache;

    public HomeController(ApplicationDbContext db, ILogger<HomeController> logger, IDistributedCache cache)
    {
        _db = db;
        _logger = logger;
        _cache = cache;
    }

    [ResponseCache(Duration = 60, VaryByHeader = "Cookie", Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Index()
    {
        // Try to get cached homepage data - reduced cache time for better responsiveness
        var cacheKey = "homepage_data_v3"; // Updated cache key to force refresh
        var cachedData = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedData))
        {
            try
            {
                var model = JsonSerializer.Deserialize<HomepageViewModel>(cachedData);
                if (model != null)
                {
                    _logger.LogInformation("Returning cached homepage data");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize cached homepage data");
                // Clear bad cache
                await _cache.RemoveAsync(cacheKey);
            }
        }

        // Build fresh data - Include both admin products and active seller products
        // Use AsSplitQuery to avoid cartesian explosion
        var baseQuery = _db.Products
            .AsNoTracking()
            .AsSplitQuery() // This fixes the slow query issue
            .Include(p => p.Images)
            .Include(p => p.Variants.Take(3)) // Limit variants to first 3
            .Include(p => p.Seller)
            .Where(p => p.IsActive && 
                (p.IsAdminProduct || (p.Seller != null && p.Seller.Status == SellerStatus.Active)));

        var newModel = new HomepageViewModel
        {
            Categories = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(),

            FeaturedProducts = await baseQuery
                .OrderByDescending(p => p.Id)
                .Take(12)
                .ToListAsync(),

            FlashDeals = await baseQuery
                .Where(p => p.DiscountPercent > 15)
                .OrderByDescending(p => p.DiscountPercent)
                .Take(8)
                .ToListAsync(),

            TrendingProducts = await baseQuery
                .Where(p => p.AverageRating >= 4.0m)
                .OrderByDescending(p => p.TotalReviews)
                .ThenByDescending(p => p.AverageRating)
                .Take(10)
                .ToListAsync()
        };

        // Cache for 2 minutes for better responsiveness
        try
        {
            var jsonOptions = new JsonSerializerOptions 
            { 
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var serialized = JsonSerializer.Serialize(newModel, jsonOptions);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache homepage data");
        }

        return View(newModel);
    }

    public async Task<IActionResult> Product(int id)
    {
        var product = await _db.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
            return NotFound();

        // Verify product access: Admin products are always accessible, seller products only if seller is active
        if (!product.IsAdminProduct && (product.Seller == null || product.Seller.Status != SellerStatus.Active))
            return NotFound();

        return View(product);
    }

    [HttpGet("/search")]
    public async Task<IActionResult> Search(string? q, string? category)
    {
        // Include both admin products and active seller products in search
        var query = _db.Products
            .AsNoTracking()
            .AsSplitQuery() // Optimize for performance
            .Include(p => p.Images)
            .Include(p => p.Variants.Take(3)) // Limit variants
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .Where(p => p.IsActive && 
                (p.IsAdminProduct || (p.Seller != null && p.Seller.Status == SellerStatus.Active)));

        // Search by keyword
        if (!string.IsNullOrWhiteSpace(q))
        {
            var searchTerm = q.Trim().ToLower();
            query = query.Where(p => 
                p.Title.ToLower().Contains(searchTerm) || 
                (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(searchTerm)));
        }

        // Filter by category
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category != null && p.Category.Slug == category);
        }

        // Get results
        var results = await query
            .OrderByDescending(p => p.Id)
            .Take(48)
            .ToListAsync();

        ViewBag.SearchQuery = q ?? "";
        ViewBag.Category = category ?? "";
        ViewBag.ResultCount = results.Count;

        return View(results);
    }

    [HttpGet("api/products/search-suggestions")]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "term" })]
    public async Task<IActionResult> GetSearchSuggestions(string term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            return Json(new List<string>());

        // Optimized query - no need to include Vendor for simple title search
        var suggestions = await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.Title.Contains(term))
            .Select(p => p.Title)
            .Distinct()
            .Take(8)
            .ToListAsync();

        return Json(suggestions);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel 
        { 
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
        });
    }


}

public class HomepageViewModel
{
    public List<Sparkle.Domain.Catalog.Category> Categories { get; set; } = new();
    public List<Sparkle.Domain.Catalog.Product> FeaturedProducts { get; set; } = new();
    public List<Sparkle.Domain.Catalog.Product> FlashDeals { get; set; } = new();
    public List<Sparkle.Domain.Catalog.Product> TrendingProducts { get; set; } = new();
}
