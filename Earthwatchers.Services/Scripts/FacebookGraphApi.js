var fb = {
    config: {
        // CONFIG VARS: 
        app_id: '1525841694313205',  //PROD 
        //app_id: '1455699711382152', //QA 

        use_xfbml: true,

        extendPermissions: 'email, public_profile',
        // info: http://developers.facebook.com/docs/reference/api/permissions/

        locale: 'es_ES'
        // all locales in: http://www.facebook.com/translations/FacebookLocales.xml

        // END CONFIG VARS
    },
    perms: [],
    hasPerm: function (perm) { for (var i = 0, l = fb.perms.length; i < l; i++) { if (fb.perms[i] == perm) { return true; } } return false; },
    logged: false,
    user: false, // when login, is a user object: http://developers.facebook.com/docs/reference/api/user
    login: function (callback) {
        FB.login(function (r) {
            if (r.status == 'connected') {
                FB.api('/me/permissions', function (perm) {
                    fb.logged = true;
                    fb.perms = [];
                    for (i in perm.data[0]) {
                        if (perm.data[0][i] == 1) {
                            fb.perms.push(i);
                        }
                    }
                });
                fb.getUser(callback);
            } else {
                fb.logged = false;
                fb.perms = [];
                callback();
            }
            //FB.api('/me/feed', 'post', {message: 'Hello, world!'});
        }, { scope: fb.config.extendPermissions });
        return false;
    },
    syncLogin: function (callback) {
        if (!callback) callback = function () { };
        FB.getLoginStatus(function (r) {
            if (r.status == 'connected') {
                FB.api('/me/permissions', function (perm) {
                    fb.logged = true;
                    fb.perms = [];
                    for (i in perm.data[0]) {
                        if (perm.data[0][i] == 1) {
                            fb.perms.push(i);
                        }
                    }
                });
                fb.getUser(callback);
                return true;
            } else {
                fb.logged = false;
                callback();
                return false;
            }
        });
    },
    logout: function (callback) { FB.logout(callback); },
    getUser: function (callback) {
        FB.api('/me', function (r) {
            var user = r;
            user.picture = "https://graph.facebook.com/" + user.id + "/picture";
            fb.user = user; callback(user);
        });
    },
    publish: function (publishObj, callback, noReTry) {
        
        FB.ui({
                   method: 'feed',
                   name: publishObj.name,
                   link: publishObj.link,
                   picture: publishObj.picture,
                   caption: publishObj.caption,
                   description: publishObj.description,
               },
               function (response) {
                   silverLightControl = document.getElementById("SilverlightPlugin");
                   if (response && response.post_id) {
                       silverLightControl.content.JsFacebookCallback.AddFbSharedPoints(true)
                   }
                   else {
                       silverLightControl.content.JsFacebookCallback.AddFbSharedPoints(false)
                   }
               }
             );
    },
    readyFuncs: [],
    ready: function (func) { fb.readyFuncs.push(func) },
    launchReadyFuncs: function () { for (var i = 0, l = fb.readyFuncs.length; i < l; i++) { fb.readyFuncs[i](); }; }
}
window.fbAsyncInit = function () {
    if (fb.config.app_id) FB.init({ appId: fb.config.app_id, status: true, cookie: true, xfbml: fb.config.use_xfbml });
    fb.syncLogin(fb.launchReadyFuncs);
};
var oldload = window.onload;
window.onload = function () {
    var d = document.createElement('div'); d.id = "fb-root"; document.getElementsByTagName('body')[0].appendChild(d);
    var e = document.createElement('script'); e.async = true; e.src = document.location.protocol + '//connect.facebook.net/' + fb.config.locale + '/all.js';
    document.getElementById('fb-root').appendChild(e);
    if (typeof oldload == 'function') oldload();
};
