(function ($) {
    $(function () {
        var $primaryNav = $('.primary-nav'),
            $languageSelector = $('.language-selector'),
            $mainSearch = $primaryNav.find('.main-search'),
            $megadrop = $primaryNav.find('.megadrop');

        initMainSearch($mainSearch);
        initPrimaryNavItem($megadrop);


        $languageSelector.find('.language-selector-select-link').click(function () {
            if (!$languageSelector.find('.language-selector-item-container').is(':visible')) {
                $primaryNav.find('.open').removeClass('open');
            }
        });

        function initMainSearch($item) {
            $item.click(function () {
                openItem($(this));

                $(".search-box-input").click(function (e) {
                    e.stopPropagation(); 
                });
            });
        }

        function initPrimaryNavItem($item) {
            $item.click(function () {
                openItem($(this));
            });

            $item.find('>.component-content').click(function (e) {
                e.stopPropagation();
            });
        }

        function openItem($navItem) {
            var isOpen = $navItem.hasClass('open');
            $primaryNav.find('.open').removeClass('open');

            if (!isOpen) {
                $navItem.addClass('open');
                $languageSelector.find('.language-selector-item-container').css('display', 'none');
            }
        }
    });
}(window.jQuery));