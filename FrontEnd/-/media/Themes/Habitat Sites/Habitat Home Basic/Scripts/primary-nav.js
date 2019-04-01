(function($) {
    $(function() { 
        var $primaryNav = $('.primary-nav'),
            $languageSelector = $('.language-selector'),
            $mainSearch = $primaryNav.find('.main-search'),
            $megadrop = $primaryNav.find('.megadrop');

        initPrimaryNavItem($mainSearch);
        initPrimaryNavItem($megadrop);


        $languageSelector.find('.language-selector-select-link').click(function(){
            if(!$languageSelector.find('.language-selector-item-container').is(':visible')) {                
                $primaryNav.find('.open').removeClass('open');    
            }
        });

        function initPrimaryNavItem($navItem) {            
            $navItem.click(function(){            
                openPrimaryNavItem($(this));
            });
            $navItem.find('>.component-content').click(function(e){
                e.stopPropagation();
            });
        }

        function openPrimaryNavItem($navItem) {
            var isOpen = $navItem.hasClass('open');
            $primaryNav.find('.open').removeClass('open');            
            
            if(!isOpen) {
                $navItem.addClass('open');
                $languageSelector.find('.language-selector-item-container').css('display', 'none');
            }
        }
    });
}(window.jQuery));