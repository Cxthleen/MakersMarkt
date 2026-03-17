using System;

namespace MakersMarkt.Data.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
        public int ProductId { get; set; }
        public int BuyerUserId { get; set; }

        public Product Product { get; set; }
        public User Buyer { get; set; }

        public string RatingDisplay =>
        new string('★', Math.Clamp(Rating, 0, 5)) +
        new string('☆', 5 - Math.Clamp(Rating, 0, 5));
    }
}