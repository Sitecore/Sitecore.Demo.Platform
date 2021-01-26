(function($) {
    var refreshExperincePanel = function(reloadPage) {
        var panel = $("#experiencedata");
        $.ajax({
            url: "/api/Demo/ExperienceDataContent",
            method: "get",
            cache: false,
            success: function(data) {
                panel.replaceWith(data);
                $("#sidebar").find('.panel-collapse').hide();
            }
        }).done(function() {
            if (reloadPage) {
                window.location.href = window.location.href;
            }
        });
    };
    $(function() {
        $(".refresh-sidebar")
            .click(function() {
                $(this).fadeOut();
                refreshExperincePanel(false);
                $(this).fadeIn();
            });
        $(".end-visit")
            .click(function() {
                $(this).fadeOut();
                $.ajax({
                    url: "/api/Demo/EndVisit",
                    method: "get",
                    cache: false,
                    success: function() {
                        refreshExperincePanel(false);
                    }
                });
                $(this).fadeIn();
            });
        $(".new-contact")
            .click(function() {
                $(this).fadeOut();
                $.ajax({
                    url: "/api/Demo/NewContact",
                    method: "get",
                    cache: false,
                    success: function() {
                        refreshExperincePanel(true);
                    }
                });
                $(this).fadeIn();
            });
    });
})(jQuery);