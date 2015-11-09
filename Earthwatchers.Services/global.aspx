<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="global.aspx.cs" Inherits="Earthwatchers.Services.global" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Guardians Global - Greenpeace</title>
    <script src="admin/jquery-1.9.1.min.js" type="text/javascript"></script>
    <script src="Scripts/cookieManagment.js"></script>
    <style type="text/css">
        html { height: 100%; }
        body { background-color: #BFD89B; }
        .logo { position: absolute; top: 50%; left: 33%; }
        .countries { position: absolute; top: 40%; left: 60%; }
            .countries img { padding: 7px; display: block; max-width: 65px; cursor: pointer; }
    </style>
</head>
<body>
    <div class="logo">
        <div>
            <img src="Images/logo.png" />
        </div>
        <div style="margin-top: 15px">
            <img src="Images/guardianes.png" />
        </div>
    </div>
    <form id="frmCountrySelect" action="login.aspx" runat="server">
        <input id="locale" name="locale" type="hidden" value="" />
        <div class="countries">
            <img data-locale="es-AR-1" alt="Argentina" title="Argentina" src="Images/flags/ar.png" />
            <img data-locale="zh-CN-1" alt="中国" title="中国" src="Images/flags/cn.png" />
            <img data-locale="en-CA-1" alt="Canada" title="Canada" src="Images/flags/ca.png" />
            <%--<img data-locale="pt-BR" alt="Brasil" title="Brasil" src="Images/flags/br.png" />--%>
            <%--<img data-locale="en-US" alt="United States" title="United States" src="Images/flags/us.png" />--%>
        </div>
    </form>

    <script type="text/javascript">
        var _localeCookieKey = '<%=Earthwatchers.Services.Localization.LocalizationService.localeKey%>';


        $(document).ready(function () {
            $('.countries img').click(function () {
                $('#locale').val($(this).attr('data-locale'));
                createCookie(_localeCookieKey, $('#locale').val());
                window.location.href = "/login.aspx";
                //$('#frmCountrySelect').submit();
            });
        });
    </script>

</body>
</html>
