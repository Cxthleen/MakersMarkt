namespace MakersMarkt.Data.Models
{
    class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
        public int ProductId { get; set; }
        public int BuyerUserId { get; set; }

        public Product Product { get; set; }
        public User Buyer { get; set; }
    }
}