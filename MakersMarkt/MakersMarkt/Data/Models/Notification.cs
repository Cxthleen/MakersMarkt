using System;

namespace MakersMarkt.Data.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public User User { get; set; }
    }
}