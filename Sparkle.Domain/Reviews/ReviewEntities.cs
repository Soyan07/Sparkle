using Sparkle.Domain.Common;
using Sparkle.Domain.Identity;
using Sparkle.Domain.Catalog;
using Sparkle.Domain.Orders;
using Sparkle.Domain.Sellers;

namespace Sparkle.Domain.Reviews;

public class ProductReview : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public int? OrderItemId { get; set; }
    public OrderItem? OrderItem { get; set; }
    
    public int? SellerId { get; set; }
    public Seller? Seller { get; set; }
    
    // Review Content
    public int Rating { get; set; } // 1-5 stars
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    
    // Product Quality Ratings
    public int? QualityRating { get; set; }
    public int? ValueForMoneyRating { get; set; }
    public int? AccuracyRating { get; set; } // Matches description?
    
    // Verification
    public bool IsVerifiedPurchase { get; set; }
    public DateTime? PurchaseDate { get; set; }
    
    // Status
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    // Interactions
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public int ReportCount { get; set; }
    
    // Seller Response
    public string? SellerResponse { get; set; }
    public DateTime? SellerResponseDate { get; set; }
    
    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
    
    public ICollection<ReviewImage> Images { get; set; } = new List<ReviewImage>();
    public ICollection<ReviewVote> Votes { get; set; } = new List<ReviewVote>();
}

public class ReviewImage : BaseEntity
{
    public int ProductReviewId { get; set; }
    public ProductReview ProductReview { get; set; } = null!;
    
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
}

public class ReviewVote : BaseEntity
{
    public int ProductReviewId { get; set; }
    public ProductReview ProductReview { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public bool IsHelpful { get; set; } // true = helpful, false = not helpful
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
}

public class ProductQuestion : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public string Question { get; set; } = string.Empty;
    public string? Context { get; set; }
    
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public DateTime AskedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    
    public int AnswerCount { get; set; }
    public int UpvoteCount { get; set; }
    
    public ICollection<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
}

public class QuestionAnswer : BaseEntity
{
    public int ProductQuestionId { get; set; }
    public ProductQuestion ProductQuestion { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public int? SellerId { get; set; }
    public Seller? Seller { get; set; }
    
    public string Answer { get; set; } = string.Empty;
    
    public bool IsSellerAnswer { get; set; }
    public bool IsVerifiedPurchaser { get; set; }
    
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    
    public int UpvoteCount { get; set; }
    public int DownvoteCount { get; set; }
}
