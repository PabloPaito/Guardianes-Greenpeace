<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="Earthwatchers.Services.login" Culture="auto:en-CA" UICulture="auto:en-CA" %>

<%@ Import Namespace="Resources" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
    <meta http-equiv="Pragma" content="no-cache" />
    <meta http-equiv="Expires" content="0" />

    <title><%= R.LoginTitle %></title>

    <link href="css/ew.css" rel="stylesheet" />
    <link href="css/jquery.fancybox.css" rel="stylesheet" />
    <script src="admin/jquery-1.9.1.min.js" type="text/javascript"></script>
    <script src="admin/jquery.redirect.min.js" type="text/javascript"></script>
    <script src="admin/jquery.placeholder.min.js" type="text/javascript"></script>
    <script src="admin/jquery.base64.js" type="text/javascript"></script>
    <script src="admin/jquery.fancybox.pack.js" type="text/javascript"></script>
    <script src="Scripts/FacebookGraphApi.js"></script>
    <script src="Scripts/cookieManagment.js"></script>
    <link rel="shortcut icon" href="Images/favicon.ico" />

    <style type="text/css">
        #signButton {
            float: none;
        }

        .noSignButton {
            float: none !Important;
        }

        .socialNetworksLogin img :hover {
            cursor: pointer;
        }

        .buttonsSmall {
            cursor: pointer;
            background: none;
            border: none;
            padding: 0;
        }

        .buttonsLong {
            cursor: pointer;
            background: none;
            border: none;
            width: 150px;
            padding: 0;
        }

        .buttonsLogin :focus {
            outline: 0;
        }

        :active {
            outline: none;
        }

        .countries {
            margin-top: 25px;
        }

            .countries img {
                width: 35px;
                margin-left: 5px;
            }

            .countries :hover {
                cursor: pointer;
            }

        .taringaButton {
            display: none;
        }
    </style>
</head>
<body>
    <div class="guardianes-login">
        <div class="floater">
            <div class="contents">
                <div>
                    <img src="Images/logo.png" />
                </div>
                <div style="margin-top: 15px">
                    <img src="<%= R.LoginGuardiansImage %>" />
                </div>
                <div class="container">
                    <div id="logindiv">
                        <div style="float: left">
                            <div class="content">
                                <div>
                                    <a href="#" title="<%= R.LoginVideoTitle %>" id="videoButton">
                                        <img src="Images/video2.jpg" style="padding: 15px" alt="<%= R.LoginVideoTitle %>" title="<%= R.LoginVideoTitle %>" border="0" />
                                    </a>
                                </div>
                            </div>

                            <div id="socialNetworksLoginSmall" style="display: none">
                                <button class="buttonsLogin buttonsSmall taringaButton" onclick="taringaLogin()">
                                    <img src="Images/taringa.png" /></button>
                                <button class="buttonsLogin buttonsSmall fbButton" onclick="facebookLogin()">
                                    <img src="Images/facebook.png" /></button>
                                <button class="buttonsLogin buttonsSmall twButton" onclick="twitterLogin()">
                                    <img src="Images/twitter.png" /></button>
                            </div>
                        </div>
                        <div style="float: right">
                            <form name="loginform" id="loginform" class="login-form">
                                <div class="content">
                                    <div class="headertxt">
                                        <%= R.LoginHeader %>
                                    </div>

                                    <input name="tbUsername" id="tbUsername" type="text" class="input" placeholder="<%=R.LoginEmailHint %>" title="<%=R.LoginEmailHint %>" />

                                    <img id="loadinggif" src="css/fancybox_loading.gif" style="display: none; float: right; margin-top: 20px" />

                                    <input name="tbPassword" type="password" id="tbPassword" class="input" placeholder="<%=R.LoginPasswordHint %>" style="display: none" />

                                    <img id="loadinggifP" src="css/fancybox_loading.gif" style="display: none; float: right; margin-top: 20px" />

                                    <div class="slink" style="display: none" id="forgotPassword">
                                        <a href="#" id="forgotPassowrdButton" title="<%=R.LoginForgotPasswordTitle %>"><%=R.LoginForgotPassword %></a>
                                    </div>
                                    <input type="button" value="<%=R.LoginNextButton %>" id="loginbutton" class="button" />

                                    <div id="rError" class="error">
                                    </div>

                                </div>
                            </form>
                        </div>
                        <div id="socialNetworksLoginLong" style="float: left; text-align: center; width: 700px;">
                            <button class="buttonsLogin buttonsLong taringaButton" onclick="taringaLogin()">
                                <img src="Images/taringaLogin.png" style="width: 150px;" /></button>
                            <button class="buttonsLogin buttonsLong fbButton" onclick="facebookLogin()">
                                <img src="Images/facebookLogin.png" style="width: 150px;" /></button>
                            <button class="buttonsLogin buttonsLong twButton" onclick="twitterLogin()">
                                <img src="Images/TwitterLogin.png" style="width: 150px;" /></button>
                        </div>
                        <div id="SilverlightInformation">
                            <p style="display: inline; font-size: 13px;">Guardianes utiliza Microsoft Silverlight.</p>
                            <a id="SlMoreInfo" style="display: inline; font-size: 12px; color: gray;" href="SLInformation.html">(Mas información)</a>
                        </div>
                    </div>
                </div>
                <div class="countries">
                    <img data-locale="es-AR-1" alt="Argentina" title="Argentina" src="Images/flags/ar-35.png" />
                    <img data-locale="zh-CN-1" alt="中国" title="中国" src="Images/flags/cn-35.png" />
                    <img data-locale="en-CA-1" alt="Canada" title="Canada" src="Images/flags/ca-35.png" />
                </div>
            </div>
        </div>

        <div id="registrationDiv" style="display: none; width: 500px;">
            <img id="loadinggif2" src="css/fancybox_loading.gif" style="margin: auto" />

            <form name="rForm" id="rForm" class="login-form" style="display: none">
                <div class="content">
                    <b><%=R.LoginUsername %></b>
                    <input name="rUsername" id="rUsername" type="text" class="input" disabled="disabled" />

                    <div style="clear: both;">
                        <br />
                        <b><%=R.LoginPassword %></b>
                    </div>
                    <input name="rPassword" type="password" id="rPassword" class="input" placeholder="<%=R.LoginPasswordHint %>" />
                    <input name="rPasswordRepeat" id="rPasswordRepeat" title="Repetí tu contraseña" type="password" class="input" placeholder="<%=R.LoginPasswordRepeatHint %>" />

                    <input name="rCountry" id="rCountry" type="hidden" class="input" />
                    <input type="button" value="<%=R.LoginStartButton %>" id="registerButton" class="button" />
                </div>
            </form>

            <div id="rError2" class="error"></div>
        </div>

        <div id="videoDiv" style="display: none; width: 730px;">
            <iframe id="ytplayer" width="720" height="405" src="//www.youtube.com/embed/8bbWsh3ssyk" frameborder="0" allowfullscreen="allowfullscreen"></iframe>
        </div>

        <div id="forgotPassowrdDiv" style="display: none; width: 500px;">
            <img src="Images/check-icon.png" alt="Ok" />
            <%=R.LoginPasswordRecoveryMessage %>
        </div>
    </div>


    <div class="guardianes-carteles" style="display: none; text-align: center;">
        <div style="position: relative; text-align: center; width: 700px; background: #F3F3F3; border: 1px solid #fff; border-radius: 5px; box-shadow: 0 1px 3px rgba(0,0,0,0.5); -moz-box-shadow: 0 1px 3px rgba(0,0,0,0.5); -webkit-box-shadow: 0 1px 3px rgba(0,0,0,0.5); left: 50%; margin-left: -350px; color: #888888; font-family: Helvetica, Arial, "Lucida Grande", sans-serif;">
            <div id="cartel1" class="content" style="padding:10px;">
                <%--tablet o celular--%>
                <h2 style="text-align: center;"><%=R.Cartel1t1 %></h2>
                <p>
                    <%=R.Cartel1t2 %> <a href="http://educeo.geodan.nl"> Guardianes 3.0</a><span class="isSl"><%=R.Cartel1t3 %></span> <br />
                    <br />
               <%=R.Cartel1t4 %> <a href="https://guardianes.typeform.com/to/By4axg"><%=R.Cartel1t5 %></a> <%=R.Cartel1t6 %> <a href="mailto:guardianes.ar@greenpeace.org">guardianes.ar@greenpeace.org</a><%=R.Cartel1t7 %>
                </p>

                <p><%=R.Cartel1t8 %></p>
                <div class="slText"><%=R.Cartel1t9 %></div>
                <div class="slText2" style="margin-top: 30px">
                    <a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=5.0.61118.0" style="text-decoration: none" class="button" onclick="logSlInstall()"><%=R.Cartel1t10 %></a>
                </div>
            </div>
            <div id="cartel2" class="content" style="padding:10px;">
                <%--Chrome mayor a v45--%>
                <div style="font-size: 14px; color: #888888; padding: 20px; text-align: left;">
                    <h2 style="text-align: center;"><%=R.Cartel1t1 %></h2>
                    <p>
                        <%=R.Cartel2t2 %> <a href="https://www.mozilla.org">FireFox</a> o <a href="https://www.microsoft.com">Internet Explorer</a>.
                    </p>
                    <h3 style="color: #268026; font-size: 15px;"><%=R.Cartel2t3 %> <a href="http://educeo.geodan.nl">Guardianes 3.0</a></h3>
                    <p><%=R.Cartel2t4 %></p>

                    <p><%=R.Cartel2t5 %> <a href="https://guardianes.typeform.com/to/By4axg"><%=R.Cartel1t5 %></a> <%=R.Cartel1t6 %> <a href="mailto:guardianes.ar@greenpeace.org">guardianes.ar@greenpeace.org</a> <br /><br /><%=R.Cartel2t6 %></p>

                    <p><%=R.Cartel1t8 %></p>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        var emailRegex = /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;
        var isValidUser = false;
        var geohexcode;

        function getAuthString() {
            return "Basic " + $.base64Encode(document.getElementById("tbUsername").value + ":" + document.getElementById("tbPassword").value);
        }

        function getEncodedMail(val) {
            return $.base64Encode(val);
        }


        function register() {
            var isValid = true;
            //var passwordRegex = /^.*(?=.{6,})(?=.*[a-zA-Z])(?=.*\d).*$/;
            //if (!passwordRegex.test($("#rPassword").val()))

            $("#rError2").empty();

            if ($("#rPassword").val().length < 5) {
                $("<span></span>").html("<%=R.LoginPasswordValidationLength%>").appendTo("#rError2");
                isValid = false;
            }

            if ($("#rPassword").val() != $("#rPasswordRepeat").val()) {
                $("<span></span>").html("<%=R.LoginPasswordValidationMatch%>").appendTo("#rError2");
                isValid = false;
            }

            if (isValid) {
                var ew = {
                    Name: $("#rUsername").val(),
                    Role: 0,
                    Password: $("#rPassword").val(),
                    Country: "Argentina",
                    Language: getCookie("guardianlocale"),
                };

                $("#loadinggif2").show();
                $.ajax({
                    url: "/api/earthwatchers",
                    type: "POST",
                    data: JSON.stringify(ew),
                    dataType: "json",
                    contentType: "application/json ; charset=utf-8",
                    success: function (result) {
                        $().redirect('home.aspx', { 'username': $("#rUsername").val(), 'password': $("#rPassword").val(), 'geohexcode': geohexcode, 'authtoken': false });
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        // $("<span></span>").html(errorThrown + " - " + textStatus).appendTo("#rError2");
                        $("#loadinggif2").hide();
                        $.fancybox.close();
                        alert('<%=R.LoginRegistrationFailed %>')
                    }
                });
            }
        }

        function validateUser() {
            $("#optionalSigns").hide();
            var username = $("#tbUsername").val();
            if (username == undefined || username.length < 4 || !emailRegex.test(username)) {
                $("#rError").html('<%=R.LoginInvalidEmail %>');
            }
            else {
                //1. Chequeo si existe en Earthwatchers
                $("#loadinggif").show();

                $.ajax({
                    type: "GET",
                    url: "api/earthwatchers/exists=" + getEncodedMail(username),
                    success: function (exists) {
                        if (exists) {
                            $("#tbPassword").show();
                            $("#tbPassword").focus();
                            $("#forgotPassword").show();
                            $("#loadinggif").hide();
                            $("#rError").hide();
                            $("#socialNetworksLoginLong").hide();
                            $("#socialNetworksLoginSmall").show();
                            $("#loginbutton").val("<%= R.LoginStartButton%>");
                        }
                        else {
                            $("#loadinggif").hide();
                            //Abro el Fancybox
                            $.fancybox.open([
                            {
                                href: '#registrationDiv',
                                minHeight: 200,
                                scrolling: 'no',
                                closeBtn: false,
                                helpers: {
                                    overlay: { closeClick: false } // prevents closing when clicking OUTSIDE fancybox
                                }
                            }]);

                            //Si ya firmó la petición
                            $("#loadinggif2").hide();
                            $.fancybox.update();
                            $("#rForm").show();
                            $("#rUsername").val(username);
                            $("#rPassword").focus();
                            $("#rError2").html("");
                        }
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        $("#loadinggif2").hide();
                        $("#rError2").html(errorThrown);
                    }
                });
            }

        }

        function login() {
            //Si el password está visible entonces termino el login
            if ($("#tbPassword").is(":visible")) {
                var username = $("#tbUsername").val();
                var password = $("#tbPassword").val();
                $("#loadinggifP").show();

                $.ajax({
                    type: "GET",
                    url: "api/authenticate/login",
                    cache: false,
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("Authorization", getAuthString());
                    },
                    success: function (data) {
                        if (data) {
                            $().redirect('home.aspx', { 'username': username, 'password': password, 'geohexcode': geohexcode, 'authtoken': false });
                        }
                        else {
                            $("#loadinggifP").hide();
                            $("#rError").show();
                            $("#rError").html('<%=R.LoginInvalidUserOrPass %>');
                        }
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        $("#loadinggifP").hide();
                        $("#rError").show();
                        $("#rError").html('<%=R.LoginInvalidUserOrPass %>');
                    }
                });
            }
            else {
                validateUser();
            }
        }

        function twitterLogin() {
            $("#loadinggif").show();
            $().redirect('home.aspx', { 'social': 'Twitter' });
        }

        function taringaLogin() {
            $("#loadinggif").show();
            $().redirect('home.aspx', { 'social': 'Taringa' });
        }

        function facebookLogin() {

            $("#loadinggif").show();
            if (fb != null && fb.logged) {
                //    fb.publish(null, null, false);  TEST SHARES

                //Tomo los datos importantes
                var fbMail = fb.user.email;
                var fbId = fb.user.id;
                var fbName = fb.user.first_name;
                var ewApi = "Facebook";
                var accessToken = FB.getAccessToken();

                //$().redirect('home.aspx', { 'social': 'Facebook', 'mail': fbMail, 'userId': fbId, 'accessToken': accessToken, 'nickName': fbName });

                //veo si existe en mi tabla, sino lo creo
                $.ajax({
                    type: "POST",
                    url: "../api/earthwatchers/loginwithapi",
                    data: JSON.stringify({ Api: ewApi, Mail: fbMail, NickName: fbName, UserId: fbId, AccessToken: accessToken }),
                    contentType: "application/json",
                    success: function (earthwatcher) {
                        $().redirect('home.aspx', { 'social': 'Facebook', 'mail': earthwatcher.Name, 'userId': earthwatcher.UserId, 'accessToken': accessToken, 'nickName': fbName });
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        $("#rError2").html(errorThrown);
                    }
                })
            }
            else {
                fb.login(function () {
                    facebookLogin();
                });
            }
        };

        var _localeCookieKey = '<%=Earthwatchers.Services.Localization.LocalizationService.localeKey%>';

        function changeLocale(newLocale) {
            deleteCookie(_localeCookieKey);
            createCookie(_localeCookieKey, newLocale);
        }

        $(document).ready(function () {

            //CAMBIAR PAIS FLAGSs
            $('.countries img').click(function () {
                changeLocale($(this).attr('data-locale'));
                location.reload();
            })

            //Redirect to Guardianes 3.0 if tablet or mobile
            var objAgent = navigator.userAgent;
            var objfullVersion = 0;
            var objBrMajorVersion = 0;
            var objOffsetName, objOffsetVersion, ix;

            //If MobileOrTablet
            if (window.mobileAndTabletcheck()) {
                $('.guardianes-login').hide();
                $('.guardianes-carteles').show();
                $('#cartel1').show();
                $('.isSl').hide();
                $('#cartel2').hide();
                $('.slText').hide();
                $('.slText2').hide();
            }
                //If Chrome
            else if ((objOffsetVersion = objAgent.indexOf("Chrome")) != -1) {
                objfullVersion = objAgent.substring(objOffsetVersion + 7);
                objBrMajorVersion = objfullVersion.substring(0, 2)
                $('.guardianes-login').hide();
                $('.guardianes-carteles').show();
                //Version +45
                if (objBrMajorVersion >= 45) {
                    $('#cartel1').hide();
                    $('#cartel2').show();
                }
            }
            else {
                $('.guardianes-login').show();
                $('.guardianes-carteles').hide();
            }

            //MOSTRAR/OCULTAR BOTONES PARA LOGIN
            var locale = getCookie("guardianlocale");
            if (locale == 'es-AR') {
                $('.taringaButton').show();
            }
            if (locale == 'zh-CN') {
                $('.fbButton').hide();
                $('.twButton').hide();
            }



            try { geohexcode = $.QueryString["geohexcode"]; }
            catch (ex) { }

            $('input, textarea').placeholder();

            $("#registerButton").click(function () {
                register();
            });

            $("#forgotPassowrdButton").click(function () {
                $("#loadinggifP").show();
                $.ajax({
                    url: "/api/earthwatchers/forgotPassword",
                    type: "POST",
                    data: "{\"Name\":\"" + $("#tbUsername").val() + "\" }",
                    contentType: "application/json ; charset=utf-8",
                    success: function () {
                        $("#loadinggifP").hide();
                        //Abrir fancy box para avisar que se mandó la nueva contraseña por mail
                        $.fancybox.open([
                            {
                                href: '#forgotPassowrdDiv',
                                scrolling: 'no'
                            }]);
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        $("#loadinggifP").hide();
                        $("#rError").show();
                        $("<span></span>").html(errorThrown + " - " + textStatus).appendTo("#rError");
                    }
                });
            });

            $("#rPasswordRepeat").keypress(function (event) {
                var keycode = (event.keyCode ? event.keyCode : event.which);
                if (keycode == '13') {
                    register();
                }
            });

            $("#tbPassword").keypress(function (event) {
                var keycode = (event.keyCode ? event.keyCode : event.which);
                if (keycode == '13') {
                    login();
                }
            });

            $("#loginbutton").click(function () {

                login();
            });

            $("#videoButton").click(function () {
                $.fancybox.open([
                            {
                                href: '#videoDiv',
                                scrolling: 'no'
                            }]);
            });

            $("#tbUsername").keypress(function (event) {
                var keycode = (event.keyCode ? event.keyCode : event.which);
                if (keycode == '13') {
                    validateUser();
                }
            });

            $("#tbUsername").focusout(function () {
                validateUser();
            });

            $(".noSignButton").click(function () {
                $.fancybox.close();
                $("#loadinggif").hide();
            });

            //Obtengo de donde viene y el mail
            var referrer = document.referrer;
            if (referrer != undefined && referrer.indexOf('greenpeace.org.ar') > -1) {
                //var r = referrer.indexOf("&referrer=&mail=") + 15;
                //var mail = referrer.substring(r, referrer.length);
                var mail = $.QueryString["mail"];
                if (mail != undefined && mail != '') {
                    $("#tbUsername").val(mail);
                    validateUser();
                }
            }
        });

        window.mobileAndTabletcheck = function () {
            var check = false;
            (function (a) { if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino|android|ipad|playbook|silk/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4))) check = true })(navigator.userAgent || navigator.vendor || window.opera);
            return check;
        }
    </script>

</body>
</html>
