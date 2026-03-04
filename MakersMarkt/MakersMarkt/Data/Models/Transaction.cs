using System;

namespace MakersMarkt.Data.Models
{
    class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public DateTime TransactionDate { get; set; }
        public int OrderId { get; set; }

        public Order Order { get; set; }
    }
}