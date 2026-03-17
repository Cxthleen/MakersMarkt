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

        // Computed — not mapped to DB
        public string RatingDisplay => new string('★', Rating) + new string('☆', 5 - Rating);
    }
}