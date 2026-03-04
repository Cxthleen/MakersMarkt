namespace MakersMarkt.Data.Models
{
    class Moderation
    {
        public int Id { get; set; }
        public string ActionType { get; set; }
        public string Note { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }

        public Product Product { get; set; }
        public User User { get; set; }
    }
}