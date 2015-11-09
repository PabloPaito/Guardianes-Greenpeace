using System;

namespace Earthwatchers.Models
{
    public class Basecamp
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double HotPointLat { get; set; }
        public double HotPointLong { get; set; }
        public int Probability { get; set; }
        public string DetailName { get; set; }
        public string ShortText { get; set; }
        public int RegionId { get; set; }
        public bool Show { get; set; }

        public int? IdDb { get; set; }
    }
}
