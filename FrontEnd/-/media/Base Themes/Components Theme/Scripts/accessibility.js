XA.component.accessibility = (function($) {
    var api = {};

    var getAllIndexedElement = function() {
        return $("body").find("[tabindex]");
    };

    var activateFirstElement = function() {
        api.indexedElements.eq(0).attr("tabindex", "0");
    };

    var getComponentName = function() {
        var $activeElement = $(document.activeElement),
            currComponent = $activeElement.closest(".initialized"),
            componentName = currComponent.length
                ? currComponent.attr("class").split(" ")[1]
                : "";
        return componentName;
    };

    var getWrapper = function(selector) {
        var $activeElement = $(document.activeElement);
        return $activeElement.closest(selector);
    };

    // Tab component helper start
    var activateTab = function(direction, currTab) {
        var activatedTab = currTab;
        if (direction == "left" && currTab.prev()) {
            currTab.attr("tabindex", "-1");
            activatedTab = currTab.prev();
        } else if (direction == "right" && currTab.next()) {
            currTab.attr("tabindex", "-1");
            activatedTab = currTab.next();
        }
        activatedTab
            .attr("tabindex", "0")
            .trigger("click")
            .focus();
    };
    // Tab component helper end

    // Accordion component helper start
    var activateItem = function(direction, currItem) {
        var itemWrapper = currItem.closest("li.item"),
            activatedTab = currItem,
            nextItem = itemWrapper.next(),
            prevItem = itemWrapper.prev();
        if (direction == "down" && nextItem) {
            currItem.attr("tabindex", "-1");
            activatedTab = nextItem;
        } else if (direction == "up" && prevItem) {
            currItem.attr("tabindex", "-1");
            activatedTab = prevItem;
        }
        activatedTab
            .find(".toggle-header")
            .attr("tabindex", "0")
            .focus();
    };
    // Tab component helper end

    //Flip component helper start
    var toggleFlip = function(currItem) {
        var nextItem = currItem.next().length ? currItem.next() : currItem.prev();
        currItem.attr("tabindex", "-1");
        nextItem
            .trigger("click")
            .attr("tabindex", "0")
            .focus();
    };
    //Flip component helper end

    // Binded logic
    var componentLogic = {
        tabs: function(keyCode) {
            var currTab = getWrapper("li.active");
            if (currTab) {
                if (keyCode == api.keys.right) {
                    activateTab("right", currTab);
                } else if (keyCode == api.keys.left) {
                    activateTab("left", currTab);
                }
            }
        },
        accordion: function(keyCode) {
            var currItem = getWrapper("div.toggle-header");
            if (currItem) {
                if (keyCode == api.keys.down) {
                    activateItem("down", currItem);
                } else if (keyCode == api.keys.up) {
                    activateItem("up", currItem);
                }
            }
        },
        flip: function(keyCode) {
            var currItem = getWrapper("[class*='Side']");
            if (currItem) {
                if (keyCode == api.keys.right || keyCode == api.keys.left) {
                    toggleFlip(currItem);
                }
            }
        }
    };

    var bindEvents = function(keyCode) {
        var component = getComponentName();
        if (componentLogic[component]) {
            componentLogic[component](keyCode);
        }
    };

    // For easy reference
    api.keys = {
        end: 35,
        home: 36,
        left: 37,
        up: 38,
        right: 39,
        down: 40,
        delete: 46,
        enter: 13,
        space: 32
    };

    api.indexedElements = getAllIndexedElement();

    api.watchEvents = function() {
        $(document).on("keyup", function(event) {
            var keyCode = event.keyCode;
            bindEvents(keyCode);
        });
    };

    api.initInstance = function() {
        api.watchEvents();
        activateFirstElement();
    };

    api.init = function() {
        api.initInstance();
    };

    return api;
})(jQuery, document);

XA.register("accessibility", XA.component.accessibility);
