namespace MakersMarkt.Data.Models
{
    class Notification
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }
        public int UserId { get; set; }

        public User User { get; set; }
    }
}