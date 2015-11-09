using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Earthwatchers.Models
{
    public class Rank
    {
        public int OrderRank { get; set; }
        public int EarthwatcherId { get; set; }
        public string Name { get; set; }
        public string Nick { get; set; }
        public int Points { get; set; }
        public DateTime Published { get; set; }
        public string flagUrl { get; set; }
        public int PlayingRegion { get; set; }
    }
}
