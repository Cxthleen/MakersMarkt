using System;
using System.Collections.Generic;

namespace MakersMarkt.Data.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public int BuyerUserId { get; set; }

        public User Buyer { get; set; }
        public ICollection<OrderProduct> OrderProducts { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}