XA.component.parallax = (function($, _) {
    var api = {};

    function checkMobile() {
        return $(window).width() < 768;
    }

    function makeParallax($el) {
        var $bg = $el.children(".component-content"),
            vHeight = $(window).height(),
            elOffset = $bg[0].offsetTop,
            elHeight = $bg[0].offsetHeight,
            isMobile = checkMobile();

        function parallax() {
            if (isMobile) {
                return false;
            }

            var offset = $(window).scrollTop();

            if ((elOffset <= offset + vHeight) && (elOffset + elHeight >= offset)) {
                $bg.css("background-position", "50% " + Math.round((elOffset - offset) * 3 / 8) + "px");
            }
        }

        parallax();

        $(document).on("scroll", _.throttle(parallax, 10));
        $(window).on("resize", _.throttle(function() {
            isMobile = checkMobile();
            isMobile ? $bg.css("background-position", "50% 0") : parallax();
        }, 150));
    }

    api.initInstance=function(component) {
        makeParallax(component);
    }

    api.init = function() {
        $('.parallax-background:not(.initialized)').each(function() {
            api.initInstance($(this));
            $(this).addClass('initialized');
        });
    };

    return api;

})(jQuery, _);

XA.register('parallax-background', XA.component.parallax);