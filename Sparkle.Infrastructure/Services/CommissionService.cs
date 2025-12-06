using Microsoft.EntityFrameworkCore;
using Sparkle.Domain.Financial;

namespace Sparkle.Infrastructure.Services;

public interface ICommissionService
{
    Task ProcessOrderCommissionAsync(int orderId);
    Task<decimal> GetSellerAvailableBalanceAsync(int sellerId);
    Task<decimal> GetAdminTotalCommissionsAsync();
}

public class CommissionService : ICommissionService
{
    private readonly ApplicationDbContext _db;
    private const decimal ADMIN_COMMISSION_RATE = 0.03m; // 3%
    private const decimal SELLER_RATE = 0.97m; // 97%

    public CommissionService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task ProcessOrderCommissionAsync(int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.ProductVariant)
            .ThenInclude(pv => pv!.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found");

        // Check if commission already processed
        var existingCommission = await _db.AdminCommissions
            .AnyAsync(c => c.OrderId == orderId);

        if (existingCommission)
            return; // Already processed

        // Group items by seller
        var sellerGroups = order.OrderItems
            .Where(i => i.ProductVariant != null)
            .GroupBy(i => i.ProductVariant!.Product.SellerId);

        foreach (var sellerGroup in sellerGroups)
        {
            var sellerId = sellerGroup.Key;
            var sellerOrderTotal = sellerGroup.Sum(i => i.UnitPrice * i.Quantity);

            var commissionAmount = sellerOrderTotal * ADMIN_COMMISSION_RATE;
            var sellerAmount = sellerOrderTotal * SELLER_RATE;

            // Create seller earning record
            var sellerEarning = new SellerEarning
            {
                SellerId = sellerId?.ToString() ?? string.Empty,
                OrderId = orderId,
                OrderAmount = sellerOrderTotal,
                SellerAmount = sellerAmount,
                CommissionAmount = commissionAmount,
                CommissionRate = ADMIN_COMMISSION_RATE,
                Status = Domain.Financial.PayoutStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _db.FinancialSellerEarnings.Add(sellerEarning);

            // Create admin commission record
            var adminCommission = new AdminCommission
            {
                OrderId = orderId,
                SellerId = sellerId?.ToString() ?? string.Empty,
                OrderAmount = sellerOrderTotal,
                CommissionAmount = commissionAmount,
                CommissionRate = ADMIN_COMMISSION_RATE,
                CreatedAt = DateTime.UtcNow
            };

            _db.AdminCommissions.Add(adminCommission);
        }

        await _db.SaveChangesAsync();
    }

    public async Task<decimal> GetSellerAvailableBalanceAsync(int sellerId)
    {
        // Total earnings
        var totalEarnings = await _db.FinancialSellerEarnings
            .Where(e => e.SellerId == sellerId.ToString())
            .SumAsync(e => e.SellerAmount);

        // Total withdrawn (paid out)
        var totalWithdrawn = await _db.FinancialSellerEarnings
            .Where(e => e.SellerId == sellerId.ToString() && e.Status == Domain.Financial.PayoutStatus.Completed)
            .SumAsync(e => e.SellerAmount);

        return totalEarnings - totalWithdrawn;
    }

    public async Task<decimal> GetAdminTotalCommissionsAsync()
    {
        return await _db.AdminCommissions.SumAsync(c => c.CommissionAmount);
    }
}
