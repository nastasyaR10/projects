using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharityFund.Entities
{
    public class Fundraising
    {
        public int Id { get; set; }
        public int DiseaseId { get; set; }
        public int ChildId { get; set; }
        public decimal Sum { get; set; }
        public DateTime DateBegin { get; set; }
        public bool FlagEnd { get; set; }
    }
}