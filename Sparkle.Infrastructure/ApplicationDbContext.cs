using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sparkle.Domain.Catalog;
using Sparkle.Domain.Financial;
using Sparkle.Domain.Notifications;
using Sparkle.Domain.Orders;
using Sparkle.Domain.Support;
using Sparkle.Domain.Sellers;
using Sparkle.Domain.Users;
using Sparkle.Domain.Reviews;
using Sparkle.Domain.Marketing;
using Sparkle.Domain.Logistics;
using Sparkle.Domain.System;
using Sparkle.Domain.Configuration;
using Sparkle.Domain.Identity;
using SystemNotification = Sparkle.Domain.System.Notification;
using LegacyNotification = Sparkle.Domain.Notifications.Notification;
using AnalyticsSellerEarning = Sparkle.Domain.System.SellerEarning;
using FinancialSellerEarning = Sparkle.Domain.Financial.SellerEarning;

namespace Sparkle.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // ==================== CATALOG ====================
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Review> Reviews => Set<Review>();

    // ==================== USER MANAGEMENT ====================
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<UserWishlistItem> UserWishlistItems => Set<UserWishlistItem>();
    public DbSet<UserSearchHistory> UserSearchHistories => Set<UserSearchHistory>();
    public DbSet<UserNotificationSettings> UserNotificationSettings => Set<UserNotificationSettings>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();

    // ==================== ORDERS & PAYMENTS ====================
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderTracking> OrderTrackings => Set<OrderTracking>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<ReturnRequest> ReturnRequests => Set<ReturnRequest>();

    // ==================== REVIEWS & RATINGS ====================
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<ReviewImage> ReviewImages => Set<ReviewImage>();
    public DbSet<ReviewVote> ReviewVotes => Set<ReviewVote>();
    public DbSet<ProductQuestion> ProductQuestions => Set<ProductQuestion>();
    public DbSet<QuestionAnswer> QuestionAnswers => Set<QuestionAnswer>();

    // ==================== MARKETING & PROMOTIONS ====================
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<LoyaltyPointHistory> LoyaltyPointHistories => Set<LoyaltyPointHistory>();
    public DbSet<LoyaltyPointConfig> LoyaltyPointConfigs => Set<LoyaltyPointConfig>();
    public DbSet<VoucherUsage> VoucherUsages => Set<VoucherUsage>();
    public DbSet<FlashDeal> FlashDeals => Set<FlashDeal>();
    public DbSet<FlashDealProduct> FlashDealProducts => Set<FlashDealProduct>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignProduct> CampaignProducts => Set<CampaignProduct>();
    public DbSet<CampaignCategory> CampaignCategories => Set<CampaignCategory>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<EmailSubscription> EmailSubscriptions => Set<EmailSubscription>();
    public DbSet<NewsletterCampaign> NewsletterCampaigns => Set<NewsletterCampaign>();

    // ==================== SELLER MANAGEMENT ====================
    public DbSet<Seller> Sellers => Set<Seller>();
    public DbSet<SellerPayoutRequest> SellerPayoutRequests => Set<SellerPayoutRequest>();
    public DbSet<SellerDocument> SellerDocuments => Set<SellerDocument>();
    public DbSet<SellerBankAccount> SellerBankAccounts => Set<SellerBankAccount>();
    public DbSet<SellerPayout> SellerPayouts => Set<SellerPayout>();
    public DbSet<SellerPerformanceMetric> SellerPerformanceMetrics => Set<SellerPerformanceMetric>();
    public DbSet<SellerSubscription> SellerSubscriptions => Set<SellerSubscription>();
    public DbSet<SellerFollower> SellerFollowers => Set<SellerFollower>();
    public DbSet<SellerAnalyticsSummary> SellerAnalyticsSummaries => Set<SellerAnalyticsSummary>();

    // ==================== SUPPORT & COMMUNICATION ====================
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();
    public DbSet<SystemNotification> SystemNotifications => Set<SystemNotification>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    
    // ==================== CONFIGURATION ====================
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    
    // Keep old support tables for backward compatibility
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketReply> TicketReplies => Set<TicketReply>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();

    // ==================== SHIPPING & LOGISTICS ====================
    public DbSet<DeliveryZone> DeliveryZones => Set<DeliveryZone>();
    public DbSet<DeliveryArea> DeliveryAreas => Set<DeliveryArea>();
    public DbSet<ShippingMethod> ShippingMethods => Set<ShippingMethod>();
    public DbSet<CourierPartner> CourierPartners => Set<CourierPartner>();
    public DbSet<ShippingZone> ShippingZones => Set<ShippingZone>();
    public DbSet<ShippingRate> ShippingRates => Set<ShippingRate>();
    public DbSet<Sparkle.Domain.Orders.Shipment> Shipments => Set<Sparkle.Domain.Orders.Shipment>();
    public DbSet<Sparkle.Domain.Orders.ShipmentItem> ShipmentItems => Set<Sparkle.Domain.Orders.ShipmentItem>();
    public DbSet<Sparkle.Domain.Orders.ShipmentTrackingEvent> ShipmentTrackingEvents => Set<Sparkle.Domain.Orders.ShipmentTrackingEvent>();

    // ==================== ANALYTICS & REPORTS ====================
    public DbSet<ProductView> ProductViews => Set<ProductView>();
    public DbSet<SearchAnalytics> SearchAnalytics => Set<SearchAnalytics>();
    public DbSet<SalesReport> SalesReports => Set<SalesReport>();
    public DbSet<AnalyticsSellerEarning> AnalyticsSellerEarnings => Set<AnalyticsSellerEarning>();
    public DbSet<PlatformMetric> PlatformMetrics => Set<PlatformMetric>();

    // ==================== LEGACY FINANCIAL & NOTIFICATIONS ====================
    public DbSet<FinancialSellerEarning> FinancialSellerEarnings => Set<FinancialSellerEarning>();
    public DbSet<AdminCommission> AdminCommissions => Set<AdminCommission>();
    public DbSet<PayoutRequest> PayoutRequests => Set<PayoutRequest>();
    public DbSet<LegacyNotification> LegacyNotifications => Set<LegacyNotification>();
    public DbSet<SmsLog> SmsLogs => Set<SmsLog>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    public override int SaveChanges()
    {
        ValidateProductRules();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ValidateProductRules();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ValidateProductRules()
    {
        var productEntries = ChangeTracker.Entries<Product>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in productEntries)
        {
            var product = entry.Entity;
            
            // RULE 1: "Sparkle Star" products (IsAdminProduct = true) must not have regular seller IDs
            // Only admin/system seller can create Sparkle Star products
            if (product.IsAdminProduct && product.SellerId.HasValue)
            {
                // Get seller to check if it's the system admin seller
                var seller = Sellers.Local.FirstOrDefault(v => v.Id == product.SellerId.Value)
                    ?? Sellers.Find(product.SellerId.Value);
                
                // Only allow if this is the Sparkle admin seller (ShopName = "Sparkle Official")
                if (seller == null || seller.ShopName != "Sparkle Official")
                {
                    throw new InvalidOperationException(
                        "Only administrators can create 'Sparkle Star' products. " +
                        "Sellers cannot set IsAdminProduct = true on their products.");
                }
            }
            
            // RULE 2: Regular seller products must have a valid SellerId
            if (!product.IsAdminProduct && !product.SellerId.HasValue)
            {
                throw new InvalidOperationException(
                    "Non-admin products must be associated with a seller. " +
                    "Please set SellerId for this product.");
            }
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HaveColumnType("decimal(18,2)");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ==================== CATALOG SCHEMA ====================
        builder.Entity<Category>().ToTable("Categories", "catalog");
        builder.Entity<Brand>().ToTable("Brands", "catalog");
        builder.Entity<Product>().ToTable("Products", "catalog");
        builder.Entity<ProductVariant>().ToTable("ProductVariants", "catalog");
        builder.Entity<ProductImage>().ToTable("ProductImages", "catalog");
        builder.Entity<Review>().ToTable("Reviews", "catalog");

        // ==================== USER SCHEMA ====================
        builder.Entity<UserProfile>().ToTable("UserProfiles", "users");
        builder.Entity<UserAddress>().ToTable("UserAddresses", "users");
        builder.Entity<UserWishlistItem>().ToTable("UserWishlistItems", "users");
        builder.Entity<UserSearchHistory>().ToTable("UserSearchHistories", "users");
        builder.Entity<UserNotificationSettings>().ToTable("UserNotificationSettings", "users");
        builder.Entity<UserDevice>().ToTable("UserDevices", "users");

        // ==================== ORDERS SCHEMA ====================
        builder.Entity<Address>().ToTable("Addresses", "orders");
        builder.Entity<Cart>().ToTable("Carts", "orders");
        builder.Entity<CartItem>().ToTable("CartItems", "orders");
        builder.Entity<Wishlist>().ToTable("Wishlists", "orders");
        builder.Entity<WishlistItem>().ToTable("WishlistItems", "orders");
        builder.Entity<Order>().ToTable("Orders", "orders");
        builder.Entity<OrderItem>().ToTable("OrderItems", "orders");
        builder.Entity<OrderTracking>().ToTable("OrderTrackings", "orders");

        // ==================== PAYMENTS SCHEMA ====================
        builder.Entity<PaymentMethod>().ToTable("PaymentMethods", "payments");
        builder.Entity<Transaction>().ToTable("Transactions", "payments");
        builder.Entity<Refund>().ToTable("Refunds", "payments");
        builder.Entity<ReturnRequest>().ToTable("ReturnRequests", "payments");

        // ==================== REVIEWS SCHEMA ====================
        builder.Entity<ProductReview>().ToTable("ProductReviews", "reviews");
        builder.Entity<ReviewImage>().ToTable("ReviewImages", "reviews");
        builder.Entity<ReviewVote>().ToTable("ReviewVotes", "reviews");
        builder.Entity<ReviewVote>()
            .HasOne(rv => rv.ProductReview)
            .WithMany(pr => pr.Votes)
            .HasForeignKey(rv => rv.ProductReviewId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<ReviewVote>()
            .HasOne(rv => rv.User)
            .WithMany()
            .HasForeignKey(rv => rv.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProductQuestion>().ToTable("ProductQuestions", "reviews");
        builder.Entity<QuestionAnswer>().ToTable("QuestionAnswers", "reviews");

        // ==================== MARKETING SCHEMA ====================
        builder.Entity<Coupon>().ToTable("Coupons", "marketing");
        builder.Entity<VoucherUsage>().ToTable("VoucherUsages", "marketing");
        builder.Entity<FlashDeal>().ToTable("FlashDeals", "marketing");
        builder.Entity<FlashDealProduct>().ToTable("FlashDealProducts", "marketing");
        builder.Entity<Campaign>().ToTable("Campaigns", "marketing");
        builder.Entity<CampaignProduct>().ToTable("CampaignProducts", "marketing");
        builder.Entity<CampaignCategory>().ToTable("CampaignCategories", "marketing");
        builder.Entity<Banner>().ToTable("Banners", "marketing");
        builder.Entity<EmailSubscription>().ToTable("EmailSubscriptions", "marketing");
        builder.Entity<NewsletterCampaign>().ToTable("NewsletterCampaigns", "marketing");

        // ==================== SELLERS SCHEMA ====================
        builder.Entity<Seller>().ToTable("Sellers", "sellers");
        builder.Entity<SellerPayoutRequest>().ToTable("SellerPayoutRequests", "sellers");
        builder.Entity<SellerDocument>().ToTable("SellerDocuments", "sellers");
        builder.Entity<SellerBankAccount>().ToTable("SellerBankAccounts", "sellers");
        builder.Entity<SellerPayout>().ToTable("SellerPayouts", "sellers");
        builder.Entity<SellerPerformanceMetric>().ToTable("SellerPerformanceMetrics", "sellers");
        builder.Entity<SellerSubscription>().ToTable("SellerSubscriptions", "sellers");
        builder.Entity<SellerFollower>().ToTable("SellerFollowers", "sellers");
        builder.Entity<SellerAnalyticsSummary>().ToTable("SellerAnalyticsSummaries", "sellers");
        
        builder.Entity<Seller>()
            .HasIndex(s => s.ShopName);

        // ==================== SUPPORT SCHEMA ====================
        builder.Entity<SupportTicket>().ToTable("SupportTickets", "support");
        builder.Entity<TicketMessage>().ToTable("TicketMessages", "support");
        builder.Entity<Ticket>().ToTable("Tickets", "support");
        builder.Entity<TicketReply>().ToTable("TicketReplies", "support");
        builder.Entity<TicketAttachment>().ToTable("TicketAttachments", "support");

        // ==================== SHIPPING SCHEMA ====================
        builder.Entity<ShippingMethod>().ToTable("ShippingMethods", "shipping");
        builder.Entity<CourierPartner>().ToTable("CourierPartners", "shipping");
        builder.Entity<ShippingZone>().ToTable("ShippingZones", "shipping");
        builder.Entity<ShippingRate>().ToTable("ShippingRates", "shipping");
        builder.Entity<Sparkle.Domain.Orders.Shipment>().ToTable("Shipments", "shipping");
        builder.Entity<Sparkle.Domain.Orders.ShipmentItem>().ToTable("ShipmentItems", "shipping");
        builder.Entity<Sparkle.Domain.Orders.ShipmentTrackingEvent>().ToTable("ShipmentTrackingEvents", "shipping");

        // ==================== ANALYTICS SCHEMA ====================
        builder.Entity<ProductView>().ToTable("ProductViews", "analytics");
        builder.Entity<SearchAnalytics>().ToTable("SearchAnalytics", "analytics");
        builder.Entity<SalesReport>().ToTable("SalesReports", "analytics");
        builder.Entity<AnalyticsSellerEarning>().ToTable("SellerEarnings", "analytics");
        builder.Entity<PlatformMetric>().ToTable("PlatformMetrics", "analytics");

        // ==================== SYSTEM SCHEMA ====================
        builder.Entity<SystemNotification>().ToTable("Notifications", "system");
        builder.Entity<ActivityLog>().ToTable("ActivityLogs", "system");
        builder.Entity<SiteSetting>().ToTable("SiteSettings", "system");

        // ==================== LEGACY SCHEMAS ====================
        builder.Entity<FinancialSellerEarning>().ToTable("SellerEarnings", "financial");
        builder.Entity<AdminCommission>().ToTable("AdminCommissions", "financial");
        builder.Entity<PayoutRequest>().ToTable("PayoutRequests", "financial");
        builder.Entity<LegacyNotification>().ToTable("Notifications", "notifications");
        builder.Entity<SmsLog>().ToTable("SmsLogs", "notifications");
        builder.Entity<EmailLog>().ToTable("EmailLogs", "notifications");

        // ==================== CONFIGURE RELATIONSHIPS & INDEXES ====================
        
        // Product - Seller relationship
        builder.Entity<Product>()
            .HasOne(p => p.Seller)
            .WithMany()
            .HasForeignKey(p => p.SellerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Product - Category relationship
        builder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // ProductVariant - Product relationship
        builder.Entity<ProductVariant>()
            .HasOne(pv => pv.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // ProductImage - Product relationship
        builder.Entity<ProductImage>()
            .HasOne(pi => pi.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Review - Product relationship
        builder.Entity<Review>()
            .HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // OrderItem - ProductVariant relationship
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.ProductVariant)
            .WithMany()
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Cart relationship configuration
        builder.Entity<Cart>()
            .HasMany(c => c.Items)
            .WithOne(ci => ci.Cart)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Wishlist relationship configuration
        builder.Entity<Wishlist>()
            .HasMany(w => w.Items)
            .WithOne(wi => wi.Wishlist)
            .HasForeignKey(wi => wi.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order relationship configuration
        builder.Entity<Order>()
            .HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Ignore backward compatibility properties
        builder.Entity<Order>().Ignore(o => o.Items);
        builder.Entity<Order>().Ignore(o => o.Subtotal);
        builder.Entity<Order>().Ignore(o => o.DiscountTotal);
        builder.Entity<Order>().Ignore(o => o.ShippingFee);
        builder.Entity<Order>().Ignore(o => o.Total);
        builder.Entity<Order>().Ignore(o => o.CreatedAt);
        
        // Order indexes for performance
        builder.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();
        
        builder.Entity<Order>()
            .HasIndex(o => new { o.UserId, o.OrderDate });
        
        builder.Entity<Order>()
            .HasIndex(o => o.Status);

        // Transaction indexes
        builder.Entity<Transaction>()
            .HasIndex(t => t.TransactionNumber)
            .IsUnique();
        
        builder.Entity<Transaction>()
            .HasIndex(t => new { t.UserId, t.TransactionDate });

        // ProductView indexes for analytics
        builder.Entity<ProductView>()
            .HasIndex(pv => new { pv.ProductId, pv.ViewedAt });
        
        builder.Entity<ProductView>()
            .HasIndex(pv => pv.UserId);

        // SearchAnalytics indexes
        builder.Entity<SearchAnalytics>()
            .HasIndex(sa => sa.SearchQuery);
        
        builder.Entity<SearchAnalytics>()
            .HasIndex(sa => sa.SearchedAt);

        // Review indexes
        builder.Entity<ProductReview>()
            .HasIndex(r => new { r.ProductId, r.Status });
        
        builder.Entity<ProductReview>()
            .HasIndex(r => r.UserId);

        // Coupon code unique index
        builder.Entity<Coupon>()
            .HasIndex(c => c.Code)
            .IsUnique();

        // Activity Log indexes
        builder.Entity<ActivityLog>()
            .HasIndex(al => new { al.UserId, al.Timestamp });
        
        builder.Entity<ActivityLog>()
            .HasIndex(al => new { al.EntityType, al.EntityId });
        
        // ==================== FIX CASCADE DELETE CONFLICTS ====================
        
        // VoucherUsages - Fix multiple cascade paths
        // VoucherUsages -> User (CASCADE) + VoucherUsages -> Orders -> User (CASCADE) = CONFLICT
        // Solution: Set VoucherUsages -> User to RESTRICT using navigation properties
        builder.Entity<VoucherUsage>()
            .HasOne(vu => vu.User)
            .WithMany()
            .HasForeignKey(vu => vu.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // VoucherUsages -> Orders set to RESTRICT to avoid cascade conflict
        builder.Entity<VoucherUsage>()
            .HasOne(vu => vu.Order)
            .WithMany()
            .HasForeignKey(vu => vu.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // VoucherUsages -> Coupons set to RESTRICT
        builder.Entity<VoucherUsage>()
            .HasOne(vu => vu.Coupon)
            .WithMany(c => c.UsageHistory)
            .HasForeignKey(vu => vu.CouponId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // TicketMessages - Fix multiple cascade paths
        // TicketMessages -> User (CASCADE) + TicketMessages -> SupportTickets -> User (CASCADE) = CONFLICT
        builder.Entity<TicketMessage>()
            .HasOne(tm => tm.User)
            .WithMany()
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<TicketMessage>()
            .HasOne(tm => tm.SupportTicket)
            .WithMany(st => st.Messages)
            .HasForeignKey(tm => tm.SupportTicketId)
            .OnDelete(DeleteBehavior.Cascade); // Keep cascade for ticket deletion
        
        // QuestionAnswers - Fix multiple cascade paths
        // QuestionAnswers -> User (CASCADE) + QuestionAnswers -> ProductQuestions -> User (CASCADE) = CONFLICT
        builder.Entity<QuestionAnswer>()
            .HasOne(qa => qa.User)
            .WithMany()
            .HasForeignKey(qa => qa.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<QuestionAnswer>()
            .HasOne(qa => qa.ProductQuestion)
            .WithMany(pq => pq.Answers)
            .HasForeignKey(qa => qa.ProductQuestionId)
            .OnDelete(DeleteBehavior.Cascade); // Keep cascade for question deletion
        
        // Refunds - Fix multiple cascade paths
        // Refunds -> User (CASCADE) + Refunds -> Orders -> User (CASCADE) = CONFLICT
        builder.Entity<Refund>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Refund>()
            .HasOne(r => r.Order)
            .WithMany()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete refunds
        
        // ReturnRequests - Fix cascade conflicts
        // ReturnRequests -> User + ReturnRequests -> Order -> User = CONFLICT  
        // ReturnRequests -> OrderItem -> Order + ReturnRequests -> Order = CONFLICT
        builder.Entity<ReturnRequest>()
            .HasOne(rr => rr.User)
            .WithMany()
            .HasForeignKey(rr => rr.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<ReturnRequest>()
            .HasOne(rr => rr.Order)
            .WithMany()
            .HasForeignKey(rr => rr.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<ReturnRequest>()
            .HasOne(rr => rr.OrderItem)
            .WithMany()
            .HasForeignKey(rr => rr.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // SellerEarnings - Fix cascade conflicts
        // SellerEarnings -> OrderItem -> Order + SellerEarnings -> Order = CONFLICT
        builder.Entity<Sparkle.Domain.System.SellerEarning>()
            .HasOne(ve => ve.OrderItem)
            .WithMany()
            .HasForeignKey(ve => ve.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Sparkle.Domain.System.SellerEarning>()
            .HasOne(ve => ve.Order)
            .WithMany()
            .HasForeignKey(ve => ve.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // ReviewVotes - Fix cascade conflicts  
        // ReviewVotes -> User + ReviewVotes -> ProductReview -> User = CONFLICT

        

        
        // ShipmentItems - Fix cascade conflicts
        // ShipmentItems -> Shipment -> Order -> User + ShipmentItems -> OrderItem -> Order -> User = CONFLICT
        builder.Entity<Sparkle.Domain.Orders.ShipmentItem>()
            .HasOne(si => si.Shipment)
            .WithMany(s => s.Items)
            .HasForeignKey(si => si.ShipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
