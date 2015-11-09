namespace Earthwatchers.Models
{
    public class Country
	{
        public string CountryCode { get; set; }
        public string Name { get; set; }
        public double TlLat { get; set; }
        public double TlLon { get; set; }
        public double BrLat { get; set; }
        public double BrLon { get; set; }
        public string Polygon { get; set; }
    }
}
