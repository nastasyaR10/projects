using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharityFund.Entities
{
    public class Disease
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Definition { get; set; }
    }
}
