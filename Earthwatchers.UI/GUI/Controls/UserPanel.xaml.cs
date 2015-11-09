using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Earthwatchers.Models;
using System.Collections.Generic;
using Earthwatchers.UI.Requests;
using System;
using Earthwatchers.UI.Layers;
using System.Linq;
using Earthwatchers.UI.Resources;
using System.Text.RegularExpressions;

namespace Earthwatchers.UI.GUI.Controls
{
    public partial class UserPanel
    {
        private List<Language> Idiomas { get; set; }
        private List<Country> Paises { get; set; }
        private string step = "ChangePassword";
        private int numberOfJaguarsFounded = Current.Instance.RegionScores.Count(x => x.Action == ActionPoints.Action.FoundTheJaguar.ToString());
        private readonly CollectionRequests collectionRequests;

        //Initialization
        public UserPanel()
        {
            InitializeComponent();

            collectionRequests = new CollectionRequests(Constants.BaseApiUrl);
            collectionRequests.ItemsReceived += collectionRequests_ItemsReceived;

            this.Loaded += UserPanel_Loaded;

            this.DataContext = Current.Instance.Earthwatcher;
        }
        void UserPanel_Loaded(object sender, RoutedEventArgs e)
        {
            //Agrego los badges
            this.AddBadges();

            //Agrego el Nombre, lo recorto si es muy largo
            txtName.Text = Current.Instance.Earthwatcher.FullName;
            if (this.txtName.Text.Length > 16)
            {
                txtName.Text = Current.Instance.Earthwatcher.FullName.Substring(0, 15) + "...";
                ToolTipService.SetToolTip(this.txtName, Current.Instance.Earthwatcher.FullName);
            }
            
            //Agrego Los Candados a las secciones que no se puede acceder
            this.JaguarLock.Visibility = Current.Instance.Features.IsUnlocked(EwFeature.JaguarGame) ? Visibility.Collapsed : Visibility.Visible;
            this.CollectionsLock.Visibility = Current.Instance.Features.IsUnlocked(EwFeature.Collections) ? Visibility.Collapsed : Visibility.Visible;

            //Cargo el listado de paises e idiomas
            this.LoadCountriesAndLanguagesCombo();

            //Jaguar
            this.ManageJaguarWindow();
           
            //Collections
            collectionRequests.GetCollectionItemsByEarthwatcher(Current.Instance.Earthwatcher.Id);
        }

        //Buton's Click
        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            if (border != null && border.Tag != null)
            {
                step = border.Tag.ToString();
                switch (step)
                {
                    case "ChangePassword":
                        SelectedTabArrow.Y = 20;
                        this.ChangePasswordGrid.Visibility = System.Windows.Visibility.Visible;
                        this.ChangeLanguageGrid.Visibility = System.Windows.Visibility.Collapsed;
                        this.CollectionsGrid.Visibility = System.Windows.Visibility.Collapsed;
                        this.JaguarGrid.Visibility = System.Windows.Visibility.Collapsed;
                        this.OkButton.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case "ChangeRegional":
                        SelectedTabArrow.Y = 80;
                        this.ChangePasswordGrid.Visibility = System.Windows.Visibility.Collapsed;
                        this.ChangeLanguageGrid.Visibility = System.Windows.Visibility.Visible;
                        this.CollectionsGrid.Visibility = System.Windows.Visibility.Collapsed;
                        this.JaguarGrid.Visibility = System.Windows.Visibility.Collapsed;
                        this.OkButton.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case "Collections":
                        if (Current.Instance.Features.IsUnlocked(EwFeature.Collections))
                        {
                            SelectedTabArrow.Y = 205;
                            this.ChangePasswordGrid.Visibility = System.Windows.Visibility.Collapsed;
                            this.ChangeLanguageGrid.Visibility = System.Windows.Visibility.Collapsed;
                            this.CollectionsGrid.Visibility = System.Windows.Visibility.Visible;
                            this.JaguarGrid.Visibility = System.Windows.Visibility.Collapsed;
                            this.OkButton.Visibility = System.Windows.Visibility.Collapsed;
                            break;
                        }
                        else
                            break;
                    case "Jaguar":
                        if (Current.Instance.Features.IsUnlocked(EwFeature.JaguarGame))
                        {
                            SelectedTabArrow.Y = 245;
                            this.ChangePasswordGrid.Visibility = System.Windows.Visibility.Collapsed;
                            this.ChangeLanguageGrid.Visibility = System.Windows.Visibility.Collapsed;
                            this.CollectionsGrid.Visibility = System.Windows.Visibility.Collapsed;
                            this.JaguarGrid.Visibility = System.Windows.Visibility.Visible;
                            this.OkButton.Visibility = System.Windows.Visibility.Collapsed;
                            break;
                        }
                        else
                            break;
                    case "LogOut":
                        App.Logout();                        
                        break;
                }
            }
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.OkButton.IsEnabled = false;
            this.ErrorMessage.Foreground = new SolidColorBrush(Colors.Red);
            this.ErrorMessage.Text = string.Empty;

            if (step == "ChangePassword")
            {
                //Valido que los datos ingresados estén ok
                if (!this.CurrentPassword.Password.Equals(Current.Instance.Password))
                {
                    this.ErrorMessage.Text += Labels.UserPanel22;
                }

                if (this.CurrentPassword.Password.Equals(this.NewPassword.Password))
                {
                    if (this.ErrorMessage.Text != string.Empty)
                    {
                        this.ErrorMessage.Text += "\n";
                    }
                    this.ErrorMessage.Text += Labels.UserPanel23;
                }

                if (!Regex.IsMatch(this.NewPassword.Password, @"^.*(?=.{6,})(?=.*[a-zA-Z])(?=.*\d).*$"))
                {
                    if (this.ErrorMessage.Text != string.Empty)
                    {
                        this.ErrorMessage.Text += "\n";
                    }
                    this.ErrorMessage.Text += Labels.UserPanel24;
                }

                if (!this.NewPassword.Password.Equals(this.NewPassword2.Password))
                {
                    if (this.ErrorMessage.Text != string.Empty)
                    {
                        this.ErrorMessage.Text += "\n";
                    }
                    this.ErrorMessage.Text += Labels.UserPanel25;
                }

                if (string.IsNullOrEmpty(this.ErrorMessage.Text))
                {
                    this.loadinAnim.Visibility = System.Windows.Visibility.Visible;
                    var req = new EarthwatcherRequests(Constants.BaseApiUrl);
                    req.PasswordChanged += req_PasswordChanged;
                    Earthwatcher ew = new Earthwatcher { Name = Current.Instance.Earthwatcher.Name, Password = this.NewPassword.Password };
                    req.ChangePassword(ew);
                }
                else
                {
                    this.OkButton.IsEnabled = true;
                }
            }

            if (step == "ChangeRegional")
            {
                var username = Current.Instance.Username;
                var name = Current.Instance.Earthwatcher.Name;

                var ew = this.DataContext as Earthwatcher;
                // PARA CAMBIAR EL MAIL
                Current.Instance.Earthwatcher.MailChanged = true;
                if (Current.Instance.Username.Equals(ew.Name)) //Si no cambio el mail
                {
                    ew.MailChanged = false;
                    Current.Instance.Earthwatcher.MailChanged = false;
                }
                else if (!Regex.IsMatch(this.MailTextBox.Text, "^([a-zA-Z0-9_\\.\\-])+\\@(([a-zA-Z0-9\\-])+\\.)+([a-zA-Z0-9]{2,4})+$")) //Si no es un formato de mail valido
                {
                    this.ErrorMessage.Text = "Ingrese un mail valido";
                    Current.Instance.Earthwatcher.Name = Current.Instance.Username;
                }

                if (ew == null)
                {
                    this.ErrorMessage.Text = Labels.UserPanel26;
                }

                if (!Regex.IsMatch(this.NickNameTextBox.Text, "^[0-9a-zA-Z ]{0,20}$"))
                {
                    if (this.ErrorMessage.Text != string.Empty)
                    {
                        this.ErrorMessage.Text += "\n";
                    }
                    this.ErrorMessage.Text += Labels.UserPanel27;
                }

                if (string.IsNullOrEmpty(this.ErrorMessage.Text))
                {
                    this.loadinAnim.Visibility = System.Windows.Visibility.Visible;
                    var req = new EarthwatcherRequests(Constants.BaseApiUrl);
                    req.EarthwatcherUpdated += req_EarthwatcherUpdated;
                    req.Update(ew);
                }
                else
                {
                    this.OkButton.IsEnabled = true;
                }
            }
        }
        private void BtnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        //Cambiar Bandera pais //REGION CAMBIAR ESTO PARA QUE PONGA EL ID DEL COUNTRY, ASI ENCUENTRA LA BANDERA (OJO CON LOS DEMAS PAISES)
        private void CountriesCombo_ChangeImage(object sender, SelectionChangedEventArgs e)
        {
            var selection = sender as ComboBox;
            var country = ((Country)selection.SelectedItem);
            var countryCode = country.Code;
            if(countryCode != null)
            {
                this.selectedCountryImg.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap(string.Format("Resources/Images/CountriesFlags/{0}.png", countryCode.ToLower()));
            }
        }

        //Handlers
        void req_EarthwatcherUpdated(object sender, EventArgs e)
        {
            this.OkButton.IsEnabled = true;
            this.loadinAnim.Visibility = System.Windows.Visibility.Collapsed;
            if (sender is System.Net.HttpStatusCode)
            {
                if (((System.Net.HttpStatusCode)sender) == System.Net.HttpStatusCode.OK)
                {
                    this.ErrorMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 105, 125, 0));
                    this.ErrorMessage.Text = Labels.UserPanel28;

                    if (Current.Instance.Earthwatcher.MailChanged == true)  
                    {
                        App.Logout();
                    }

                    #region Chequeo para ver si cambió el idioma (comentado)
                    
                    string newCulture = string.Empty;
                 
                    newCulture = Current.Instance.Earthwatcher.Language;
                    if (newCulture != string.Empty)
                    {
                        System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(newCulture);
                        System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(newCulture);
                        //((CustomLabels)App.Current.Resources["CustomLabels"]).LocalizedLabels = new Labels();
                    }
                    #endregion
                }
                else if (((System.Net.HttpStatusCode)sender) == System.Net.HttpStatusCode.MultipleChoices) //Si ya existia el mail
                {
                    this.ErrorMessage.Text = Labels.UserPanel33;
                }
                else
                {
                    this.ErrorMessage.Text = Labels.UserPanel26;
                }
            }
        }
        void req_PasswordChanged(object sender, EventArgs e)
        {
            this.OkButton.IsEnabled = true;
            this.loadinAnim.Visibility = System.Windows.Visibility.Collapsed;

            if (sender != null)
            {
                this.ErrorMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 105, 125, 0));
                this.ErrorMessage.Text = Labels.UserPanel30;
                Current.Instance.Password = this.NewPassword.Password;
                this.CurrentPassword.Password = string.Empty;
                this.NewPassword.Password = string.Empty;
                this.NewPassword2.Password = string.Empty;
            }
            else
            {
                this.ErrorMessage.Text = Labels.UserPanel31;
            }
        }
        void collectionRequests_ItemsReceived(object sender, EventArgs e)
        {
            //Genero la Grid con los items de la colección
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(typeof(Earthwatchers.UI.Resources.Labels));

            //hide loading animation
            this.loading.Visibility = Visibility.Collapsed;

            List<CollectionItem> items = sender as List<CollectionItem>;
            if (items != null)
            {
                string colName = string.Empty;
                int rows = 0;
                int column = 0;
                foreach (var item in items)
                {
                    if (item.CollectionName != colName)
                    {
                        rows++;
                        column = 0;
                        TextBlock colNameTb = new TextBlock { Text = rm.GetString(item.CollectionName), FontSize = 12, Foreground = new SolidColorBrush(Color.FromArgb(255, 25, 25, 25)), TextWrapping = TextWrapping.Wrap, VerticalAlignment = System.Windows.VerticalAlignment.Center };
                        Grid.SetRow(colNameTb, rows);
                        this.CollectionsGrid.Children.Add(colNameTb);

                        if (rows < 9)
                        {
                            Border border = new Border { Height = 1, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Color.FromArgb(255, 159, 186, 14)) };
                            rows++;
                            Grid.SetRow(border, rows);
                            Grid.SetColumnSpan(border, 6);
                            this.CollectionsGrid.Children.Add(border);
                        }
                    }
                    column++;

                    //Agrego Imagen
                    Image image = new Image { Stretch = System.Windows.Media.Stretch.None };
                    ToolTipService.SetToolTip(image, rm.GetString(item.Name));
                    Grid.SetRow(image, rows < 9 ? rows - 1 : rows);
                    Grid.SetColumn(image, column);
                    image.Source = ResourceHelper.GetBitmap(string.Format("/Resources/Images/Collections/{0}", item.Icon));
                    if (!item.HasItem)
                    {
                        image.Opacity = 0.2;
                    }
                    this.CollectionsGrid.Children.Add(image);

                    colName = item.CollectionName;
                }
            }
        }

        //Methods
        private void LoadCountriesAndLanguagesCombo()
        {
            #region Languages Combo
            this.Idiomas = new List<Language>();
            this.Idiomas.Add(new Language { Name = "en-CA", LocalizedName = "ENGLISH" });
            this.Idiomas.Add(new Language { Name = "es-AR", LocalizedName = "ESPAÑOL" });
            this.Idiomas.Add(new Language { Name = "zh-CN", LocalizedName = "中国" });
            this.LanguagesCombo.ItemsSource = this.Idiomas;
            #endregion

            #region Countries Combo 
            this.Paises = new List<Country>(); //CODIGOS DE PAISES
            this.Paises.Add(new Country { Code = "AF", Name = "AFGHANISTAN" });
            this.Paises.Add(new Country { Code = "AL", Name = "ALBANIA" });
            this.Paises.Add(new Country { Code = "DZ", Name = "ALGERIA" });
            this.Paises.Add(new Country { Code = "AS", Name = "AMERICAN SAMOA" });
            this.Paises.Add(new Country { Code = "AD", Name = "ANDORRA" });
            this.Paises.Add(new Country { Code = "AO", Name = "ANGOLA" });
            this.Paises.Add(new Country { Code = "AI", Name = "ANGUILLA" });
            this.Paises.Add(new Country { Code = "AQ", Name = "ANTARCTICA" });
            this.Paises.Add(new Country { Code = "AG", Name = "ANTIGUA AND BARBUDA" });
            this.Paises.Add(new Country { Code = "AR", Name = "ARGENTINA" });
            this.Paises.Add(new Country { Code = "AM", Name = "ARMENIA" });
            this.Paises.Add(new Country { Code = "AW", Name = "ARUBA" });
            this.Paises.Add(new Country { Code = "AU", Name = "AUSTRALIA" });
            this.Paises.Add(new Country { Code = "AT", Name = "AUSTRIA" });
            this.Paises.Add(new Country { Code = "AZ", Name = "AZERBAIJAN" });
            this.Paises.Add(new Country { Code = "BS", Name = "BAHAMAS" });
            this.Paises.Add(new Country { Code = "BH", Name = "BAHRAIN" });
            this.Paises.Add(new Country { Code = "BD", Name = "BANGLADESH" });
            this.Paises.Add(new Country { Code = "BB", Name = "BARBADOS" });
            this.Paises.Add(new Country { Code = "BY", Name = "BELARUS" });
            this.Paises.Add(new Country { Code = "BE", Name = "BELGIUM" });
            this.Paises.Add(new Country { Code = "BZ", Name = "BELIZE" });
            this.Paises.Add(new Country { Code = "BJ", Name = "BENIN" });
            this.Paises.Add(new Country { Code = "BM", Name = "BERMUDA" });
            this.Paises.Add(new Country { Code = "BT", Name = "BHUTAN" });
            this.Paises.Add(new Country { Code = "BO", Name = "BOLIVIA" });
            this.Paises.Add(new Country { Code = "BA", Name = "BOSNIA AND HERZEGOWINA" });
            this.Paises.Add(new Country { Code = "BW", Name = "BOTSWANA" });
            this.Paises.Add(new Country { Code = "BV", Name = "BOUVET ISLAND (Norway)" });
            this.Paises.Add(new Country { Code = "BR", Name = "BRAZIL" });
            this.Paises.Add(new Country { Code = "IO", Name = "BRITISH INDIAN OCEAN TERRITORY" });
            this.Paises.Add(new Country { Code = "BN", Name = "BRUNEI DARUSSALAM" });
            this.Paises.Add(new Country { Code = "BG", Name = "BULGARIA" });
            this.Paises.Add(new Country { Code = "BF", Name = "BURKINA FASO" });
            this.Paises.Add(new Country { Code = "BI", Name = "BURUNDI" });
            this.Paises.Add(new Country { Code = "KH", Name = "CAMBODIA" });
            this.Paises.Add(new Country { Code = "CM", Name = "CAMEROON" });
            this.Paises.Add(new Country { Code = "CA", Name = "CANADA" });
            this.Paises.Add(new Country { Code = "CV", Name = "CAPE VERDE" });
            this.Paises.Add(new Country { Code = "KY", Name = "CAYMAN ISLANDS" });
            this.Paises.Add(new Country { Code = "CF", Name = "CENTRAL AFRICAN REPUBLIC" });
            this.Paises.Add(new Country { Code = "TD", Name = "CHAD" });
            this.Paises.Add(new Country { Code = "CL", Name = "CHILE" });
            this.Paises.Add(new Country { Code = "CN", Name = "CHINA" });
            this.Paises.Add(new Country { Code = "CX", Name = "CHRISTMAS ISLAND" });
            this.Paises.Add(new Country { Code = "CC", Name = "COCOS (KEELING) ISLANDS (Austrailia)" });
            this.Paises.Add(new Country { Code = "CO", Name = "COLOMBIA" });
            this.Paises.Add(new Country { Code = "KM", Name = "COMOROS" });
            this.Paises.Add(new Country { Code = "CG", Name = "CONGO" });
            this.Paises.Add(new Country { Code = "CD", Name = "CONGO, THE DRC" });
            this.Paises.Add(new Country { Code = "CK", Name = "COOK ISLANDS" });
            this.Paises.Add(new Country { Code = "CR", Name = "COSTA RICA" });
            this.Paises.Add(new Country { Code = "CI", Name = "COTE D'IVOIRE" });
            this.Paises.Add(new Country { Code = "HR", Name = "CROATIA (local name: Hrvatska)" });
            this.Paises.Add(new Country { Code = "CU", Name = "CUBA" });
            this.Paises.Add(new Country { Code = "CY", Name = "CYPRUS" });
            this.Paises.Add(new Country { Code = "CZ", Name = "CZECH REPUBLIC" });
            this.Paises.Add(new Country { Code = "DK", Name = "DENMARK" });
            this.Paises.Add(new Country { Code = "DJ", Name = "DJIBOUTI" });
            this.Paises.Add(new Country { Code = "DM", Name = "DOMINICA" });
            this.Paises.Add(new Country { Code = "DO", Name = "DOMINICAN REPUBLIC" });
            this.Paises.Add(new Country { Code = "TP", Name = "EAST TIMOR" });
            this.Paises.Add(new Country { Code = "EC", Name = "ECUADOR" });
            this.Paises.Add(new Country { Code = "EG", Name = "EGYPT" });
            this.Paises.Add(new Country { Code = "SV", Name = "EL SALVADOR" });
            this.Paises.Add(new Country { Code = "GQ", Name = "EQUATORIAL GUINEA" });
            this.Paises.Add(new Country { Code = "ER", Name = "ERITREA" });
            this.Paises.Add(new Country { Code = "EE", Name = "ESTONIA" });
            this.Paises.Add(new Country { Code = "ET", Name = "ETHIOPIA" });
            this.Paises.Add(new Country { Code = "FK", Name = "FALKLAND ISLANDS (MALVINAS)" });
            this.Paises.Add(new Country { Code = "FO", Name = "FAROE ISLANDS" });
            this.Paises.Add(new Country { Code = "FJ", Name = "FIJI" });
            this.Paises.Add(new Country { Code = "FI", Name = "FINLAND" });
            this.Paises.Add(new Country { Code = "FR", Name = "FRANCE" });
            this.Paises.Add(new Country { Code = "FX", Name = "FRANCE, METROPOLITAN" });
            this.Paises.Add(new Country { Code = "GF", Name = "FRENCH GUIANA" });
            this.Paises.Add(new Country { Code = "PF", Name = "FRENCH POLYNESIA" });
            this.Paises.Add(new Country { Code = "TF", Name = "FRENCH SOUTHERN TERRITORIES" });
            this.Paises.Add(new Country { Code = "GA", Name = "GABON" });
            this.Paises.Add(new Country { Code = "GM", Name = "GAMBIA" });
            this.Paises.Add(new Country { Code = "GE", Name = "GEORGIA" });
            this.Paises.Add(new Country { Code = "DE", Name = "GERMANY" });
            this.Paises.Add(new Country { Code = "GH", Name = "GHANA" });
            this.Paises.Add(new Country { Code = "GI", Name = "GIBRALTAR" });
            this.Paises.Add(new Country { Code = "GR", Name = "GREECE" });
            this.Paises.Add(new Country { Code = "GL", Name = "GREENLAND" });
            this.Paises.Add(new Country { Code = "GD", Name = "GRENADA" });
            this.Paises.Add(new Country { Code = "GP", Name = "GUADELOUPE" });
            this.Paises.Add(new Country { Code = "GU", Name = "GUAM" });
            this.Paises.Add(new Country { Code = "GT", Name = "GUATEMALA" });
            this.Paises.Add(new Country { Code = "GN", Name = "GUINEA" });
            this.Paises.Add(new Country { Code = "GW", Name = "GUINEA-BISSAU" });
            this.Paises.Add(new Country { Code = "GY", Name = "GUYANA" });
            this.Paises.Add(new Country { Code = "HT", Name = "HAITI" });
            this.Paises.Add(new Country { Code = "HM", Name = "HEARD AND MC DONALD ISLANDS" });
            this.Paises.Add(new Country { Code = "VA", Name = "HOLY SEE (VATICAN CITY STATE)" });
            this.Paises.Add(new Country { Code = "HN", Name = "HONDURAS" });
            this.Paises.Add(new Country { Code = "HK", Name = "HONG KONG" });
            this.Paises.Add(new Country { Code = "HU", Name = "HUNGARY" });
            this.Paises.Add(new Country { Code = "IS", Name = "ICELAND" });
            this.Paises.Add(new Country { Code = "IN", Name = "INDIA" });
            this.Paises.Add(new Country { Code = "ID", Name = "INDONESIA" });
            this.Paises.Add(new Country { Code = "IR", Name = "IRAN (ISLAMIC REPUBLIC OF)" });
            this.Paises.Add(new Country { Code = "IQ", Name = "IRAQ" });
            this.Paises.Add(new Country { Code = "IE", Name = "IRELAND" });
            this.Paises.Add(new Country { Code = "IL", Name = "ISRAEL" });
            this.Paises.Add(new Country { Code = "IT", Name = "ITALY" });
            this.Paises.Add(new Country { Code = "JM", Name = "JAMAICA" });
            this.Paises.Add(new Country { Code = "JP", Name = "JAPAN" });
            this.Paises.Add(new Country { Code = "JO", Name = "JORDAN" });
            this.Paises.Add(new Country { Code = "KZ", Name = "KAZAKHSTAN" });
            this.Paises.Add(new Country { Code = "KE", Name = "KENYA" });
            this.Paises.Add(new Country { Code = "KI", Name = "KIRIBATI" });
            this.Paises.Add(new Country { Code = "KP", Name = "KOREA, D.P.R.O." });
            this.Paises.Add(new Country { Code = "KR", Name = "KOREA, REPUBLIC OF" });
            this.Paises.Add(new Country { Code = "KW", Name = "KUWAIT" });
            this.Paises.Add(new Country { Code = "KG", Name = "KYRGYZSTAN" });
            this.Paises.Add(new Country { Code = "LA", Name = "LAOS" });
            this.Paises.Add(new Country { Code = "LV", Name = "LATVIA" });
            this.Paises.Add(new Country { Code = "LB", Name = "LEBANON" });
            this.Paises.Add(new Country { Code = "LS", Name = "LESOTHO" });
            this.Paises.Add(new Country { Code = "LR", Name = "LIBERIA" });
            this.Paises.Add(new Country { Code = "LY", Name = "LIBYAN ARAB JAMAHIRIYA" });
            this.Paises.Add(new Country { Code = "LI", Name = "LIECHTENSTEIN" });
            this.Paises.Add(new Country { Code = "LT", Name = "LITHUANIA" });
            this.Paises.Add(new Country { Code = "LU", Name = "LUXEMBOURG" });
            this.Paises.Add(new Country { Code = "MO", Name = "MACAU" });
            this.Paises.Add(new Country { Code = "MK", Name = "MACEDONIA" });
            this.Paises.Add(new Country { Code = "MG", Name = "MADAGASCAR" });
            this.Paises.Add(new Country { Code = "MW", Name = "MALAWI" });
            this.Paises.Add(new Country { Code = "MY", Name = "MALAYSIA" });
            this.Paises.Add(new Country { Code = "MV", Name = "MALDIVES" });
            this.Paises.Add(new Country { Code = "ML", Name = "MALI" });
            this.Paises.Add(new Country { Code = "MT", Name = "MALTA" });
            this.Paises.Add(new Country { Code = "MH", Name = "MARSHALL ISLANDS" });
            this.Paises.Add(new Country { Code = "MQ", Name = "MARTINIQUE" });
            this.Paises.Add(new Country { Code = "MR", Name = "MAURITANIA" });
            this.Paises.Add(new Country { Code = "MU", Name = "MAURITIUS" });
            this.Paises.Add(new Country { Code = "YT", Name = "MAYOTTE" });
            this.Paises.Add(new Country { Code = "MX", Name = "MEXICO" });
            this.Paises.Add(new Country { Code = "FM", Name = "MICRONESIA, FEDERATED STATES OF" });
            this.Paises.Add(new Country { Code = "MD", Name = "MOLDOVA, REPUBLIC OF" });
            this.Paises.Add(new Country { Code = "MC", Name = "MONACO" });
            this.Paises.Add(new Country { Code = "MN", Name = "MONGOLIA" });
            this.Paises.Add(new Country { Code = "ME", Name = "MONTENEGRO" });
            this.Paises.Add(new Country { Code = "MS", Name = "MONTSERRAT" });
            this.Paises.Add(new Country { Code = "MA", Name = "MOROCCO" });
            this.Paises.Add(new Country { Code = "MZ", Name = "MOZAMBIQUE" });
            this.Paises.Add(new Country { Code = "MM", Name = "MYANMAR (Burma)" });
            this.Paises.Add(new Country { Code = "NA", Name = "NAMIBIA" });
            this.Paises.Add(new Country { Code = "NR", Name = "NAURU" });
            this.Paises.Add(new Country { Code = "NP", Name = "NEPAL" });
            this.Paises.Add(new Country { Code = "NL", Name = "NETHERLANDS" });
            this.Paises.Add(new Country { Code = "AN", Name = "NETHERLANDS ANTILLES" });
            this.Paises.Add(new Country { Code = "NC", Name = "NEW CALEDONIA" });
            this.Paises.Add(new Country { Code = "NZ", Name = "NEW ZEALAND" });
            this.Paises.Add(new Country { Code = "NI", Name = "NICARAGUA" });
            this.Paises.Add(new Country { Code = "NE", Name = "NIGER" });
            this.Paises.Add(new Country { Code = "NG", Name = "NIGERIA" });
            this.Paises.Add(new Country { Code = "NU", Name = "NIUE" });
            this.Paises.Add(new Country { Code = "NF", Name = "NORFOLK ISLAND" });
            this.Paises.Add(new Country { Code = "MP", Name = "NORTHERN MARIANA ISLANDS" });
            this.Paises.Add(new Country { Code = "NO", Name = "NORWAY" });
            this.Paises.Add(new Country { Code = "OM", Name = "OMAN" });
            this.Paises.Add(new Country { Code = "PK", Name = "PAKISTAN" });
            this.Paises.Add(new Country { Code = "PW", Name = "PALAU" });
            this.Paises.Add(new Country { Code = "PA", Name = "PANAMA" });
            this.Paises.Add(new Country { Code = "PG", Name = "PAPUA NEW GUINEA" });
            this.Paises.Add(new Country { Code = "PY", Name = "PARAGUAY" });
            this.Paises.Add(new Country { Code = "PE", Name = "PERU" });
            this.Paises.Add(new Country { Code = "PH", Name = "PHILIPPINES" });
            this.Paises.Add(new Country { Code = "PN", Name = "PITCAIRN" });
            this.Paises.Add(new Country { Code = "PL", Name = "POLAND" });
            this.Paises.Add(new Country { Code = "PT", Name = "PORTUGAL" });
            this.Paises.Add(new Country { Code = "PR", Name = "PUERTO RICO" });
            this.Paises.Add(new Country { Code = "QA", Name = "QATAR" });
            this.Paises.Add(new Country { Code = "RE", Name = "REUNION" });
            this.Paises.Add(new Country { Code = "RO", Name = "ROMANIA" });
            this.Paises.Add(new Country { Code = "RU", Name = "RUSSIAN FEDERATION" });
            this.Paises.Add(new Country { Code = "RW", Name = "RWANDA" });
            this.Paises.Add(new Country { Code = "KN", Name = "SAINT KITTS AND NEVIS" });
            this.Paises.Add(new Country { Code = "LC", Name = "SAINT LUCIA" });
            this.Paises.Add(new Country { Code = "VC", Name = "SAINT VINCENT AND THE GRENADINES" });
            this.Paises.Add(new Country { Code = "WS", Name = "SAMOA" });
            this.Paises.Add(new Country { Code = "SM", Name = "SAN MARINO" });
            this.Paises.Add(new Country { Code = "ST", Name = "SAO TOME AND PRINCIPE" });
            this.Paises.Add(new Country { Code = "SA", Name = "SAUDI ARABIA" });
            this.Paises.Add(new Country { Code = "SN", Name = "SENEGAL" });
            this.Paises.Add(new Country { Code = "RS", Name = "SERBIA" });
            this.Paises.Add(new Country { Code = "SC", Name = "SEYCHELLES" });
            this.Paises.Add(new Country { Code = "SL", Name = "SIERRA LEONE" });
            this.Paises.Add(new Country { Code = "SG", Name = "SINGAPORE" });
            this.Paises.Add(new Country { Code = "SK", Name = "SLOVAKIA (Slovak Republic)" });
            this.Paises.Add(new Country { Code = "SI", Name = "SLOVENIA" });
            this.Paises.Add(new Country { Code = "SB", Name = "SOLOMON ISLANDS" });
            this.Paises.Add(new Country { Code = "SO", Name = "SOMALIA" });
            this.Paises.Add(new Country { Code = "ZA", Name = "SOUTH AFRICA" });
            this.Paises.Add(new Country { Code = "SS", Name = "SOUTH SUDAN" });
            this.Paises.Add(new Country { Code = "GS", Name = "SOUTH GEORGIA AND SOUTH S.S." });
            this.Paises.Add(new Country { Code = "ES", Name = "SPAIN" });
            this.Paises.Add(new Country { Code = "LK", Name = "SRI LANKA" });
            this.Paises.Add(new Country { Code = "SH", Name = "ST. HELENA" });
            this.Paises.Add(new Country { Code = "PM", Name = "ST. PIERRE AND MIQUELON" });
            this.Paises.Add(new Country { Code = "SD", Name = "SUDAN" });
            this.Paises.Add(new Country { Code = "SR", Name = "SURINAME" });
            this.Paises.Add(new Country { Code = "SJ", Name = "SVALBARD AND JAN MAYEN ISLANDS" });
            this.Paises.Add(new Country { Code = "SZ", Name = "SWAZILAND" });
            this.Paises.Add(new Country { Code = "SE", Name = "SWEDEN" });
            this.Paises.Add(new Country { Code = "CH", Name = "SWITZERLAND" });
            this.Paises.Add(new Country { Code = "SY", Name = "SYRIAN ARAB REPUBLIC" });
            this.Paises.Add(new Country { Code = "TW", Name = "TAIWAN, PROVINCE OF CHINA" });
            this.Paises.Add(new Country { Code = "TJ", Name = "TAJIKISTAN" });
            this.Paises.Add(new Country { Code = "TZ", Name = "TANZANIA, UNITED REPUBLIC OF" });
            this.Paises.Add(new Country { Code = "TH", Name = "THAILAND" });
            this.Paises.Add(new Country { Code = "TG", Name = "TOGO" });
            this.Paises.Add(new Country { Code = "TK", Name = "TOKELAU" });
            this.Paises.Add(new Country { Code = "TO", Name = "TONGA" });
            this.Paises.Add(new Country { Code = "TT", Name = "TRINIDAD AND TOBAGO" });
            this.Paises.Add(new Country { Code = "TN", Name = "TUNISIA" });
            this.Paises.Add(new Country { Code = "TR", Name = "TURKEY" });
            this.Paises.Add(new Country { Code = "TM", Name = "TURKMENISTAN" });
            this.Paises.Add(new Country { Code = "TC", Name = "TURKS AND CAICOS ISLANDS" });
            this.Paises.Add(new Country { Code = "TV", Name = "TUVALU" });
            this.Paises.Add(new Country { Code = "UG", Name = "UGANDA" });
            this.Paises.Add(new Country { Code = "UA", Name = "UKRAINE" });
            this.Paises.Add(new Country { Code = "AE", Name = "UNITED ARAB EMIRATES" });
            this.Paises.Add(new Country { Code = "GB", Name = "UNITED KINGDOM" });
            this.Paises.Add(new Country { Code = "US", Name = "UNITED STATES" });
            this.Paises.Add(new Country { Code = "UM", Name = "U.S. MINOR ISLANDS" });
            this.Paises.Add(new Country { Code = "UY", Name = "URUGUAY" });
            this.Paises.Add(new Country { Code = "UZ", Name = "UZBEKISTAN" });
            this.Paises.Add(new Country { Code = "VU", Name = "VANUATU" });
            this.Paises.Add(new Country { Code = "VE", Name = "VENEZUELA" });
            this.Paises.Add(new Country { Code = "VN", Name = "VIETNAM" });
            this.Paises.Add(new Country { Code = "VG", Name = "VIRGIN ISLANDS (BRITISH)" });
            this.Paises.Add(new Country { Code = "VI", Name = "VIRGIN ISLANDS (U.S.)" });
            this.Paises.Add(new Country { Code = "WF", Name = "WALLIS AND FUTUNA ISLANDS" });
            this.Paises.Add(new Country { Code = "EH", Name = "WESTERN SAHARA" });
            this.Paises.Add(new Country { Code = "YE", Name = "YEMEN" });
            this.Paises.Add(new Country { Code = "ZM", Name = "ZAMBIA" });
            this.Paises.Add(new Country { Code = "ZW", Name = "ZIMBABWE" });

            this.CountriesCombo.ItemsSource = this.Paises;

            #endregion
        }
        private void AddBadges()
        {
            //PowerUser
            if (Current.Instance.Earthwatcher.IsPowerUser)
            {
                this.badgeIcon.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/badgej.png");
                ToolTipService.SetToolTip(this.badgeIcon, Labels.Jaguar);
            }
            //ContestWinner
            if (Current.Instance.RegionScores.Any(x => x.Action.StartsWith(ActionPoints.Action.ContestWon.ToString())))
            {
                this.ContestWinnerBadge.Visibility = Visibility.Visible;
            }
            //Jaguar
            if (numberOfJaguarsFounded != 0)
            {
                this.jaguarbadge.Visibility = Visibility.Visible;
            }
        }
        private void ManageJaguarWindow()
        {
            if (numberOfJaguarsFounded != 0)
            {
                this.JaguarTitle.Text = string.Format(Labels.UserPanel20, numberOfJaguarsFounded);
            }
            else
                this.JaguarTitle.Text = Labels.UserPanel21;

            //Add Jaguar Images to window
            int row = 0;
            int column = 0;
            for (int i = 0; i < numberOfJaguarsFounded; i++)
            {
                Image image = new Image();
                image.Source = Earthwatchers.UI.Resources.ResourceHelper.GetBitmap("/Resources/Images/jaguar.png");
                image.Width = 40;
                Grid.SetColumn(image, column);
                Grid.SetRow(image, row);
                this.JaguarsImagesGrid.Children.Add(image);

                column++;
                if (column >= 8)
                {
                    row++;
                    column = 0;
                }
            }
        }
    }

    //Helper Clases
    public class Language
    {
        public string Name { get; set; }
        public string LocalizedName { get; set; }
    }
    public class Country
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }


}

