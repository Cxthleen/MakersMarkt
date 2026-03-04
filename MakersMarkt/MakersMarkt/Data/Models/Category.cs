using System.Collections.Generic;

namespace MakersMarkt.Data.Models
{
    class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}