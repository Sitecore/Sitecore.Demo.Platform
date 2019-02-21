XA.component.flash = (function ($) {

    var api = {};


    function setSize(object) {
        var oldHeight = object.attr('height');
        var oldWidth = object.attr('width');
        var newWidth = object.width();
        var newHeight = oldHeight * newWidth / oldWidth;
        object.height(newHeight);
    }

    function initFlash(component, properties) {
        var content = component.find('.component-content > div');
        content.flash(properties);
    }

    function attachEvents(component) {
        $(document).ready(function () {
            var object = component.find('embed');
            object.css('width', '100%');
            setSize(object);

            $(window).resize(function () {
                setSize(object);
            });
        });
    }


    api.initInstance = function (component, prop) {
        initFlash(component, prop);
        attachEvents(component);
    }

    api.init = function () {
        var flash = $('.flash:not(.initialized)');
        if (flash.length > 0) {
            flash.each(function () {
                var properties = $(this).data('properties');
                api.initInstance($(this), properties)
                $(this).addClass('initialized');
            });
        }
    };

    return api;
}(jQuery, document));

XA.register('flash', XA.component.flash);