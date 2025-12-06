using Sparkle.Domain.Sellers;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sparkle.Domain.Catalog;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Brand
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
}

public class Product
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? Features { get; set; } // JSON or comma-separated features
    public decimal BasePrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Sparkle Star Badge - ADMIN PRODUCTS ONLY
    /// When true, this product is an official Sparkle platform product.
    /// SECURITY: Only products owned by "Sparkle Official" vendor can have this flag set to true.
    /// Regular vendors CANNOT create Sparkle Star products - this is enforced at the database level.
    /// </summary>
    public bool IsAdminProduct { get; set; } = false;
    
    public decimal AverageRating { get; set; } = 4.5m;
    public int TotalReviews { get; set; } = 0;
    public string? Weight { get; set; }
    public string? Dimensions { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }

    public int? SellerId { get; set; }
    public Seller? Seller { get; set; }

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    [NotMapped]
    public string Name => Title;

    [NotMapped]
    public decimal Price => DiscountPercent.HasValue 
        ? BasePrice * (1 - (DiscountPercent.Value / 100m)) 
        : BasePrice;

    [NotMapped]
    public decimal OldPrice => BasePrice;
}

public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public string UserId { get; set; } = default!;
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public string? Sku { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public string Url { get; set; } = default!;
    
    [NotMapped]
    public string ImagePath => Url;

    public int SortOrder { get; set; }
}
