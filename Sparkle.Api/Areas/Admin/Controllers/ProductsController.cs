using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sparkle.Domain.Catalog;
using Sparkle.Infrastructure;

namespace Sparkle.Api.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ProductsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p => 
                p.Title.ToLower().Contains(term) || 
                (p.Description != null && p.Description.ToLower().Contains(term)) ||
                (p.Seller != null && p.Seller.ShopName.ToLower().Contains(term)));
            
            ViewBag.SearchQuery = q;
        }

        var totalItems = await query.CountAsync();
        var pageSize = 20;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(products);
    }
}
