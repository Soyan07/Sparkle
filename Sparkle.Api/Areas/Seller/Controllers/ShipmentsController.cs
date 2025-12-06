using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sparkle.Domain.Identity;
using Sparkle.Domain.Orders;
using Sparkle.Infrastructure;
using Sparkle.Infrastructure.Services;

namespace Sparkle.Api.Areas.Seller.Controllers;

[Area("Seller")]
[Authorize(Roles = "Seller")]
public class ShipmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ShipmentService _shipmentService;

    public ShipmentsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ShipmentService shipmentService)
    {
        _context = context;
        _userManager = userManager;
        _shipmentService = shipmentService;
    }

    // GET: /Seller/Shipments
    public async Task<IActionResult> Index(int page = 1, string? status = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");
        var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.UserId == user.Id);
        
        if (seller == null)
            return RedirectToAction("Register", "Seller");

        var query = _context.Shipments
            .Include(s => s.Order)
            .Include(s => s.Items)
            .Where(s => s.SellerId == seller.Id);

        // Filter by status if provided
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ShipmentStatus>(status, out var shipmentStatus))
        {
            query = query.Where(s => s.Status == shipmentStatus);
        }

        var pageSize = 20;
        var totalShipments = await query.CountAsync();
        var shipments = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalShipments / (double)pageSize);
        ViewBag.StatusFilter = status;

        return View(shipments);
    }

    // GET: /Seller/Shipments/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");
        var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.UserId == user.Id);

        if (seller == null)
            return RedirectToAction("Register", "Seller");

        var shipment = await _context.Shipments
            .Include(s => s.Order)
            .Include(s => s.Items)
                .ThenInclude(si => si.OrderItem)
                    .ThenInclude(oi => oi.Product)
            .Include(s => s.TrackingEvents.OrderByDescending(te => te.EventTime))
            .FirstOrDefaultAsync(s => s.Id == id && s.SellerId == seller.Id);

        if (shipment == null)
            return NotFound();

        return View(shipment);
    }

    // GET: /Seller/Shipments/Create?orderId=123
    public async Task<IActionResult> Create(int orderId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");
        var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.UserId == user.Id);

        if (seller == null)
            return RedirectToAction("Register", "Seller");

        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return NotFound();

        // Get only items that belong to this seller and not yet shipped
        var sellerItems = order.OrderItems
            .Where(oi => oi.SellerId == seller.Id)
            .ToList();

        if (!sellerItems.Any())
            return NotFound("No items found for this seller in this order");

        // Check if already shipped
        var existingShipments = await _context.Shipments
            .Include(s => s.Items)
            .Where(s => s.OrderId == orderId && s.SellerId == seller.Id)
            .ToListAsync();

        var shippedItemIds = existingShipments
            .SelectMany(s => s.Items.Select(i => i.OrderItemId))
            .ToHashSet();

        var unshippedItems = sellerItems
            .Where(oi => !shippedItemIds.Contains(oi.Id))
            .ToList();

        if (!unshippedItems.Any())
        {
            TempData["Warning"] = "All items in this order have already been shipped.";
            return RedirectToAction("Details", "Orders", new { id = orderId });
        }

        ViewBag.Order = order;
        ViewBag.UnshippedItems = unshippedItems;
        ViewBag.Couriers = new[] { "Pathao", "Steadfast", "Redx", "Sundarban", "SA Paribahan", "Other" };

        return View();
    }

    // POST: /Seller/Shipments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int orderId, List<int> itemIds, string courierName, string? trackingNumber, string? notes)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");
        var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.UserId == user.Id);

        if (seller == null)
            return RedirectToAction("Register", "Seller");

        if (!itemIds.Any())
        {
            TempData["Error"] = "Please select at least one item to ship.";
            return RedirectToAction("Create", new { orderId });
        }

        try
        {
            var shipment = await _shipmentService.CreateShipmentAsync(
                orderId: orderId,
                sellerId: seller.Id,
                orderItemIds: itemIds,
                courierName: courierName,
                trackingNumber: trackingNumber
            );

            if (!string.IsNullOrEmpty(notes))
            {
                shipment.InternalNotes = notes;
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"Shipment {shipment.ShipmentNumber} created successfully!";
            return RedirectToAction("Details", new { id = shipment.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error creating shipment: {ex.Message}";
            return RedirectToAction("Create", new { orderId });
        }
    }

    // GET: /Seller/Shipments/UpdateStatus/5
    public async Task<IActionResult> UpdateStatus(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");
        var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.UserId == user.Id);

        if (seller == null)
            return RedirectToAction("Register", "Seller");

        var shipment = await _context.Shipments
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == id && s.SellerId == seller.Id);

        if (shipment == null)
            return NotFound();

        return View(shipment);
    }

    // POST: /Seller/Shipments/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ShipmentStatus status, string message, string? location, string? trackingNumber)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");
        var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.UserId == user.Id);

        if (seller == null)
            return RedirectToAction("Register", "Seller");

        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.Id == id && s.SellerId == seller.Id);

        if (shipment == null)
            return NotFound();

        try
        {
            // Update tracking number if provided
            if (!string.IsNullOrEmpty(trackingNumber) && trackingNumber != shipment.TrackingNumber)
            {
                await _shipmentService.UpdateTrackingInfoAsync(id, trackingNumber);
            }

            // Update status
            await _shipmentService.UpdateShipmentStatusAsync(
                shipmentId: id,
                status: status,
                message: message,
                location: location,
                userId: user.Id
            );

            TempData["Success"] = "Shipment status updated successfully!";
            return RedirectToAction("Details", new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error updating status: {ex.Message}";
            return RedirectToAction("UpdateStatus", new { id });
        }
    }

    // POST: /Seller/Shipments/AddTracking
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTracking(int id, string trackingNumber, string? trackingUrl)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");
        var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.UserId == user.Id);

        if (seller == null)
            return Json(new { success = false, message = "Seller not found" });

        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.Id == id && s.SellerId == seller.Id);

        if (shipment == null)
            return Json(new { success = false, message = "Shipment not found" });

        try
        {
            await _shipmentService.UpdateTrackingInfoAsync(id, trackingNumber, null, trackingUrl);
            return Json(new { success = true, message = "Tracking information updated successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // GET: /Seller/Shipments/PrintLabel/5
    public async Task<IActionResult> PrintLabel(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");
        var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.UserId == user.Id);

        if (seller == null)
            return RedirectToAction("Register", "Seller");

        var shipment = await _context.Shipments
            .Include(s => s.Order)
            .Include(s => s.Seller)
            .Include(s => s.Items)
                .ThenInclude(si => si.OrderItem)
                    .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(s => s.Id == id && s.SellerId == seller.Id);

        if (shipment == null)
            return NotFound();

        return View(shipment);
    }
}
