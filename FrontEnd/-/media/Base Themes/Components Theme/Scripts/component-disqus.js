XA.component.disqus = (function ($, document) {

    var api = {};

    function initDisqus(prop) {
        var dsq = document.createElement('script');
        dsq.type = 'text/javascript';
        dsq.async = true;
        dsq.src = '//' + prop.disqus_shortname + '.disqus.com/embed.js';
        (document.getElementsByTagName('head')[0] ||
            document.getElementsByTagName('body')[0]).appendChild(dsq);
    }

    api.initInstance = function (component, prop) {
        window.disqus_config = function () {
            this.page.url = prop.disqus_url;
            this.page.identifier = prop.disqus_identifier;
            this.page.title = prop.disqus_title;
            this.page.category_id = prop.disqus_category_id;
        };

        if (component.find("#disqus_thread").length > 0) {
            initDisqus(prop);
        }
    }


    api.init = function () {
        var disqus = $('.disqus:not(.initialized)');
        disqus.each(function () {
            var properties = $(this).data('properties');
            api.initInstance($(this),properties)
            $(this).addClass('initialized');
        });
    };

    return api;
}(jQuery, document));

XA.register('disqus', XA.component.disqus);