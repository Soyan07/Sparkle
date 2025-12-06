namespace Sparkle.Domain.Financial;

public class SellerEarning
{
    public int Id { get; set; }
    public string SellerId { get; set; } = null!;
    public int OrderId { get; set; }
    public decimal OrderAmount { get; set; }
    public decimal SellerAmount { get; set; } // 97%
    public decimal CommissionAmount { get; set; } // 3%
    public decimal CommissionRate { get; set; } = 0.03m;
    public PayoutStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidOutAt { get; set; }
    public string? PayoutTransactionId { get; set; }
}

public class AdminCommission
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string SellerId { get; set; } = null!;
    public decimal OrderAmount { get; set; }
    public decimal CommissionAmount { get; set; } // 3%
    public decimal CommissionRate { get; set; } = 0.03m;
    public DateTime CreatedAt { get; set; }
}

public class PayoutRequest
{
    public int Id { get; set; }
    public string SellerId { get; set; } = null!;
    public decimal RequestedAmount { get; set; }
    public string BkashMerchantNumber { get; set; } = null!;
    public PayoutStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public string? Remarks { get; set; }
    public string? TransactionId { get; set; }
}

public enum PayoutStatus
{
    Pending,
    Processing,
    Completed,
    Rejected
}
