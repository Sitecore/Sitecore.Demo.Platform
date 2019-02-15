XA.component.breadcrumb = (function ($) {
    var api = {};

    function BreadcrumbManager(elem) {
        this.breadcrumb = elem;
        this.hideHistory = [];
        this.hideHistory.elems = [];
        this.hideHistory.widths = [];
    }

    BreadcrumbManager.prototype.getElements = function ($list) {
        var elements = [];

        $list.find('li').each(function () {
            elements.push(this);
        });

        return elements;
    };

    BreadcrumbManager.prototype.calculateListElementsWidth = function ($list) {
        var widthSum = 0;

        $list.find('>li').each(function () {
            widthSum += $(this).width();
        });

        return widthSum;
    };

    BreadcrumbManager.prototype.calculateWidth = function () {
        var inst = this,
            $list = $(inst.breadcrumb).find('nav>ol'),
            listWidth = $list.width(),
            widthSum = this.calculateListElementsWidth($list),
            elements = this.getElements($list),
            $elementToHide,
            removeIndx = 0;

        var width = inst.hideHistory.widths[inst.hideHistory.widths.length - 1];
        if (listWidth > (widthSum + width)) {
            var elem = inst.hideHistory.elems.pop();
            inst.hideHistory.widths.pop();
            $(elem).removeClass('item-hide');
        }


        while ((listWidth < widthSum) && (elements.length > 2)) {
            removeIndx = Math.round(elements.length / 2) - 1;
            $elementToHide = $(elements[removeIndx]);

            inst.hideHistory.elems.push(elements[removeIndx]);
            inst.hideHistory.widths.push($elementToHide.width());
            $elementToHide.addClass('item-hide');

            widthSum = inst.calculateListElementsWidth($list);
            elements.splice(removeIndx, 1);
        }
    };

    BreadcrumbManager.prototype.init = function () {
        var inst = this;

        inst.calculateWidth();
        $(window).resize(function () {
            inst.calculateWidth();
        });
    };

    BreadcrumbManager.prototype.makeNavigation = function () {
        var breadcrumb = $(this.breadcrumb),
            children = breadcrumb.find('li > ol');

        if (children.length > 0) {
            breadcrumb.addClass('breadcrumb-navigation');
        }
    };

    api.initInstance = function (component) {
        var breadcrumb = new BreadcrumbManager(component),
            $component = $(component);
        if ($component.hasClass('breadcrumb-hide')) {
            breadcrumb.init();
        } else {
            breadcrumb.makeNavigation();
        }

    }

    api.init = function () {
        var breadcrumb = $('.breadcrumb:not(.initialized)');
        breadcrumb.each(function () {
            api.initInstance($(this))
            $(this).addClass('initialized');
        });
    };

    return api;

})(jQuery, document);

XA.register('breadcrumb', XA.component.breadcrumb);