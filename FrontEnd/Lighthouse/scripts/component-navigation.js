(function($) {
    $(function() {
        //Sidebar
        var $sidebar = $("#sidebar");
        $sidebar.find('.panel-collapse').hide();
        $(document).on('click', '.menu-icon', function(e) {
            e.preventDefault();
            $('.navigation-mobile').slideToggle(300);
            $('.menu-icon').toggleClass('open');
        });

        //Header
        var $nav = $('.top-nav, #footer'),
            $languageSelector = $('.language-selector'),
            $expandableSearch = $nav.find('.expandable-searchbox'),
            $footerExpandableSearch = $("#footer").find('.expandable-searchbox');

        initLanguageSelector();
        initSearchbox();

        function closeOpenItems() {
            $nav.find('.open').removeClass('open');
        }

        function initLanguageSelector() {
            $languageSelector.find('.language-selector-select-link').click(function() {
                if (!$languageSelector.find('.language-selector-item-container').is(':visible')) {
                    closeOpenItems();
                }
            });
        }

        function initSearchButton($item) {
            if ($item.length === 0) {
                // The search button is not rendered on the page.
                return;
            }

            $item.find('.component-content-open').click(function(e) {
                e.stopPropagation();
            });

            $item.click(function() {
                openItem($(this));
            });
        }

        function initFooterSearchbox() {
            if ($($expandableSearch).parents('div#footer').length) {
                $($footerExpandableSearch).addClass("footer");
            }
        }

        function openItem($navItem) {
            var isOpen = $navItem.hasClass('open');

            closeOpenItems();

            if (!isOpen) {
                $navItem.addClass('open');
                $languageSelector.find('.language-selector-item-container').css('display', 'none');
            }
        }

        function initSearchbox() {
            initFooterSearchbox();
            initSearchButton($expandableSearch);
        }
    });
}(window.jQuery));