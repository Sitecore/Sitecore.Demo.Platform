XA.component.toggle = (function($) {
    var api = {

    };

    var properties, instance;

    var getFlipList = function(instance) {
        return $(instance).find('.component.flip');
    }

    var openToggle = function(instance) {
        var flipList = getFlipList(instance),
            eventCalendar = $(instance).find('.event-calendar');
        $(instance).find('details').attr('open', 'open');
        if (eventCalendar.length){
            eventCalendar.trigger('resize')
        }
        if (flipList.lengt) {
            try {
                XA.component.flip.equalSideHeightInToggle(instance)
            } catch (e) {
                /* eslint-disable no-console */
                console.warn('Error during calculation height of Flip list in toggle'); // jshint ignore:line
                /* eslint-enable no-console */

            }
        }

    }
    var closeToggle = function(instance) {
        $(instance).find('details').removeAttr('open');
    }

    var setAnimation = function(instance) {
        $(instance).find('details summary~.component>.component-content').css({
            'animation-name': properties.easing,
            'animation-duration': (properties.speed || 500) + 'ms'
        })
    }
    var isExpandOnHover = function() {
        return properties.expandOnHover;
    }
    var isExpanded = function() {
        return properties.expandedByDefault;
    }
    var bindEvents = function(instance) {
        var summary = $(instance).find('summary');
        if (isExpandOnHover()) {
            summary.on('mouseenter', function() {
                openToggle(instance);
            })
        }
        if (isExpanded()) {
            openToggle(instance)
        }

        summary.on('click', function(event) {
            event.preventDefault();
            var details = $(this).closest('details')
            if (details.attr('open')) {
                closeToggle(instance);
            } else {
                openToggle(instance);
            }

        })

    }

    var initToggle = function(component) {
        bindEvents(component)
        setAnimation(component);
    };

    api.initInstance=function(component) {
        initToggle(component);
    }

    api.init = function() {
        $('.toggle:not(.initialized)').each(function() {
            instance = $(this);
            properties = instance.data('properties');
            api.initInstance(this,properties);
            instance.addClass('initialized');
        });
    };

    return api;
})(jQuery);

XA.register('component-toggle', XA.component.toggle);