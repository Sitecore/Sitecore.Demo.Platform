(function($) {
    $(function() {
        //Sidebar 
        var $sidebar = $("#sidebar");
        $sidebar.find('.panel-collapse').hide();

        $(document).on('click', '#sidebar .panel-title > a', function(e){
            e.preventDefault();
            var targetSelector = $(this).attr('href');

            if(!$(targetSelector).parents('.panel-primary').hasClass('open')){
                $sidebar.find('.panel-primary.open').toggleClass('open').find('.panel-collapse').slideToggle(300);            
            }
            
            $(targetSelector).slideToggle(300);
            $(targetSelector).parent().toggleClass('open');        
        });

        $('button.sidebar-closed[data-toggle="sidebar"]').on("click", function () {
            var btn = $(this);
            $('html').addClass('show-sidebar-right');
            btn.hide();
            btn.siblings('button').show();
        });
    
        $('button.sidebar-opened[data-toggle="sidebar"]').on('click', function (e) {
            e.preventDefault();
            var selector = $(this).parent('div.btn-group-vertical');
            selector.find('button').hide();
            selector.find('button.sidebar-closed[data-toggle="sidebar"]').show();
            $('html').removeClass('show-sidebar-right');
        });
    });
}(window.jQuery));

//init jQuery Sidebar tabs if bootstrap isn't present
jQuery(window).bind("load", function() {
    var $ = $ || jQuery;
    if(typeof(jQuery.fn.popover) == 'undefined'){                
        $(document).on('click', '[data-toggle="tab"]', function(e){
            e.preventDefault();
            var href = $(this).attr('href');

            var $navTabs = $(this).parents('.nav-tabs'),
                $tabContent = $navTabs.siblings('.tab-content');
                
            if($(this).parent().hasClass('active')){
                return;
            }
            
            $navTabs.find('.active').removeClass('active');
            $(this).parent().addClass('active');

            $tabContent.find('.active').removeClass('active');
            $tabContent.find(href).addClass('active');
        });        
    }
 });