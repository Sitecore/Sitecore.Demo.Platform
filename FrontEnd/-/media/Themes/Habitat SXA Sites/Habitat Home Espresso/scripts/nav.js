(function($, window) {
    $(function() {
        var $meganav = $('.megadrop-nav');
        
        $meganav.find('.rel-level1').first().addClass('active');

        $meganav.find('.rel-level1').each(function(){
            $(this).hover(function(){
                if($(window).width() > 768) {
                    $meganav.find('.active').removeClass('active');
                    $(this).addClass('active');
                }                
            });
        });

        /* $(".megadrop .megadrop-navbar ul li div.megadrop-secondary-title-link, .megadrop .megadrop-secondary-title-link").on("mouseover", function () {
            if($(window).width() > 768) {
                $(this).closest('ul').find('li').removeClass('active');
                $(this).closest('li').addClass('active');
            }
        }); */

        $meganav.find('.megadrop-secondary-title-link a').click(function(e){
            var $li = $(this).closest('li');
    
            if($(window).width() < 768) {
                if($li.hasClass('active')){
                    $li.removeClass('active');
                }else {
                    $(this).closest('ul').find('li').removeClass('active');        
                    $li.addClass('active');
                }            
                e.preventDefault();
                return;
            }        
     
            $(this).closest('ul').find('li').removeClass('active');
            $li.addClass('active');
        });
    
        $meganav.find(".megadrop-secondary-title-link a").on("touchstart", function (e) {
            e.preventDefault();
            $(this).closest('ul').find('li').removeClass('active');
            $(this).closest('li').addClass('active');
        });

    });
}(window.jQuery, window));