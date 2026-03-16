using System.Collections.Generic;

namespace MakersMarkt.Data.Models
{
    public class Flag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Pattern { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Report> Reports { get; set; }
    }
}