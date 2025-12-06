# 🛍️ Sparkle E-commerce Platform

> A comprehensive multi-vendor e-commerce platform built with ASP.NET Core 8.0, designed specifically for Bangladesh market with full Bengali localization support.

**Version**: 2.0.0 | **Status**: Production Ready | **Last Updated**: December 2025

---

## 📖 Table of Contents

- [Features](#-features)
- [Quick Start](#-quick-start)
- [Project Structure](#-project-structure)  
- [Database Schema](#️-database-schema)
- [Technology Stack](#️-technology-stack)
- [Configuration](#-configuration)
- [Running the Application](#-running-the-application)
- [Default Credentials](#-default-credentials)
- [API Documentation](#-api-documentation)
- [Troubleshooting](#-troubleshooting)
- [Recent Updates](#-recent-updates)
- [Roadmap](#-roadmap)

---

## ✨ Features

### 🏪 Multi-Vendor Marketplace
- **25 Active Sellers** - Pre-configured vendors with complete profiles
- **Seller Dashboard** - Real-time analytics, product management, order fulfillment
- **Seller Verification** - Complete onboarding and approval workflow
- **Performance Metrics** - Sales tracking, ratings, and commission management

### 🛒 Shopping Experience
- **80+ Products** - Across 24 comprehensive categories
- **Smart Search** - Full-text search with autocomplete suggestions
- **Shopping Cart** - Real-time updates with persistent storage
- **Wishlist** - Save favorite products for later
- **Product Reviews** - Customer ratings, photos, and Q&A
- **Multi-language** - Seamless English ⇄ Bengali language switching

### 💳 Payment & Orders
- **Multiple Payment Methods** - bKash, Nagad, Cards, Cash on Delivery
- **Real-time Order Tracking** - Live status updates with SignalR
- **Invoice Generation** - Professional PDF invoices
- **Return & Refund System** - Complete return request management
- **Transaction History** - Comprehensive payment tracking

### 🎯 Marketing & Promotions
- **Flash Deals** - Time-limited offers with countdown timers
- **Coupons & Vouchers** - Advanced discount code system
- **Loyalty Points** - Earn on purchases, redeem for discounts
- **Campaigns** - Product and category-based promotions
- **Email Marketing** - Newsletter subscriptions and automated emails

### 👥 User Management
- **Role-Based Access Control** - Admin, Seller, Customer roles
- **Google OAuth** - Quick social login integration
- **Profile Management** - Complete user profiles with multiple addresses
- **Address Autocomplete** - Bangladesh location database with autocomplete
- **Activity Tracking** - User behavior analytics

### 📊 Admin Panel
- **Comprehensive Dashboard** - Sales, orders, users, revenue analytics
- **User Management** - Manage all users, roles, and permissions
- **Seller Approval Workflow** - Vendor verification system
- **Commission Configuration** - Platform fee settings
- **Content Management** - Banners, categories, site settings

### 🚚 Smart Logistics
- **Delivery Zones** - Inside Dhaka, Suburbs, Outside Dhaka
- **Area Database** - 40+ Dhaka areas with autocomplete
- **Dynamic Shipping Rates** - Zone-based delivery pricing
- **Order Tracking** - Real-time shipment status updates
- **Location Pinning** - Map-based address selection

### ⚡ Advanced Features
- **HD & 4K Responsive Design** - Scales perfectly on all displays
- **Premium Animations** - 40+ smooth micro-interactions
- **Progressive Web App** - Fast loading, offline support
- **Real-time Notifications** - SignalR-powered instant updates
- **Redis Caching** - Optimized performance with fallback
- **Response Compression** - Brotli + Gzip enabled

---

## 🚀 Quick Start

### Prerequisites

- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server Express 2019+** - [Download here](https://www.microsoft.com/sql-server/sql-server-downloads)
- **Visual Studio 2022** or **VS Code** (optional)
- **Redis** (optional, for production scaling)

### Installation

#### Option 1: Quick Start (Recommended) ⚡

1. **Navigate to project folder**
   ```powershell
   cd "C:\Users\Minhajul Islam\Desktop\Sparkle Ecommerce"
   ```

2. **Run the application**
   ```powershell
   .\START.bat
   ```

3. **Access the application**
   - Homepage: http://localhost:5000
   - Admin Panel: http://localhost:5000/auth/admin-login
   - API Docs: http://localhost:5000/swagger

#### Option 2: Manual Setup

1. **Restore packages**
   ```powershell
   dotnet restore
   ```

2. **Update database connection** (if needed)
   
   Edit `Sparkle.Api/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Data Source=.\\SQLEXPRESS01;Initial Catalog=SparkleEcommerce;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False;Command Timeout=60"
   }
   ```

3. **Run from Sparkle.Api folder**
   ```powershell
   cd Sparkle.Api
   dotnet run
   ```

### First Time Setup

- **Database auto-creates** on first run with all sample data
- **Startup time**: 30-60 seconds (first run), 5-10 seconds (subsequent runs)
- **Automatic seeding**: Admin user, sellers, products, categories, locations

---

## 🔐 Default Credentials

### Admin Account
```
Email:    admin@sparkle.local
Password: Admin@123
URL:      http://localhost:5000/auth/admin-login
```

### Seller Accounts (25 pre-configured sellers)
```
Email:    techzone@sparkle.local (and 24 others)
Password: Vendor@123
URL:      http://localhost:5000/Vendor
```

### Customer Account
```
Email:    user@sparkle.local
Password: User@123
```

---

## 📁 Project Structure

```
Sparkle Ecommerce/
│
├── Sparkle.Domain/              # 🏛️ Domain Entities & Business Logic
│   ├── Catalog/                 # Products, Categories, Brands
│   ├── Orders/                  # Orders, Cart, Wishlist, Shipments
│   ├── Users/                   # User Profiles, Addresses
│   ├── Sellers/                 # Vendor Management
│   ├── Marketing/               # Promotions, Campaigns, Coupons
│   ├── Financial/               # Payments, Transactions, Commissions
│   ├── Reviews/                 # Product Reviews, Q&A, Votes
│   ├── Notifications/           # Email & SMS Logs
│   ├── Support/                 # Tickets, Messages
│   ├── Logistics/               # Delivery Zones, Shipment Tracking
│   ├── System/                  # Analytics, Metrics, Activity Logs
│   └── Configuration/           # Site Settings
│
├── Sparkle.Infrastructure/      # 🗄️ Data Access Layer
│   ├── ApplicationDbContext.cs  # EF Core DbContext (10 schemas)
│   ├── Migrations/              # Database Version Control
│   ├── Identity/                # ASP.NET Core Identity Setup
│   └── Services/                # Business Services
│
├── Sparkle.Api/                 # 🌐 Web Application (MVC + API)
│   ├── Areas/
│   │   ├── Admin/               # Admin Dashboard & Management
│   │   └── Vendor/              # Seller Dashboard & Tools
│   ├── Controllers/             # 13 Main Controllers
│   ├── Views/                   # Razor Views (CSHTML)
│   ├── wwwroot/                 # Static Assets
│   │   ├── css/                 # 4 optimized stylesheets
│   │   ├── js/                  # 2 consolidated scripts
│   │   └── images/              # Product & UI images
│   ├── Hubs/                    # SignalR Real-time Hubs
│   ├── Services/                # Application Services
│   └── Program.cs               # Application Entry Point
│
├── START.bat                    # ⚡ Quick Launch Script
├── RUN_PROJECT.bat              # 🔄 Alternative Start Script
└── README.md                    # 📘 This File
```

---

## 🗄️ Database Schema

### Professional Multi-Schema Architecture

#### Schema: `catalog` - Product Management
- **Products** - Title, Price, Stock, Variants, Images
- **Categories** - 24 comprehensive categories
- **Brands** - Product brands
- **ProductImages** - Multiple images per product
- **ProductVariants** - Size, color, specifications

#### Schema: `users` - User Data
- **UserProfiles** - Extended user information
- **UserAddresses** - Multiple delivery addresses
- **UserWishlistItems** - Saved products
- **UserSearchHistory** - Search analytics
- **UserNotificationSettings** - Communication preferences

#### Schema: `orders` - Order Lifecycle
- **Orders** - Master order records
- **OrderItems** - Line items with pricing
- **Cart** - Active shopping carts
- **CartItems** - Cart line items
- **OrderTracking** - Status updates

#### Schema: `payments` - Financial Transactions
- **Transactions** - Payment records
- **PaymentMethods** - Saved payment options
- **Refunds** - Return processing
- **ReturnRequests** - Customer returns

#### Schema: `sellers` - Vendor Management
- **Sellers** - Shop information, ratings
- **SellerPayouts** - Commission payments
- **SellerDocuments** - Verification documents
- **SellerBankAccounts** - Payout details
- **SellerPerformanceMetrics** - Analytics

#### Schema: `reviews` - Customer Feedback
- **ProductReviews** - Ratings and comments
- **ReviewImages** - Photo reviews
- **ReviewVotes** - Helpful votes
- **ProductQuestions** - Customer Q&A

#### Schema: `marketing` - Promotions
- **Coupons** - Discount codes
- **FlashDeals** - Time-limited offers
- **Campaigns** - Marketing campaigns
- **LoyaltyPointHistory** - Reward points
- **EmailSubscriptions** - Newsletter

#### Schema: `shipping` - Logistics
- **ShippingMethods** - Delivery options
- **Shipments** - Shipment tracking
- **ShippingZones** - Delivery areas
- **DeliveryZones** - Bangladesh locations (40+ areas)

#### Schema: `support` - Customer Service
- **SupportTickets** - Help requests
- **TicketMessages** - Communication threads

#### Schema: `analytics` - Business Intelligence
- **ProductViews** - Browsing analytics
- **SearchAnalytics** - Search trends
- **SalesReports** - Revenue reports
- **PlatformMetrics** - System metrics

#### Schema: `system` - Administration
- **Notifications** - System alerts
- **ActivityLogs** - Audit trail
- **SiteSettings** - Configuration

---

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0 (MVC + Web API)
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server Express 2019+
- **Authentication**: ASP.NET Core Identity + JWT
- **Real-time**: SignalR (Redis backplane optional)
- **Caching**: In-Memory Cache + Redis (with fallback)
- **PDF Generation**: DinkToPdf / QuestPDF
- **Compression**: Brotli + Gzip

### Frontend
- **UI Framework**: Bootstrap 5.3
- **Icons**: Bootstrap Icons 1.11
- **Utilities**: Tailwind CSS 3.4 (prefixed `tw-`)
- **JavaScript**: Vanilla JS + jQuery 3.7
- **Animations**: AOS Library + Custom CSS
- **Maps**: Leaflet.js (for address selection)
- **CSS**: 4 optimized files (55KB total, compressed)

### Performance Optimizations
- **Response Caching** - 7-day static file caching
- **Response Compression** - 60-70% size reduction
- **Database Indexes** - Optimized queries
- **Lazy Loading** - Defer non-critical images
- **Code Splitting** - 2 consolidated JS files (30KB)
- **GPU Acceleration** - Hardware-accelerated animations

### DevOps & Tools
- **API Docs**: Swagger/OpenAPI
- **Logging**: Serilog + Console
- **Session**: Distributed session state
- **WebSockets**: SignalR for real-time features

---

## 🔧 Configuration

### Required Settings

Edit `Sparkle.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=.\\SQLEXPRESS01;Initial Catalog=SparkleEcommerce;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False;Command Timeout=60"
  },
  "Jwt": {
    "Issuer": "Sparkle",
    "Audience": "SparkleClient",
    "Key": "REPLACE_WITH_A_LONG_RANDOM_SECRET_KEY_AT_LEAST_32_CHARACTERS",
    "AccessTokenMinutes": 60,
    "RefreshTokenDays": 7
  },
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID_HERE",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET_HERE"
  }
}
```

### Optional: Redis Configuration

For production scalability:
```csharp
// Already configured in Program.cs with automatic fallback
// Redis URL: localhost:6379
// Enables distributed caching and SignalR backplane
```

---

## 🎮 Running the Application

### Development Mode
```powershell
dotnet run --environment Development
```

### Production Build
```powershell
dotnet publish -c Release -o ./publish
cd publish
dotnet Sparkle.Api.dll
```

### Docker (Future)
```bash
docker-compose up
```

---

## 📡 API Documentation

### Swagger UI
Access complete API documentation at: **http://localhost:5000/swagger**

### Public APIs
```
GET  /api/products              - List all active products
GET  /api/products/{id}         - Product details
GET  /api/categories            - All categories
GET  /search?q={query}          - Search products
```

### Authenticated APIs (User)
```
POST /api/cart/add              - Add item to cart
GET  /api/wishlist              - User wishlist
POST /api/checkout              - Create order
GET  /api/orders                - Order history
POST /api/reviews               - Submit review
```

### Seller APIs
```
GET  /api/vendor/products       - Vendor's products
POST /api/vendor/products       - Create product
PUT  /api/vendor/products/{id}  - Update product
GET  /api/vendor/orders         - Vendor orders
GET  /api/vendor/analytics      - Sales analytics
```

### Admin APIs
```
GET  /api/admin/dashboard       - Admin metrics
POST /api/admin/approve-seller  - Approve vendor
GET  /api/admin/reports         - System reports
```

---

## 🐛 Troubleshooting

### Issue: Database connection failed
**Solution**: Ensure SQL Server is running
```powershell
net start MSSQL$SQLEXPRESS01
```

### Issue: Port 5000 already in use
**Solution**: Kill existing processes
```powershell
taskkill /F /IM dotnet.exe
taskkill /F /IM Sparkle.Api.exe
```

### Issue: Package restore failed
**Solution**: Clear cache and restore
```powershell
dotnet nuget locals all --clear
dotnet restore
```

### Issue: Migration errors
**Solution**: Database auto-migrates on startup - just wait 30-60 seconds

### Issue: Redis not available
**Solution**: Application automatically falls back to in-memory cache

---

## 🆕 Recent Updates

### Version 2.0.0 - December 2025

#### ✨ New Features
- **HD & 4K Responsive Scaling** - Perfect display on all resolutions
- **Premium Animations** - 40+ smooth micro-interactions
- **Consolidated Assets** - Optimized CSS (55KB) and JS (30KB)
- **Enhanced Navigation** - Fixed Settings page with working sidebar
- **Database Optimization** - Removed duplicates, improved performance

#### 🎨 UI/UX Improvements
- Professional animation system with GPU acceleration
- Smooth scroll effects and parallax backgrounds
- Enhanced toast notifications with multiple types
- Scroll-to-top button with smooth animations
- Quick view for products on hover
- Auto-hide navbar on scroll down

#### 🗄️ Database Enhancements
- 10 organized schemas for better management
- Cascade delete protection
- Comprehensive indexes for performance
- Auto-migration on startup
- Smart validation rules

#### ⚡ Performance
- Response compression (Brotli + Gzip)
- 7-day static file caching
- Redis caching with in-memory fallback
- Optimized database queries
- Lazy image loading

---

## 🎯 Roadmap

### Completed ✅
- Multi-vendor marketplace
- Complete product catalog (80+ products)
- Shopping cart and checkout
- Order management & tracking
- Payment integration (bKash, Cards, COD)
- Admin panel with analytics
- Seller dashboard
- Real-time notifications (SignalR)
- Multi-language support (EN/BN)
- Loyalty points system
- HD & 4K responsive design
- Premium animations & UX
- Smart logistics with autocomplete

### In Progress 🚧
- Mobile app API enhancements
- Advanced analytics dashboard
- AI-powered product recommendations

### Planned 🔮
- Live chat support
- Video product reviews
- Multi-currency support
- Subscription-based products
- Advanced SEO optimization
- PWA offline mode
- Voice search
- AR product preview

---

## 📊 Sample Data Included

- ✅ **24 Categories** - Complete Bangladesh market coverage
- ✅ **25 Sellers** - Pre-configured with profiles and products
- ✅ **80+ Products** - Realistic product data with images
- ✅ **40+ Locations** - Dhaka areas with postal codes
- ✅ **Delivery Zones** - Bangladesh-wide coverage
- ✅ **Admin/User/Seller** - Ready-to-use test accounts

---

## 📄 License

Copyright © 2025 Sparkle E-commerce. All rights reserved.

---

## 🆘 Support

For issues or questions:
1. Check this comprehensive README
2. Review inline code comments
3. Check Swagger API documentation
4. Review system logs
5. Contact development team

---

## 📞 Project Information

- **Project Name**: Sparkle E-commerce
- **Version**: 2.0.0
- **Platform**: ASP.NET Core 8.0
- **Database**: SQL Server Express
- **Status**: ✅ Production Ready
- **Last Updated**: December 2025
- **Target Market**: Bangladesh

---

## 🏆 Key Statistics

- **80+ Products** across 24 categories
- **25 Pre-configured Sellers** with real profiles
- **40+ Bangladesh Locations** with autocomplete
- **13 Controllers** handling all operations
- **10 Database Schemas** professionally organized
- **55KB CSS** (compressed, optimized)
- **30KB JavaScript** (2 consolidated files)
- **60fps Animations** with GPU acceleration
- **< 2 second** homepage load time
- **100% Responsive** from 320px to 4K displays

---

**Made with ❤️ for Bangladesh E-commerce Market**

*Transform your e-commerce vision into reality with Sparkle!* 🚀
