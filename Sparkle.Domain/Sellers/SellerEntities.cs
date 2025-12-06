namespace Sparkle.Domain.Sellers;

public enum SellerStatus
{
    Pending = 0,
    Active = 1,
    Suspended = 2
}

public class Seller
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!; // FK to ApplicationUser.Id

    // Business Information
    public string ShopName { get; set; } = default!;
    public string? ShopDescription { get; set; }
    public string? StoreLogo { get; set; }
    public string? StoreBanner { get; set; }
    public string? LogoUrl { get; set; } // Backwards compatibility
    
    // Store Metrics
    public decimal Rating { get; set; } = 4.5m;
    public int TotalProducts { get; set; } = 0;
    public int TotalSales { get; set; } = 0;
    
    // Location Details
    public string? City { get; set; }
    public string? District { get; set; }
    public string Country { get; set; } = "Bangladesh";

    // NID Verification (Bangladesh Requirement)
    public string? NidNumber { get; set; }
    public string? NidFrontImageUrl { get; set; }
    public string? NidBackImageUrl { get; set; }
    public bool IsNidVerified { get; set; }

    // Business Address with Map
    public string? BusinessAddress { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Contact Information
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }

    // Payment Information (Bangladesh)
    public string? BkashMerchantNumber { get; set; }

    public SellerStatus Status { get; set; } = SellerStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? RejectionReason { get; set; }

    public ICollection<SellerPayoutRequest> PayoutRequests { get; set; } = new List<SellerPayoutRequest>();
}

public enum PayoutStatus
{
    Requested = 0,
    Approved = 1,
    Rejected = 2,
    Paid = 3
}

public class SellerPayoutRequest
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public Seller Seller { get; set; } = default!;

    public decimal Amount { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public PayoutStatus Status { get; set; } = PayoutStatus.Requested;
    public DateTime? ProcessedAt { get; set; }
}
