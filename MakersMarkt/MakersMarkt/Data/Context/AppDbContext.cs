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

            // Seed data
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = BCrypt.Net.BCrypt.HashPassword("Admin!"), Role = "admin", DisplayName = "Administrator", Biography = "Platform administrator." },
                new User { Id = 2, Username = "maker", Password = BCrypt.Net.BCrypt.HashPassword("Maker!"), Role = "seller", DisplayName = "Jan the Maker", Biography = "I craft handmade wooden furniture." },
                new User { Id = 3, Username = "buyer", Password = BCrypt.Net.BCrypt.HashPassword("Buyer!"), Role = "buyer", DisplayName = "Lisa", Biography = "I love unique handmade products." }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Woodwork" },
                new Category { Id = 2, Name = "Textiles" },
                new Category { Id = 3, Name = "Ceramics" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Oak Wall Shelf", Description = "Handcrafted floating wall shelf made from solid European oak with a natural oil finish.", CategoryId = 1, SellerUserId = 2, MaterialUsage = "Solid oak wood, natural oil finish", ProductionTime = "3 days", Complexity = "Medium", Durability = "High" },

                new Product { Id = 2, Name = "Knitted Merino Scarf", Description = "Warm hand-knitted scarf made from soft merino wool, perfect for winter.", CategoryId = 2, SellerUserId = 2, MaterialUsage = "Merino wool", ProductionTime = "2 days", Complexity = "Low", Durability = "Medium" },

                new Product { Id = 3, Name = "Handmade Wool Beanie", Description = "Cozy handmade beanie knitted with premium wool for comfort and warmth.", CategoryId = 2, SellerUserId = 2, MaterialUsage = "Wool yarn", ProductionTime = "2 days", Complexity = "Low", Durability = "Medium" },

                new Product { Id = 4, Name = "Chunky Knit Blanket", Description = "Large chunky knit blanket made with thick yarn, ideal for cozy home decor.", CategoryId = 2, SellerUserId = 2, MaterialUsage = "Chunky wool yarn", ProductionTime = "4 days", Complexity = "Medium", Durability = "High" },

                new Product { Id = 5, Name = "Handmade Wool Mittens", Description = "Warm knitted mittens crafted from soft wool to keep hands comfortable in cold weather.", CategoryId = 2, SellerUserId = 2, MaterialUsage = "Merino wool", ProductionTime = "2 days", Complexity = "Low", Durability = "High" });

            modelBuilder.Entity<Order>().HasData(
                new Order { Id = 1, Status = "completed", OrderDate = new DateTime(2024, 3, 10, 9, 0, 0), BuyerUserId = 3 }
            );

            modelBuilder.Entity<OrderProduct>().HasData(
                new OrderProduct { Id = 1, OrderId = 1, ProductId = 1, Quantity = 1 },
                new OrderProduct { Id = 2, OrderId = 1, ProductId = 2, Quantity = 2 }
            );

            modelBuilder.Entity<Transaction>().HasData(
                new Transaction { Id = 1, Amount = 89.99m, Type = "payment", TransactionDate = new DateTime(2024, 3, 10, 9, 5, 0), OrderId = 1 }
            );

            modelBuilder.Entity<Review>().HasData(
                new Review { Id = 1, Rating = 5, ReviewText = "Beautiful craftsmanship, exactly as described!", ProductId = 1, BuyerUserId = 3 }
            );

            modelBuilder.Entity<Notification>().HasData(
                new Notification { Id = 1, Type = "order_completed", IsRead = false, UserId = 3 },
                new Notification { Id = 2, Type = "new_review", IsRead = false, UserId = 2 }
            );

            modelBuilder.Entity<Flag>().HasData(
                new Flag { Id = 1, Name = "Spam", Pattern = "spam|advertisement|buy now", IsActive = true },
                new Flag { Id = 2, Name = "Inappropriate content", Pattern = "offensive|prohibited", IsActive = true }
            );

            modelBuilder.Entity<Moderation>().HasData(
                new Moderation { Id = 1, ActionType = "warning", Note = "Product contains potential spam indicators.", ProductId = 2, UserId = 1 }
            );

            modelBuilder.Entity<Report>().HasData(
                new Report { Id = 1, Reason = "This product appears to be fake or misleading.", Status = "open", ProductId = 2, UserId = 3, FlagId = 1 }
            );
        }
    }
}