using System;
using System.Threading;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Earthwatchers.UI.Requests;
using Earthwatchers.Models;
using Mapsui;
//using Mapsui;
using Earthwatchers.UI.Layers;
using Earthwatchers.UI.GUI.Controls;
using System.Windows.Browser;
using System.Collections.Generic;
using Earthwatchers.UI.Resources;
using Earthwatchers.UI.Models;
using System.Globalization;

namespace Earthwatchers.UI
{
    [ScriptableType]
    public partial class MainPage : UserControl
    {
        private LayerHelper layerHelper;
        private ReportWindow reportWindow;
        private Land selectedLand;
        private Point currentMousePos;
        private SignalRClient signalRClient;
        private SharedEventArgs sharedEventArgs;
        private List<String> adresses_list = new List<string>();
        private List<Rank> top10InRanking = null;
        private bool replayTutorial = false;
        private bool isScoreAdding = false;
        private bool shown = false;
        private bool flag = false;
        private bool leftMouseButtonDown;
        private string contestText;
        private string address = " ";
        private string geohexcode;

        private readonly LandRequests landRequest = new LandRequests(Constants.BaseApiUrl);
        private readonly JaguarRequests jaguarRequests = new JaguarRequests(Constants.BaseApiUrl);
        private readonly PopupMessageRequests popupMessageRequests = new PopupMessageRequests(Constants.BaseApiUrl);
        private readonly ContestRequests contestRequests = new ContestRequests(Constants.BaseApiUrl);
        private readonly ScoreRequests scoreRequest = new ScoreRequests(Constants.BaseApiUrl);
        private readonly RegionRequests regionRequest = new RegionRequests(Constants.BaseApiUrl);
        private readonly CustomShareTextRequests customShareTextRequest = new CustomShareTextRequests(Constants.BaseApiUrl);

        public MainPage(Earthwatcher earthwatcher, string _geohexcode)
        {
            InitializeComponent();

            HtmlPage.RegisterScriptableObject("JsFacebookCallback", this);

            geohexcode = _geohexcode;

            //Logo
            this.logo.Source = ResourceHelper.GetBitmap(string.Format("/Resources/Images/{0}", Labels.LogoPath));

            Current.Instance.Earthwatcher = earthwatcher;
            Current.Instance.PrecisionScore = 100;
            Current.Instance.Username = earthwatcher.Name;
            Current.Instance.Password = earthwatcher.Password;
            Current.Instance.IsAuthenticated = true;
            Current.Instance.AddScore = new List<Score>();
            Current.Instance.MapControl = mapControl;
            this.DataContext = earthwatcher;

            //Poner la bandera del pais en juego
            playingCountryFlag.Source = ResourceHelper.GetBitmap(string.Format("/Resources/Images/Flags/{0}-35.png", Current.Instance.Earthwatcher.PlayingRegion));

            if (this.UserFullName.Text.Length > 13)
            {
                UserFullName.Text = Current.Instance.Earthwatcher.FullName.Substring(0, 12) + "...";
            }
            else
                UserFullName.Text = Current.Instance.Earthwatcher.FullName;

            scoreRequest.ScoresReceived += scoreRequest_ScoresReceived;
            scoreRequest.ScoreAdded += scoreRequest_ScoreAdded;
            scoreRequest.ScoreUpdated += scoreRequest_ScoreUpdated;
            scoreRequest.ServerDateTimeReceived += scoreRequest_ServerDateTimeReceived;
            scoreRequest.PositionInRankingReceived += scoreRequest_PositionInRankingReceived;
            regionRequest.RegionReceived += regionRequest_RegionReceived;
            customShareTextRequest.TextsReceived += customShareTextRequest_TextsReceived;


            Current.Instance.MapControl.zoomStarted += MapControl_zoomStarted;
            Current.Instance.MapControl.zoomFinished += MapControl_zoomFinished;

            Loaded += MainPageLoaded;
        }


        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            //Land Events
            landRequest.LandReceived += LandChanged;
            landRequest.LandsReceived += landRequest_LandsReceived;
            landRequest.VerifiedLandCodesReceived += landRequest_VerifiedLandCodesReceived;
            landRequest.CheckPercentageReceived += landRequest_CheckPercentageReceived;
            landRequest.PresicionPercentageReceived += landRequest_PresicionPercentageReceived;

            //Jaguar, DailyMessage, Contest Events
            jaguarRequests.PositionReceived += jaguarRequests_PositionReceived;
            popupMessageRequests.MessageReceived += popupMessageRequests_MessageReceived;
            contestRequests.ContestReceived += contestRequests_ContestReceived;
            
            //Tutorial Events
            StoryBoardTutorialUI0.Completed += StoryBoardTutorialUI0_Completed; //termino de mostrar el mapa y los textos
            StoryBoardLoadingTutorialUI0.Completed += StoryBoardLoadingTutorialUI0_Completed; //termino de mostrar el mapa y el loading para cargarlos
            StoryBoardTutorialUI1.Completed += StoryBoardTutorialUI1_Completed; //termino de presentarle la parcela y dejo el boton [mi parcela] titilando

            //Layers Events
            this.OpacitiesStoryBoard.Completed += OpacitiesStoryBoard_Completed;
            this.layerList.ChangingOpacity += layerList_ChangingOpacity;
            this.layerList.HexLayerVisibilityChanged += layerList_HexLayerVisibilityChanged;
            this.layerList.ArgentineLawLayerLayerVisibilityChanged += layerList_ArgentineLawLayerLayerVisibilityChanged;
            this.layerList.ArgentineLawLoaded += layerList_ArgentineLawLoaded;
            this.layerList.ArgentineLawLoading += layerList_ArgentineLawLoading;
            this.ChangeOpacityStoryBoard.Completed += ChangeOpacityStoryBoard_Completed;

            HtmlPage.RegisterScriptableObject("Credentials", this);

            //Create and set map
            var map = new Map();
            mapControl.Map = map;

            //Initialize the singleton
            Current.Instance.MapControl = mapControl;

            //Pass the LayerCollection used on the map to the helper, this is used to distinguish base, normal, ... layers 
            layerHelper = new LayerHelper(map.Layers);

            //Activate GeoHexCode Search only if admin
            if (Current.Instance.Earthwatcher.Role == Role.Admin)
            {
                this.TestGeoHexTextBox.Visibility = Visibility.Visible;
            }

            Current.Instance.LayerHelper = layerHelper;

            //Load the preset layers
            LayerInitialization.Initialize(layerHelper);
            layerList.SetLayerHelper(layerHelper);

            //Report Window First And Unic Inicialization
            reportWindow = new ReportWindow(new Land(), Current.Instance.Earthwatcher);
            reportWindow.ReportWindowClosed += reportWindow_ReportWindowClosed;
            reportWindow.CollectionCompleted += reportWindow_CollectionCompleted;
            reportWindow.ReportWindowDemand += reportWindow_ReportWindowDemand;
            reportWindow.Shared += reportWindow_Shared;
            reportWindow.ReportWindowConfirmationEnded += reportWindow_ReportWindowConfirmationEnded;
            reportWindow.ReportWindowLandStatusChanged += reportWindow_ReportWindowLandStatusChanged;
            landInfoWrapper.Children.Add(reportWindow);

            //Get PlayingRegion Object
            regionRequest.GetById(Current.Instance.Earthwatcher.PlayingRegion);

            //GetShareTexts for region and language
            customShareTextRequest.GetByRegionIdAndLanguage(Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.Earthwatcher.Language);

            MapControl_zoomFinished(null, null);
            MapControl_zoomStarted(null, null);
            scoreRequest.GetByUser(Current.Instance.Earthwatcher.Id);
        }

        void Start(bool replay)
        {
            Current.Instance.TutorialStarted = false;
            OpacitiesStoryBoard.Begin();

            if (!replay)
            {
                this.StatsControl.Show();

                //SignalR Push Notificacionts
                signalRClient = new SignalRClient(Constants.BaseUrl);
                signalRClient.NotificationReceived += signalRClient_NotificationReceived;
                signalRClient.RunAsync();
                LandChanged(Current.Instance.Earthwatcher.Lands.First());

                //Cargo todos los lands en Background
                if (Current.Instance.Lands == null)
                {
                    this.loadinAnim.Visibility = System.Windows.Visibility.Visible;
                    landRequest.GetAll(Current.Instance.Earthwatcher.Id, Current.Instance.Earthwatcher.PlayingRegion);
                }

                //Cargo el porcentaje de parcelas revisadas
                if(Current.Instance.Earthwatcher.PlayingRegion != 0)
                {
                    landRequest.GetCheckPercentage(Current.Instance.Earthwatcher.PlayingRegion);
                }

                //Cargo el mensaje del día solo si ya vio el tutorial de contexto de ese pais (para que no sea tanto junto)
                if (Current.Instance.RegionScores.Any(x => x.Action.Equals(ActionPoints.Action.ContextTutorialCompleted.ToString()) && (x.RegionId == Current.Instance.Earthwatcher.PlayingRegion)))
                {
                    popupMessageRequests.GetMessage();
                }
                //Veo si tengo que notificar el ganador de un concurso
                contestRequests.GetWinner();

                //Cargo los concursos para mostrar la fecha del resumen diario
                contestRequests.GetContest(Current.Instance.Earthwatcher.PlayingRegion);
                scoreRequest.GetLeaderBoardNationalRanking(Current.Instance.Earthwatcher.Id);

                //Inicio el tutorial de contexto
                if (!Current.Instance.RegionScores.Any(x => x.Action.Equals(ActionPoints.Action.ContextTutorialCompleted.ToString()) && (x.RegionId == Current.Instance.Earthwatcher.PlayingRegion)))
                {
                    ContextTutorial contextTut = new ContextTutorial();
                    contextTut.PointsAdded += contextTut_PointsAdded;
                    contextTut.Closed += contextTut_Closed;
                    contextTut.Show();
                }
            }
        }

        //Left Menu Buttons
        private void UserControlPanelGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UserPanel userPanelWindow = new UserPanel();
            userPanelWindow.Show();
        }
        private void helpButton_click(object sender, MouseButtonEventArgs e)
        {
            var helpWindow = new HelpWindow();
            helpWindow.Closed += helpWindow_Closed;
            helpWindow.PointsAdded += helpWindow_PointsAdded;
            helpWindow.Show();
        }
        private void fullScreenButton_click(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Host.Content.IsFullScreen = !Application.Current.Host.Content.IsFullScreen;
        }
        private void RankingClick(object sender, RoutedEventArgs e)
        {
            var rankingWindow = new Earthwatchers.UI.GUI.Controls.Ranking();
            rankingWindow.Show();
        }
        private void changePlayingCountryButton_click(object sender, MouseButtonEventArgs e)
        {
            ChangeCountryHooverOff(null, null);
            CountrySelector countrySelector = new CountrySelector(Current.Instance.Earthwatcher.PlayingCountry, Current.Instance.Earthwatcher.PlayingRegion);
            countrySelector.ChangeStarted += CountrySelector_ChangeStarting;
            countrySelector.ChangeCompleted += CountrySelector_ChangeCompleted;
            countrySelector.Show();
        }
        private void reshuffleButton_click(object sender, MouseButtonEventArgs e)
        {
            NewPlotHooverOff(null, null);
            NotificationsWindow notificationsWindow = new NotificationsWindow(ActionPoints.Action.LandReassigned.ToString());
            notificationsWindow.LandReassigned += notificationsWindow_LandReassigned;
            notificationsWindow.Show();
        }
        private void myPlotButton_click(object sender, RoutedEventArgs e)
        {
            MyPlotHooverOff(null, null);
            MapHelper.ZoomToHexagon(Current.Instance.MapControl, Current.Instance.Earthwatcher.Lands.First().GeohexKey);
        }
        private void zoomInButton_click(object sender, MouseButtonEventArgs e)
        {
            ZoomInHooverOff(null, null);
            mapControl.ZoomIn();
        }
        private void zoomOutButton_click(object sender, MouseButtonEventArgs e)
        {
            ZoomOutHooverOff(null, null);
            if (Current.Instance.MapControl.Viewport.Resolution < 1200) //Stop zooming out at this resolution
            {
                mapControl.ZoomOut();
            }
        }
       
        //Buttons Hoover
        private void ChangeCountryHooverOn(object sender, RoutedEventArgs e)
        {
            this.ChangeRegion.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/LeftMenuIcons/changeregion2.png");
        }
        private void ChangeCountryHooverOff(object sender, RoutedEventArgs e)
        {
            this.ChangeRegion.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/LeftMenuIcons/changeregion.png");
        }
        private void NewPlotHooverOn(object sender, RoutedEventArgs e)
        {
            this.NewPlot.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/LeftMenuIcons/newplot2.png");
        }
        private void NewPlotHooverOff(object sender, RoutedEventArgs e)
        {
            this.NewPlot.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/LeftMenuIcons/newplot.png");
        }
        private void MyPlotHooverOn(object sender, RoutedEventArgs e)
        {
            this.MyPlot.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/LeftMenuIcons/myplot2.png");
        }
        private void MyPlotHooverOff(object sender, RoutedEventArgs e)
        {
            this.MyPlot.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/LeftMenuIcons/myplot.png");
        }
        private void ZoomOutHooverOn(object sender, RoutedEventArgs e)
        {
            this.ZoomOut.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/LeftMenuIcons/zoomout2.png");
        }
        private void ZoomOutHooverOff(object sender, RoutedEventArgs e)
        {
            this.ZoomOut.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/LeftMenuIcons/zoomout.png");
        }
        private void ZoomInHooverOn(object sender, RoutedEventArgs e)
        {
            this.ZoomIn.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/LeftMenuIcons/zoomin2.png");
        }
        private void ZoomInHooverOff(object sender, RoutedEventArgs e)
        {
            this.ZoomIn.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/LeftMenuIcons/zoomin.png");
        }

        //Hexagons Legend
        private void HexLegend_TogleLegend(object sender, MouseButtonEventArgs e)
        {
            if (this.HexagonsLegend.Opacity > 0 && this.HexagonsLegend.Visibility == Visibility.Visible)
            {
                HideHexLayer.Completed += HideHexLayer_Completed;
                this.HexLegend.Visibility = Visibility.Visible;
                this.HideHexLayer.Begin();
            }
            else
            {
                this.HexagonsLegend.Visibility = Visibility.Visible;
                this.HexLegend.Visibility = Visibility.Collapsed;
                this.ShowHexLayer.Begin();
            }
        }
        void HideHexLayer_Completed(object sender, EventArgs e)
        {
            this.HexagonsLegend.Visibility = Visibility.Collapsed;
        }

    
        //Tutorial UI Handlers y Llamados de los metodos en orden.
        void StoryBoardLoadingTutorialUI0_Completed(object sender, EventArgs e)
        {
            loadinAnim.Visibility = Visibility.Collapsed;
            this.loadingAnimText.Text = Labels.Cargando;
        }
        void StoryBoardTutorialUI0_Completed(object sender, EventArgs e)
        {
            if (!replayTutorial) 
            {
                //Tardo 5 segundos mas ocultando el overlay.
                StoryBoardLoadingTutorialUI0.Begin();
            }

            this.Overlay.Visibility = System.Windows.Visibility.Collapsed;

            CompleteTutorialStep(); //case 0
        }
        private void StartTutorialUI1()
        {
            //Arranco el step 1 y dejo mi parcela titilando (todo esta retrasado 3 segundos para que se vea el globo anterior)
            this.StoryBoardTutorialUI1.Begin();
            this.TutorialMyPlotButton.Visibility = System.Windows.Visibility.Visible; //habilito el boton mi parcela
            this.MiParcelaStoryBoard.Begin();

            mapControl.ZoomIn();
            mapControl.ZoomIn();

        }
        void StoryBoardTutorialUI1_Completed(object sender, EventArgs e)
        {
            this.Overlay.Visibility = System.Windows.Visibility.Collapsed; //oculto el overlay para que pueda hacer click en [mi parcela] que esta titilando
        }
        private void TutorialMyPlotButtonClick(object sender, MouseButtonEventArgs e)
        {
            this.MiParcelaStoryBoard.Stop(); //deja de titilar el boton [mi parcela]
            LandChanged(Current.Instance.Earthwatcher.Lands.FirstOrDefault()); //lo llevo a la parcela
            CompleteTutorialStep(); //case 1
        }
        private void TutorialTimelineContinueClick(object sender, RoutedEventArgs e)
        {
            //Termina Timeline [CONTINUE] Button
            CompleteTutorialStep(); //case 2
        }
        private void TutorialVerifyPlotContinueClick(object sender, RoutedEventArgs e)
        {
            CompleteTutorialStep(); //case 5
        }
        private void TutorialFinishClick(object sender, RoutedEventArgs e)
        {
            CompleteTutorialStep();

            this.Overlay.Visibility = System.Windows.Visibility.Collapsed;

            //habilitar el Move y el Zoom del mapa
            this.mapControl.AllowMoveAndZoom = true;

            //Vuelvo a su parcela
            MapHelper.ZoomToHexagon(Current.Instance.MapControl, Current.Instance.Earthwatcher.Lands.First().GeohexKey);

            //Vuelvo a mostrar el timeline
            this.layerList.Visibility = System.Windows.Visibility.Visible;

            //Termina el tutorial
            Start(replayTutorial);

            //AutomaticShareEvent
            AutomaticShareEvents(ActionPoints.Action.TutorialCompleted.ToString());

            CompleteTutorialStep(); //case 7 TUTORIAL COMPLETED

        }
        void CompleteTutorialStep()
        {
            switch (Current.Instance.TutorialCurrentStep)
            {
                case 0:
                    LogStep(0);
                    //Deshabilitar el Move y el Zoom del mapa
                    this.mapControl.AllowMoveAndZoom = false;
                    this.Overlay.Visibility = System.Windows.Visibility.Visible; //Overlay para no tocar nada
                    StartTutorialUI1(); //hace mas zoom y arranca mi parcelaStoryBoard
                    break;
                case 1:
                    LogStep(1);
                    this.TutorialMyPlotButton.Visibility = System.Windows.Visibility.Collapsed; //inhabilito el click en [mi parcela]
                    this.Overlay2.Visibility = System.Windows.Visibility.Visible;               //vuelvo a tapar la columna 2
                    this.layerList.Visibility = System.Windows.Visibility.Visible;              //habilito el layer del timeline
                    this.layerList.Opacity = 1;                                                 //habilito el layer del timeline
                    this.StoryBoardTutorialUI2.Begin();                                         //arranco los globos del slider
                    this.layerList.StartSliderAnimation();                                      //arranco la animacion del slider y la muestro
                    break;
                case 2:
                    LogStep(2);
                    this.StoryBoardTutorialUI3.Begin(); //arranco los globos y la flecha a mi parcela Click on your plot to report what you saw
                    break;
                case 3:
                    LogStep(3);
                    this.StoryBoardTutorialUI4.Begin(); //arranco el globo nuevo que deja reportar tu parcela
                    break;
                case 4:
                    LogStep(4);
                    this.StoryBoardTutorialUI5.Begin(); //oculto el globo reportar tu parcela y muestro el de reporta la de otros usuarios tmb
                    break;
                case 5:
                    LogStep(5);
                    this.StoryBoardTutorialUI6.Begin(); //oculto el globo de reporta la de otro usuarios y muestro el ultimo
                    break;
                case 6:
                    LogStep(6);
                    this.StoryBoardTutorialUI7.Begin(); //Oculto el ultimo globo y termina el tutorial.
                    break;
                case 7:
                    LogStep(7);
                    break;
            }

            Current.Instance.TutorialCurrentStep++;
        }
        private void LogStep(int step)
        {
            ActionPoints.Action stepAction = ActionPoints.Action.TutorialStep0;

            if (!Current.Instance.AllCountriesScores.Any(x => x.Action == ActionPoints.Action.TutorialCompleted.ToString())) // solo si nunca lo completo
            {
                switch (step)
                {
                    case 0: stepAction = ActionPoints.Action.TutorialStep0; break;
                    case 1: stepAction = ActionPoints.Action.TutorialStep1; break;
                    case 2: stepAction = ActionPoints.Action.TutorialStep2; break;
                    case 3: stepAction = ActionPoints.Action.TutorialStep3; break;
                    case 4: stepAction = ActionPoints.Action.TutorialStep4; break;
                    case 5: stepAction = ActionPoints.Action.TutorialStep5; break;
                    case 6: stepAction = ActionPoints.Action.TutorialStep6; break;
                    case 7: stepAction = ActionPoints.Action.TutorialCompleted; break;
                }

                Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, stepAction.ToString(), ActionPoints.Points(stepAction), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore));
                AddPoints(Current.Instance.AddScore);
            }

        }

        //Tutorial Handlers
        void welcomeWindow_Closed(object sender, EventArgs e)
        {
            //Esta hecho en un metodo aparte para que se pueda llamar al hacer click en tutorial desde el menu de ayuda
            WelcomeWindowClosed();
        }
        void helpWindow_Closed(object sender, EventArgs e)
        {
            var helpWindow = sender as HelpWindow;
            if (helpWindow != null && !string.IsNullOrEmpty(helpWindow.SelectedOption))
            {
                if (helpWindow.SelectedOption.Equals("PreTutorial") || helpWindow.SelectedOption.Equals("PreTutorial1"))
                {
                    WelcomeWindow welcomeWindow = new WelcomeWindow();
                    welcomeWindow.PointsAdded += welcomeWindow_PointsAdded;
                    welcomeWindow.Show();
                }

                if (helpWindow.SelectedOption.Equals("Tutorial") || helpWindow.SelectedOption.Equals("Tutorial1"))
                {
                    Current.Instance.TutorialStarted = true;
                    replayTutorial = true;
                    WelcomeWindowClosed();
                }

                if (helpWindow.SelectedOption.Equals("MiniGame1") || helpWindow.SelectedOption.Equals("MiniGame01"))
                {
                    /*
                    TutorialGameWindow gameWindow = new TutorialGameWindow();
                    gameWindow.Closed += gameWindow_Closed;
                    gameWindow.Show();
                     * */
                    TutorialGame2Window gameWindow = new TutorialGame2Window();
                    gameWindow.Closed += gameWindow_Closed;
                    gameWindow.Show();
                }
            }
        }
        void contextTut_Closed(object sender, EventArgs e)
        {
            //TODO: Evento que se dispara al finalizar el tuto de contexto
        }
        private void contextTut_PointsAdded(object sender, EventArgs e)
        {
            AddPoints(Current.Instance.AddScore);
        }
        private void ctxTut_PointsAdded(object sender, EventArgs e)
        {
            AddPoints(Current.Instance.AddScore);
        }
        void helpWindow_PointsAdded(object sender, EventArgs e)
        {
            AddPoints(Current.Instance.AddScore);
        }
        void welcomeWindow_PointsAdded(object sender, EventArgs e)
        {
            AddPoints(Current.Instance.AddScore);
        }
       
        //Tutorial Methods
        private void StartTutorialsSequence()
        {
            Current.Instance.TutorialStarted = true;

            WelcomeWindow welcomeWindow = new WelcomeWindow();
            welcomeWindow.PointsAdded += welcomeWindow_PointsAdded;
            welcomeWindow.Closed += welcomeWindow_Closed;
            welcomeWindow.Show();
        }
        void WelcomeWindowClosed()
        {
            Current.Instance.TutorialCurrentStep = 0;
            this.layerList.Visibility = System.Windows.Visibility.Collapsed;

            this.Overlay.Visibility = System.Windows.Visibility.Visible;
            this.Overlay1.Visibility = System.Windows.Visibility.Visible;
            this.Overlay2.Visibility = System.Windows.Visibility.Visible;
            this.Overlay2_1.Visibility = System.Windows.Visibility.Visible;
            this.Overlay2_2.Visibility = System.Windows.Visibility.Visible;
            this.Overlay2_3.Visibility = System.Windows.Visibility.Visible;
            this.Overlay3.Visibility = System.Windows.Visibility.Visible;
            this.Overlay4.Visibility = System.Windows.Visibility.Visible;
            this.Overlay4_1.Visibility = System.Windows.Visibility.Visible;
            this.Overlay4_2.Visibility = System.Windows.Visibility.Visible;
            this.Overlay4_3.Visibility = System.Windows.Visibility.Visible;
            this.Overlay4_4.Visibility = System.Windows.Visibility.Visible;
            this.Overlay5.Visibility = System.Windows.Visibility.Visible;
            this.Overlay6.Visibility = System.Windows.Visibility.Visible;

            this.Overlay.Opacity = 1;
            this.Overlay1.Opacity = 1;
            this.Overlay2.Opacity = 1;
            this.Overlay2_1.Opacity = 1;
            this.Overlay2_2.Opacity = 1;
            this.Overlay2_3.Opacity = 1;
            this.Overlay3.Opacity = 1;
            this.Overlay4.Opacity = 1;
            this.Overlay4_1.Opacity = 1;
            this.Overlay4_2.Opacity = 1;
            this.Overlay4_3.Opacity = 1;
            this.Overlay4_4.Opacity = 1;
            this.Overlay5.Opacity = 1;
            this.Overlay6.Opacity = 1;

            this.TutorialUI0.Visibility = System.Windows.Visibility.Visible;
            this.TutorialUI1.Visibility = System.Windows.Visibility.Visible;
            this.TutorialUI2.Visibility = System.Windows.Visibility.Visible;
            this.TutorialUI3.Visibility = System.Windows.Visibility.Visible;
            this.TutorialUI3Arrow.Visibility = System.Windows.Visibility.Visible;
            this.TutorialUI4.Visibility = System.Windows.Visibility.Visible;
            this.TutorialUI5.Visibility = System.Windows.Visibility.Visible;
            this.TutorialUI6.Visibility = System.Windows.Visibility.Visible;

            //Navego hacia Salta
            var sphericalTopLeft = SphericalMercator.FromLonLat(-66.693003, -21.758917);
            var sphericalBottomRight = SphericalMercator.FromLonLat(-61.30395, -26.567339);
            mapControl.ZoomToBox(new Mapsui.Geometries.Point(sphericalTopLeft.x, sphericalTopLeft.y), new Mapsui.Geometries.Point(sphericalBottomRight.x, sphericalBottomRight.y));

            //TODO: Blergh awfull dirty dirty hack to show hexagon after zoomToHexagon (problem = Extend is a center point after ZoomToBox)
            mapControl.ZoomIn();
            mapControl.ZoomOut();
            mapControl.ZoomIn();  //Extra zoom to see the plots

            this.StoryBoardTutorialUI0.Begin();

            //Pongo un loading de unos segundos para darle un feedback al usuario mientras se carga el mapa
            if (!replayTutorial)
            {
                this.loadingAnimText.Text = string.Format(Labels.LoadingMaps, Environment.NewLine);
                this.loadinAnim.Visibility = System.Windows.Visibility.Visible;
            }
        }


        //Layer Handlers & Methods(SatelliteImages, Basecamps, LawLayer) 
        void layerList_HexLayerVisibilityChanged(object sender, SharedEventArgs e)
        {
            if (e.Action == "False")
            {
                this.HexagonsLegend.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                this.HexagonsLegend.Visibility = System.Windows.Visibility.Visible;
            }
        }
        void layerList_ChangingOpacity(object sender, ChangingOpacityEventArgs e)
        {
            string newTitle = string.Format("{0} {1}{2}", Labels.Timeline1, e.Title, e.Title.Equals("2008") ? Labels.PreviousDeforestation : e.IsCloudy ? Labels.Main2 : string.Empty);
            if (this.NotificationText.Visibility == System.Windows.Visibility.Collapsed || this.NotificationText.Text != newTitle)
            {
                this.NotificationText.Visibility = System.Windows.Visibility.Visible;
                this.NotificationText.Text = newTitle;
                if (!e.IsInitial)
                {
                    ShowSatelliteImageText();
                }
            }
        }
        void ChangeOpacityStoryBoard_Completed(object sender, EventArgs e)
        {
            this.NotificationText.Visibility = System.Windows.Visibility.Collapsed;
        }
        void ShowSatelliteImageText()
        {
            this.ChangeOpacityStoryBoard.Begin();
        }
        void layerList_AddingLayer(object sender, EventArgs e)
        {
            if (Current.Instance.TutorialStarted)
            {
                this.TutorialUI2.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        void layerList_LayerAdded(object sender, EventArgs e)
        {
            if (Current.Instance.TutorialStarted)
            {
                CompleteTutorialStep();
            }
        }
        private void AddBasecampsLayer()
        {
            this.layerList.AddBasecampsLayer();
        }
        private void AddRegionLawLayer()
        {
            this.layerList.AddRegionLawLayer();
        }
        
        //No se usan
        void layerList_ArgentineLawLayerLayerVisibilityChanged(object sender, SharedEventArgs e)
        {
            if (e.Action == "False")
            {
                this.ArgentineLawLayerLegend.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                this.ArgentineLawLayerLegend.Visibility = System.Windows.Visibility.Visible;
            }
        }
        void layerList_ArgentineLawLoading(object sender, EventArgs e)
        {
            //this.loadinAnim.Visibility = System.Windows.Visibility.Visible;
        }
        void layerList_ArgentineLawLoaded(object sender, EventArgs e)
        {
            //this.loadinAnim.Visibility = System.Windows.Visibility.Collapsed;
        }
        private void TutorialArgentineLawClick(object sender, RoutedEventArgs e)
        {
            this.TutorialArgentineLaw.Visibility = System.Windows.Visibility.Collapsed;
        }


        //Find The Jaguar Handlers & Methods
        private void jaguarRequests_PositionReceived(object sender, EventArgs e)
        {
            JaguarLayer jaguarLayer = Current.Instance.LayerHelper.FindLayer(Constants.jaguarLayerName) as JaguarLayer;
            Current.Instance.JaguarGame = sender as JaguarGame;
            ToolTipService.SetToolTip(this.JaguarIcon, string.Format(Labels.Main3, Current.Instance.JaguarGame.GetFinalizationTime().ToString("hh:mm")));
            jaguarLayer.NotifyJaguarReceived();
        }
        private void JaguarIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowFindTheJaguarPopUp();
        }
        private void JaguarGameInitialize(int jaguarPositionId)
        {
            if (Current.Instance.Features.IsUnlocked(EwFeature.JaguarGame))
            {
                if (!Current.Instance.JaguarGameStarted)
                {
                    jaguarRequests.GetJaguarGameByID(jaguarPositionId);
                    this.JaguarIcon.Visibility = System.Windows.Visibility.Visible;
                    this.ToggleJaguarIcon.Begin();

                    Current.Instance.JaguarGameStarted = true;
                }
            }
        }
        private void JaguarGameFinalize()
        {
            if (Current.Instance.JaguarGameStarted)
            {
                this.JaguarIcon.Visibility = System.Windows.Visibility.Collapsed;
                this.ToggleJaguarIcon.Stop();
                JaguarLayer jaguarLayer = Current.Instance.LayerHelper.FindLayer(Constants.jaguarLayerName) as JaguarLayer;
                jaguarLayer.ClearJaguar();

                Current.Instance.JaguarGameStarted = false;
            }
        }
        private void ShowFindTheJaguarPopUp()
        {
            var jaguarWindow = new FindTheJaguar();
            jaguarWindow.Show();
        }
        private void ShowFindTheJaguarFoundPopUp(string ewMail)
        {
            var jaguarWindow = new FindTheJaguarFound(ewMail);
            jaguarWindow.Show();
        }
        private void ShowFindTheJaguarCongratsPopUp()
        {
            var jaguarWindow = new FindTheJaguarCongrats();
            jaguarWindow.Show();
        }
        private bool JaguarClickLogic(Point mousePos)
        {
            //Find the Jaguar Logic
            bool isClickingJaguar = false;

            if (Current.Instance.JaguarGame != null) //esto significa que hay un juego findthejaguar corriendo
            {
                if (Current.Instance.MapControl.Viewport.Resolution <= 2.4)
                {
                    var jaguar = Current.Instance.JaguarGame;
                    var sphericalMid = SphericalMercator.FromLonLat(jaguar.Longitude, jaguar.Latitude);
                    var jaguarPoint = new System.Windows.Point(sphericalMid.x, sphericalMid.y);

                    var mouseSpherical = mapControl.Viewport.ScreenToWorld(mousePos.X, mousePos.Y);
                    var mousePoint = new System.Windows.Point(mouseSpherical.X, mouseSpherical.Y);

                    if (GetDistanceBetweenPoints(mousePoint, jaguarPoint) <= 300)
                    {
                        isClickingJaguar = true;
                        var posId = Current.Instance.JaguarGame.Id;
                        Current.Instance.JaguarGame = null; //game over.
                        JaguarRequests requests = new JaguarRequests(Constants.BaseApiUrl);
                        requests.UpdateWinner(Current.Instance.Earthwatcher.Id, posId, Current.Instance.Earthwatcher.FullName);  //Updatear el que lo encontro     

                        Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.FoundTheJaguar.ToString(), ActionPoints.Points(ActionPoints.Action.FoundTheJaguar), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore));
                        AddPoints(Current.Instance.AddScore);
                        ShowFindTheJaguarCongratsPopUp();
                    }
                }
            }

            return isClickingJaguar;
        }


        //ReportWindow Handlers
        void reportWindow_ReportWindowLandStatusChanged(object sender, EventArgs e)
        {
            if (Current.Instance.AddScore.Count > 0)
            {
                AddPoints(Current.Instance.AddScore);
            }

            //Refresh Percentage on Own plot Status Changed
            landRequest.GetCheckPercentage(Current.Instance.Earthwatcher.PlayingRegion);

        }
        void reportWindow_ReportWindowConfirmationEnded(object sender, EventArgs e)
        {
            if (Current.Instance.AddScore.Count > 0)
            {
                AddPoints(Current.Instance.AddScore);
            }
        }
        void reportWindow_ReportWindowClosed(object sender, EventArgs e)
        {
            if (Current.Instance.TutorialStarted)
            {
                CompleteTutorialStep(); //case 4
            }
        }
        void reportWindow_ReportWindowDemand(object sender, EventArgs e)
        {
            if (Current.Instance.AddScore.Count > 0)
            {
                AddPoints(Current.Instance.AddScore);
            }
        }
        void reportWindow_CollectionCompleted(object sender, CollectionCompleteEventArgs e)
        {
            if (Current.Instance.RegionScores.Any(x => x.Action == ActionPoints.Action.CollectionComplete.ToString() + " " + e.CollectionId))
            {
                Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.CollectionComplete.ToString() + " " + e.CollectionId, e.Points, Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore));
                AddPoints(Current.Instance.AddScore);
            }
        }
        void reportWindow_Shared(object sender, SharedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Action))
            {
                sharedEventArgs = e;

                //Veo el time del server
                scoreRequest.GetServerTime();
            }
        }


        //Score Handlers & Methods
        void scoreRequest_ServerDateTimeReceived(object sender, EventArgs e)
        {
            Score score = sender as Score;
            DateTime serverTime = score.Published;

            if (serverTime != null && sharedEventArgs != null)
            {
                int timesPerDayAllowed = 10;
                int timesPerDay = 0;
                if (sharedEventArgs.Action.StartsWith(ActionPoints.Action.DemandAuthorities.ToString()))
                {
                    //Si ya le dí puntaje por demandar esa parcela entonces no le vuelvo a dar
                    if (Current.Instance.RegionScores.Any(x => x.Action.Equals(sharedEventArgs.Action)))
                    {
                        return;
                    }
                    timesPerDay = Current.Instance.RegionScores.Count(x => x.Action.StartsWith(ActionPoints.Action.DemandAuthorities.ToString()) && x.Published.Date == serverTime.Date);
                    timesPerDayAllowed = 5;
                }
                else
                {
                    timesPerDay = Current.Instance.RegionScores.Count(x => x.Action.Equals(sharedEventArgs.Action) && x.Published.Date == serverTime.Date);
                }
                if (timesPerDay < timesPerDayAllowed)
                {
                    Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, sharedEventArgs.Action, sharedEventArgs.Points, Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore, null, serverTime.Date));
                    AddPoints(Current.Instance.AddScore);
                }
            }
        }
        void scoreRequest_ScoresReceived(object sender, EventArgs e)
        {
            var isRank = sender as List<Rank> != null;

            if (!isRank) //Si no esta recibiendo el top 10, sigo normalmente
            {
                //Puntajes de todos los paises
                Current.Instance.AllCountriesScores = sender as List<Score>;

                //Puntajes de la region en la que esta jugando
                if (Current.Instance.AllCountriesScores == null)
                {
                    Current.Instance.AllCountriesScores = new List<Score>();
                }
                Current.Instance.RegionScores = Current.Instance.AllCountriesScores.Where(x => x.RegionId == Current.Instance.Earthwatcher.PlayingRegion).ToList();
                this.TotalScore.Text = Current.Instance.AllCountriesScores.Sum(x => x.Points).ToString();
                this.RegionScore.Text = Current.Instance.RegionScores.Sum(x => x.Points).ToString();
                //initialize features and turn on unlocked ones
                Current.Instance.Features = new Features(Current.Instance.RegionScores, Current.Instance.Earthwatcher.Id, Current.Instance.Earthwatcher.PlayingRegion);

                //Correccion para desloqueos por puntos que se obtienen mediante acciones de terceros.
                var toLog = Current.Instance.Features.GetNewUnlocksToLog();
                if (toLog.Any())
                {
                    this.AddPoints(toLog);
                }

                //chequear si hay algo para habilitar la primera vez que recibo los puntos del usuario
                this.UpdateRankingPosition(Current.Instance.Earthwatcher.Id);
                this.UpdateTotalLandsVerified(Current.Instance.RegionScores);
                this.TurnOnUnlockedFeatures();
                this.UpdateHelpNotifications();
                this.PostTutorialTasks.CheckDoneList(Current.Instance.AllCountriesScores, true);
                this.AutomaticShareEvents(ActionPoints.Action.CheckAchivements.ToString());
                this.AddBadges();

                if (!Current.Instance.TutorialStarted)
                {
                    if (Current.Instance.AllCountriesScores == null || !Current.Instance.AllCountriesScores.Any(x => x.Action.Equals(ActionPoints.Action.TutorialCompleted.ToString())))
                    {
                        StartTutorialsSequence();
                    }
                    else
                    {
                        //Corro la animación solo si el login le dio puntos
                        var lastLogin = Current.Instance.RegionScores.Where(x => x.Action.Equals(ActionPoints.Action.Login.ToString())).FirstOrDefault();
                        if (lastLogin != null && lastLogin.Points > 0)
                        {
                            ShowPoints();
                        }
                        Start(false);
                    }
                }
            }
            else
            {
                List<Rank> ranking = sender as List<Rank>;
                if (ranking != null || ranking.Count != 0)
                {
                    top10InRanking = ranking.Where(x => x.OrderRank <= 10).ToList();

                    //AutomaticShareEvent
                    this.AutomaticShareEvents(ActionPoints.Action.Top10.ToString());
                }
            }
        }
        void scoreRequest_ScoreAdded(object sender, EventArgs e)
        {
            isScoreAdding = false;
            List<Score> scores = sender as List<Score>;
            if (scores != null)
            {
                Current.Instance.AllCountriesScores.AddRange(scores.Where(x => x.EarthwatcherId == Current.Instance.Earthwatcher.Id));
                Current.Instance.RegionScores.AddRange(scores.Where(x => x.EarthwatcherId == Current.Instance.Earthwatcher.Id));

                if (scores.Count > 0 && scores.Any(x => x.Points != 0))
                    ShowPoints();

                //loggear si se desbloqueo algun nuevo feature
                var toLog = Current.Instance.Features.GetNewUnlocksToLog();
                if (toLog.Any())
                {
                    this.AddPoints(toLog);
                }

                //Vuelvo a llamar a estos metodos cada vez que se agrega un score
                this.UpdateRankingPosition(Current.Instance.Earthwatcher.Id);
                this.UpdateTotalLandsVerified(Current.Instance.RegionScores);
                this.TurnOnUnlockedFeatures();
                this.UpdateHelpNotifications();
                this.PostTutorialTasks.CheckDoneList(Current.Instance.AllCountriesScores, false);
                this.AutomaticShareEvents(ActionPoints.Action.CheckAchivements.ToString());
                this.AddBadges();
            }
        }
        void scoreRequest_ScoreUpdated(object sender, EventArgs e)
        {
            isScoreAdding = false;
            Score score = sender as Score;
            if (score != null)
            {
                var oldRegionScore = Current.Instance.RegionScores.Where(x => x.EarthwatcherId == score.EarthwatcherId && x.Action == score.Action).FirstOrDefault();
                if (oldRegionScore != null)
                {
                    Current.Instance.RegionScores.Remove(oldRegionScore);
                }
                var oldTotalScore = Current.Instance.AllCountriesScores.Where(x => x.EarthwatcherId == score.EarthwatcherId && x.Action == score.Action).FirstOrDefault();
                if (oldTotalScore != null)
                {
                    Current.Instance.AllCountriesScores.Remove(oldTotalScore);
                }
                Current.Instance.RegionScores.Add(score);
                Current.Instance.AllCountriesScores.Add(score);

                ShowPoints();

                //loggear si se desbloqueo algun nuevo feature
                var toLog = Current.Instance.Features.GetNewUnlocksToLog();
                if (toLog.Any())
                {
                    this.AddPoints(toLog);
                }

                //chequear si hay algo para habilitar
                this.TurnOnUnlockedFeatures();
            }

        }
        void scoreRequest_PositionInRankingReceived(object sender, EventArgs e)
        {
            var userRank = sender as Rank;
            if (userRank != null)
            {
                var userPosition = userRank.OrderRank;
                this.RankingPosition.Text = "#" + userPosition.ToString();
                this.RankingTotalUsers.Text = "+25000";
            }
        }
        void customShareTextRequest_TextsReceived(object sender, EventArgs e)
        {
            Current.Instance.CustomShareTexts = sender as CustomShareText;
        }
        /// <summary>
        /// OJO: Tener en cuenta que este metodo se ejecuta varias veces durante la ejecucion (cuando se trae los scores x primera vez y cada vez que se agrega otro)
        ///  Poner alguna restriccion si se van a agregar elementos a una lista, ya que puede repetirlos varias veces.
        /// </summary>
        public void TurnOnUnlockedFeatures()
        {
            if (Current.Instance.Features.IsUnlocked(EwFeature.ForestLaw))
            {
            }

            AddBasecampsLayer();
            AddRegionLawLayer();

            if (Current.Instance.Features.IsUnlocked(EwFeature.Image2008Warning)) //Para que no la muestre hasta que este habilitado
            {
                if (!Current.Instance.RegionScores.Any(x => x.Action == ActionPoints.Action.FeatureUnlocked_Image2008Warning.ToString()) && Current.Instance.Earthwatcher.PlayingRegion == 1)
                    AddImage2008Warning();
            }
        }
        private void ShowPoints()
        {
            this.RegionScore.Text = Current.Instance.RegionScores.Sum(x => x.Points).ToString();
            this.TotalScore.Text = Current.Instance.AllCountriesScores.Sum(x => x.Points).ToString();

            pointsSound.Stop();
            pointsSound.Play();

            PointsStoryBoard.Stop();
            PointsStoryBoard.Begin();

            UpdateHelpNotifications();
        }
        void UpdatePoints(string action, int points)
        {
            var score = new Score(Current.Instance.Earthwatcher.Id, action, points, Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore);
            isScoreAdding = true;
            scoreRequest.Update(score, Current.Instance.Username, Current.Instance.Password);
        }
        public void AddPoints(List<Score> scores)
        {
            //var score = new Score { EarthwatcherId = earthwatcherId, Action = action, Points = points, Published = DateTime.UtcNow };
            isScoreAdding = true;
            scoreRequest.Post(scores, Current.Instance.Username, Current.Instance.Password);
            Current.Instance.AddScore.Clear();
        }
        [ScriptableMember]
        public void AddFbSharedPoints(bool addScore)
        {
            if (addScore)
            {
                //LE ASIGNO LOS PUNTOS
                if (!Current.Instance.RegionScores.Any(x => x.Action.Equals(ActionPoints.Action.Shared.ToString()) && x.LandId == selectedLand.Id && x.Published > DateTime.Now.AddDays(-30)))
                {
                    Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.Shared.ToString(), ActionPoints.Points(ActionPoints.Action.Shared), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore, selectedLand.Id));
                    AddPoints(Current.Instance.AddScore);
                }
            }
        }
        public void UpdateRankingPosition(int earthwatcherId)
        {
            scoreRequest.GetRankingPosition(earthwatcherId);
        }
        public void UpdateTotalLandsVerified(List<Score> scores)
        {
            var TotalLandsVerified = scores.Where(x => x.Action == ActionPoints.Action.LandStatusChanged.ToString() || x.Action == ActionPoints.Action.ConfirmationAdded.ToString());
            this.TotalVerifications.Text = TotalLandsVerified.Count().ToString();
        }

        //LandRequest Handlers & Methods
        void landRequest_VerifiedLandCodesReceived(object sender, EventArgs e)
        {
            List<string> codes = sender as List<string>;
            if (codes.Count > 0)
            {
                PollsWindow pollsWindow = new PollsWindow(codes);
                pollsWindow.BonusReached += pollsWindow_BonusReached;
                pollsWindow.Show();
            }
        }
        void landRequest_LandsReceived(object sender, EventArgs e)
        {
            loadinAnim.Visibility = Visibility.Collapsed;
            ShowSatelliteImageText();
            List<Land> lands = sender as List<Land>;
            if (lands != null)
            {
                Current.Instance.Lands = lands;
            }

            //Inicializo el Reporte Diario cuando recibo el dato del porcentaje 
            //(HAY QUE HACERLO ACA QUE ES CUANDO SI O SI TIENE LAS LANDS CARGADAS) (solo si ya registro los context tutorial de ese pais para q no sea tanto)
            if (Current.Instance.RegionScores.Any(x => x.Action.Equals(ActionPoints.Action.ContextTutorialCompleted.ToString()) && (x.RegionId == Current.Instance.Earthwatcher.PlayingRegion)) && Current.Instance.Lands != null)
            {
                landRequest.GetPresicionDenouncePercentage(Current.Instance.Earthwatcher);
            }
        }
        void landRequest_CheckPercentageReceived(object sender, EventArgs e)
        {
            //Actualizo el nombre de la region donde se está Jugando
            this.RegionName.Text = Current.Instance.Region.Name; //TODO: Modificar para localizar los nombres con un getName
            this.RegionNametxt.Text = Current.Instance.Region.Name; //TODO: Modificar para localizar los nombres con un getName

            //Progress Bar
            var percentage = Convert.ToDecimal(sender.ToString(), new CultureInfo("en-US"));
            this.circularPBPercentage.Percentage = Convert.ToDouble(percentage);
            this.PBTextContent.Text = percentage.ToString() + "%";
            this.CheckPercentageGrid.Opacity = 1;
        }
        void landRequest_PresicionPercentageReceived(object sender, EventArgs e)
        {
            if (sender as string != "null")
            {
                var percentage = Convert.ToDecimal(sender.ToString(), new CultureInfo("en-US"));
                Current.Instance.Precision = percentage;
                Current.Instance.PrecisionScore = SetPrecisionScore(percentage, Current.Instance.Region);
                this.showDailySummary(contestText, percentage);
            }
            else
            {
                Current.Instance.Precision = null;
                Current.Instance.PrecisionScore = SetPrecisionScore(null, Current.Instance.Region);
                this.showDailySummary(contestText, null);
            }
        }
        void notificationsWindow_LandVerified(object sender, EventArgs e)
        {
            Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.LandVerified.ToString(), ActionPoints.Points(ActionPoints.Action.LandVerified), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore));
            Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.LandVerifiedInformed.ToString(), ActionPoints.Points(ActionPoints.Action.LandVerifiedInformed), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore));
            AddPoints(Current.Instance.AddScore);

            //AutomaticShareEvent
            AutomaticShareEvents(ActionPoints.Action.LandVerifiedInformed.ToString());
        }
        void notificationsWindow_LandReassigned(object sender, EventArgs e)
        {
            Land land = sender as Land;
            if (land != null)
            {
                Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.LandReassigned.ToString(), ActionPoints.Points(ActionPoints.Action.LandReassigned), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore, land.Id, land.StatusChangedDateTime));
                AddPoints(Current.Instance.AddScore);
            }
        }
        private void LandChanged(Land land)
        {
            selectedLand = land;
            bool openPoll = true;

            if (land == null)
                return;


            if (!Current.Instance.TutorialStarted)
            {
                //Chequeo Scoring para abrir Modals
                var logins = Current.Instance.RegionScores.Where(x => x.Action.Equals(ActionPoints.Action.Login.ToString())).OrderByDescending(x => x.Published);
                Score lastLogin = null;
                if (logins.Count() > 1)
                {
                    lastLogin = logins.Skip(1).First();
                }
                if (lastLogin != null)
                {
                    if (Current.Instance.RegionScores.Any(x => x.Action.Equals("DemandAuthoritiesApproved") && x.Published > lastLogin.Published))
                    {
                        NotificationsWindow notificationsWindow = new NotificationsWindow("DemandAuthoritiesApproved");
                        notificationsWindow.Show();
                        openPoll = false;
                    }

                    if (selectedLand.LastReset > lastLogin.Published)
                    {
                        NotificationsWindow notificationsWindow = new NotificationsWindow("NewLand");
                        notificationsWindow.Show();
                        openPoll = false;
                    }
                }

                var last2 = Current.Instance.RegionScores.OrderByDescending(x => x.Published).Take(2);
                if (last2.Any(x => x.Action.Equals(ActionPoints.Action.LandVerified.ToString())) && !last2.Any(x => x.Action.Equals(ActionPoints.Action.LandVerifiedInformed.ToString())))
                {
                    NotificationsWindow notificationsWindow = new NotificationsWindow(ActionPoints.Action.LandVerifiedInformed.ToString());
                    notificationsWindow.Show();
                    openPoll = false;
                }

                //    //DESCOMENTAR ESTO PARA HABILITAR LAS POLLS
                //    //Si el ultimo login es de más de 2 horas, abrir el poll
                //    if (openPoll && lastLogin != null && DateTime.UtcNow.AddHours(-2) >= lastLogin.Published)
                //    {
                //        //TODO: DB. verificar con lucas 
                //        //agrego como condicion final que el feature de poll este desbloqueado.
                //        if (Current.Instance.Features.IsUnlocked(EwFeature.Polls))
                //        {
                //            landRequest.GetVerifiedLandsGeoHexCodes(Current.Instance.Earthwatcher.Id, true);
                //        }
                //    }
            }

            if (!Current.Instance.TutorialStarted || Current.Instance.TutorialCurrentStep < 4)
            {
                var hexagonLayer = Current.Instance.LayerHelper.FindLayer(Constants.Hexagonlayername) as HexagonLayer;

                if (hexagonLayer != null)
                    hexagonLayer.AddHexagon(selectedLand);

                //Inicializar fincas paito
                var basecampLayer = Current.Instance.LayerHelper.FindLayer(Constants.BasecampsLayer) as BasecampLayer;

                if (basecampLayer != null)
                    basecampLayer.LoadData();
            }

            if (string.IsNullOrEmpty(geohexcode))
            {
                if (!string.IsNullOrEmpty(land.GeohexKey))
                {
                    MapHelper.ZoomToHexagon(Current.Instance.MapControl, land.GeohexKey);
                }
                else
                {
                    MapHelper.ZoomToHexagon(Current.Instance.MapControl, "NY8582044"); // Hack: Si viene nulo por algun motivo lo mando a la land del tutor(siempre verde, lockeada)
                    //MessageBox.Show("TEST: MAPA CHICO, LLENDO A PARCELA DEL TUTOR");
                }
            }
            else
            {
                MapHelper.ZoomToHexagon(Current.Instance.MapControl, "NY8582044");
            }

            //Si tengo la land del tutor / No hay mas parcelas - Deshabilito el boton "Mi parcela" y "Cambiar Parcela" y muestro el msj
            if (!Current.Instance.TutorialStarted && Current.Instance.Earthwatcher.Lands.FirstOrDefault().IsTutorLand)
            {
                //Muestro el mensaje
                NotificationsWindow notificationsWindow = new NotificationsWindow(ActionPoints.Action.RegionCompleted.ToString());
                notificationsWindow.Show();

                this.newPlotDiv.IsHitTestVisible = false;
                this.newPlotDiv.Opacity = 0.2;
                this.myPlotDiv.IsHitTestVisible = false;
                this.myPlotDiv.Opacity = 0.2;
            }
            else
            {
                this.newPlotDiv.IsHitTestVisible = true;
                this.newPlotDiv.Opacity = 1;
                this.myPlotDiv.IsHitTestVisible = true;
                this.myPlotDiv.Opacity = 1;
            }
        }
        private void LandChanged(object sender, EventArgs e)
        {
            var land = sender as Land;
            LandChanged(land);
        }

        //CountrySelector Handlers
        void CountrySelector_ChangeStarting(object sender, EventArgs e)
        {
            loadinAnim.Visibility = Visibility.Visible;
        }
        void CountrySelector_ChangeCompleted(object sender, EventArgs e)
        {

            //Agregar los puntos
            if (Current.Instance.AddScore.Count > 0)
            {
                AddPoints(Current.Instance.AddScore);
            }
            scoreRequest.GetByUser(Current.Instance.Earthwatcher.Id);

            //Actualizo las imagenes satelitales y timeline
            layerList.ImageRequests.GetAllImagery(Current.Instance.Earthwatcher.PlayingRegion);

            //Actualizo la imagen de la bandera con el playingCountry
            playingCountryFlag.Source = ResourceHelper.GetBitmap(string.Format("/Resources/Images/Flags/{0}-35.png", Current.Instance.Earthwatcher.PlayingRegion));
            CountryFlag.Source = ResourceHelper.GetBitmap(string.Format("/Resources/Images/Flags/{0}-35.png", Current.Instance.Earthwatcher.PlayingRegion));

            //Actualizo el nombre de la region
            this.RegionName.Text = Current.Instance.Region.Name; //TODO: Modificar para localizar los nombres con un getName
            this.RegionNametxt.Text = Current.Instance.Region.Name; //TODO: Modificar para localizar los nombres con un getName

            //Refresco la presicion del usuario en esa region
            landRequest.GetPresicionDenouncePercentage(Current.Instance.Earthwatcher);

            customShareTextRequest.GetByRegionIdAndLanguage(Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.Earthwatcher.Language);

            //Si tengo la land del tutor / No hay mas parcelas - Deshabilito el boton "Mi parcela" y "Cambiar Parcela"
            if (Current.Instance.Earthwatcher.Lands.FirstOrDefault().IsTutorLand)
            {
                this.newPlotDiv.IsHitTestVisible = false;
                this.newPlotDiv.Opacity = 0.2;
                this.myPlotDiv.IsHitTestVisible = false;
                this.myPlotDiv.Opacity = 0.2;
            }
            else
            {
                this.newPlotDiv.IsHitTestVisible = true;
                this.newPlotDiv.Opacity = 1;
                this.myPlotDiv.IsHitTestVisible = true;
                this.myPlotDiv.Opacity = 1;
            }

            loadinAnim.Visibility = Visibility.Collapsed;
        }


        //Regions Handlers
        private void regionRequest_RegionReceived(object sender, EventArgs e)
        {
            Current.Instance.Region = sender as Region;
        }
        private string GetLocalizedRegionName(string regionName)
        {
            switch(regionName.ToLower())
            {
                //case"yaan":
                //    return Labels.Yaan;
                //    break;
                //case "zejian":
                //    return Labels.Zejian;
                //    break;
                //case "Salta":
                //    return Labels.Salta;
                //    break;
                //case "Canada":
                //    return Labels.Canada;
                //    break;
            }
            return "-";
        }

        //Map Handlers & Methods
        private void MapControl_zoomFinished(object sender, EventArgs e)
        {
            if (flag == false)
            {
                if (mapControl.Viewport.Resolution <= 2.4)
                {
                    JaguarImg.Visibility = Visibility.Collapsed;
                    BinocularsImg.Visibility = Visibility.Visible;
                }
                else
                {
                    JaguarImg.Visibility = Visibility.Visible;
                    BinocularsImg.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (mapControl.Viewport.Resolution <= 2.4)
                {
                    JaguarImg.Visibility = Visibility.Collapsed;
                    BinocularsImg.Visibility = Visibility.Visible;
                }
                else
                {
                    JaguarImg.Visibility = Visibility.Visible;
                    BinocularsImg.Visibility = Visibility.Collapsed;
                }
            }
            flag = true;
        }
        private void MapControl_zoomStarted(object sender, EventArgs e)
        {
            if (mapControl.Viewport.Resolution <= 2.4)
            {
                JaguarImg.Visibility = Visibility.Collapsed;
                BinocularsImg.Visibility = Visibility.Visible;
            }
            else
            {
                JaguarImg.Visibility = Visibility.Visible;
                BinocularsImg.Visibility = Visibility.Collapsed;
            }
        }
        void OpacitiesStoryBoard_Completed(object sender, EventArgs e)
        {
            this.Overlay.Visibility = System.Windows.Visibility.Collapsed;
            this.Overlay1.Visibility = System.Windows.Visibility.Collapsed;
            this.Overlay2.Visibility = System.Windows.Visibility.Collapsed;
            this.Overlay2_1.Visibility = System.Windows.Visibility.Collapsed;
            this.Overlay2_2.Visibility = System.Windows.Visibility.Collapsed;
            this.Overlay2_3.Visibility = System.Windows.Visibility.Collapsed;
            this.Overlay3.Visibility = System.Windows.Visibility.Collapsed;
            this.Overlay4.Visibility = System.Windows.Visibility.Collapsed;
            this.Overlay4_1.Visibility = System.Windows.Visibility.Collapsed;
            this.Overlay4_2.Visibility = System.Windows.Visibility.Collapsed;
            this.Overlay4_3.Visibility = System.Windows.Visibility.Collapsed;
            this.Overlay4_4.Visibility = System.Windows.Visibility.Collapsed;
            this.Overlay5.Visibility = System.Windows.Visibility.Collapsed;
            this.Overlay6.Visibility = System.Windows.Visibility.Collapsed;

            this.TutorialUI0.Visibility = System.Windows.Visibility.Collapsed;
            this.TutorialUI1.Visibility = System.Windows.Visibility.Collapsed;
            this.TutorialUI2.Visibility = System.Windows.Visibility.Collapsed;
            this.TutorialUI3.Visibility = System.Windows.Visibility.Collapsed;
            this.TutorialUI3Arrow.Visibility = System.Windows.Visibility.Collapsed;
            this.TutorialUI4.Visibility = System.Windows.Visibility.Collapsed;
            this.TutorialUI5.Visibility = System.Windows.Visibility.Collapsed;
            this.TutorialUI6.Visibility = System.Windows.Visibility.Collapsed;
        }
        /// <summary>
        ///  The map is shown in the middle of the screen, since Silverlight is unable to clipToBounds,
        ///  the grid needs Clip, the rect has to change when the window is resized.
        /// </summary>
        private void MainPageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //clipMap.Rect = new Rect(0, 0, mapWrapper.ActualWidth, mapWrapper.ActualHeight);
        }
        private void MapControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            currentMousePos = e.GetPosition(mapControl);
        }
        private void MapControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Si movi el mouse mientras apreté entonces es un drag
            var mousePos = e.GetPosition(mapControl);
            if (GetDistanceBetweenPoints(currentMousePos, mousePos) > 5)
            {
                return;
            }

            bool isClickingJaguar = JaguarClickLogic(mousePos);

            if (!isClickingJaguar)
            {
                if (!isScoreAdding) //esto evita que abran el hexagono nuevamente mientras se está guardando un puntaje
                {
                    if (!Current.Instance.TutorialStarted || (Current.Instance.TutorialStarted && (Current.Instance.TutorialCurrentStep == 3)))
                    {
                        leftMouseButtonDown = true;

                        reportWindow.Initialize();

                        //INTERSECT BASECAMPS  
                        if (layerHelper.FindLayer(Constants.BasecampsLayer).Enabled) // si esta activado el layer de basecamps
                        {

                            var bsphericalCoordinate = mapControl.Viewport.ScreenToWorld(mousePos.X, mousePos.Y);
                            var blonLat = SphericalMercator.ToLonLat(bsphericalCoordinate.X, bsphericalCoordinate.Y);

                            var bfeature = reportWindow.GetFeature(blonLat.x, blonLat.y, 7);

                            var bhexCode = GeoHex.Encode(blonLat.x, blonLat.y, 7);
                        }
                        if (!layerHelper.FindLayer(Constants.Hexagonlayername).Enabled)
                            return;

                        var sphericalCoordinate = mapControl.Viewport.ScreenToWorld(mousePos.X, mousePos.Y);
                        var lonLat = SphericalMercator.ToLonLat(sphericalCoordinate.X, sphericalCoordinate.Y);

                        // first try on level 7...
                        var feature = reportWindow.GetFeature(lonLat.x, lonLat.y, 7);
                        var hexCode = GeoHex.Encode(lonLat.x, lonLat.y, 7);
                        if (feature == null)
                        {
                            // try on level 6...
                            hexCode = GeoHex.Encode(lonLat.x, lonLat.y, 6);
                            feature = reportWindow.GetFeature(lonLat.x, lonLat.y, 6);
                            if (feature == null)
                            {
                                reportWindow.Visibility = Visibility.Collapsed;
                                return;
                            }
                        }

                        //Just for tutorial on clicking Report Buttons
                        if (Current.Instance.TutorialCurrentStep == 3)
                        {
                            CompleteTutorialStep(); //case 3
                            if (selectedLand != null && selectedLand.GeohexKey.Equals(hexCode))
                            {
                                this.TutorialUI3.Visibility = System.Windows.Visibility.Collapsed;
                                this.TutorialUI3Arrow.Visibility = System.Windows.Visibility.Collapsed;
                            }
                            else
                                return;
                        }

                        reportWindow.LoadReportWindowContent(hexCode);

                        reportWindow.ShowInfo(lonLat.x, lonLat.y, hexCode);
                    }
                }
            }

            leftMouseButtonDown = false;
        }
        private void MapControlMouseLeave(object sender, MouseEventArgs e)
        {
            leftMouseButtonDown = true;
        }
        private void MapControlMouseMove(object sender, MouseEventArgs e)
        {
            if (!leftMouseButtonDown)
                return;

            //reportWindow.Move();
        }


        //Help Handlers & Methods
        void UpdateHelpNotifications()
        {
            int helpNotifications = 0;
            if (!Current.Instance.AllCountriesScores.Any(x => x.Action.Equals(ActionPoints.Action.TutorialCompleted.ToString())))
            {
                helpNotifications += 2;
            }

            if (!Current.Instance.AllCountriesScores.Any(x => x.Action.Equals(ActionPoints.Action.MiniJuegoI.ToString())))
            {
                helpNotifications++;
            }

            if (!Current.Instance.AllCountriesScores.Any(x => x.Action.Equals(ActionPoints.Action.ScoringHelp.ToString())))
            {
                helpNotifications++;
            }

            if (helpNotifications > 0)
            {
                this.HelpNotifications.Visibility = System.Windows.Visibility.Visible;
                this.HelpNotificationsText.Text = helpNotifications.ToString();
            }
            else
            {
                this.HelpNotifications.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        void gameWindow_Closed(object sender, EventArgs e)
        {
            TutorialGame2Window gameWindow = sender as TutorialGame2Window;
            if (gameWindow != null && gameWindow.points > 0)
            {
                var score = Current.Instance.AllCountriesScores.Where(x => x.EarthwatcherId == Current.Instance.Earthwatcher.Id && x.Action == ActionPoints.Action.MiniJuegoI.ToString()).FirstOrDefault();
                if (score != null)
                {
                    if (score.Points < gameWindow.points)
                    {
                        UpdatePoints(ActionPoints.Action.MiniJuegoI.ToString(), gameWindow.points);
                    }
                }
                else
                {
                    Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.MiniJuegoI.ToString(), gameWindow.points, Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore));
                    AddPoints(Current.Instance.AddScore);
                }
            }
        }


        //SignalR Handlers
        void signalRClient_NotificationReceived(object sender, NotificationReceivedEventArgs e)
        {
            //Si alguien tomo la parcela de este usuario conectado
            if (e.Data.M == "LandChanged")
            {
                //Logueo el Land Changed
                //Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.Log.ToString(), ActionPoints.Points(ActionPoints.Action.Log), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore, null, null, "Llega a signalRClient_NotificationReceived POR LandChanged"));
                AddPoints(Current.Instance.AddScore);

                if (!Current.Instance.Earthwatcher.Id.ToString().Equals(e.Data.A.Last()) && Current.Instance.Earthwatcher.Lands.Any(x => x.GeohexKey.Equals(e.Data.A.First())))
                {
                    NotificationsWindow notificationsWindow = new NotificationsWindow("ChangeLand");
                    notificationsWindow.LandReassigned += notificationsWindow_LandReassigned;
                    notificationsWindow.Show();
                }
            }
            else if (e.Data.M == ActionPoints.Action.LandVerified.ToString())
            {
                if (Current.Instance.Earthwatcher.Id.ToString().Equals(e.Data.A.First()))
                {
                    NotificationsWindow notificationsWindow = new NotificationsWindow(ActionPoints.Action.LandVerified.ToString());
                    notificationsWindow.LandVerified += notificationsWindow_LandVerified;
                    notificationsWindow.Show();
                }
            }
            else if (e.Data.M == "updateUsersOnlineCount")
            {
                this.StatsControl.UpdateOnlineUsers(e.Data.A.First());

            }
            else if (e.Data.M == "FindTheJaguar")
            {
                int jagPosId = Convert.ToInt32(e.Data.A.First());

                JaguarGameInitialize(jagPosId);
            }
            else if (e.Data.M == "JaguarFound")
            {
                JaguarGameFinalize();

                if (e.Data.A.Last() != Current.Instance.Earthwatcher.Id.ToString())
                {
                    ShowFindTheJaguarFoundPopUp(e.Data.A.First());
                }
            }
            else if (e.Data.M == "FindTheJaguarFinished")
            {
                JaguarGameFinalize();
            }
        }


        //Polls, Contests, DailyMessage, DailySummary, Image2008, AutomaticShareEvents, Badges Handlers & Methods
        void pollsWindow_BonusReached(object sender, SharedEventArgs e)
        {
            Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, e.Action, e.Points, Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore));
            AddPoints(Current.Instance.AddScore);
        }
        void contestRequests_ContestReceived(object sender, EventArgs e)
        {
            Contest contest = sender as Contest;
            if (contest != null && !Current.Instance.RegionScores.Any(x => (x.Action == ActionPoints.Action.ContestWinnerAnnounced.ToString() && x.Param1 == contest.Id.ToString()) || (x.Action == ActionPoints.Action.ContestWon.ToString() && x.Param1 == contest.Id.ToString())))
            {
                if (contest.WinnerId != null)
                {
                    //Le doy los 10.000 puntos o registro que ya se lo anuncié
                    if (contest.WinnerId.Value == Current.Instance.Earthwatcher.Id)
                    {
                        Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.ContestWon.ToString(), ActionPoints.Points(ActionPoints.Action.ContestWon), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore, null, null, contest.Id.ToString()));
                        
                        //AutomaticShareEvent
                        AutomaticShareEvents(ActionPoints.Action.ContestWon.ToString());
                    }
                    else
                    {
                        Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.ContestWinnerAnnounced.ToString(), ActionPoints.Points(ActionPoints.Action.ContestWinnerAnnounced), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore, null, null, contest.Id.ToString()));
                    }

                    AddPoints(Current.Instance.AddScore);

                    ContestWinner winner = new ContestWinner(contest);
                    winner.Show();
                }
            }
            if (contest != null)
            {
                contestText = string.Format("{0} {1}", Labels.DailySummaryContest, contest.EndDate.ToString("dd/MM/yyyy")); // HH:mm:ss
            }
        }
        private void popupMessageRequests_MessageReceived(object sender, EventArgs e)
        {
            var messageInfo = sender as List<PopupMessage>;
            var read1 = true;
            foreach (var m in messageInfo)
            {
                if (m != null && m.RegionId == Current.Instance.Earthwatcher.PlayingRegion)
                {
                    if (!Current.Instance.RegionScores.Any(x => x.Action == ActionPoints.Action.DailyMessage.ToString() && x.Param1 == m.Id.ToString()))
                    {
                        if (read1 == true)
                        {
                            read1 = false;
                            //loguear que recibio el dailyMessage
                            Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.DailyMessage.ToString(), ActionPoints.Points(ActionPoints.Action.DailyMessage), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore, null, null, m.Id.ToString()));
                            AddPoints(Current.Instance.AddScore);

                            PopupMessageWindow popup = new PopupMessageWindow(m);
                            popup.Show();
                        }
                    }
                }
            }
        }
        private void showDailySummary(string contestText, decimal? percentage)
        {
            //Mostrar el dailySummary una sola vez al día
            if (!Current.Instance.RegionScores.Any(x => x.Action == ActionPoints.Action.DailySummary.ToString() && x.Published.Date == DateTime.UtcNow.Date))
            {
                //loguear que recibio el DailySummary
                Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.DailySummary.ToString(), ActionPoints.Points(ActionPoints.Action.DailySummary), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore));
                AddPoints(Current.Instance.AddScore);

                var dailySummary = new DailySummary(contestText, percentage);
                dailySummary.Show();
            }

            //Cargar la precision e imagen
            SetPrecisionColorAndImage(percentage);
        }
        private void AddImage2008Warning()
        {
            if (Current.Instance.Features.IsNew(EwFeature.Image2008Warning) && shown == false)
            {
                shown = true;
                Image2008Warning popup = new Image2008Warning();
                popup.Show();
            }
        }
        private void AutomaticShareEvents(string action)
        {
            if (Current.Instance.Earthwatcher.AllowAutoShare)
            {
                CustomShareText customTexts = Current.Instance.CustomShareTexts;
                bool share = false;
                string title = "";
                string link = Labels.GuardianesUrl;
                string pictureUrl = "http://guardianes.greenpeace.org.ar";
                string shareText = "";
                string description = Labels.GuardianesUrl;

                string param1 = "";

                //Registration
                if (action == ActionPoints.Action.TutorialCompleted.ToString() && !Current.Instance.AllCountriesScores.Any(x => x.Action == ActionPoints.Action.TutorialCompleted.ToString()))
                {
                    share = true;
                    title = Labels.AsReg;
                    pictureUrl = pictureUrl + "/SatelliteImages/shareImages/registration.png";
                    shareText = Labels.ASRegText + " " + ShortenUrl(link) + " " + Labels.HashTag;
                    shareText = customTexts != null && !string.IsNullOrEmpty(customTexts.HashTagRegister) ? customTexts.HashTagRegister : shareText;
                    param1 = "Registration";
                }
                else
                    //Report Confirmed
                    if (action == ActionPoints.Action.LandVerifiedInformed.ToString())
                    {
                        share = true;
                        title = Labels.AsRepOPConf;
                        pictureUrl = pictureUrl + "/SatelliteImages/IMAGEN DE PARCELA.png";
                        link = Labels.GuardianesUrl;
                        shareText = Labels.AsRepOPConfText + " " + ShortenUrl(link) + " " + Labels.HashTag;
                        shareText = customTexts != null && !string.IsNullOrEmpty(customTexts.HashTagReportConfirmed) ? customTexts.HashTagReportConfirmed : shareText;
                        param1 = "Report Aproved";
                    }
                    else
                        //Contest Winner
                        if (action == ActionPoints.Action.ContestWon.ToString())
                        {
                            share = true;
                            title = Labels.AsContestWin;
                            pictureUrl = pictureUrl + "/SatelliteImages/shareImages/competittionWon.png";
                            shareText = Labels.AsContestWinText + " " + ShortenUrl(link) + " " + Labels.HashTag;
                            shareText = customTexts != null && !string.IsNullOrEmpty(customTexts.HashTagContestWon) ? customTexts.HashTagContestWon : shareText;
                            param1 = "Competittion Won";
                        }
                        else
                            //Actions when player reach certain number of checks, verifications, denounces, score
                            #region Achivements per X times user do something
                            if (action == ActionPoints.Action.CheckAchivements.ToString())
                            {
                                var checks = Current.Instance.RegionScores.Where(x => x.Action == ActionPoints.Action.LandStatusChanged.ToString()).Count();
                                var verifications = Current.Instance.RegionScores.Where(x => x.Action == ActionPoints.Action.ConfirmationAdded.ToString()).Count();
                                var denounces = Current.Instance.RegionScores.Where(x => x.Action == ActionPoints.Action.DemandAuthorities.ToString()).Count();

                                //First 10 Checks in own plot
                                if (checks == 10 && Current.Instance.RegionScores.Where(x => x.Action == ActionPoints.Action.AutomaticShare.ToString() && x.Param1 == "Check 10 Own").Count() < 1)
                                {
                                    share = true;
                                    title = Labels.AsCheck10OwPText;
                                    pictureUrl = pictureUrl + "/SatelliteImages/shareImages/ownPlot.png";
                                    shareText = Labels.AsCheck10OwPText + " " + Labels.HashTag;
                                    shareText = customTexts != null && !string.IsNullOrEmpty(customTexts.HashTagCheck) ? string.Format(customTexts.HashTagCheck, checks) : shareText;
                                    param1 = "Check 10 Own";
                                }
                                else
                                    //First 10 Verifications
                                    if (verifications == 10 && Current.Instance.RegionScores.Where(x => x.Action == ActionPoints.Action.AutomaticShare.ToString() && x.Param1 == "Check 10 Others").Count() < 1)
                                    {
                                        share = true;
                                        title = Labels.AsCheck10OU;
                                        pictureUrl = pictureUrl + "/SatelliteImages/shareImages/othersPlot.png";
                                        shareText = Labels.AsCheck10OUText + " " + Labels.HashTag;
                                        shareText = customTexts != null && !string.IsNullOrEmpty(customTexts.HashTagVerification) ? string.Format(customTexts.HashTagVerification, verifications) : shareText;
                                        param1 = "Check 10 Others";
                                    }
                                    else
                                        //First 10 Denounces
                                        if (denounces == 10 && Current.Instance.RegionScores.Where(x => x.Action == ActionPoints.Action.AutomaticShare.ToString() && x.Param1 == "Denounce 10").Count() < 1)
                                        {
                                            share = true;
                                            title = Labels.AsDen10;
                                            pictureUrl = pictureUrl + "/SatelliteImages/shareImages/denounce.png";
                                            shareText = Labels.AsDen10Tex + " " + Labels.HashTag;
                                            shareText = customTexts != null && !string.IsNullOrEmpty(customTexts.HashTagDenounce) ? string.Format(customTexts.HashTagDenounce, denounces) : shareText;
                                            param1 = "Denounce 10";
                                        }

                                //First 50 Checks in own plot
                                if (checks == 50 && Current.Instance.RegionScores.Where(x => x.Action == ActionPoints.Action.AutomaticShare.ToString() && x.Param1 == "Check 50 Own").Count() < 1)
                                {
                                    share = true;
                                    title = Labels.AsCheck50OwP;
                                    pictureUrl = pictureUrl + "/SatelliteImages/shareImages/ownPlot.png";
                                    shareText = Labels.AsCheck50OwPText + " " + Labels.HashTag;
                                    shareText = customTexts != null && !string.IsNullOrEmpty(customTexts.HashTagCheck) ? string.Format(customTexts.HashTagCheck, checks) : shareText;
                                    param1 = "Check 50 Own";
                                }
                                else
                                    //First 50 Verifications
                                    if (verifications == 50 && Current.Instance.RegionScores.Where(x => x.Action == ActionPoints.Action.AutomaticShare.ToString() && x.Param1 == "Check 50 Others").Count() < 1)
                                    {
                                        share = true;
                                        title = Labels.AsCheck50OU;
                                        pictureUrl = pictureUrl + "/SatelliteImages/shareImages/othersPlot.png";
                                        shareText = Labels.AsCheck50OUText + " " + Labels.HashTag;
                                        shareText = customTexts != null && !string.IsNullOrEmpty(customTexts.HashTagVerification) ? string.Format(customTexts.HashTagVerification, verifications) : shareText;
                                        param1 = "Check 50 Others";
                                    }
                                    else
                                        //First 50 Denounces
                                        if (denounces == 50 && Current.Instance.RegionScores.Where(x => x.Action == ActionPoints.Action.AutomaticShare.ToString() && x.Param1 == "Denounce 50").Count() < 1)
                                        {
                                            share = true;
                                            title = Labels.AsDen50;
                                            pictureUrl = pictureUrl + "/SatelliteImages/shareImages/denounce.png";
                                            shareText = Labels.AsDen50Text + " " + Labels.HashTag;
                                            shareText = customTexts != null && !string.IsNullOrEmpty(customTexts.HashTagDenounce) ? string.Format(customTexts.HashTagDenounce, denounces) : shareText;
                                            param1 = "Denounce 50";
                                        }

                                //Every 100 Checks in own plot
                                if (checks != 0 && (checks % 100) == 0 && Current.Instance.RegionScores.Where(x => x.Action == ActionPoints.Action.AutomaticShare.ToString() && x.Param1 == "Check " + checks + " Own").Count() < 1)
                                {
                                    share = true;
                                    title = Labels.As1001 + checks + Labels.As1002;
                                    pictureUrl = pictureUrl + "/SatelliteImages/shareImages/ownPlot.png";
                                    shareText = Labels.As1001Text + " " + checks + " " + Labels.As1002Text + " " + Labels.HashTag;
                                    shareText = customTexts != null && !string.IsNullOrEmpty(customTexts.HashTagCheck) ? string.Format(customTexts.HashTagCheck, checks) : shareText;
                                    param1 = "Check " + checks + " Own";
                                }
                                else  //Every 100 Verifications
                                    if (verifications != 0 && (verifications % 100) == 0 && Current.Instance.RegionScores.Where(x => x.Action == ActionPoints.Action.AutomaticShare.ToString() && x.Param1 == "Check " + verifications + " Others").Count() < 1)
                                    {
                                        share = true;
                                        title = Labels.As1001Ou + verifications + Labels.As1002Ou;
                                        pictureUrl = pictureUrl + "/SatelliteImages/shareImages/othersPlot.png";
                                        shareText = Labels.As1001OuText + " " + verifications + " " + Labels.As1002OuText + " " + Labels.HashTag;
                                        shareText = customTexts != null && !string.IsNullOrEmpty(customTexts.HashTagVerification) ? string.Format(customTexts.HashTagVerification, verifications) : shareText;
                                        param1 = "Check " + verifications + " Others";
                                    }
                                    else  //Every 100 Denounces
                                        if (denounces != 0 && (denounces % 100) == 0 && Current.Instance.RegionScores.Where(x => x.Action == ActionPoints.Action.AutomaticShare.ToString() && x.Param1 == "Denounce" + denounces).Count() < 1)
                                        {
                                            share = true;
                                            title = Labels.As100Den1 + denounces + Labels.As100Den2;
                                            pictureUrl = pictureUrl + "/SatelliteImages/shareImages/denounce.png";
                                            shareText = Labels.As100Den1Text + " " + denounces + " " + Labels.As100Den2Text + " " + Labels.HashTag;
                                            shareText = customTexts != null && !string.IsNullOrEmpty(customTexts.HashTagDenounce) ? string.Format(customTexts.HashTagDenounce, denounces) : shareText;
                                            param1 = "Denounce" + denounces;
                                        }
                            }
                            #endregion
                            else
                                //Top 10 or Top 1 IN THE WEEK
                                if (action == ActionPoints.Action.Top10.ToString() &&
                                    !Current.Instance.RegionScores.Any(x => x.Published > DateTime.Now.AddDays(-7)))
                                {
                                    //Si esta en el top 10
                                    if (top10InRanking.Any(x => x.EarthwatcherId == Current.Instance.Earthwatcher.Id)) //Si esta en el top 10
                                    {
                                        share = true;

                                        if ((top10InRanking.First(x => x.OrderRank == 1).EarthwatcherId == Current.Instance.Earthwatcher.Id) //Si es el primero
                                            && (!Current.Instance.RegionScores.Any(s => s.Param1 == ActionPoints.Action.Top1.ToString())))         //Y no lo registro nunca el de esta semana
                                        {
                                            title = Labels.AsTop1;
                                            pictureUrl = pictureUrl + "/SatelliteImages/shareImages/firstInContest.png";
                                            shareText = Labels.AsTop1Text + " " + Labels.HashTag;
                                            shareText = customTexts != null && !string.IsNullOrEmpty(customTexts.HashTagTop1) ? customTexts.HashTagTop1 : shareText;
                                            param1 = ActionPoints.Action.Top1.ToString();
                                        }

                                        else if (!Current.Instance.RegionScores.Any(s => s.Param1 == ActionPoints.Action.Top1.ToString())) //Si no lo registro nunca el de esta semana
                                        {
                                            title = Labels.AsTop10;
                                            pictureUrl = pictureUrl + "/SatelliteImages/shareImages/firstInContest.png";
                                            shareText = Labels.AsTop10Text + " " + Labels.HashTag;
                                            shareText = customTexts != null && !string.IsNullOrEmpty(customTexts.HashTagRanking) ? customTexts.HashTagRanking : shareText;
                                            param1 = ActionPoints.Action.Top10.ToString();
                                        }
                                    }
                                }


                if (share)
                {
                    //FB
                    var itemFb = HtmlPage.Window.CreateInstance("Object");
                    itemFb.SetProperty("name", title);
                    itemFb.SetProperty("link", link);
                    itemFb.SetProperty("picture", pictureUrl);
                    itemFb.SetProperty("caption", shareText);
                    itemFb.SetProperty("description", description);
                    HtmlPage.Window.Invoke("postInFacebook", itemFb);

                    Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.AutomaticShare.ToString(), ActionPoints.Points(ActionPoints.Action.AutomaticShare), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore, null, null, param1));
                    AddPoints(Current.Instance.AddScore);

                    ////T!
                    //var taringaShout = string.Format("http://www.taringa.net/widgets/share.php?url=http://guardianes.greenpeace.org.ar/?body={0}", shareText);
                    //taringaShout = taringaShout.Replace("#", "%23");
                    //var itemTa = HtmlPage.Window.CreateInstance("Object");
                    //itemTa.SetProperty("url", taringaShout);
                    //HtmlPage.Window.Invoke("shoutInTaringa", itemTa);

                    //TW
                    //var shortUrlTw = ShortenUrl("http://guardianes.greenpeace.org.ar/?twshare");
                    //var twitterText = shareText + " " + shortUrlTw;
                    //var finalTwUrl = string.Format("https://twitter.com/intent/tweet?text={0}&data-url={1}", Uri.EscapeUriString(twitterText).Replace("#", "%23"), shortUrlTw);
                    //HtmlPage.Window.Invoke("postInTwitter", finalTwUrl);

                }
            }
        }
        private void AddBadges()
        {
            //Jaguar
            if (Current.Instance.RegionScores.Count(x => x.Action == ActionPoints.Action.FoundTheJaguar.ToString()) != 0)
            {
                this.jaguarbadge.Visibility = Visibility.Visible;
            }

            //ContestWon
            if (Current.Instance.RegionScores.Any(x => x.Action.StartsWith(ActionPoints.Action.ContestWon.ToString())))
            {
                this.ContestWinnerBadge.Visibility = Visibility.Visible;
            }

            //PowerUser
            if (Current.Instance.Earthwatcher.IsPowerUser)
            {
                this.badgeIcon.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/badgej.png");
                ToolTipService.SetToolTip(this.badgeIcon, Labels.Jaguar);
            }
        }


        //Precision Methods
        private void SetPrecisionColorAndImage(decimal? precision)
        {
                if (precision != null && precision >= Current.Instance.Region.HighThreshold) 
                {
                    this.PrecisionTxt.Text = Convert.ToInt32(precision).ToString() + "%";
                    this.PrecisionTxt.Foreground = new SolidColorBrush(Color.FromArgb(255, 247, 232, 25)); //Bonus (Amarillo)
                    this.PrecisionImg.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/LeftMenuIcons/verifiedplotsg.png");

                }
                else if (precision != null && precision < Current.Instance.Region.LowThreshold) 
                {
                    this.PrecisionTxt.Text = Convert.ToInt32(precision).ToString() + "%";
                    this.PrecisionTxt.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)); //Penalty (Rojo)
                    this.PrecisionImg.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/LeftMenuIcons/verifiedplotsr.png");
                }
                else if(precision != null)
                {
                    this.PrecisionTxt.Text = Convert.ToInt32(precision).ToString() + "%";
                    this.PrecisionTxt.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)); //Normal (Blanco)
                    this.PrecisionImg.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/LeftMenuIcons/verifiedplotsw.png");
                }
                else
                {
                    this.PrecisionTxt.Text = "- %";
                    this.PrecisionTxt.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)); //Normal (Blanco)
                    this.PrecisionImg.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/LeftMenuIcons/verifiedplotsw.png");
                }
        }
        private int SetPrecisionScore(decimal? precision, Region region)
        {
            if (precision != null && precision >= region.HighThreshold)
                return region.BonusPoints;
            else if (precision != null && precision <= region.LowThreshold)
                return region.PenaltyPoints;
            else 
                return region.NormalPoints;
        }


        //Aditional Helper Handlers & Methods
        private void _3D_Loaded(object sender, RoutedEventArgs e)
        {
        }
        private void pointsSound_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
        }
        public string ShortenUrl(string longUrl)
        {
            var bitly = HtmlPage.Window.Invoke("shorten", new string[] { longUrl }) as string;
            return bitly;
        }
        public double GetDistanceBetweenPoints(Point p, Point q)
        {
            double a = p.X - q.X;
            double b = p.Y - q.Y;
            double distance = Math.Sqrt(a * a + b * b);
            return distance;
        }
        [ScriptableMember]
        public void SetCredentials(string username, string password)
        {
            MessageBox.Show(Labels.Main1 + username);
            // todo: do the same after login method is succeeded: set global username, password for other requests)
        }
        /// <summary>
        /// Recuadro en el mainpage para buscar los Hexagonos por su GeoHexCode
        /// </summary>
        private void TestGeoHexTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MapHelper.ZoomToHexagon(Current.Instance.MapControl, this.TestGeoHexTextBox.Text);
            }
        }


        //Handlers que hay que revisar, ya que creo que no se usan por ser de Earthwatchers
        #region Obsolete Code
        private void callbackSuggestion(List<String> suggestions)
        {
            if (suggestions != null)
            {
                adresses_list = suggestions;
            }

        }
        private void callback(QueryResult queryResult)
        {
            if (queryResult != null)
            {
                var xyStart = SphericalMercator.FromLonLat(queryResult.bbox[0], queryResult.bbox[1]);
                var xyEnd = SphericalMercator.FromLonLat(queryResult.bbox[2], queryResult.bbox[3]);

                var beginPoint = new Mapsui.Geometries.Point(xyStart.x, xyStart.y);
                var endPoint = new Mapsui.Geometries.Point(xyEnd.x, xyEnd.y);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    mapControl.ZoomToBox(beginPoint, endPoint)
                );
            }
        }
        private void SearchClick(object sender, MouseButtonEventArgs e)
        {
            OpengeocoderRequests.GetQueryResult(callback, address);
        }
        #endregion

    }
 
}