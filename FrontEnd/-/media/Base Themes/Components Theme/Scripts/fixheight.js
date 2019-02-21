XA.component.equalHeight = (function ($) {
    var settings = {
        parentSelector: '.equalized-content',
        selector: '.equal:not(.flip.component),.flip .Side0,.flip .Side1'
    };

    var api = {};

    api.fixHeight = function () {

        $(settings.parentSelector).each(function () {
            var $elements = $(this).find(settings.selector),
                maxHeight = 0;



            $elements.each(function () {
                var $element = $(this);
                $element.css('min-height', 'inherit');
                $element.find('>.component-content').css('min-height', 'inherit');

                if ($element.outerHeight(true) > maxHeight) {
                    maxHeight = $element.outerHeight(true);
                }
            });

            if (maxHeight > 0) {
                $elements.each(function () {
                    var $element = $(this);
                    if ($element.hasClass("Side0") || $element.hasClass("Side1")) {
                        $element.parent().attr('class', 'flip').css({ 'min-height': maxHeight });
                    }
                    $element.css({
                        'min-height': maxHeight
                    });
                    $element.find('>.component-content').css({
                        'min-height': maxHeight
                    });
                });
            }
        });
    }

    api.init = function () {
        $(window).bind('load', function () {
            setTimeout(api.fixHeight, 0);
        });

        $(window).bind('resize', function () {
            api.fixHeight();
        });
    };

    return api;

}(jQuery, document));

XA.register("equalHeight", XA.component.equalHeight);