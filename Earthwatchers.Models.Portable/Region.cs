using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Earthwatchers.Models
{
    public class Region
    {
        public Region()
        {
        }

        public Region(int Id, string Name, string CountryCode)
        {
            this.Id = Id;
            this.Name = Name;
            this.CountryCode = CountryCode;
        }
        public int Id { get; set; }
        public string Name{ get; set; }
        public string CountryCode { get; set; }
        public Country Country { get; set; }
        public int LowThreshold { get; set; }
        public int HighThreshold { get; set; }
        public int NormalPoints { get; set; }
        public int BonusPoints { get; set; }
        public int PenaltyPoints { get; set; }
    }
}
