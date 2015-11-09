using Earthwatchers.Models;
using Earthwatchers.UI.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Earthwatchers.UI.Models;
using Earthwatchers.UI.Resources;
using System.Collections.ObjectModel;

namespace Earthwatchers.UI.GUI.Controls
{
    public partial class CountrySelector
    {
        #region Fields
        private bool _debugModeOn = false;
        private BitmapImage _worldMapImage;
        private string _defaultText;
        private MapCountry _selectedCountry;
        private Region _selectedRegion;
        private List<MapCountry> _mapCoutries;
        private List<Region> _allRegionsForSelectedCountry;
        private int? _selectedRegionId = null;

        private EarthwatcherRequests earthwatcherRequests;
        private RegionRequests regionRequests;
        private LandRequests landRequests;
        private ScoreRequests scoreRequests;
        #endregion
            

        #region Constructors
        public CountrySelector(string currentPlayingCountry, int currentPlayingRegion)
        {
            
            InitializeComponent();

            _worldMapImage = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/world-map.png");
            _defaultText = Labels.SelectCountryFromMap;
            _selectedCountry = null;

            _mapCoutries = new List<MapCountry>();
            _mapCoutries.Add(new MapCountry("AR", Labels.ARGENTINA, Color.FromArgb(255, 0, 73, 126), Color.FromArgb(255, 0, 147, 254)));
            _mapCoutries.Add(new MapCountry("CA", Labels.CANADA, Color.FromArgb(255, 255, 216, 0), Color.FromArgb(255, 255, 246, 0)));
            _mapCoutries.Add(new MapCountry("CN", Labels.CHINA, Color.FromArgb(255, 127, 0, 0), Color.FromArgb(255, 224, 0, 0)));


            var currentCountry = GetCountryByCode(currentPlayingCountry);

            if (currentCountry == null)
            {
                this.mapImage.Source = _worldMapImage;
                this.txtSelectedCountry.Text = _defaultText;
            }
            else
            {
                this.mapImage.Source = currentCountry.GetMapImage();
                this.txtSelectedCountry.Text = currentCountry.GetDescription();
            }

            this.nextButton.IsEnabled = false;
            
            earthwatcherRequests = new EarthwatcherRequests(Constants.BaseApiUrl);
            landRequests = new LandRequests(Constants.BaseApiUrl);
            scoreRequests = new ScoreRequests(Constants.BaseApiUrl);
            regionRequests = new RegionRequests(Constants.BaseApiUrl);
            earthwatcherRequests.PlayingRegionChanged += earthwatcherRequests_PlayingRegionChanged;
            earthwatcherRequests.LandReassignedByPlayingRegion += earthwatcherRequest_LandReassignedByPlayingRegion;
            landRequests.LandsReceived += landRequests_LandsReceived;
            regionRequests.RegionsReceived += regionRequests_RegionsReceived;
        }
        #endregion


        #region Event Handlers
        void earthwatcherRequests_PlayingRegionChanged(object sender, EventArgs e)
        {
            earthwatcherRequests.ReassignLandByPlayingRegion(Current.Instance.Earthwatcher.Id, Current.Instance.Earthwatcher.PlayingRegion);
        }

        void earthwatcherRequest_LandReassignedByPlayingRegion(object sender, EventArgs e)
        {
            Land land = sender as Land;
            try
            {
                if (land != null)
                {                    
                    //Cambios en memoria de lands
                    var oldLand = Current.Instance.Lands.Where(x => x.Id == Current.Instance.Earthwatcher.Lands.FirstOrDefault().Id).FirstOrDefault();
                    if (oldLand != null)
                    {
                        //Si no está en amarillo o en verde la saco 
                        if (oldLand.LandStatus != LandStatus.Alert && oldLand.LandStatus != LandStatus.Ok)
                        {
                            Current.Instance.Lands.Remove(oldLand);
                            Current.Instance.LandInView.Remove(oldLand);
                        }
                        else
                        {   //Si es amarilla o verde la paso a greenpeace 
                            oldLand.EarthwatcherId = Configuration.GreenpeaceId;
                            oldLand.EarthwatcherName = Configuration.GreenpeaceName;
                            oldLand.IsPowerUser = true;
                        }
                        //Saco del current instance del Ew la land que quiero cambiar
                        var oldEarthwatcherLand = Current.Instance.Earthwatcher.Lands.Where(x => x.Id == oldLand.Id).FirstOrDefault();
                        Current.Instance.Earthwatcher.Lands.Remove(oldEarthwatcherLand);
                    }
                    //si la land que me asignaron no esta en el current instance de lands, la agrega y se la agrega al Ew
                    if (!Current.Instance.Lands.Any(x => x.Id == land.Id))
                    {
                        Current.Instance.Lands.Add(land);
                        Current.Instance.Earthwatcher.Lands.Add(land);
                    }

                    //Actualizo en memoria la nueva region
                    Current.Instance.Region = _selectedRegion;

                    var hexagonLayer = Current.Instance.LayerHelper.FindLayer(Constants.Hexagonlayername) as Earthwatchers.UI.Layers.HexagonLayer;

                    if (hexagonLayer != null)
                        hexagonLayer.AddHexagon(land);

                    if (land.IsTutorLand) //Is Tutor Land - All region is complete
                    {
                        //Muestro el mensaje, Deshabilito mi parcela
                        NotificationsWindow notificationsWindow = new NotificationsWindow(ActionPoints.Action.RegionCompleted.ToString());
                        notificationsWindow.Show();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                //Traigo nuevamente las lands para el pais que estoy jugando
                landRequests.GetAll(Current.Instance.Earthwatcher.Id, Current.Instance.Earthwatcher.PlayingRegion);

                Earthwatcher earthwatcher = Current.Instance.Earthwatcher;

                if (land == null)
                {
                    MessageBox.Show(Labels.NoMoreLands);
                    ChangeCompleted(this, EventArgs.Empty);
                }
                else
                {
                    //Logueo el cambio de pais de juego
                    MapHelper.ZoomToHexagon(Current.Instance.MapControl, land.GeohexKey);
                    Current.Instance.AddScore.Add(new Score(earthwatcher.Id, ActionPoints.Action.PlayingRegionChanged.ToString(), ActionPoints.Points(ActionPoints.Action.PlayingRegionChanged), earthwatcher.PlayingRegion, Current.Instance.PrecisionScore, earthwatcher.Lands.FirstOrDefault().Id));
                    // Recibe el evento en mainpage y le asigna el puntaje
                    ChangeCompleted(this, EventArgs.Empty);
                }
            }
        }

        void landRequests_LandsReceived(object sender, EventArgs e)
        {
            //Actualizo las lands de memoria
            List<Land> lands = sender as List<Land>;
            if (lands != null)
            {
                Current.Instance.Lands = lands;
            }

            this.Close();
        }

        private void ChangePlayingCountry(object sender, EventArgs e) 
        {
            if (_selectedCountry != null)
            {
                Earthwatcher earthwatcher = Current.Instance.Earthwatcher;

                if (_selectedRegion != null && _selectedRegion.Id != 0)
                {
                    //TODO: GI REVISAR QUE PUEDA HACER UN CAMBIO DE PARCELA/PAIS EN X TIEMPO, sino se cambiarian siempre
                    if (earthwatcher.PlayingRegion != _selectedRegion.Id)
                    {
                        this.Visibility = System.Windows.Visibility.Collapsed;
                        ChangeStarted(this, EventArgs.Empty);
                        earthwatcher.PlayingCountry = _selectedRegion.CountryCode;
                        earthwatcher.PlayingRegion = _selectedRegion.Id;
                        earthwatcherRequests.ChangePlayingRegion(earthwatcher);
                        this.errorMsj.Text = "";
                        this.errorMsj.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        //Avisarle que ya esta jugando en ese pais/region!
                        this.errorMsj.Text = Labels.ChangeCountryError;
                        this.errorMsj.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    //Avisarle que tiene que seleccionar una REGION de ese pais
                    this.errorMsj.Text = Labels.ChangeCountryError2;
                    this.errorMsj.Visibility = Visibility.Visible;
                }
            }
        }

        private void mapImage_MouseMove(object sender, MouseEventArgs e)
        {
            Color color = GetColorFromImage(e);
            MapCountry mapCountry = GetCountryByColor(color);

            if (mapCountry != null)
            {
                this.mapImage.Cursor = Cursors.Hand;
                this.mapImage.Source = mapCountry.GetMapImage();
                this.txtSelectedCountry.Text = mapCountry.GetDescription();
                this.refColor.Fill = new SolidColorBrush(mapCountry.GetSecondaryColor());
            }
            else
            {
                this.mapImage.Cursor = Cursors.Arrow;

                if (_selectedCountry != null)
                {
                    this.mapImage.Source = _selectedCountry.GetMapImage();
                    this.txtSelectedCountry.Text = _selectedCountry.GetDescription();
                    this.refColor.Fill = new SolidColorBrush(_selectedCountry.GetSecondaryColor());
                }
                else
                {
                    this.mapImage.Source = _worldMapImage;
                    this.txtSelectedCountry.Text = _defaultText;
                }
            }
        }

        private void mapImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Color color = GetColorFromImage(e);
                MapCountry mapCountry = GetCountryByColor(color);

                if (mapCountry != null)
                {
                    _selectedCountry = mapCountry;

                    this.nextButton.IsEnabled = true;
                    this.txtSelectedCountry.Text = mapCountry.GetDescription();
                    this.mapImage.Source = mapCountry.GetMapImage();
                    this.refColor.Visibility = System.Windows.Visibility.Visible;
                    this.refColor.Fill = new SolidColorBrush(mapCountry.GetSecondaryColor());
                }
                if (_selectedCountry != null)
                {
                    if(_selectedCountry.IsOwnCode("AR")) //HACK, Como siempre es argentina, hardcodeo las regiones y sus Ids 
                    {
                        _allRegionsForSelectedCountry = new List<Region>();
                            //Inicializacion de variables
                            var selectRegionOption = new Region(0, Labels.SelectRegion, "AR");
                            var saltaRegionOption = new Region(1, "Salta", "AR");
                            var chacoRegionOption = new Region(2, "Chaco", "AR");
                            _allRegionsForSelectedCountry.Add(selectRegionOption);
                            _allRegionsForSelectedCountry.Add(saltaRegionOption);
                            _allRegionsForSelectedCountry.Add(chacoRegionOption);

                            this.regionsCombo.ItemsSource = _allRegionsForSelectedCountry;
                            this.regionsCombo.SelectedValue = selectRegionOption.Id;
                            this.Regions.Visibility = Visibility.Visible;
                            this.nextButton.IsEnabled = true;
                    }
                    else
                    {
                        regionRequests.GetRegionsByCountryCode(_selectedCountry.GetCode());
                        this.nextButton.IsEnabled = false;

                    }

                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        void regionRequests_RegionsReceived(object sender, EventArgs e)
        {
            _allRegionsForSelectedCountry = sender as List<Region>;
            //Recibo todas las regiones para ese pais, asi cargo el combo
            if (_allRegionsForSelectedCountry.Count > 1)
            {
                //Inicializacion de variables
                var selectRegionOption = new Region(0, Labels.SelectRegion, _allRegionsForSelectedCountry.First().CountryCode);
                _allRegionsForSelectedCountry.Add(selectRegionOption);
                _selectedRegion = null;

                this.regionsCombo.ItemsSource = _allRegionsForSelectedCountry;
                this.regionsCombo.SelectedValue = selectRegionOption.Id;
                this.Regions.Visibility = Visibility.Visible;
                this.nextButton.IsEnabled = true;
            }
            else
            {
                this.Regions.Visibility = Visibility.Collapsed;
                _selectedRegion = _allRegionsForSelectedCountry.FirstOrDefault();
                this.nextButton.IsEnabled = true;
            }
        }
        private void regionsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var c = sender as ComboBox;
            //Si es la primera vez que inicializa, le cargo el valor default (seleccionar region)
            if (Convert.ToInt32(c.SelectedValue) == null)
            {
                _selectedRegionId = 0;
            }
            else
            {
                _selectedRegionId = Convert.ToInt32(c.SelectedValue);
            }

            _selectedRegion = _allRegionsForSelectedCountry.Where(r => r.Id == _selectedRegionId).FirstOrDefault();
        }

        #endregion


        #region Methods
        private Color GetColorFromImage(MouseEventArgs e)
        {
            Color color = Color.FromArgb(0, 0, 0, 0);

            try
            {
                var mousePos = e.GetPosition(this.mapImage);
                int x = (int)mousePos.X;
                int y = (int)mousePos.Y;

                var bitmap = new WriteableBitmap(this.mapImage, null);
                color = bitmap.GetPixel(x, y);

                if (_debugModeOn)
                {
                    string strColorARGB = string.Join(", ", color.A.ToString(), color.R.ToString(), color.G.ToString(), color.B.ToString());
                    txtSelectedCountry.Text = string.Format("ARGB: {0}", strColorARGB);
                }
            }
            catch { }

            return color;
        }

        private MapCountry GetCountryByColor(Color color)
        {
            foreach (var mapCountry in _mapCoutries)
            {
                if (mapCountry.HasColor(color))
                    return mapCountry;
            }

            return null;
        }

        private MapCountry GetCountryByCode(string code)
        {
            foreach (var mapCountry in _mapCoutries)
            {
                if (mapCountry.IsOwnCode(code))
                    return mapCountry;
            }

            return null;
        }
        
        #endregion


        #region Events
        public event OnChangeCompleted ChangeCompleted;
        public event OnChangeStarted ChangeStarted;
        public delegate void OnChangeCompleted(object sender, EventArgs e);
        public delegate void OnChangeStarted(object sender, EventArgs e);
        #endregion

       
    }



}