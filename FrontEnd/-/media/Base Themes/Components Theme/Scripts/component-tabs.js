XA.component.tabs = (function ($) {

    var pub = {},
        instance;
		
    var isFlipInside = function (instance) {
        return !!instance.find('.component.flip').length
    }

    function tabsScrollable($tabsScroll) {
        var speed = 150; //tabs scroll speed

        function initNavScroll($tabsNav) {
            var sum = 0,
                maxHeight = 0;

            if ($tabsNav.length) {
                $tabsNav.parent().find(".prev").remove();
                $tabsNav.parent().find(".next").remove();
                $tabsNav.unwrap();

                $tabsNav.css("width", "auto");
                $tabsNav.css("height", "auto");
                $tabsNav.css("left", 0);
            }


            $tabsNav.find("li").each(function () {
                sum += $(this).outerWidth(true);
            });


            $tabsNav.find("li").each(function () {
                maxHeight = Math.max(maxHeight, $(this).height());
            });


            $tabsNav.wrap("<div class='wrapper'>");
            $("<div class='next tab-slider'>></div>").insertAfter($tabsNav);
            $("<div class='prev tab-slider'><</div>").insertBefore($tabsNav);


            $tabsNav.parent().css("height", parseInt(maxHeight, 10));
            $tabsNav.parent().find(".tab-slider").css("height", parseInt(maxHeight, 10) - 2);
            //fix for 8111
            sum += 10;
            if (sum > $tabsNav.parent().width()) {
                $tabsNav.parent().find(".prev").hide();
                $tabsNav.width(sum);
            } else {
                $tabsNav.parent().find(".prev").hide();
                $tabsNav.parent().find(".next").hide();
            }
        }


        function bindPrevNextEvents(current, $tabsNav) {
            current.find(".prev").click(function () {
                var left = parseInt($tabsNav.css("left"), 10);
                left += speed;

                if (left > 0) {
                    left = 0;
                    $tabsNav.stop().animate({
                        "left": left
                    });

                    $tabsNav.parent().find(".prev").hide();
                    $tabsNav.parent().find(".next").show();
                } else {
                    $tabsNav.stop().animate({
                        "left": left
                    });

                    $tabsNav.parent().find(".prev").show();
                    $tabsNav.parent().find(".next").show();
                }
            });


            current.find(".next").click(function () {
                var left = parseInt($tabsNav.css("left"), 10),
                    navWidth = $tabsNav.width(),
                    navParentWidth = $tabsNav.parent().width();

                left -= speed;

                if ((navWidth + left) < navParentWidth) {
                    left = navWidth - navParentWidth + 20;
                    $tabsNav.stop().animate({
                        "left": -left
                    });

                    $tabsNav.parent().find(".prev").show();
                    $tabsNav.parent().find(".next").hide();
                } else {
                    $tabsNav.stop().animate({
                        "left": left
                    });

                    $tabsNav.parent().find(".prev").show();
                    $tabsNav.parent().find(".next").show();
                }
            });
        }

        function bindChangeTabs($tabsNav, $tabsContainer) {
            $tabsNav.find("li").click(function(event) {
                var index = $(this).index(),
                    instance = $(this).closest('.component.tabs');

                $(this).addClass("active");
                $(this).siblings().removeClass("active");

                $tabsContainer.find(".tab").removeClass("active");
                $tabsContainer.find(".tab:eq(" + index + ")").addClass("active");

                if (isFlipInside(instance)) {
                    try {
                        XA.component.flip.equalSideHeight(instance);
                    } catch (e) {
                        /* eslint-disable no-console */
                        console.warn('Error during calculation height of Flip list in toggle'); // jshint ignore:line
                        /* eslint-enable no-console */
                    }
                }
                event.preventDefault();
            });
        }

        function initTabsScrollable($tabs) {
            $tabs.each(function () {
                var $tabsNav = $(this).find(".tabs-heading"),
                    $tabsContainer = $(this).find(".tabs-container");

                $tabsNav.find("li:first-child").addClass("active");
                $tabsContainer.find(".tab:eq(0)").addClass("active");

                bindChangeTabs($tabsNav, $tabsContainer);
                initNavScroll($tabsNav);
                bindPrevNextEvents($(this), $tabsNav);
            });
        }

        initTabsScrollable($tabsScroll);
        $(window).resize(function () {
            initTabsScrollable($tabsScroll);
        });
    }

    

    function selectActiveTab($tabNav, $tabs) {
        var tabRootId = $tabNav.closest('.component.tabs').attr('id');
        var tabToSelect = XA.queryString.getQueryParam(tabRootId || "tab");

        if (tabToSelect != null && !isNaN(parseInt(tabToSelect)) && isFinite(tabToSelect)) {
            tabToSelect = parseInt(tabToSelect);
            if ($tabNav.length > tabToSelect) {
                $($tabNav[tabToSelect]).addClass("active");
                $($tabs[tabToSelect]).addClass("active");
                
                return;
            }
        }
        $tabNav.first().addClass("active");
        $tabNav.first().attr("tabindex","0");
        $tabs.first().addClass("active");
        $tabNav.first("li").focus();
    }

    pub.initInstance = function (component) {
        var $tabModule = component.find(".tabs-inner");
        if (component.hasClass("tabs-scrollable")) {
            tabsScrollable(component);
        } else {
            $tabModule.each(function () {
                var $tabNav = $(this).find(".tabs-heading > li"),
                    $tabs = $(this).find("> .tabs-container > .tab");

                selectActiveTab($tabNav, $tabs);

                $tabNav.click(function (event) {
                    var current = $(this),
                        index = current.index(),                        
                        tabInner = current.parent().parent();

                    current.siblings().removeClass("active");
                    tabInner.find("> .tabs-container > .tab").removeClass("active");
                    current.addClass("active");
                    $(tabInner.children(".tabs-container").children(".tab").eq(index)).addClass("active");
                    if (isFlipInside(component)) {
                        try {
                            XA.component.flip.equalSideHeight(component);
                        } catch (e) {
                            /* eslint-disable no-console */
                            console.warn('Error during calculation height of Flip list in toggle'); // jshint ignore:line
                            /* eslint-enable no-console */

                        }
                    }
                    current.focus();
                    event.preventDefault();
                }).keyup(function (event) {
                    var currentHeading = $(this);
                    switch (event.keyCode) {
                        case 38:
                        {
                            currentHeading.prev("li").click();
                            break;
                        }
                        case 40:
                        {
                            currentHeading.next("li").click();
                            break;
                        }
                    }
                });
            });
        }


        if (isFlipInside(component)) {
            try {
                XA.component.flip.equalSideHeight(component);
            } catch (e) {
                /* eslint-disable no-console */
                console.warn('Error during calculation height of Flip list in toggle'); // jshint ignore:line
                /* eslint-enable no-console */

            }
        }
    }

    pub.init = function () {
        var $tabs = $(".tabs:not(.initialized)");
        $tabs.each(function () {
            var instance = $(this);
            pub.initInstance(instance);
            $(this).addClass("initialized");
        });
    }

    return pub;
}(jQuery));

XA.register("tabs", XA.component.tabs);