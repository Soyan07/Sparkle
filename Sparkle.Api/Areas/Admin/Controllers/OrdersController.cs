using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sparkle.Domain.Orders;
using Sparkle.Infrastructure;

namespace Sparkle.Api.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _db;

    public OrdersController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var pageSize = 20;
        var query = _db.Orders
            .Include(o => o.User)
            .AsQueryable();

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(orders);
    }
}
