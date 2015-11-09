using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Earthwatchers.UI.Models
{
    class MapCountry
    {
        private static string _imageFormat = "/Resources/Images/world-map-{0}.png";

        private string _code;
        private string _description;
        private BitmapImage _mapImage;
        private Color _primary;
        private Color _secondary;

        public MapCountry(string code, string description, Color primary, Color secondary)
        {
            _code = code;
            _description = description;
            _mapImage = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap(string.Format(MapCountry._imageFormat, _code));
            _primary = primary;
            _secondary = secondary;
        }

        public BitmapImage GetMapImage()
        {
            return _mapImage;
        }

        public string GetDescription()
        {
            return _description;
        }

        public string GetCode()
        {
            return _code;
        }

        public Color GetSecondaryColor()
        {
            return _secondary;
        }

        public bool HasColor(Color color)
        {
            return color.Equals(_primary) || color.Equals(_secondary);
        }

        public bool IsOwnCode(string code)
        {
            return _code == code;
        }
    }
}
