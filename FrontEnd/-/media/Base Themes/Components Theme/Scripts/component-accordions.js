/* global _:true */
XA.component.accordions = (function($) {
    var pub = {};

    var toogleEvents = {
        focus: function() {
            $(this).addClass("show");
        },
        blur: function() {
            $(this).removeClass("show");
        }
    };

    function headerBackground(header) {
        var backgroundIcon = $(header),
            background = $(header).find("img"),
            backgroundSrc = background.attr("src");

        if (backgroundSrc) {
            backgroundIcon.parents(".accordion").addClass("accordion-image");

            backgroundIcon.css({
                background: "url(" + backgroundSrc + ")",
                "background-repeat": "no-repeat",
                "background-size": "cover",
                "background-position": "50% 100%"
            });

            background.hide();
        }
    }

    function calcSlideSize(acc) {
        var accordionWidth = $(acc).width(),
            accordionItems = $(acc).find(".item"),
            maxHeight = 0;

        _.each(accordionItems, function(item) {
            var itemContent = $(item).find(".toggle-content"),
                itemHeader = $(item).find(".toggle-header"),
                slideWidth =
                    accordionWidth -
                    accordionItems.length * itemHeader.outerWidth();

            if ($(item).hasClass("active")) {
                $(item).css({
                    width: slideWidth
                });
            }

            //width
            itemContent.css({
                width: $(acc).hasClass("accordion-image")
                    ? slideWidth + itemHeader.outerWidth()
                    : slideWidth -
                      itemHeader.outerWidth() -
                      parseInt(itemHeader.css("padding"))
            });

            //height
            if (
                $(item)
                    .find(".toggle-content")
                    .height() > maxHeight
            ) {
                maxHeight = $(item)
                    .find(".toggle-content")
                    .height();
            }
        });
    }

    pub.animateHorizontal = function(properties) {
        var accordion = $(this).parents(".accordion"),
            panel = $(this).closest(".item"),
            header = panel.find(".toggle-header"),
            content = panel.find(".toggle-content"),
            items = accordion.find(".item"),
            siblings = panel.siblings(),
            siblingsContent = siblings.find(".toggle-content");

        panel.toggleClass("active");
        siblings.removeClass("active");
        siblings.stop(true).animate(
            {
                width: 0
            },
            properties.speed,
            properties.easing,
            function() {
                siblingsContent.css({
                    display: "none"
                });
            }
        );

        if (panel.hasClass("active")) {
            var slideWidth = accordion.hasClass("accordion-image")
                    ? content.outerWidth()
                    : accordion.width() -
                      ((items.length - 1) * panel.outerWidth() + 2),
                contentWidth = accordion.hasClass("accordion-image")
                    ? slideWidth
                    : slideWidth - header.outerWidth();

            panel.stop(true).animate(
                {
                    width: slideWidth
                },
                properties.speed,
                properties.easing,
                function() {}
            );

            content.css({
                width: contentWidth,
                display: "block"
            });
        } else {
            panel.stop(true).animate(
                {
                    width: 0
                },
                properties.speed,
                properties.easing,
                function() {
                    content.css({
                        display: "none"
                    });
                }
            );
        }
    };

    function getQueryParamKey(accordions) {
        var firstAccordionId = accordions[0].id;
        if (accordions.length > 0 && firstAccordionId != "") {
            return firstAccordionId.toLocaleLowerCase();
        }
        return null;
    }

    function accordion(acc, properties) {
        var ev = "click",
            $body = $("body"),
            toggleHeader = acc.find(".toggle-header");

        if (properties.expandOnHover) {
            ev = "mouseenter";
        }

        toggleHeader.on("mouseover", toogleEvents.focus);
        toggleHeader.on("mouseleave", toogleEvents.blur);
        toggleHeader.on("focus", toogleEvents.focus);
        toggleHeader.on("blur", toogleEvents.blur);
        toggleHeader.on("keyup", function(e) {
            if (e.keyCode == 13 || e.keyCode == 32) {
                $(this).click();
            }
        });

        if (acc.hasClass("accordion-horizontal")) {
            //calc slide width
            $(document).ready(function() {
                calcSlideSize(acc);
            });

            _.each(toggleHeader, function(header) {
                headerBackground(header);
            });
        }

        toggleHeader.on(ev, function() {
            var $this = $(this),
                panel = $this.closest(".item"),
                accordion = $this.parents(".accordion"),
                content = panel.find(".toggle-content"),
                $body = $("body"),
                siblings = panel.siblings(),
                siblingsContent = siblings.find(".toggle-content");

            // Horizontal animation
            if (accordion.hasClass("accordion-horizontal")) {
                pub.animateHorizontal.call(this, properties);
                // Vertical animation
            } else {
                // Close all other items if open
                // multiple option disabled
                if (!properties.canOpenMultiple) {
                    siblings.removeClass("active");
                    siblingsContent.stop().slideUp({
                        duration: properties.speed,
                        easing: properties.easing
                    });
                }

                panel.toggleClass("active");
                // Toggle state of selected item
                // If toggle state is enabled
                if (properties.canToggle) {
                    content.slideToggle({
                        duration: properties.speed,
                        easing: properties.easing
                    });
                } else {
                    content.slideDown({
                        duration: properties.speed,
                        easing: properties.easing
                    });
                }
            }
        });
    }

    pub.initInstance = function(component, prop) {
        // Set tabindex=0 for first header
        component.find('.toggle-header').eq(0).attr('tabindex','0');
        //
        if (component.hasClass("toggle")) {
            $.extend(prop, {
                canToggle: true
            });
        }
        component.find(".toggle-content").hide();
        var current = XA.queryString.getQueryParam(getQueryParamKey(component));
        if (current != null) {
            var arr = current.split(",");
            var items = component.find("li");
            for (var index = 0; index < items.length; index++) {
                var element = items[index];
                if (arr.indexOf(index + "") > -1) {
                    $(element).addClass("active");
                    $(element)
                        .find(".toggle-content")
                        .show();
                }
            }
        } else if (prop.expandedByDefault) {
            component.find("li:first-child").addClass("active");
            component
                .find("li:first-child")
                .find(".toggle-content")
                .show();
        }
        accordion(component, prop);
    };

    pub.init = function() {
        var accordions = $(".accordion:not(.initialized)");
        accordions.each(function() {
            var properties = $(this).data("properties"),
                acc = $(this);
            pub.initInstance(acc, properties);
            acc.addClass("initialized");
        });
    };

    return pub;
})(jQuery);

XA.register("accordions", XA.component.accordions);
