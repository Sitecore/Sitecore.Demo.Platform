XA.component.social = (function ($, document) {

    var api = {};
    api.shareProperties;
    api.initFacebook = function () {
        (function (d, s, id) {
            var js, fjs = d.getElementsByTagName(s)[0];
            if (d.getElementById(id)) {
                return;
            }
            js = d.createElement(s);
            js.id = id;
            js.src = "//connect.facebook.net/en_US/all.js#xfbml=1";
            fjs.parentNode.insertBefore(js, fjs);
        }(document, 'script', 'facebook-jssdk'));
    }

    var attachExternalScript = function (properties) {
        var component = $(".sharethis"),
            shareThisExternal;
        if (window.stLight === undefined) {
            shareThisExternal = document.createElement("script");
            shareThisExternal.type = "text/javascript";
            shareThisExternal.src = "//w.sharethis.com/button/buttons.js";
            $(window).on("load", function () {
                $(properties).each(function () {
                    try {
                        window.stLight.options(this);
                    } catch (e) {

                    }
                });
            });
            $(component[0]).append(shareThisExternal);
        }
    };

    api.initInstance = function (component, prop) {
        api.shareProperties.push(prop);
    }

    api.init = function () {
        var shareThis = $(".sharethis:not(.initialized)");
        api.shareProperties = [];
        shareThis.each(function () {
            var properties = $(this).data("properties");
            api.initInstance($(this), properties)
            $(this).addClass("initialized");
        });
        attachExternalScript(api.shareProperties);
    };
    return api;
}(jQuery, document));

XA.register("social", XA.component.social);