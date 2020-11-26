(function($) {
    $(function() {
        //Sidebar
        var $sidebar = $("#sidebar");
        $sidebar.find('.panel-collapse').hide();

        $(document).on('click', '.menu-icon', function(e){
            e.preventDefault();
            $('.navigation-mobile').slideToggle(300);
            $('.menu-icon').toggleClass('open');
        });

        //Main Search
        var $topNav = $('.top-nav'),
            $languageSelector = $('.language-selector'),
            $mainSearch = $topNav.find('.main-search');

        initMainSearch($mainSearch);
        initLanguageSelector();

        function initMainSearch($item) {
            $item.click(function () {
                openItem($(this));

                $(".search-box-input").click(function (e) {
                    e.stopPropagation();
                });
            });
        }

        function openItem($navItem) {
            var isOpen = $navItem.hasClass('open');
            $topNav.find('.open').removeClass('open');

            if (!isOpen) {
                $navItem.addClass('open');
                $languageSelector.find('.language-selector-item-container').css('display', 'none');
            }
        }

        function initLanguageSelector() {
            $languageSelector.find('.language-selector-select-link').click(function () {
                if (!$languageSelector.find('.language-selector-item-container').is(':visible')) {
                    $topNav.find('.open').removeClass('open');
                }
            });
        }
    });
}(window.jQuery));