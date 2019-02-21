XA.component.facebook = (function($, document) {

    var api = {};

    api.initInstance=function() {
        (function(d, s, id) {
            var js, script = d.getElementsByTagName(s)[0];
            if (d.getElementById(id)) {
                return
            }
            js = d.createElement(s);
            js.id = id;
            js.src = "//connect.facebook.net/en_US/sdk.js#xfbml=1&version=v2.7";
            script.insertBefore(js, script.firstChild);
        }(document, 'script', 'facebook-jssdk'));
    }

    api.init = function() {
        var facebook = $(".fb-comments:not(.initialized)");

        facebook.each(function() {
            api.initInstance();
            $(this).addClass("initialized");
        });
    };

    return api;
}(jQuery, document));

XA.register("facebook", XA.component.facebook);