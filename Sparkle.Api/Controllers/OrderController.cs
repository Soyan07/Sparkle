using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sparkle.Domain.Orders;
using Sparkle.Infrastructure;

namespace Sparkle.Api.Controllers;

[Authorize(Roles = "User")]
[Route("orders")]
public class OrderController : Controller
{
    private readonly ApplicationDbContext _db;

    public OrderController(ApplicationDbContext db)
    {
        _db = db;
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue(ClaimTypes.Name) ??
        throw new InvalidOperationException("User id not found in token");

    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var userId = GetUserId();
        var query = _db.Orders
            .Where(o => o.UserId == userId);

        var totalCount = await query.CountAsync();
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var vm = new PagedUserOrdersViewModel(orders, page, pageSize, totalCount);
        return View(vm);
    }

    public record PagedUserOrdersViewModel(
        IReadOnlyCollection<Order> Items,
        int Page,
        int PageSize,
        int TotalCount)
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var userId = GetUserId();
        var order = await _db.Orders
            .Include(o => o.ShippingAddress)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpGet("confirmation/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Confirmation(int id)
    {
        var order = await _db.Orders
            .Include(o => o.ShippingAddress)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpGet("track/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Track(int id)
    {
        var order = await _db.Orders
            .Include(o => o.ShippingAddress)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }
}
