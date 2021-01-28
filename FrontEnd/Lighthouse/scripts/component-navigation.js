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
            $expandableSearch = $nav.find('.coveo-expandable-searchbox'),
            $footerExpandableSearch = $("#footer").find('.coveo-expandable-searchbox'),
            $demoSearchToggler = $nav.find(".demo-search-toggler"),
            cookieName = "search-provider",
            activeClass = "active",
            cookieExpirationDays = 7,
            $coveoSelector = $nav.find(".coveo-search-placeholder, .demo-coveosearch-label"),
            $sxaSelector = $nav.find(".sxa-search-placeholder, .demo-sxasearch-label");

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
            if (!$($demoSearchToggler) || $($demoSearchToggler).length === 0) {
                return;
            }
            var activeSearchProvider = getCookie(cookieName);
            if (activeSearchProvider === "coveo") {
                $($demoSearchToggler).prop("checked", true);
            } else {
                $($demoSearchToggler).prop("checked", false);
            }

            setSearchState();

            $($demoSearchToggler).change(function() {
                setCookie(cookieName, $($demoSearchToggler)[0].checked ? "coveo" : "sxa", cookieExpirationDays);
                setSearchState();
            });
        }

        function setSearchState() {
            if ($($demoSearchToggler).length > 0 && $($demoSearchToggler)[0].checked) {
                $($coveoSelector).addClass(activeClass);
                $($sxaSelector).removeClass(activeClass);
            } else {
                $($coveoSelector).removeClass(activeClass);
                $($sxaSelector).addClass(activeClass);
            }
        }

        function setCookie(name, value, days) {
            var expires = "";
            if (days) {
                var date = new Date();
                date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
                expires = "; expires=" + date.toUTCString();
            }
            document.cookie = name + "=" + (value || "") + expires + "; path=/";
        }

        function getCookie(name) {
            var nameAndEqualSign = name + "=";
            var cookies = document.cookie.split(';');
            for (var i = 0; i < cookies.length; i++) {
                var cookie = cookies[i].trim();
                if (cookie.indexOf(nameAndEqualSign) == 0) {
                    return cookie.substring(nameAndEqualSign.length, cookie.length);
                }
            }
            return null;
        }
    });
}(window.jQuery));