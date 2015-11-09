using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Earthwatchers.Services.Localization
{
    public class LocalizationService
    {
        public static string localeKey = "guardianlocale";

        public string GetLocale()
        {
            if (IsLocaleSetted())
            {
                HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies[LocalizationService.localeKey];
                return cookie.Value;
            }

            return null;
        }

        public string GetShortLocale()
        {
            if (IsLocaleSetted())
            {
                HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies[LocalizationService.localeKey];
                return cookie.Value.Substring(0,5);
            }

            return null;
        }

        public string GetCountryCode()
        {
            if (IsLocaleSetted())
            {
                HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies[LocalizationService.localeKey];
                return cookie.Value.Substring(3,2);
            }

            return null;
        }

        public int GetRegionId()
        {
            if (IsLocaleSetted())
            {
                HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies[LocalizationService.localeKey];
                return Convert.ToInt32(cookie.Value.Substring(6,1));
            }

            return 0;
        }

        public string GetLanguageCode()
        {
            if (IsLocaleSetted())
            {
                HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies[LocalizationService.localeKey];
                return cookie.Value.Substring(0, 2);
            }

            return null;
        }

        

        public bool IsLocaleSetted()
        {
            HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies[LocalizationService.localeKey];
            if (cookie != null)
            {
                return true;
            }

            return false;
        }

        public void SetLocale(string locale)
        {
            HttpCookie cookie = new HttpCookie(LocalizationService.localeKey);
            cookie.Value = locale;
            System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public void AutoDetectCulture()
        {
            CultureInfo cul;
            try
            {
                //DB: autodetect user preference by reading browser languages.
                cul = CultureInfo.CreateSpecificCulture(System.Web.HttpContext.Current.Request.UserLanguages[0]);
            }
            catch
            {
                //DB: if fails, defaults to this culture
                cul = CultureInfo.CreateSpecificCulture("es-AR");
            }

            //TODO: this settig should change post-login according to the language selected by user.
            System.Threading.Thread.CurrentThread.CurrentUICulture = cul;

            //TODO: this settig should change post-login according to the playing country of the user.
            System.Threading.Thread.CurrentThread.CurrentCulture = cul;
        }
    }
}