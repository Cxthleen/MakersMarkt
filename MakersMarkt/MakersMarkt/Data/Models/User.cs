using System.Collections.Generic;

namespace MakersMarkt.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string DisplayName { get; set; }
        public string Biography { get; set; }
        public string? ProfileImage { get; set; }


        public ICollection<Product> Products { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<Moderation> Moderations { get; set; }
        public ICollection<Report> Reports { get; set; }
    }
}