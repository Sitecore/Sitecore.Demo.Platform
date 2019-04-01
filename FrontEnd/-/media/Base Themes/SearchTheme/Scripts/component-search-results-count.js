XA.component.search.results.count = (function ($, document) {

    var api = {}, initialized = false;

    var SearchResultCountView = Backbone.View.extend({
        initialize: function(){
            var dataProperties = this.$el.data(),
                resultsCountContainer = this.$el.find(".results-count"),
                inst = this;

            this.resultsCountText = resultsCountContainer.html();

            //check if we're opening page from disc - if yes then we are in Creative Exchange mode so let's show fake results count
            if (window.location.href.startsWith("file://")) {
                resultsCountContainer.html(inst.resultsCountText.replace('{count}', 1));
                inst.$el.find(".results-count").show();
                return;
            } 

            XA.component.search.vent.on("results-loaded", function (data) {
                resultsCountContainer.html(inst.resultsCountText.replace('{count}', data.dataCount));
                inst.$el.find(".results-count").show();
            });            
        }
    });


    api.init = function () {
        if ($("body").hasClass("on-page-editor") || initialized) {
            return;
        }

        var searchResults = $(".search-results-count");
        _.each(searchResults, function (elem) {
            var searchResultsCountView = new SearchResultCountView({el: $(elem)});
        });

        initialized = true;
    };

    return api;

}(jQuery, document));

XA.register('searchResultsCount', XA.component.search.results.count);