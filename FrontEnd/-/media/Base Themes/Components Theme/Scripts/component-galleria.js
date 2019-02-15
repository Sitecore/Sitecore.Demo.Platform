
XA.component.galleria = (function($) {
    /* global Galleria:false */
    var api = {};

    api.initInstance=function(component, prop) {
        var id = component.find(".gallery-inner").attr("id");
        Galleria.loadTheme(prop.theme);
        Galleria.run("#" + id, prop);
    }

    api.init = function() {
            var gallery = $(".gallery:not(.initialized)");

            gallery.each(function() {
                var properties = $(this).data("properties");
                api.initInstance($(this), properties);
                $(this).addClass("initialized");
            });
        }

    return api;
}(jQuery, document));

XA.register("galleria", XA.component.galleria);