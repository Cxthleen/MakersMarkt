using System.Collections.Generic;
namespace MakersMarkt.Data.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public int? SellerUserId { get; set; }
        public string MaterialUsage { get; set; }
        public string ProductionTime { get; set; }
        public string Complexity { get; set; }
        public string Durability { get; set; }
        public Category Category { get; set; }
        public User Seller { get; set; }
        public ICollection<OrderProduct> OrderProducts { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Moderation> Moderations { get; set; }
        public ICollection<Report> Reports { get; set; }
    }
}