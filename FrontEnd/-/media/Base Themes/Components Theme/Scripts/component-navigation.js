XA.component.navigation = (function ($) {

    var timeout = 200,
        timer = 0,
        submenu,
        dropDownEvents = {
            show: function (sm) {
                this.debounce();
                //if (submenu) {
                //    submenu.closest('li').removeClass("show");
                //}
                submenu = sm;
                submenu.closest('li').addClass("show");
            },
            debounce: function () {
                if (timer) {
                    clearTimeout(timer);
                    timer = null;
                }
            },
            hide: function () {
                if (submenu) {
                    submenu.closest('li').removeClass("show")
                }
            },
            queueHide: function () {
                timer = setTimeout(function () {
                    dropDownEvents.hide();
                }, timeout);
            },
            focus: function () {
                $(this).closest('li').siblings().removeClass("show");
                $(this).closest('li').addClass("show");
            },
            blur: function () {
                if ($(this).closest('li').is(".last")) {
                    $(this).parents(".rel-level1").removeClass("show");
                }
            }

        };

    function dropDownNavigation(navi) {
        navi.on("mouseover", ".rel-level1 > a, .rel-level1 >.navigation-title>a, .rel-level2 >.navigation-title>a", function () {
            $(this).closest('li').siblings().removeClass("show");
            $(this).closest('li.rel-level1').siblings().removeClass("show");
            $(this).closest('li.rel-level1').siblings().find(".show").removeClass("show");
            // $(this).closest('li').find('show').removeClass('show')
            var elem = $(this).closest('li').find(">ul");
            dropDownEvents.show(elem);
        });
        navi.on("mouseleave", ".rel-level1 > a, .rel-level1 >.navigation-title>a",
            dropDownEvents.queueHide);
        navi.on("mouseover", ".rel-level1 > ul", dropDownEvents.debounce);
        navi.on("mouseleave", ".rel-level1 > ul", dropDownEvents.queueHide);
        navi.on("focus", ".rel-level1 > a, .rel-level1 >.navigation-title>a, .rel-level1 >.navigation-title>a",
            dropDownEvents.focus);
        navi.on("blur", ".rel-level2 > a", dropDownEvents.blur);

        navi.find(".rel-level1").each(function () {
            if ($(this).find("ul").length) {
                $(this).addClass("submenu");
            }
        });

        navi.find(".rel-level2").each(function () {
            //if level2 menu have children
            if ($(this).parents('#header') > 0) {
                if ($(this).find("ul").length) {
                    $(this).addClass("submenu");
                    $(this).parents('.rel-level1').addClass('wide-nav');
                }
            }


            //if level2 menu should be navigation-image variant
            if ($(this).find('> img').length) {
                $(this).addClass("submenu navigation-image");
            }
        });

        navi.on("click", ".sxaToogleNavBtn", function () {
            var $this = jQuery(this);
            $this.find(".sxaWrappedList").toggleClass("hidden");
        })
    }

    function mobileNavigation(navi) {

        function checkChildren(nav) {
            nav.find(".rel-level1").each(function () {
                if (!$(this).find("ul").length) {
                    $(this).addClass("no-child");
                }
            });
        }

        function bindEvents(nav) {
            nav.find(".navigation-title").on("click", function (e) {
                var navlvl = $(this).closest('li'),
                    menuParent = navlvl.parents('.navigation');

                if (menuParent.hasClass('navigation-mobile')) {
                    if (!$(e.target).is("a")) {
                        if (navlvl.hasClass("active")) {
                            navlvl.find(">ul").slideToggle(function () {
                                navlvl.removeClass("active");
                            });
                        } else {
                            navlvl.find(">ul").slideToggle(function () {
                                navlvl.addClass("active");
                            });
                        }
                    }
                }
            });

            nav.find(".rel-level1 > a").on("focus", function () {
                $(this).siblings("ul").slideDown();
                $(this).closest('li').siblings().find("ul").slideUp();
            });
        }
        checkChildren(navi);
        bindEvents(navi);
    }

    function openBtnResolver(addFlag, $rootElem) {
        if (addFlag) {
            if ($rootElem.nextAll(".sxaToogleNavBtn").length == 0) {
                $rootElem.addClass("toggledNav");
                jQuery("<div class='sxaToogleNavBtn'><i class='fa fa-angle-double-right' aria-hidden='true'></i></div>").insertAfter($rootElem);
                jQuery("<ul class='sxaWrappedList hidden'></ul>").appendTo(".sxaToogleNavBtn");
            }
        } else {
            $rootElem.find(".sxaToogleNavBtn").remove();
        }
    }

    function horizontalHideOverflowed(rootElem,component) {
        var $rootElem = jQuery(rootElem),
            $component = jQuery(component),
            $listElem = $rootElem.find('>li'),
            lvl1 = $component.find('.rel-level1'),
            minHeight = jQuery(lvl1.get(0)).height();
            
        if(!($component.hasClass('navigation-main-horizontal') || $component.hasClass('navigation-main-vertical'))){
            return;
        }
        for (var listElemPos = $listElem.length; listElemPos > 0; listElemPos--) {
            if ($component.find('nav>ul').height()>minHeight){
                openBtnResolver(true, $rootElem);
                jQuery($listElem[listElemPos]).prependTo(".sxaWrappedList");
            }
        }

    }


    function toggleIcon(toggle) {
        $(toggle).on("click", function () {
            $(this).parents(".navigation").toggleClass("active");
            $(this).toggleClass("active");
        });
    }


    var api = {};

    api.initInstance = function (component) {
        if (component.hasClass("navigation-main")) {
            dropDownNavigation(component);
            mobileNavigation(component);
            horizontalHideOverflowed(component.find('nav>ul'),component);
        } else if (component.hasClass("navigation-mobile")) {
            mobileNavigation(component);
        }
    }

    api.init = function () {
        var navigation = $(".navigation:not(.initialized)");
        navigation.each(function () {
            api.initInstance($(this));
            $(this).addClass("initialized");
        });

        var toggle = $(".mobile-nav:not(.initialized)");
        toggle.each(function () {
            toggleIcon(this);
            $(this).addClass("initialized");
        });

    };

    return api;
}(jQuery, document));

XA.register("navigation", XA.component.navigation);