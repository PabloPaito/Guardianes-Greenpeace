using System;
using System.Net;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Input;
using System.Collections.Generic;
using Earthwatchers.Models;
using System.Windows.Controls;
using System.Windows.Media;
using Earthwatchers.UI.Requests;
using Earthwatchers.UI.Layers;
using Earthwatchers.UI.Resources;
using System.Collections.ObjectModel;
using Mapsui.Providers;
using Earthwatchers.UI.GUI.Controls.Comments;

namespace Earthwatchers.UI.GUI.Controls
{
    public partial class ReportWindow
    {
        private readonly CollectionRequests collectionRequests;
        private readonly CommentRequests commentRequests;
        private readonly LandRequests landRequests;
        private readonly HexagonLayer hexagonLayer;
        private readonly BasecampLayer bcLayer; 
        private ObservableCollection<Comment> comments;
        private Earthwatcher selectedEarthwatcher;
        private Land selectedLand;
        private Boolean isShown;
        string taringaShout = "";
        string twitterShareUrl = "";
        string longUrlFb = "";
        private int _total;
        LandStatus status;
        bool first = true;

        public delegate void ReportWindowClosedEventHandler(object sender, EventArgs e);
        public delegate void ReportWindowConfirmationEndedEventHandler(object sender, EventArgs e);
        public delegate void ReportWindowDemandEventHandler(object sender, EventArgs e);
        public delegate void ReportWindowLandStatusChangedEventHandler(object sender, EventArgs e);
        public delegate void SharedEventHandler(object sender, SharedEventArgs e);
        public delegate void CollectionCompleteEventHandler(object sender, CollectionCompleteEventArgs e);
        
        public event ReportWindowConfirmationEndedEventHandler ReportWindowConfirmationEnded;
        public event ReportWindowLandStatusChangedEventHandler ReportWindowLandStatusChanged;
        public event CollectionCompleteEventHandler CollectionCompleted;
        public event ReportWindowClosedEventHandler ReportWindowClosed;
        public event ReportWindowDemandEventHandler ReportWindowDemand;
        public event SharedEventHandler Shared;
        

        public ReportWindow(Land land, Earthwatcher earthwatcher)
        {
            InitializeComponent();

            commentRequests = new CommentRequests(Constants.BaseApiUrl);
            collectionRequests = new CollectionRequests(Constants.BaseApiUrl);
            landRequests = new LandRequests(Constants.BaseApiUrl);
            hexagonLayer = (HexagonLayer)Current.Instance.LayerHelper.FindLayer(Constants.Hexagonlayername);
            bcLayer = (BasecampLayer)Current.Instance.LayerHelper.FindLayer(Constants.BasecampsLayer); //TEST
            //Add event listeners
            commentRequests.CommentsByLandReceived += CommentRequestCommentsByLandReceived;
            collectionRequests.NewItemReceived += collectionRequests_NewItemReceived;
            collectionRequests.ItemsCountReceived += collectionRequests_ItemsCountReceived;
            collectionRequests.GetTotalItems(Current.Instance.Earthwatcher.Id);
            Current.Instance.MapControl.zoomFinished += MapControlZoomFinished;
            Current.Instance.MapControl.zoomStarted += MapControlZoomStarted;

           
            landRequests = new LandRequests(Constants.BaseApiUrl);
            landRequests.StatusChanged += SetLandStatusStatusChanged;
            landRequests.ConfirmationAdded += landRequests_ConfirmationAdded;

            hexagonLayer = (HexagonLayer)Current.Instance.LayerHelper.FindLayer(Constants.Hexagonlayername);

            this.Loaded += ReportWindow_Loaded;

            this.ShareStoryBoard.Completed += ShareStoryBoard_Completed;
        }

        //Init ReportWindow Window
        /// <summary>
        /// Se llama del mainpage cada vez que se hace click en una parcela
        /// </summary>
        public void Initialize()
        {
            this.commentsBorder.Visibility = System.Windows.Visibility.Collapsed;

            txtName.Text = string.Empty;
            txtName.Visibility = System.Windows.Visibility.Collapsed;
            txtBasecampName.Text = string.Empty;
            txtBasecampName.Visibility = System.Windows.Visibility.Collapsed;
            txtBasecamp.Text = string.Empty;
            txtBasecamp.Visibility = System.Windows.Visibility.Collapsed;
            this.LastUsersWithActivityText.Visibility = System.Windows.Visibility.Collapsed;
            this.LastUsersWithActivityText.MouseLeftButtonDown -= LastUsersWithActivityText_MouseLeftButtonDown;
            this.LastUsersWithActivityText.MouseEnter -= LastUsersWithActivityText_MouseEnter;
            this.LastUsersWithActivityText.MouseLeave -= LastUsersWithActivityText_MouseLeave;
        }
        public void LoadReportWindowContent(string geoHexCode)
        {
            //Limpio los bordes y rellenos de las manitos 
            var noBorder = new Thickness(0);
            var white = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            this.AlertButton.BorderThickness = noBorder;
            this.AlertButton.Background = white;
            this.OkButton.BorderThickness = noBorder;
            this.OkButton.Background = white;
            this.ConfirmButton.BorderThickness = noBorder;
            this.ConfirmButton.Background = white;
            this.DeconfirmButton.BorderThickness = noBorder;
            this.DeconfirmButton.Background = white;

            //Muestro la ventanita
            this.Visibility = Visibility.Visible;

            //Oculto el TITLE y los grid de REPORTE, VERIFICACION, SHARE, DEMANDA. Mostrar FOOTER, COMMENTS, USER TITLE
            this.ValidateMessageText.Visibility = System.Windows.Visibility.Collapsed;
            this.Title.Visibility = System.Windows.Visibility.Collapsed;
            this.ReportGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.ConfirmGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.DemandGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.ShareGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.FooterGrid.Visibility = System.Windows.Visibility.Visible;
            this.commentsBorder.Visibility = System.Windows.Visibility.Visible;
            this.FincasGrid.Visibility = System.Windows.Visibility.Visible;
            this.UserTitle.Visibility = System.Windows.Visibility.Visible;
           //Luego de mostrar el shareGrid la animacion pone opacity 0 a las grillas de report y confirm
            this.ShareGrid.Opacity = 0;
            this.ConfirmGrid.Opacity = 1;
            this.ReportGrid.Opacity = 1;

            //Cargo los datos de la land y el earthwatcher de esa land
            foreach (var land in Current.Instance.LandInView)
            {
                if (land.GeohexKey.Equals(geoHexCode))
                {
                    selectedLand = land;
                    selectedEarthwatcher = new Earthwatcher { Name = selectedLand.EarthwatcherName, Id = selectedLand.EarthwatcherId.Value, IsPowerUser = selectedLand.IsPowerUser.Value };
                }
            }

            if (selectedLand == null)
                return;

            if (selectedLand.DemandAuthorities == false) //Si es reporte o verificacion
            {
               LoadReportORVerificationGrid();
            }
            else //Si es Denuncia
            {
                LoadDenounceGrid();  
            }
        }
        private void LoadReportORVerificationGrid()
        {
            #region Load Data For Both
                string[] oks = null;
                string[] alerts = null;
                char[] charsep = new char[] { ',' };

                this.GeoHexCodeText.Text = selectedLand.GeohexKey;

                if (!string.IsNullOrEmpty(selectedLand.OKs))
                {
                    oks = selectedLand.OKs.Split(charsep, StringSplitOptions.RemoveEmptyEntries);
                }

                if (!string.IsNullOrEmpty(selectedLand.Alerts))
                {
                    alerts = selectedLand.Alerts.Split(charsep, StringSplitOptions.RemoveEmptyEntries);
                }

                int countConfirm = 0;
                int countDeconfirm = 0;

                if (oks != null)
                {
                    countConfirm = oks.Length;
                }

                if (alerts != null)
                {
                    countDeconfirm = alerts.Length;
                }

                //asocia esto a algún mensaje que muestre confirmación / deconfirmación
                this.countConfirm1.Text = string.Format("{0} {1}", countConfirm + countDeconfirm, Labels.Report12);

                //Limite de 30 verificaciones
                if (selectedLand.IsLocked)
                {
                    DisableActions();
                }
                else
                {
                    EnableActions();
                }

                //Agrega los Badges
                if (selectedEarthwatcher.IsPowerUser)
                {
                    this.badgeIcon.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/badgej.png");
                    ToolTipService.SetToolTip(this.badgeIcon, Labels.Jaguar);
                }
                else
                {
                    this.badgeIcon.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/badge.png");
                    ToolTipService.SetToolTip(this.badgeIcon, null);
                }
                if (Current.Instance.RegionScores.Count(x => x.Action == ActionPoints.Action.FoundTheJaguar.ToString()) != 0)
                    JaguarBadge.Visibility = Visibility.Visible;
                else
                    JaguarBadge.Visibility = Visibility.Collapsed;

                if (Current.Instance.RegionScores.Any(x => x.Action.StartsWith(ActionPoints.Action.ContestWon.ToString())))
                    this.ContestWinnerBadge.Visibility = Visibility.Visible;
                else
                    this.ContestWinnerBadge.Visibility = Visibility.Collapsed;

            #endregion
            #region REPORTE
                if (Current.Instance.Earthwatcher.Lands.Any(x => x.Id == selectedLand.Id))
                {
                    this.ReportButton.Content = Labels.Report1;
                    this.ReportGrid.Visibility = System.Windows.Visibility.Visible;

                    //NO SELECCIONAR LA MANITO OK o ALERT SEGUN EL STATUS, SINO LO QUE EL USER REPORTO
                    if (alerts != null && alerts.Any(x => x.Equals(Current.Instance.Earthwatcher.Id.ToString())))
                    {
                        this.AlertButton.BorderThickness = new Thickness(4);
                        this.AlertButton.Background = new SolidColorBrush(Color.FromArgb(255, 241, 251, 187));
                    }

                    else if (oks != null && oks.Any(x => x.Equals(Current.Instance.Earthwatcher.Id.ToString())))
                    {
                        this.OkButton.BorderThickness = new Thickness(4);
                        this.OkButton.Background = new SolidColorBrush(Color.FromArgb(255, 241, 251, 187));
                    }
                    loadinAnim.Visibility = System.Windows.Visibility.Collapsed;
                }
                #endregion
            #region VERIFICACION
                else
                {
                    //this.Title.Text = Labels.Report5;
                    this.ReportButton.Content = Labels.Report11;
                    this.ConfirmGrid.Visibility = System.Windows.Visibility.Visible;

                    if (oks != null && oks.Any(x => x.Equals(Current.Instance.Earthwatcher.Id.ToString())))
                    {
                        this.ConfirmButton.BorderThickness = new Thickness(4);
                        this.ConfirmButton.Background = new SolidColorBrush(Color.FromArgb(255, 241, 251, 187));

                        this.ConfirmButton.IsHitTestVisible = false;
                        this.ConfirmButton.Cursor = Cursors.Arrow;
                    }

                    if (alerts != null && alerts.Any(x => x.Equals(Current.Instance.Earthwatcher.Id.ToString())))
                    {
                        this.DeconfirmButton.BorderThickness = new Thickness(4);
                        this.DeconfirmButton.Background = new SolidColorBrush(Color.FromArgb(255, 241, 251, 187));

                        this.DeconfirmButton.IsHitTestVisible = false;
                        this.DeconfirmButton.Cursor = Cursors.Arrow;
                    }
                    loadinAnim.Visibility = System.Windows.Visibility.Collapsed;
                }
                #endregion
        }
        private void LoadDenounceGrid()
        {
            this.Title.Visibility = System.Windows.Visibility.Visible;
            this.DemandGrid.Visibility = System.Windows.Visibility.Visible;
            this.FooterGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.commentsBorder.Visibility = System.Windows.Visibility.Collapsed;
            this.FincasGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.UserTitle.Visibility = System.Windows.Visibility.Collapsed;
            this.GeoHexCodeText2.Text = selectedLand.GeohexKey;

            //Si ya habia denunciado esa parcela, Comparte
            if (Current.Instance.RegionScores.Any(x => x.Action.Equals(ActionPoints.Action.DemandAuthorities.ToString()) && (x.LandId == selectedLand.Id)))
            {
                this.DemandIcon.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/demandarShare.png");
                this.Title.Text = Labels.DemandWindow2;
                this.DemandText.Text = Labels.DemandWindow3;
                this.DemandText2.Text = Labels.DemandWindow4;
                this.DemandTitleText.Text = Labels.DemandWindow2;
            }
            else //Si no habia denunciado esa parcela, Denuncia
            {
                this.DemandIcon.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/demandar.png");
                this.Title.Text = Labels.DemandWindow1;
                this.DemandText.Text = Labels.DemandWindow5;
                this.DemandText2.Text = Labels.DemandWindow6;
                this.DemandTitleText.Text = Labels.DemandWindow1;
            }
            loadinAnim.Visibility = System.Windows.Visibility.Collapsed;
        }


        //ReportWindow Handlers
        void ReportWindow_Loaded(object sender, RoutedEventArgs e)
        {
            txtName.Text = Current.Instance.Earthwatcher.FullName;
            if (this.txtName.Text.Length > 16)
            {
                txtName.Text = Current.Instance.Earthwatcher.FullName.Substring(0, 15) + "...";
                ToolTipService.SetToolTip(this.txtName, Current.Instance.Earthwatcher.FullName);
            }

            if (Current.Instance.TutorialStarted)
            {
                this.Overlay.Visibility = System.Windows.Visibility.Visible;
            }
            loadinAnim.Visibility = System.Windows.Visibility.Collapsed;
        }
        void reportWindow_Closed(object sender, EventArgs e)
        {
            ReportWindowClosed(this, EventArgs.Empty);
            this.Visibility = System.Windows.Visibility.Collapsed;
        }
        void reportWindow_Shared(object sender, SharedEventArgs e)
        {
            Shared(sender, e);
        }


        //Collections Handlers
        void collectionRequests_NewItemReceived(object sender, EventArgs e)
        {
            CollectionItem item = sender as CollectionItem;

            if (item != null)
            {
                Collections collections = new Collections(item);
                if (_total == 0)
                {
                    collections.CollectionsTutorial.Visibility = Visibility.Visible;
                    collections.ContinueButton.Visibility = Visibility.Visible;
                    collections.Show();
                }
                else
                {
                    collections.loadCollections();
                    collections.CollectionCompleted += collections_CollectionCompleted;
                    collections.Show();
                }
            }
        }
        void collectionRequests_ItemsCountReceived(object sender, EventArgs e)
        {
            int items = Convert.ToInt32(sender);
            if (items != -1)
                _total = items;
        }
        void collections_CollectionCompleted(object sender, CollectionCompleteEventArgs e)
        {
            CollectionCompleted(this, e);
        }


        //Last Users with activity Handlers
        void LastUsersWithActivityText_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.LastUsersWithActivityText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 65, 65, 65));
        }
        void LastUsersWithActivityText_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.LastUsersWithActivityText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 105, 125, 0));
        }
        void LastUsersWithActivityText_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (selectedLand != null)
            {
                this.LastUsersWithActivityText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 65, 65, 65));
                LandActivity landActivity = new LandActivity(selectedLand.Id);
                landActivity.Show();
            }
        }


        //Logica de los botones de reporte/verificacion (manitos)
        private void Action_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) 
        {
            var customTexts = Current.Instance.CustomShareTexts;
            Border border = sender as Border;
            if (border != null)
            {
                bool ischecked = false;
                if (border.BorderThickness.Left > 0)
                {
                    border.BorderThickness = new Thickness(0);
                    border.Background = new SolidColorBrush(Colors.Transparent);
                }
                else
                {
                    ischecked = true;
                    border.BorderThickness = new Thickness(4);
                    border.Background = new SolidColorBrush(Color.FromArgb(255, 241, 251, 187));
                }

                //Si es tu propia parcela
                if (border.Name.Equals("OkButton") && ischecked)
                {
                    this.AlertButton.BorderThickness = new Thickness(0);
                    this.AlertButton.Background = new SolidColorBrush(Colors.Transparent);
                    this.shareText.Text = string.Format("{0} {1}", Labels.Share1, Labels.HashTag);
                    if(customTexts != null && !string.IsNullOrEmpty(customTexts.ShareOk))
                    {
                        this.shareText.Text = customTexts.ShareOk;
                    }
                }

                if (border.Name.Equals("AlertButton") && ischecked)
                {
                    this.OkButton.BorderThickness = new Thickness(0);
                    this.OkButton.Background = new SolidColorBrush(Colors.Transparent);
                    this.shareText.Text = string.Format("{0} {1}", Labels.Share2, Labels.HashTag);
                    if (customTexts != null && !string.IsNullOrEmpty(customTexts.ShareAlert))
                    {
                        this.shareText.Text = customTexts.ShareAlert;
                    }
                    if (selectedLand != null && !string.IsNullOrEmpty(selectedLand.ShortText))
                    {
                        shareText.Text = String.Format(Labels.Share7 + " {0}" + Labels.Share8 + "{1}", selectedLand.ShortText, Labels.HashTag);
                        if (customTexts != null && !string.IsNullOrEmpty(customTexts.ShareAlertFinca))
                        {
                            this.shareText.Text = customTexts.ShareAlertFinca + " " + selectedLand.ShortText;
                        }
                    }
                   
                }

                //Si es la parcela de otro usuario
                if (border.Name.Equals("ConfirmButton") && ischecked)
                {
                    this.DeconfirmButton.BorderThickness = new Thickness(0);
                    this.DeconfirmButton.Background = new SolidColorBrush(Colors.Transparent);
                    this.shareText.Text = string.Format("{0} {1}", Labels.Share1, Labels.HashTag);
                    if (customTexts != null && !string.IsNullOrEmpty(customTexts.ShareOk))
                    {
                        this.shareText.Text = customTexts.ShareOk;
                    }
                }

                if (border.Name.Equals("DeconfirmButton") && ischecked)
                {
                    this.ConfirmButton.BorderThickness = new Thickness(0);
                    this.ConfirmButton.Background = new SolidColorBrush(Colors.Transparent);
                    this.shareText.Text = string.Format("{0} {1}", Labels.Share2, Labels.HashTag);
                    if (customTexts != null && !string.IsNullOrEmpty(customTexts.ShareAlert))
                    {
                        this.shareText.Text = customTexts.ShareAlert;
                    }
                    if (selectedLand != null && !string.IsNullOrEmpty(selectedLand.ShortText))
                    {
                        shareText.Text = String.Format(Labels.Share7 + " {0}" + Labels.Share8 + "{1}", selectedLand.ShortText, Labels.HashTag);
                        if (customTexts != null && !string.IsNullOrEmpty(customTexts.ShareAlertFinca))
                        {
                            this.shareText.Text = customTexts.ShareAlertFinca + " " + selectedLand.ShortText;
                        }
                    }
                }
            }
        }
        void EnableActions()
        {
            this.ConfirmButton.IsHitTestVisible = true;
            this.ConfirmButton.Cursor = Cursors.Hand;
            this.ConfirmIcon.Source = ResourceHelper.GetBitmap("/Resources/Images/confirm.png");

            this.DeconfirmButton.IsHitTestVisible = true;
            this.DeconfirmButton.Cursor = Cursors.Hand;
            this.DeconfirmIcon.Source = ResourceHelper.GetBitmap("/Resources/Images/deconfirm.png");

            this.OkButton.IsHitTestVisible = true;
            this.OkButton.Cursor = Cursors.Hand;
            this.OkButtonIcon.Source = ResourceHelper.GetBitmap("/Resources/Images/confirm.png");

            this.AlertButton.IsHitTestVisible = true;
            this.AlertButton.Cursor = Cursors.Hand;
            this.AlertButtonIcon.Source = ResourceHelper.GetBitmap("/Resources/Images/deconfirm.png");

            this.ReportButton.IsEnabled = true;
        }
        void DisableActions()
        {
            this.ConfirmButton.IsHitTestVisible = false;
            this.ConfirmButton.Cursor = Cursors.Arrow;
            this.ConfirmIcon.Source = ResourceHelper.GetBitmap("/Resources/Images/confirm_off.png");

            this.DeconfirmButton.IsHitTestVisible = false;
            this.DeconfirmButton.Cursor = Cursors.Arrow;
            this.DeconfirmIcon.Source = ResourceHelper.GetBitmap("/Resources/Images/deconfirm_off.png");

            this.OkButton.IsHitTestVisible = false;
            this.OkButton.Cursor = Cursors.Arrow;
            this.OkButtonIcon.Source = ResourceHelper.GetBitmap("/Resources/Images/confirm_off.png");

            this.AlertButton.IsHitTestVisible = false;
            this.AlertButton.Cursor = Cursors.Arrow;
            this.AlertButtonIcon.Source = ResourceHelper.GetBitmap("/Resources/Images/deconfirm_off.png");
            this.ReportButton.IsEnabled = false;

            //Si tiene la parcela del tutor y esta en el tutorial, habilitar el compartir porque la region está llena.
            if (Current.Instance.TutorialStarted && Current.Instance.Earthwatcher.Lands.FirstOrDefault().IsTutorLand)
            {
                this.ReportButton.IsEnabled = true;
            }
        }


        //Share Methods
        private void ShowShareControl()
        {
            //Facebook y Twitter links
            var geohexcode = selectedLand.GeohexKey;
            var title = HttpUtility.UrlEncode(Labels.PlotChecked);
            var summary = HttpUtility.UrlEncode(this.shareText.Text);

            longUrlFb = string.Format("http://guardianes.greenpeace.org.ar/?fbshare&geohexcode={0}", geohexcode);
            var shortUrlFb = ShortenUrl(longUrlFb);

            var longUrlTw = string.Format("http://guardianes.greenpeace.org.ar/?twshare&geohexcode={0}", geohexcode);
            var shortUrlTw = ShortenUrl(longUrlTw);
            var twitterText = shareText.Text + " " + shortUrlTw;
            twitterShareUrl = string.Format("https://twitter.com/intent/tweet?text={0}&data-url={1}", Uri.EscapeUriString(twitterText).Replace("#", "%23"), shortUrlTw);
            
            this.TwitterButton.NavigateUri = new Uri(string.Format("https://twitter.com/intent/tweet?text={0}&data-url={1}", Uri.EscapeUriString(twitterText).Replace("#", "%23"), shortUrlTw), UriKind.Absolute);

            taringaShout = string.Format("http://www.taringa.net/widgets/share.php?url=http://guardianes.greenpeace.org.ar/?geohexcode={0}&body={1}", geohexcode, shareText.Text);
            //taringaShout = string.Format("http://www.taringa.net/widgets/share.php?url=http://guardianes2.iantech.net/?geohexcode={0}&body={1}", geohexcode, shareText.Text);
            //End Facebook y Twitter

            this.FooterGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.FincasGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.ShareGrid.Visibility = System.Windows.Visibility.Visible;
            this.ShareStoryBoard.Begin();
        }
        private string ShortenUrl(string longUrl)
        {
            var bitly = HtmlPage.Window.Invoke("shorten", new string[] { longUrl }) as string;
            return bitly;
        }
        private void ShareTwitter_Click(object sender, RoutedEventArgs e)
        {
            HtmlPage.Window.Invoke("postInTwitter", twitterShareUrl);
            AddTwFbSharePoints();  //Llamar a esta funcion en el callback
        }
        private void ShareFacebook_Click(object sender, RoutedEventArgs e)
        {
            var item = HtmlPage.Window.CreateInstance("Object");
            item.SetProperty("name", Labels.PlotChecked);
            item.SetProperty("link", longUrlFb);
            item.SetProperty("picture", "http://guardianes.greenpeace.org.ar/SatelliteImages/demand/" + selectedLand.GeohexKey + "-d.jpg");
            item.SetProperty("caption", this.shareText.Text);
            item.SetProperty("description", Labels.GuardianesUrl); 

            HtmlPage.Window.Invoke("postInFacebook", item);
        }
        private void ShareTaringa_Click(object sender, RoutedEventArgs e)
        {
            taringaShout = taringaShout.Replace("#", "%23");
            var item = HtmlPage.Window.CreateInstance("Object");
            item.SetProperty("url", taringaShout);

            HtmlPage.Window.Invoke("shoutInTaringa", item);
            AddTwFbSharePoints();
        }
        void ShareStoryBoard_Completed(object sender, EventArgs e)
        {
            this.MainGrid.IsHitTestVisible = true;
            this.ReportGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.ConfirmGrid.Visibility = System.Windows.Visibility.Collapsed;
        }
        private void AddSharedPoints(string action, int points)
        {
            Shared(action, new SharedEventArgs { Action = action, Points = points });
        }
        private void AddTwFbSharePoints()
        {
            if (!Current.Instance.RegionScores.Any(x => x.Action.Equals(ActionPoints.Action.Shared.ToString()) && x.LandId == selectedLand.Id && x.Published > DateTime.UtcNow.AddDays(-7)))
            {
                Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.Shared.ToString(), ActionPoints.Points(ActionPoints.Action.Shared), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore, selectedLand.Id));
                ReportWindowDemand(this, EventArgs.Empty);
            }
        }

        
        //Action Clicks
        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (Current.Instance.TutorialStarted)
            {
                reportWindow_Closed(null, null); //close the report window and excecute callback on mainpage
                return;
            }

            this.ValidateMessageText.Visibility = System.Windows.Visibility.Collapsed;

            if (Current.Instance.Earthwatcher.Lands != null)
            {
                //Si es tu propia Parcela
                if (Current.Instance.Earthwatcher.Lands.Any(x => x.Id == selectedLand.Id))
                {
                    ConfirmationSort confirmationSort = ConfirmationSort.Confirm;

                    int statusNumber = 0;
                    if (this.AlertButton.BorderThickness.Left > 0)
                    {
                        statusNumber = 4;
                        confirmationSort = ConfirmationSort.Deconfirm; //Alert
                    }

                    if (this.OkButton.BorderThickness.Left > 0)
                    {
                        statusNumber = 3;
                        confirmationSort = ConfirmationSort.Confirm; //OK
                    }

                    if (statusNumber == 0)
                    {
                        statusNumber = 2; //Not Checked
                    }

                    status = (LandStatus)statusNumber;

                    if (selectedLand.LandStatus != status)
                    {
                        //Cambio el color de la parcela, segun el reporte del usuario
                        Current.Instance.Earthwatcher.Lands.Where(x => x.Id == selectedLand.Id).First().LandStatus = status;

                        loadinAnim.Visibility = System.Windows.Visibility.Visible;
                        this.MainGrid.IsHitTestVisible = false;
                        landRequests.Confirm(selectedLand, confirmationSort, Current.Instance.Username, Current.Instance.Password);
                        landRequests.UpdateStatus(selectedLand.Id, status, Current.Instance.Username, Current.Instance.Password);
                    }
                }
                else //Si es la parcela de otro usuario
                {
                    ConfirmationSort confirmationSort = ConfirmationSort.Confirm;
                    bool hasAction = false;
                    if (this.ConfirmButton.BorderThickness.Left > 0)
                    {
                        if (!selectedLand.OKs.Split(',').Any(x => x.Equals(Current.Instance.Earthwatcher.Id.ToString())))
                        {
                            confirmationSort = ConfirmationSort.Confirm;
                            hasAction = true;
                        }
                    }

                    if (this.DeconfirmButton.BorderThickness.Left > 0)
                    {
                        if (!selectedLand.Alerts.Split(',').Any(x => x.Equals(Current.Instance.Earthwatcher.Id.ToString())))
                        {
                            confirmationSort = ConfirmationSort.Deconfirm;
                            hasAction = true;
                        }
                    }

                    if (hasAction)
                    {
                        loadinAnim.Visibility = System.Windows.Visibility.Visible;
                        this.MainGrid.IsHitTestVisible = false;
                        landRequests.Confirm(selectedLand, confirmationSort, Current.Instance.Username, Current.Instance.Password);
                    }
                    else
                    {
                        this.ValidateMessageText.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
        }
        private void DemandButton_Click(object sender, RoutedEventArgs e)
        {
            //Al hacer click redirige a la pagina de denuncias
            string basecamp;
            Uri demandUri;
            
            if (Current.Instance.Earthwatcher.PlayingRegion == 1)
            {
                basecamp = "salta01";
                
                if (selectedLand.BasecampId != null)
                {
                    basecamp = selectedLand.BasecampId.ToString();
                }
                demandUri = new Uri(string.Format("http://greenpeace.org.ar/denuncias/index.php?id_ciberaccion={0}&mail={1}&area={2}&GeohexKey={3}&prev=0&lat={4}&long={5}", 5157, Current.Instance.Earthwatcher.Name, basecamp, selectedLand.GeohexKey, Math.Round(selectedLand.Latitude, 4), Math.Round(selectedLand.Longitude, 4)), UriKind.Absolute);
            }
            else
            {
                if (selectedLand.BasecampId != null)
                {
                    basecamp = selectedLand.BasecampId.ToString();
                }
                basecamp = "chaco01";
                demandUri = new Uri(string.Format("http://greenpeace.org.ar/denuncias/index.php?id_ciberaccion={0}&mail={1}&area={2}&GeohexKey={3}&prev=0&lat={4}&long={5}", 0, Current.Instance.Earthwatcher.Name, basecamp, selectedLand.GeohexKey, Math.Round(selectedLand.Latitude, 4), Math.Round(selectedLand.Longitude, 4)), UriKind.Absolute);
            }
            this.DemandButton.NavigateUri = demandUri;

            //Agrega el score de demanda 
            if (!Current.Instance.RegionScores.Any(x => x.Action.Equals(ActionPoints.Action.DemandAuthorities.ToString()) && x.LandId == selectedLand.Id))
            {
                Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.DemandAuthorities.ToString(), ActionPoints.Points(ActionPoints.Action.DemandAuthorities), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore, selectedLand.Id));
                ReportWindowDemand(this, EventArgs.Empty);
                hexagonLayer.UpdateHexagonsInView();
            }
            //Agrega el score de Share 
            else if (!Current.Instance.RegionScores.Any(x => x.Action.Equals(ActionPoints.Action.Shared.ToString()) && x.LandId == selectedLand.Id))
            {
                Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.Shared.ToString(), ActionPoints.Points(ActionPoints.Action.Shared), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore, selectedLand.Id));
                ReportWindowDemand(this, EventArgs.Empty);
            }
        }
        private void BtnCloseClick(object sender, RoutedEventArgs e)
        {
            this.isShown = false;
            this.Visibility = Visibility.Collapsed;
        }
        private void TxtCommentsClick(object sender, RoutedEventArgs e)
        {
            if (selectedLand == null) return;

            var cs = new CommentScreen(selectedLand.Id, comments);
            cs.OpenCommentScreen();

            this.Visibility = System.Windows.Visibility.Collapsed;
        }


        //Land Requests Handlers
        void landRequests_ConfirmationAdded(object sender, EventArgs e)
        {

            if ((ConfirmationSort)sender == ConfirmationSort.Confirm)
            {
                //Si el usuario antes habia dicho que habia desmonte y cambiode opinion, lo borro del listado de confirmaciones de desmontes
                if (selectedLand.Alerts.Split(',').Any(x => x.Equals(Current.Instance.Earthwatcher.Id.ToString())))
                {
                    //Si no es el primero de la lista
                    if (selectedLand.Alerts.IndexOf("," + Current.Instance.Earthwatcher.Id.ToString()) > -1)
                        selectedLand.Alerts = selectedLand.Alerts.Replace("," + Current.Instance.Earthwatcher.Id.ToString(), "");
                    else  //Si es el primero
                        if (selectedLand.Alerts.IndexOf(Current.Instance.Earthwatcher.Id.ToString()) > -1)
                            selectedLand.Alerts = selectedLand.Alerts.Replace(Current.Instance.Earthwatcher.Id.ToString(), "");
                }
                //Agrego su verificacion
                if (!string.IsNullOrEmpty(selectedLand.OKs))
                {
                    selectedLand.OKs += ",";
                }
                selectedLand.OKs += Current.Instance.Earthwatcher.Id.ToString();
            }
            else
            {
                //Si el usuario antes habia dicho que NO habia desmonte y cambio de opinion, lo borro del listado de confirmaciones de Sin desmontes
                if (selectedLand.OKs.Split(',').Any(x => x.Equals(Current.Instance.Earthwatcher.Id.ToString())))
                {
                    //Si no es el primero de la lista
                    if (selectedLand.OKs.IndexOf("," + Current.Instance.Earthwatcher.Id.ToString()) > -1)
                        selectedLand.OKs = selectedLand.OKs.Replace("," + Current.Instance.Earthwatcher.Id.ToString(), "");
                    else  //Si es el primero 
                        if (selectedLand.OKs.IndexOf(Current.Instance.Earthwatcher.Id.ToString()) > -1)
                            selectedLand.OKs = selectedLand.OKs.Replace(Current.Instance.Earthwatcher.Id.ToString(), "");
                }
                //Agrego su verificacion
                if (!string.IsNullOrEmpty(selectedLand.Alerts))
                {
                    selectedLand.Alerts += ",";
                }
                selectedLand.Alerts += Current.Instance.Earthwatcher.Id.ToString();
            }

            //Si el usuario antes habia tenido actividad en la parcela lo borro del listado
            if (selectedLand.LastUsersWithActivity != null && selectedLand.LastUsersWithActivity.Split(',').Any(x => x.Equals(Current.Instance.Earthwatcher.FullName)))
            {
                //Si ya estaba en la lista, lo saco
                if (selectedLand.LastUsersWithActivity.IndexOf("," + Current.Instance.Earthwatcher.FullName) > -1)
                    selectedLand.LastUsersWithActivity = selectedLand.LastUsersWithActivity.Replace("," + Current.Instance.Earthwatcher.FullName, "");
                else  //Si ya estaba en la lista y es el primero lo saco
                    if (selectedLand.LastUsersWithActivity.IndexOf(Current.Instance.Earthwatcher.FullName) > -1)
                        selectedLand.LastUsersWithActivity = selectedLand.LastUsersWithActivity.Replace(Current.Instance.Earthwatcher.FullName, "");
            }
            //Lo vuelvo a agregar en el orden que corresponda
            if (!string.IsNullOrEmpty(selectedLand.LastUsersWithActivity))
            {
                selectedLand.LastUsersWithActivity += ", ";
            }
            selectedLand.LastUsersWithActivity += Current.Instance.Earthwatcher.FullName;

            bool isUserLand = Current.Instance.Earthwatcher.Lands.Any(x => x.Id == selectedLand.Id);
            bool hasConfirmedThisLandBeforeLastReset = Current.Instance.RegionScores.Any(x => x.Action.Equals(ActionPoints.Action.ConfirmationAdded.ToString()) && x.LandId == selectedLand.Id && x.Published > selectedLand.LastReset);
            if (!isUserLand && !hasConfirmedThisLandBeforeLastReset)
            {
                Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.ConfirmationAdded.ToString(), ActionPoints.Points(ActionPoints.Action.ConfirmationAdded), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore, selectedLand.Id));
            }

            hexagonLayer.UpdateHexagonsInView();

            UpdateInfo(selectedLand.GeohexKey, true);
            ReportWindowConfirmationEnded(this, EventArgs.Empty);

            if (this.ShareGrid.Visibility == System.Windows.Visibility.Collapsed)
            {
                ShowShareControl();
            }
        }
        private void landRequests_ConfirmationAddedUpdateLand(object sender, EventArgs e)
        {
            hexagonLayer.UpdateHexagonsInView();
        }


        //MapControl Methods
        private void MapControlZoomStarted(object sender, EventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
        private void MapControlZoomFinished(object sender, EventArgs e)
        {
            if (!isShown)
                return;

            //Move();
        }


        //Metodos 
        /// <summary>
        /// Se llama del mainpage
        /// </summary>
        public void ShowInfo(double lon, double lat, string hexCode)
        {
            if (!string.IsNullOrEmpty(hexCode))
            {
                UpdateInfo(hexCode, false);
                //Move();
                isShown = true;
                Visibility = Visibility.Visible;
            }
        }
        private void UpdateInfo(string hexCode, bool isRefresh)
        {
            RenderActivity();
            commentRequests.GetCommentsByLand(selectedLand.Id);
        }
        private void RenderActivity()
        {
            if (selectedLand == null)
                return;

            if (selectedLand.DemandAuthorities)
            {
                //Deshabilito los comentarios
                this.commentsBorder.Visibility = System.Windows.Visibility.Collapsed;
            }

            //Mostrar la finca a la que pertenece
            if (selectedLand.BasecampName != null)
            {
                txtBasecamp.Text = Labels.Report13;
                txtBasecampName.Text = selectedLand.BasecampName;
                ToolTipService.SetToolTip(this.txtBasecampName, null);
                if (this.txtBasecampName.Text.Length > 60)
                {
                    txtBasecampName.Text = selectedLand.BasecampName.Substring(0, 58) + "...";
                    ToolTipService.SetToolTip(this.txtBasecampName, selectedLand.BasecampName);
                }
                txtBasecamp.Visibility = System.Windows.Visibility.Visible;
                txtBasecampName.Visibility = System.Windows.Visibility.Visible;
            }

            txtName.Text = selectedEarthwatcher.FullName;
            ToolTipService.SetToolTip(this.txtName, null);
            if (this.txtName.Text.Length > 10)
            {
                txtName.Text = selectedEarthwatcher.FullName.Substring(0, 10) + "...";
                ToolTipService.SetToolTip(this.txtName, selectedEarthwatcher.FullName);
            }
            txtName.Visibility = System.Windows.Visibility.Visible;


            //Last users with activity
            if (selectedLand != null && !string.IsNullOrEmpty(selectedLand.LastUsersWithActivity))
            {
                char[] charsep = new char[] { ',' };
                bool moreThan5 = selectedLand.OKs.Split(charsep, StringSplitOptions.RemoveEmptyEntries).Length + selectedLand.Alerts.Split(charsep, StringSplitOptions.RemoveEmptyEntries).Length > 5 ? true : false;
                var lstUsersWithActivity = selectedLand.LastUsersWithActivity.Split(charsep, StringSplitOptions.RemoveEmptyEntries);
                var lstFirst5 = lstUsersWithActivity.Where(x => x != Configuration.GreenpeaceName).Take(5).ToList();
                if (lstUsersWithActivity.Contains(Configuration.GreenpeaceName))
                {
                    lstFirst5.Insert(0, Configuration.GreenpeaceName);
                    lstFirst5.Take(5);
                }
                this.LastUsersWithActivityText.Text = string.Join(",", lstFirst5) + (moreThan5 ? ", ..." : string.Empty);

                if (moreThan5)
                {
                    this.LastUsersWithActivityText.Cursor = System.Windows.Input.Cursors.Hand;
                    this.LastUsersWithActivityText.MouseLeftButtonDown += LastUsersWithActivityText_MouseLeftButtonDown;
                    this.LastUsersWithActivityText.MouseEnter += LastUsersWithActivityText_MouseEnter;
                    this.LastUsersWithActivityText.MouseLeave += LastUsersWithActivityText_MouseLeave;
                    ToolTipService.SetToolTip(this.LastUsersWithActivityText, Labels.HexInfo6);
                }
                else
                {
                    this.LastUsersWithActivityText.Cursor = System.Windows.Input.Cursors.Arrow;
                    ToolTipService.SetToolTip(this.LastUsersWithActivityText, Labels.HexInfo7);
                }
                this.LastUsersWithActivityText.Visibility = System.Windows.Visibility.Visible;
                this.LastUsersWithActivityTextTitle.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.LastUsersWithActivityTextTitle.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (selectedLand != null && selectedLand.IsLocked)
            {
                btnComments.Visibility = System.Windows.Visibility.Collapsed;
                if(Current.Instance.Earthwatcher.Role == Role.Admin)
                {
                    commentsBorder.Visibility = Visibility.Visible;
                    btnComments.Visibility = System.Windows.Visibility.Visible;                    
                }
            }
            else
            {
                btnComments.Visibility = System.Windows.Visibility.Visible;
            }
        }
        //public void Move()
        //{
        //    if (selectedLand == null)
        //        return;

        //    var spherical = SphericalMercator.FromLonLat(selectedLand.Longitude, selectedLand.Latitude);
        //    var point = Current.Instance.MapControl.Viewport.WorldToScreen(spherical.x, spherical.y);
        //    Margin = new Thickness(point.X + 70, point.Y - 30, 0, 0);
        //}
        private void SetLandStatusStatusChanged(object sender, EventArgs e)
        {
            if (status == LandStatus.Alert || status == LandStatus.Ok)
            {
                bool hasCheckedThisLandBeforeLastReset = Current.Instance.RegionScores.Any(x => x.Action.Equals(ActionPoints.Action.LandStatusChanged.ToString()) && x.LandId == selectedLand.Id && x.Published > selectedLand.LastReset);

                if (!hasCheckedThisLandBeforeLastReset)
                {
                    Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.LandStatusChanged.ToString(), ActionPoints.Points(ActionPoints.Action.LandStatusChanged), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore, selectedLand.Id));
                }
            }

            ReportWindowLandStatusChanged(this, EventArgs.Empty);
            if (this.ShareGrid.Visibility == System.Windows.Visibility.Collapsed)
            {
                ShowShareControl();
            }
        }


        //Helpers
        public Feature GetFeature(double longitude, double latitude, int level)
        {
            //Get the hexcode for the clicked area and try to find if its a feature on the map
            var hexCode = GeoHex.Encode(longitude, latitude, level);

            Feature feature = null; //TEST
            if (first)
            {
                feature = bcLayer.GetFeatureByHex(hexCode); //TEST
                first = false;
            }
            else
                feature = hexagonLayer.GetFeatureByHex(hexCode);

            return feature;
        }
        private void CommentRequestCommentsByLandReceived(object sender, EventArgs e)
        {
            comments = new ObservableCollection<Comment>(sender as List<Comment>);
            if (comments != null)
            {
                NumberOfCommentsText.Text = comments.Count.ToString();
                commentList.ItemsSource = comments.OrderByDescending(x => x.Published).Take(2);
                if (!selectedLand.DemandAuthorities)
                {
                    this.commentsBorder.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }
       

       
    }
}

