XA.component.search.loadMore = (function ($, document) {

    var api = {},
        initialized = false,
        SearchLoadMoreModel,
        SearchLoadMoreView;

    SearchLoadMoreModel = Backbone.Model.extend({
        defaults: {
            dataProperties: {},
            sig: []
        }
    });

    SearchLoadMoreView = Backbone.View.extend({
        initialize : function () {
            var dataProperties = this.$el.data(),
                that = this;

            if (dataProperties.properties.searchResultsSignature === null){
                dataProperties.properties.searchResultsSignature = "";
            }

            if (this.model) {
                this.model.set('sig', dataProperties.properties.searchResultsSignature.split(','));
            }

            XA.component.search.vent.on("results-loaded", function (results) {
                var sig = that.model.get('sig'),
                    i;

                for (i = 0; i < sig.length; i++) {
                    if (results.pageSize >= results.dataCount) //in case if results.offset and/or results.searchResultsSignature are undefined
                    {
                        that.$el.hide();
                    } else {
                        if (sig[i] === results.searchResultsSignature)
                            if (results.offset + results.pageSize >= results.dataCount) {
                                that.$el.hide();
                            } else {
                                that.$el.show();
                            }
                    }
                }
            });            
        },
        events: {
            'mousedown input': 'loadMore'
        },
        loadMore: function () {
            var sig = this.model.get('sig'),
                i;

            for (i = 0; i < sig.length; i++) {                
                XA.component.search.vent.trigger('loadMore', {
                    sig: sig[i]
                });
            }
        }
    });

    api.init = function () {
        if ($('body').hasClass('on-page-editor') || initialized) {
            return;
        }

        var searchLoadMore = $('.load-more');
        _.each(searchLoadMore, function (elem) {
            new SearchLoadMoreView({el: $(elem), model : new SearchLoadMoreModel()});
        });

        initialized = true;
    };

    return api;

}(jQuery, document));

XA.register('searchLoadMore', XA.component.search.loadMore);
