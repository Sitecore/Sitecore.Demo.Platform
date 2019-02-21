XA.component.flip = (function($) {
    var api = {
            getSideSortByHeight: function(valArr) {
                return (sortedSides =
                    sortedSides ||
                    valArr.sort(function(a, b) {
                        return a.outerHeight(true) > b.outerHeight(true);
                    }));
            },

            equalSideHeight: function($el) {
                var side0 = $el.find(".Side0"),
                    side1 = $el.find(".Side1"),
                    slide0Height = this.calcSlideSizeInToggle(side0),
                    slide1Height = this.calcSlideSizeInToggle(side1),
                    maxHeight = Math.max(slide0Height, slide1Height);
                $el.find(".flipsides").css({ "min-height": maxHeight + "px" });
                side0.add(side1).css({ bottom: 0 });
            },
            calcSlideSizeInToggle: function($slide) {
                var child = $slide.find(">div"),
                    size = 0;
                child.each(function(pos, el) {
                    size += $(el).outerHeight(true);
                });
                size += parseInt($slide.css("padding-top"));
                size += parseInt($slide.css("padding-bottom"));
                return size;
            },
            equalSideHeightInToggle: function($el) {
                var side0 = $el.find(".Side0"),
                    side1 = $el.find(".Side1"),
                    slide0Height = this.calcSlideSizeInToggle(side0),
                    slide1Height = this.calcSlideSizeInToggle(side1),
                    maxHeight = Math.max(slide0Height, slide1Height);
                $el.find(".flipsides").css({ "min-height": maxHeight + "px" });
                side0.add(side1).css({ bottom: 0 });
            }
        },
        sortedSides;

    function detectMobile() {
        return "ontouchstart" in window;
    }

    function calcHeightOnResize() {
        var flip = $(".flip.initialized");
        flip.each(function() {
            api.equalSideHeight($(this));
        });
    }

    api.initInstance = function(component) {
         // Set tabindex=0 for first header
         component.find('[class*="Side0"]').attr('tabindex','0');
         //
        if (component.hasClass("flip-hover") && !detectMobile()) {
            component.hover(
                function() {
                    component.addClass("active");
                },
                function() {
                    component.removeClass("active");
                }
            );
        } else {
            component.on("click", function() {
                component.toggleClass("active");
            });
        }
    };

    api.init = function() {
        var flip = $(".flip:not(.initialized)");
        $(window).on("resize", function() {
            calcHeightOnResize();
        });
        flip.each(function() {
            var $flipModule = $(this).find(".flipsides");
            $flipModule.find(".Side0").attr("tabindex","0");
            api.initInstance($(this));
            $(this).addClass("initialized");
            api.equalSideHeight($(this));
        });
    };
    return api;
})(jQuery, document);

XA.register("flip", XA.component.flip);
