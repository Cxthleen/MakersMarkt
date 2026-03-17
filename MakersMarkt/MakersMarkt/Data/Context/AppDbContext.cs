using MakersMarkt.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;

namespace MakersMarkt.Data.Context
{
    class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Moderation> Moderations { get; set; }
        public DbSet<Flag> Flags { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                ConfigurationManager.ConnectionStrings["MakersMarktDataBase"].ConnectionString,
                ServerVersion.Parse("10.4.17-mariadb")
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique index on Username
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Unique composite index on OrderProducts
            modelBuilder.Entity<OrderProduct>()
                .HasIndex(op => new { op.OrderId, op.ProductId })
                .IsUnique();

            // Relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Seller)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.SellerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Buyer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.BuyerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Order)
                .WithMany(o => o.OrderProducts)
                .HasForeignKey(op => op.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Product)
                .WithMany(p => p.OrderProducts)
                .HasForeignKey(op => op.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Order)
                .WithMany(o => o.Transactions)
                .HasForeignKey(t => t.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Buyer)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.BuyerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Moderation>()
                .HasOne(m => m.User)
                .WithMany(u => u.Moderations)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Moderation>()
                .HasOne(m => m.Product)
                .WithMany(p => p.Moderations)
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reports)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reports)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Flag)
                .WithMany(f => f.Reports)
                .HasForeignKey(r => r.FlagId)
                .OnDelete(DeleteBehavior.Restrict);

            // ─── SEED DATA ───────────────────────────────────────────────

            // Users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = BCrypt.Net.BCrypt.HashPassword("Admin!"), Role = "admin", DisplayName = "Administrator", Biography = "Platform administrator." },
                new User { Id = 2, Username = "maker", Password = BCrypt.Net.BCrypt.HashPassword("Maker!"), Role = "seller", DisplayName = "Jan the Maker", Biography = "I craft handmade wooden furniture." },
                new User { Id = 3, Username = "buyer", Password = BCrypt.Net.BCrypt.HashPassword("Buyer!"), Role = "buyer", DisplayName = "Lisa", Biography = "I love unique handmade products." },
                new User { Id = 4, Username = "sofie", Password = BCrypt.Net.BCrypt.HashPassword("Sofie!"), Role = "seller", DisplayName = "Sofie Ceramics", Biography = "Ceramic artist specialising in functional pottery." },
                new User { Id = 5, Username = "thomas", Password = BCrypt.Net.BCrypt.HashPassword("Thomas!"), Role = "seller", DisplayName = "Thomas Textiles", Biography = "Passionate about handwoven fabrics and natural dyes." },
                new User { Id = 6, Username = "emma", Password = BCrypt.Net.BCrypt.HashPassword("Emma!"), Role = "buyer", DisplayName = "Emma", Biography = "Always on the lookout for unique home decor." },
                new User { Id = 7, Username = "luca", Password = BCrypt.Net.BCrypt.HashPassword("Luca!"), Role = "buyer", DisplayName = "Luca", Biography = "Gift buyer and craft enthusiast." }
            );

            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Woodwork" },
                new Category { Id = 2, Name = "Textiles" },
                new Category { Id = 3, Name = "Ceramics" }
            );

            // Products
            modelBuilder.Entity<Product>().HasData(
                // Jan – Woodwork (seller 2)
                new Product { Id = 1, Name = "Oak Wall Shelf", Description = "Handcrafted floating wall shelf made from solid European oak with a natural oil finish.", CategoryId = 1, SellerUserId = 2, MaterialUsage = "Solid oak, natural oil", ProductionTime = "3 days", Complexity = "Medium", Durability = "High" },
                new Product { Id = 2, Name = "Walnut Serving Board", Description = "Elegant serving board made from rich walnut wood, ideal for cheese and charcuterie.", CategoryId = 1, SellerUserId = 2, MaterialUsage = "Walnut wood", ProductionTime = "2 days", Complexity = "Low", Durability = "High" },
                new Product { Id = 3, Name = "Pine Bookshelf", Description = "Sturdy three-shelf pine bookshelf with a smooth sanded finish, perfect for any room.", CategoryId = 1, SellerUserId = 2, MaterialUsage = "Pine wood, beeswax", ProductionTime = "5 days", Complexity = "High", Durability = "High" },
                new Product { Id = 4, Name = "Oak Side Table", Description = "Minimalist side table crafted from solid oak with tapered legs and a smooth top.", CategoryId = 1, SellerUserId = 2, MaterialUsage = "Solid oak, linseed oil", ProductionTime = "4 days", Complexity = "High", Durability = "High" },
                new Product { Id = 5, Name = "Wooden Candle Holder", Description = "Set of three turned wooden candle holders in varying heights, made from ash wood.", CategoryId = 1, SellerUserId = 2, MaterialUsage = "Ash wood", ProductionTime = "1 day", Complexity = "Low", Durability = "Medium" },

                // Thomas – Textiles (seller 5)
                new Product { Id = 6, Name = "Knitted Merino Scarf", Description = "Warm hand-knitted scarf made from soft merino wool, perfect for winter.", CategoryId = 2, SellerUserId = 5, MaterialUsage = "Merino wool", ProductionTime = "2 days", Complexity = "Low", Durability = "Medium" },
                new Product { Id = 7, Name = "Handmade Wool Beanie", Description = "Cozy handmade beanie knitted with premium wool for comfort and warmth.", CategoryId = 2, SellerUserId = 5, MaterialUsage = "Wool yarn", ProductionTime = "2 days", Complexity = "Low", Durability = "Medium" },
                new Product { Id = 8, Name = "Chunky Knit Blanket", Description = "Large chunky knit blanket made with thick yarn, ideal for cozy home decor.", CategoryId = 2, SellerUserId = 5, MaterialUsage = "Chunky wool yarn", ProductionTime = "4 days", Complexity = "Medium", Durability = "High" },
                new Product { Id = 9, Name = "Handwoven Table Runner", Description = "Naturally dyed handwoven table runner using traditional loom techniques.", CategoryId = 2, SellerUserId = 5, MaterialUsage = "Cotton, natural dyes", ProductionTime = "3 days", Complexity = "Medium", Durability = "High" },
                new Product { Id = 10, Name = "Linen Tote Bag", Description = "Sturdy hand-stitched tote bag made from natural linen, great for daily use.", CategoryId = 2, SellerUserId = 5, MaterialUsage = "Natural linen", ProductionTime = "1 day", Complexity = "Low", Durability = "High" },

                // Sofie – Ceramics (seller 4)
                new Product { Id = 11, Name = "Stoneware Mug", Description = "Handthrown stoneware mug with a speckled glaze and a comfortable handle, holds 300ml.", CategoryId = 3, SellerUserId = 4, MaterialUsage = "Stoneware clay, food-safe glaze", ProductionTime = "4 days", Complexity = "Medium", Durability = "High" },
                new Product { Id = 12, Name = "Ceramic Vase", Description = "Elegant hand-built ceramic vase with a matte finish, perfect for dried or fresh flowers.", CategoryId = 3, SellerUserId = 4, MaterialUsage = "Earthenware clay, matte glaze", ProductionTime = "5 days", Complexity = "High", Durability = "Medium" },
                new Product { Id = 13, Name = "Pottery Bowl Set", Description = "Set of four matching handthrown pottery bowls, dishwasher safe with a reactive glaze.", CategoryId = 3, SellerUserId = 4, MaterialUsage = "Stoneware, reactive glaze", ProductionTime = "6 days", Complexity = "High", Durability = "High" },
                new Product { Id = 14, Name = "Ceramic Plant Pot", Description = "Small handmade ceramic plant pot with drainage hole and matching saucer.", CategoryId = 3, SellerUserId = 4, MaterialUsage = "Terracotta clay, clear glaze", ProductionTime = "3 days", Complexity = "Low", Durability = "Medium" },
                new Product { Id = 15, Name = "Espresso Cup Set", Description = "Set of two matching handthrown espresso cups with saucers, holds 80ml each.", CategoryId = 3, SellerUserId = 4, MaterialUsage = "Porcelain, food-safe glaze", ProductionTime = "4 days", Complexity = "Medium", Durability = "High" }
            );

            // Orders
            modelBuilder.Entity<Order>().HasData(
                new Order { Id = 1, Status = "completed", OrderDate = new DateTime(2024, 3, 10, 9, 0, 0), BuyerUserId = 3 },
                new Order { Id = 2, Status = "completed", OrderDate = new DateTime(2024, 4, 5, 11, 0, 0), BuyerUserId = 6 },
                new Order { Id = 3, Status = "pending", OrderDate = new DateTime(2024, 5, 1, 14, 0, 0), BuyerUserId = 7 },
                new Order { Id = 4, Status = "completed", OrderDate = new DateTime(2024, 5, 18, 10, 0, 0), BuyerUserId = 3 },
                new Order { Id = 5, Status = "pending", OrderDate = new DateTime(2024, 6, 2, 16, 0, 0), BuyerUserId = 6 }
            );

            // OrderProducts
            modelBuilder.Entity<OrderProduct>().HasData(
                new OrderProduct { Id = 1, OrderId = 1, ProductId = 1, Quantity = 1 },
                new OrderProduct { Id = 2, OrderId = 1, ProductId = 6, Quantity = 2 },
                new OrderProduct { Id = 3, OrderId = 2, ProductId = 11, Quantity = 2 },
                new OrderProduct { Id = 4, OrderId = 2, ProductId = 14, Quantity = 1 },
                new OrderProduct { Id = 5, OrderId = 3, ProductId = 3, Quantity = 1 },
                new OrderProduct { Id = 6, OrderId = 4, ProductId = 12, Quantity = 1 },
                new OrderProduct { Id = 7, OrderId = 4, ProductId = 15, Quantity = 2 },
                new OrderProduct { Id = 8, OrderId = 5, ProductId = 8, Quantity = 1 },
                new OrderProduct { Id = 9, OrderId = 5, ProductId = 9, Quantity = 1 }
            );

            // Transactions
            modelBuilder.Entity<Transaction>().HasData(
                new Transaction { Id = 1, Amount = 89.99m, Type = "payment", TransactionDate = new DateTime(2024, 3, 10, 9, 5, 0), OrderId = 1 },
                new Transaction { Id = 2, Amount = 124.50m, Type = "payment", TransactionDate = new DateTime(2024, 4, 5, 11, 5, 0), OrderId = 2 },
                new Transaction { Id = 3, Amount = 210.00m, Type = "payment", TransactionDate = new DateTime(2024, 5, 18, 10, 5, 0), OrderId = 4 }
            );

            // Reviews
            modelBuilder.Entity<Review>().HasData(
                new Review { Id = 1, Rating = 5, ReviewText = "Beautiful craftsmanship, exactly as described!", ProductId = 1, BuyerUserId = 3 },
                new Review { Id = 2, Rating = 4, ReviewText = "Really warm and well made, great quality merino wool.", ProductId = 6, BuyerUserId = 3 },
                new Review { Id = 3, Rating = 5, ReviewText = "The mugs are stunning, perfect weight and the glaze is gorgeous.", ProductId = 11, BuyerUserId = 6 },
                new Review { Id = 4, Rating = 4, ReviewText = "Love the plant pot, fits perfectly on my windowsill.", ProductId = 14, BuyerUserId = 6 },
                new Review { Id = 5, Rating = 5, ReviewText = "Absolutely gorgeous vase, even better in person.", ProductId = 12, BuyerUserId = 3 },
                new Review { Id = 6, Rating = 3, ReviewText = "Nice blanket but took longer to arrive than expected.", ProductId = 8, BuyerUserId = 6 },
                new Review { Id = 7, Rating = 5, ReviewText = "The espresso cups are perfect, my morning coffee feels fancy now.", ProductId = 15, BuyerUserId = 3 }
            );

            // Notifications
            modelBuilder.Entity<Notification>().HasData(
                new Notification { Id = 1, Type = "order_completed", IsRead = false, UserId = 3 },
                new Notification { Id = 2, Type = "new_review", IsRead = false, UserId = 2 },
                new Notification { Id = 3, Type = "new_order", IsRead = false, UserId = 5 },
                new Notification { Id = 4, Type = "new_review", IsRead = true, UserId = 4 },
                new Notification { Id = 5, Type = "order_completed", IsRead = false, UserId = 6 },
                new Notification { Id = 6, Type = "new_order", IsRead = false, UserId = 4 }
            );

            // Flags
            modelBuilder.Entity<Flag>().HasData(
                new Flag { Id = 1, Name = "Spam", Pattern = "spam|advertisement|buy now", IsActive = true },
                new Flag { Id = 2, Name = "Inappropriate content", Pattern = "offensive|prohibited", IsActive = true },
                new Flag { Id = 3, Name = "Counterfeit", Pattern = "fake|replica|copy", IsActive = true }
            );

            // Moderations
            modelBuilder.Entity<Moderation>().HasData(
                new Moderation { Id = 1, ActionType = "warning", Note = "Product contains potential spam indicators.", ProductId = 6, UserId = 1 },
                new Moderation { Id = 2, ActionType = "reviewed", Note = "Product reviewed and approved after seller clarified.", ProductId = 7, UserId = 1 },
                new Moderation { Id = 3, ActionType = "warning", Note = "Description flagged for review, possible counterfeit.", ProductId = 10, UserId = 1 }
            );

            // Reports
            modelBuilder.Entity<Report>().HasData(
                new Report { Id = 1, Reason = "This product appears to be fake or misleading.", Status = "open", ProductId = 6, UserId = 3, FlagId = 1 },
                new Report { Id = 2, Reason = "Seller is using copied images from another listing.", Status = "open", ProductId = 10, UserId = 6, FlagId = 3 },
                new Report { Id = 3, Reason = "Product description contains inappropriate language.", Status = "resolved", ProductId = 7, UserId = 7, FlagId = 2 }
            );
        }
    }
}