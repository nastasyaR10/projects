using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharityFund.Entities
{
    public class Donation
    {
        public int Id { get; set; }
        public int FundraisingId { get; set; }
        public int? CountryId { get; set; }
        public decimal SumDonation { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}