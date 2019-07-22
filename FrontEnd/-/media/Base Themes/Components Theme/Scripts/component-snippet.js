XA.component.snippet = (function ($) {

    var pub = {},
        instance;
    

    pub.initInstance=function(component,snippetModule) {
        
    }


    pub.init = function () {
        var $snippets = $(".snippet:not(.initialized)");
        $snippets.each(function () {
            var $snippetModule = $(this).find(".snippet-inner");
            instance = $(this);
            pub.initInstance(instance,$snippetModule);
            $(this).addClass("initialized");
        });
    }

    return pub;
}(jQuery));

XA.register("snippet", XA.component.snippet);