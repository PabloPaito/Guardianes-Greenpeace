using System;

namespace Earthwatchers.Models
{
    public class SatelliteImage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Wkt { get; set; }
        public string UrlTileCache { get; set; }
        public DateTime? Published { get; set; }
        public Extent Extent { get; set; }
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }

        public double xmin { get; set; }
        public double ymin { get; set; }
        public double xmax { get; set; }
        public double ymax { get; set; }
        public bool IsCloudy { get; set; }
        public int RegionId { get; set; }
        public bool IsForestLaw { get; set; }
        
        public string DateName
        {
            get
            {
                if (this.Published.HasValue)
                {
                    return string.Format("{0} de {1}", this.Published.Value.ToString("dd"), this.Published.Value.ToString("MMMM"));
                }
                return string.Empty;
            }
        }
    }
}
