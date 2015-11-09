using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Earthwatchers.Services.Localization;


namespace Earthwatchers.Services
{
    public partial class login : System.Web.UI.Page
    {
        private LocalizationService _localizationService;

        public login()
        {
            _localizationService = new LocalizationService();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (_localizationService.IsLocaleSetted())
                {
                    this.Locale = _localizationService.GetLocale();
                }
                else if (this.Request.Params.AllKeys.Contains("guardianlocale"))
                {
                    this.Locale = this.Request.Params["guardianlocale"];
                    _localizationService.SetLocale(this.Locale);
                }
                else
                {
                    Response.Redirect("global.aspx");
                }

                if (this.Locale != null)
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(this.Locale.Substring(0,5));
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(this.Locale.Substring(0, 5));
                }

            }
        }

        public string Locale { get; set; }
        
    }
}