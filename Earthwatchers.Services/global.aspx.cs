using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Earthwatchers.Services.Localization;

namespace Earthwatchers.Services
{
    public partial class global : System.Web.UI.Page
    {
        private LocalizationService _localizationService;

        public global()
        {
            _localizationService = new LocalizationService();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if(_localizationService.IsLocaleSetted())
            {
                Response.Redirect("login.aspx");
            }
            else
            {
                _localizationService.AutoDetectCulture();
            }
        }
    }
}