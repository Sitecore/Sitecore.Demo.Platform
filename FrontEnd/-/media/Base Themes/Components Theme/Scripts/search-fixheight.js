XA.component.searchEqualHeight = (function($) {
    var settings = {
        parentSelector: '.search-results.components',
        selector: 'li'
    };

    var api = {};

    function fixHeight() {

        $(settings.parentSelector).each(function() {
            var $elements = $(this).find(settings.selector),
                maxHeight = 0,
                maxPadding = 0;

            $elements.each(function() {
                $(this).css('min-height', 'inherit');

                if ($(this).height() > maxHeight) {
                    maxHeight = $(this).outerHeight(true);
                }

            });

            if (maxHeight > 0) {
                $elements.css({
                    'padding-bottom': maxPadding,
                    'min-height': maxHeight
                });
            }
        });
    }

    api.init = function() {
		//Latest Theme version ise flexboxes.
		//In case you don`t want use flex you should 
		//uncoment code below
		
       /* $(document).ready(function() {
            setTimeout(fixHeight, 0);
        });

        $(window).bind('resize', function() {
            fixHeight();
        });
		*/
    };
    return api;
}(jQuery, document));

XA.register("searchEqualHeight", XA.component.searchEqualHeight);