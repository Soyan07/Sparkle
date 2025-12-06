using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sparkle.Infrastructure.Services;
using Sparkle.Domain.Orders;
using Sparkle.Infrastructure;

namespace Sparkle.Api.Controllers;

[Authorize(Roles = "User")]
[Route("checkout")]
public class CheckoutController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICommissionService _commissionService;

    public CheckoutController(ApplicationDbContext db, ICommissionService commissionService)
    {
        _db = db;
        _commissionService = commissionService;
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue(ClaimTypes.Name) ??
        throw new InvalidOperationException("User id not found in token");

    [HttpGet("")]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(Address));
    }

    // Step 1: Shipping Address
    [HttpGet("address")]
    public async Task<IActionResult> Address()
    {
        var userId = GetUserId();
        var cart = await _db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            return Redirect("/cart");
        }

        var addresses = await _db.Addresses.Where(a => a.UserId == userId).ToListAsync();
        var vm = new AddressStepViewModel
        {
            Cart = cart,
            SavedAddresses = addresses,
            NewAddress = new CheckoutAddressModel()
        };
        return View(vm);
    }

    [HttpPost("address")]
    public async Task<IActionResult> Address(CheckoutAddressModel model, int? savedAddressId)
    {
        var userId = GetUserId();
        
        int addressId;
        if (savedAddressId.HasValue)
        {
            addressId = savedAddressId.Value;
        }
        else
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Address));
            }
            
            var addr = new Address
            {
                UserId = userId,
                FullName = model.FullName,
                Phone = model.Phone,
                Line1 = model.Line1,
                Line2 = model.Line2,
                City = model.City,
                State = model.State,
                PostalCode = model.PostalCode,
                Country = model.Country
            };
            _db.Addresses.Add(addr);
            await _db.SaveChangesAsync();
            addressId = addr.Id;
        }

        HttpContext.Session.SetInt32("CheckoutAddressId", addressId);
        return RedirectToAction(nameof(Delivery));
    }

    // Step 2: Delivery Method
    [HttpGet("delivery")]
    public async Task<IActionResult> Delivery()
    {
        var userId = GetUserId();
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            return Redirect("/cart");
        }

        var addressId = HttpContext.Session.GetInt32("CheckoutAddressId");
        if (addressId == null)
        {
            return RedirectToAction(nameof(Address));
        }

        var vm = new DeliveryStepViewModel
        {
            Cart = cart
        };
        return View(vm);
    }

    [HttpPost("delivery")]
    public IActionResult Delivery(string deliveryMethod, decimal shippingFee)
    {
        HttpContext.Session.SetString("DeliveryMethod", deliveryMethod);
        HttpContext.Session.SetString("ShippingFee", shippingFee.ToString());
        return RedirectToAction(nameof(Payment));
    }

    // Step 3: Payment Method
    [HttpGet("payment")]
    public async Task<IActionResult> Payment()
    {
        var userId = GetUserId();
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            return Redirect("/cart");
        }

        var addressId = HttpContext.Session.GetInt32("CheckoutAddressId");
        var deliveryMethod = HttpContext.Session.GetString("DeliveryMethod");
        if (addressId == null || string.IsNullOrEmpty(deliveryMethod))
        {
            return RedirectToAction(nameof(Address));
        }

        var vm = new PaymentStepViewModel
        {
            Cart = cart
        };
        return View(vm);
    }

    [HttpPost("payment")]
    public IActionResult Payment(string paymentMethod)
    {
        HttpContext.Session.SetString("PaymentMethod", paymentMethod);
        return RedirectToAction(nameof(Review));
    }

    // Step 4: Review & Place Order
    [HttpGet("review")]
    public async Task<IActionResult> Review()
    {
        var userId = GetUserId();
        var cart = await _db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            return Redirect("/cart");
        }

        var addressId = HttpContext.Session.GetInt32("CheckoutAddressId");
        var deliveryMethod = HttpContext.Session.GetString("DeliveryMethod");
        var paymentMethod = HttpContext.Session.GetString("PaymentMethod");
        var shippingFeeStr = HttpContext.Session.GetString("ShippingFee");

        if (addressId == null || string.IsNullOrEmpty(deliveryMethod) || string.IsNullOrEmpty(paymentMethod))
        {
            return RedirectToAction(nameof(Address));
        }

        var address = await _db.Addresses.FindAsync(addressId.Value);
        var shippingFee = decimal.Parse(shippingFeeStr ?? "0");

        var vm = new ReviewStepViewModel
        {
            Cart = cart,
            Address = address!,
            DeliveryMethod = deliveryMethod,
            PaymentMethod = paymentMethod,
            ShippingFee = shippingFee
        };
        return View(vm);
    }

    [HttpPost("review")]
    public async Task<IActionResult> PlaceOrder()
    {
        var userId = GetUserId();
        var cart = await _db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            return Redirect("/cart");
        }

        var addressId = HttpContext.Session.GetInt32("CheckoutAddressId");
        var shippingFeeStr = HttpContext.Session.GetString("ShippingFee");
        if (addressId == null)
        {
            return RedirectToAction(nameof(Address));
        }

        var shippingFee = decimal.Parse(shippingFeeStr ?? "0");

        var subtotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);
        var discountTotal = 0m;
        var total = subtotal - discountTotal + shippingFee;

        var order = new Order
        {
            UserId = userId,
            ShippingAddressId = addressId.Value,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Subtotal = subtotal,
            DiscountTotal = discountTotal,
            ShippingFee = shippingFee,
            Total = total,
            OrderItems = cart.Items.Select(i => new OrderItem
            {
                ProductVariantId = i.ProductVariantId,
                ProductTitle = i.ProductVariant.Product.Title,
                VariantDescription = $"{i.ProductVariant.Color} {i.ProductVariant.Size}".Trim(),
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.UnitPrice * i.Quantity
            }).ToList()
        };

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cart.Items);
        _db.Carts.Remove(cart);
        await _db.SaveChangesAsync();

        // Process commission (97% vendor, 3% admin)
        await _commissionService.ProcessOrderCommissionAsync(order.Id);

        // Clear session
        HttpContext.Session.Remove("CheckoutAddressId");
        HttpContext.Session.Remove("DeliveryMethod");
        HttpContext.Session.Remove("PaymentMethod");
        HttpContext.Session.Remove("ShippingFee");

        return Redirect($"/orders/confirmation/{order.Id}");
    }

    public class CheckoutAddressModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public class AddressStepViewModel
    {
        public Cart Cart { get; set; } = default!;
        public List<Address> SavedAddresses { get; set; } = new();
        public CheckoutAddressModel NewAddress { get; set; } = default!;
        public decimal Subtotal => Cart.Items.Sum(i => i.UnitPrice * i.Quantity);
    }

    public class DeliveryStepViewModel
    {
        public Cart Cart { get; set; } = default!;
        public decimal Subtotal => Cart.Items.Sum(i => i.UnitPrice * i.Quantity);
    }

    public class PaymentStepViewModel
    {
        public Cart Cart { get; set; } = default!;
        public decimal Subtotal => Cart.Items.Sum(i => i.UnitPrice * i.Quantity);
    }

    public class ReviewStepViewModel
    {
        public Cart Cart { get; set; } = default!;
        public Address Address { get; set; } = default!;
        public string DeliveryMethod { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal ShippingFee { get; set; }
        public decimal Subtotal => Cart.Items.Sum(i => i.UnitPrice * i.Quantity);
        public decimal Total => Subtotal + ShippingFee;
    }
}
