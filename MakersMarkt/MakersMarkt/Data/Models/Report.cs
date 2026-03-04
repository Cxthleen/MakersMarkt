namespace MakersMarkt.Data.Models
{
    class Report
    {
        public int Id { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int FlagId { get; set; }

        public Product Product { get; set; }
        public User User { get; set; }
        public Flag Flag { get; set; }
    }
}