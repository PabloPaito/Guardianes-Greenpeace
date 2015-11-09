<%@ Page Language="C#" AutoEventWireup="true" %>

<%@ Import Namespace="Resources" %>

<script runat="server">  
    private string initParams = string.Empty;
    private string hideHex = System.Configuration.ConfigurationManager.AppSettings.Get("Hexagons.HideByZoom").ToString();
    void Page_Load(object sender, EventArgs e)
    {

        var socialApi = ((System.Web.HttpApplication)(((ASP.home_aspx)(sender)).ApplicationInstance)).Request.Params["social"];
        var mail = "";
        var userId = "";
        var accessToken = "";
        var nickName = "";

        if (socialApi == "Facebook")
        {
            mail = ((System.Web.HttpApplication)(((ASP.home_aspx)(sender)).ApplicationInstance)).Request.Params["mail"];
            userId = ((System.Web.HttpApplication)(((ASP.home_aspx)(sender)).ApplicationInstance)).Request.Params["userId"];
            accessToken = ((System.Web.HttpApplication)(((ASP.home_aspx)(sender)).ApplicationInstance)).Request.Params["accessToken"];
            nickName = ((System.Web.HttpApplication)(((ASP.home_aspx)(sender)).ApplicationInstance)).Request.Params["nickName"];
        }

        if (socialApi == "Twitter")
        {
            Earthwatchers.Services.OAuthHelper.GenerateOauthCookie(socialApi);

            Earthwatchers.Services.OAuthHelper oauthhelper = new Earthwatchers.Services.OAuthHelper("Twitter");
            string requestToken = oauthhelper.GetRequestToken();

            if (string.IsNullOrEmpty(oauthhelper.oauth_error))
                Response.Redirect(oauthhelper.GetAuthorizeUrl(requestToken));
            else
                Response.Write(oauthhelper.oauth_error);
        }

        if (socialApi == "Taringa")
        {
            Earthwatchers.Services.OAuthHelper.GenerateOauthCookie(socialApi);

            Earthwatchers.Services.OAuthHelper oauthhelper = new Earthwatchers.Services.OAuthHelper("Taringa");

            //descomentar la linea de abajo para probar el shout desde el "login" del index con mis credenciales
            //oauthhelper.PostTaringaShout("", "", "Posting from guardianesl"); //"631669", "a1eYMSUcXVDB3.olxEk_G7d5s-SyxUbyJfkjX4lI", "Posting from guardianesl"

            string requestToken = oauthhelper.GetRequestToken();

            if (string.IsNullOrEmpty(oauthhelper.oauth_error))
            {
                //WebBrowser popup = new WebBrowser();
                //popup.Navigate(authorize);
                Response.Redirect(oauthhelper.GetAuthorizeUrl(requestToken));
            }

            else
                Response.Write(oauthhelper.oauth_error);
        }

        //Callback de twitter y taringa luego del oauth_authorize, para obtener el AccessToken
        if (Request.QueryString["oauth_token"] != null && Request.QueryString["oauth_verifier"] != null)
        {
            var OauthCookieApi = Earthwatchers.Services.OAuthHelper.GetCookieInfo();

            string oauth_token = Request.QueryString["oauth_token"];
            string oauth_verifier = Request.QueryString["oauth_verifier"];

            Earthwatchers.Services.OAuthHelper oauthhelper = new Earthwatchers.Services.OAuthHelper(OauthCookieApi);
            if (OauthCookieApi == "Twitter")
            {
                oauthhelper.GetUserTwAccessToken(oauth_token, oauth_verifier);

                if (string.IsNullOrEmpty(oauthhelper.oauth_error))
                {
                    Session["twtoken"] = oauthhelper.oauth_access_token;
                    Session["twsecret"] = oauthhelper.oauth_access_token_secret;
                    Session["twuserid"] = oauthhelper.user_id;
                    Session["twname"] = oauthhelper.screen_name;
                }
            }

            else if (OauthCookieApi == "Taringa")
            {
                oauthhelper.GetUserTaringaAccessToken(oauth_token, oauth_verifier);
                if (string.IsNullOrEmpty(oauthhelper.oauth_error))
                {
                    //si de verdad quiero hacer el shout
                    //oauthhelper.PostTaringaShout(oauthhelper.oauth_access_token, oauthhelper.oauth_access_token_secret, "Posting from guardianes");  //Si habia venido a postear ejecuto el post

                    oauthhelper.GetTaringaUserData(oauthhelper.oauth_access_token, oauthhelper.oauth_access_token_secret);
                    if (string.IsNullOrEmpty(oauthhelper.oauth_error))
                    {
                        Session["tartoken"] = oauthhelper.oauth_access_token;
                        Session["tarsecret"] = oauthhelper.oauth_access_token_secret;
                        Session["taruserid"] = oauthhelper.user_id;
                        Session["tarname"] = oauthhelper.screen_name;
                    }
                }
                else if (oauthhelper.oauth_error.IndexOf("status=401") != -1) //Si falla la autenticacion, vuelve a realizar el circuito hasta entrar. (queda el loading)
                {
                    Earthwatchers.Services.OAuthHelper oauthhelper2 = new Earthwatchers.Services.OAuthHelper("Taringa");
                    string requestToken = oauthhelper2.GetRequestToken();
                    if (string.IsNullOrEmpty(oauthhelper2.oauth_error))
                    {
                        Response.Redirect(oauthhelper2.GetAuthorizeUrl(requestToken));
                    }

                    else
                        Response.Write(oauthhelper2.oauth_error);
                }
            }
            else
                Response.Write(oauthhelper.oauth_error);
        }



        if (!string.IsNullOrEmpty(Request["username"]))
        {
            initParams = ("credentials=" + Request["username"] + ":" + Request["password"] + ":" + Request["geohexcode"] + ":" + Request["country"]);
        }
        //else if (!string.IsNullOrEmpty(Request["authtoken"]))
        //{
        //    initParams = ("authtoken=" + Request["authtoken"]);
        //}
        else if (socialApi == "Facebook")
        {
            initParams = ("api=" + "Facebook" + ",userId=" + userId + ",accessToken=" + accessToken + ",mail=" + mail + ",nickName=" + nickName);
        }
        else if (Session["twuserid"] != null)
        {
            initParams = ("api=" + "Twitter" + ",userId=" + Session["twuserid"] + ",accessToken=" + Session["twtoken"] + ",nickName=" + Session["twname"]);
        }
        else if (Session["taruserid"] != null)
        {
            initParams = ("api=" + "Taringa" + ",userId=" + Session["taruserid"] + ",accessToken=" + Session["tartoken"] + ",secretToken=" + Session["tarsecret"] + ",nickName=" + Session["tarname"]);
        }

        else
        {
            Response.Redirect("/login.aspx", true);
        }
    }
        
</script>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Guardianes - Greenpeace</title>
    <link href="css/ew.css" rel="stylesheet" />
    <link rel="icon" href="/favicon.ico" />
    <link rel="shortcut icon" href="/favicon.ico" />
    <script src="Scripts/FacebookGraphApi.js"></script>
    <style type="text/css">
        html, body {
            height: 100%;
            /*overflow: hidden;*/
        }

        body {
            padding: 0;
            margin: 0;
        }

        #silverlightControlHost {
            height: 100%;
            text-align: center;
        }

        li {
            list-style-type: decimal;
        }
    </style>
    <script type="text/javascript" src="Silverlight.js"></script>
    <script src="Scripts/jquery-1.6.4.js"></script>
    <script type="text/javascript">
        function onSilverlightError(sender, args) {
            var appSource = "";
            if (sender != null && sender != 0) {
                appSource = sender.getHost().Source;
            }

            var errorType = args.ErrorType;
            var iErrorCode = args.ErrorCode;

            if (errorType == "ImageError" || errorType == "MediaError") {
                return;
            }

            var errMsg = "Unhandled Error in Silverlight Application " + appSource + "\n";

            errMsg += "Code: " + iErrorCode + "    \n";
            errMsg += "Category: " + errorType + "       \n";
            errMsg += "Message: " + args.ErrorMessage + "     \n";

            if (errorType == "ParserError") {
                errMsg += "File: " + args.xamlFile + "     \n";
                errMsg += "Line: " + args.lineNumber + "     \n";
                errMsg += "Position: " + args.charPosition + "     \n";
            }
            else if (errorType == "RuntimeError") {
                if (args.lineNumber != 0) {
                    errMsg += "Line: " + args.lineNumber + "     \n";
                    errMsg += "Position: " + args.charPosition + "     \n";
                }
                errMsg += "MethodName: " + args.methodName + "     \n";
            }

            alert(errMsg);
        }
    </script>





</head>
<body scroll="no">

    <script>
        (function (i, s, o, g, r, a, m) {
            i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () {
                (i[r].q = i[r].q || []).push(arguments)
            }, i[r].l = 1 * new Date(); a = s.createElement(o),
            m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)
        })(window, document, 'script', '//www.google-analytics.com/analytics.js', 'ga');

        ga('create', 'UA-47414806-1', 'auto');
        ga('send', 'pageview');

    </script>
    <form id="form1" runat="server" style="height: 100%">
        <div id="silverlightControlHost">
            <object id="SilverlightPlugin" data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="100%" height="100%">
                <param name="source" value="ClientBin/Earthwatchers.UI.xap?v=2.7.0.0" />
                <%--VERSION SILVERLIGHT XAP--%>
                <param name="onError" value="onSilverlightError" />
                <param name="background" value="white" />
                <param name="minRuntimeVersion" value="5.0.61118.0" />
                <param name="autoUpgrade" value="true" />
                <param name="hideHexagon" value="<% System.Configuration.ConfigurationManager.AppSettings.Get("Hexagons.HideByZoom"); %>" />
                <param name="initParams" value="<%=initParams %>" />
                <asp:Literal ID="ParamInitParams" runat="server"></asp:Literal>
                <div style="position: absolute; top: 5%; width: 100%; height: 1px; overflow: visible; display: block;">
                    <%--class="floater" --%>
                    <div style="position: relative; text-align: center; background-image: url('../images/hexagons.png');">
                        <%--class="contents" --%>
                        <div>
                            <img src="Images/logo.png" />
                        </div>
                        <div style="margin-top: 15px">
                            <img src="Images/guardianes.png" />
                        </div>
                        <div style="margin: auto; margin-top: 40px; margin-bottom: 10px; width: 700px; background: #F3F3F3; border: 1px solid #fff; border-radius: 5px; box-shadow: 0 1px 3px rgba(0,0,0,0.5); -moz-box-shadow: 0 1px 3px rgba(0,0,0,0.5); -webkit-box-shadow: 0 1px 3px rgba(0,0,0,0.5);">
                            <%--class="container"--%>
                            <div id="logindiv">

                                <div id="cartel1" class="content">
                                    <div class="slHeader">
                                        <%=R.Cartel1t11 %>
                                    </div>
                                    <p>
                                        <%=R.Cartel1t2 %> <a href="http://educeo.geodan.nl"><%=R.Cartel1t3 %></a>. 
               <%=R.Cartel1t4 %> <a href="https://guardianes.typeform.com/to/By4axg"><%=R.Cartel1t5 %></a> <%=R.Cartel1t6 %> <a href="mailto:guardianes.ar@greenpeace.org">guardianes.ar@greenpeace.org</a><%=R.Cartel1t7 %>
                                    </p>
                                    <p><%=R.Cartel1t8 %></p>
                                    <div class="slText"><%=R.Cartel1t9 %></div>
                                    <div style="margin-top: 30px">
                                        <a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=5.0.61118.0" style="text-decoration: none" class="button" onclick="logSlInstall()"><%=R.Cartel1t10 %></a>
                                    </div>
                                    <div style="margin-top: 30px">
                                        <a class="slText" href="http://www.greenpeace.org.ar/denuncias/index.php?id=0" target="_blank" style="text-decoration: none"><%=R.Cartel1t12 %></a>
                                    </div>
                                </div>
                                <div id="cartel2" class="content">
                                    <div style="font-size: 14px; color: #888888; padding: 20px; text-align: left;">
                                        <h2 style="text-align: center;"><%=R.Cartel1t1 %></h2>
                                        <p>
                                            <%=R.Cartel2t2 %> <a href="https://www.mozilla.org/es-AR/firefox/new/">FireFox</a> o <a href="http://windows.microsoft.com/es-ar/windows/home">Internet Explorer</a>.
                                        </p>
                                        <h3 style="color: #268026; font-size: 15px;"><%=R.Cartel2t3 %><a href="http://educeo.geodan.nl">Guardianes 3.0</a></h3>
                                        <p><%=R.Cartel2t4 %></p>

                                        <p><%=R.Cartel2t5 %> <a href="https://guardianes.typeform.com/to/By4axg"><%=R.Cartel1t5 %></a> <%=R.Cartel1t6 %><a href="mailto:guardianes.ar@greenpeace.org">guardianes.ar@greenpeace.org</a><%=R.Cartel2t6 %></p>

                                        <p><%=R.Cartel1t8 %></p>
                                        <div id="cartel3">
                                            <p><b><%=R.Cartel3t1 %></b></p>
                                            <ul>
                                                <li><%=R.Cartel3t2 %></li>
                                                <li><%=R.Cartel3t3 %></li>
                                                <li><%=R.Cartel3t4 %></li>
                                                <li><%=R.Cartel3t5 %></li>
                                            </ul>
                                            <img src="Images/steps.png" alt="Pasos a seguir" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
        </div>
        <%--</object>--%>
            <iframe id="_sl_historyFrame" style="visibility: hidden; height: 0px; width: 0px; border: 0px"></iframe>
        </form>

        <script type="text/javascript">

            $(document).ready(function () {
                var objAgent = navigator.userAgent;
                var objfullVersion = 0;
                var objBrMajorVersion = 0;
                var objOffsetName, objOffsetVersion, ix;

                //If MobileOrTablet
                if (window.mobileAndTabletcheck()) {
                    window.location.href = "http://educeo.geodan.nl";
                }
                    //If Chrome
                else if ((objOffsetVersion = objAgent.indexOf("Chrome")) != -1) {
                    objfullVersion = objAgent.substring(objOffsetVersion + 7);
                    objBrMajorVersion = objfullVersion.substring(0, 2)
                    //Version +45
                    if (objBrMajorVersion >= 45) {
                        $('#cartel1').hide();
                        $('#cartel2').show();
                        $('#cartel3').hide();
                    }
                    else {
                        $('#cartel1').hide();
                        $('#cartel2').show();
                        $('#cartel3').show();
                    }
                }
                else {
                    $('#cartel1').show();
                    $('#cartel2').hide();
                    $('#cartel3').hide();
                }
            });

            window.mobileAndTabletcheck = function () {
                var check = false;
                (function (a) { if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino|android|ipad|playbook|silk/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4))) check = true })(navigator.userAgent || navigator.vendor || window.opera);
                return check;
            }

            function logSlInstall() //Loguear install de Silverlight
            {

                $.ajax({
                    type: 'POST',
                    url: "../api/earthwatchers/logSlInstall",
                    success: function () {
                    },
                    error: function () {
                    }
                });
            }

            function shorten(url) {
                var access_token = 'c31a6068bb170d6b9c50ac625c59e25888abb0e0';
                var api_req = 'https://api-ssl.bitly.com/v3/shorten?access_token=' + access_token + '&longUrl=' + encodeURIComponent(url);
                var res = null;

                $.ajax({
                    dataType: "json",
                    async: false,
                    url: api_req,
                    success: function (result) {
                        res = result.data.url;
                    }
                });

                return res;
            }

            var tries = 1;
            function postInFacebook(post) {
                if (fb != null && fb.logged) {
                    tries = 1;
                    fb.publish(post, function (published) { }, false);
                }
                else {
                    if (tries == 1) {
                        fb.login(function () {
                            tries += 1;
                            postInFacebook(post);
                        });
                    }
                }
            }

            function shoutInTaringa(item) {
                window.open(item.url.toString(), "_blank", "width=640, height=480");
            }

            function postInTwitter(finalTwUrl) {
                window.open(finalTwUrl.toString(), "_blank", "width=600, height=250");
            }

            function deleteCookie(cookieName) {
                var exp = new Date();
                exp.setTime(exp.getTime() - 1);
                document.cookie = cookieName + "=;expires=" + exp.toGMTString();
            }

            function logout() {
                $.get('../api/authenticate/logout', function () {
                    deleteCookie('authtoken');
                    window.location.href = "/login.aspx?action=noreturn";
                });
            }

            $(document).ajaxError(function (event, jqxhr, settings, exception) {
                if (console && console.log) {
                    console.log(exception);
                }
                else {
                    alert('Exception');
                }


            });

        </script>
</body>
</html>
