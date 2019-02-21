XA.component.search.pageSize = (function ($, document) {

    var api = {},
        queryModel,
        queryParameters,
        searchResultModels,
        initialized = false,
        facetName = "p";

    var SearchPageSizeModel = Backbone.Model.extend({
        defaults: {
            sig: []
        }
    });

    var SearchPageSizeView = XA.component.search.baseView.extend({
        clicks: 0,
        initialize : function () {
            var inst = this,                
                pageSize = this.getCurrentPageSize(),
                dataProperties = this.$el.data().properties;


            this.model.set("sig", this.translateSignatures(dataProperties.searchResultsSignature, facetName));

            this.selectOption(pageSize);

            if (typeof pageSize === 'undefined' && dataProperties.defaultPageSize !== "") {
                inst.selectOption(dataProperties.defaultPageSize);
            }

            XA.component.search.vent.on("hashChanged", function (hash) {
                var sig = inst.model.get("sig"), i;
                for (i = 0; i < sig.length; i++) {
                    if (hash.hasOwnProperty(sig[i])) {
                        inst.selectOption(hash[sig[i]]);
                    }
                }
            });
        },
        events : {
            'click select': 'updatePageSizeClick',
            'change select' : 'updatePageSizeSelect'
        },
        getCurrentPageSize: function() {
            var hashObj = queryModel.parseHashParameters(window.location.hash),
                sig = this.model.get("sig"),
                i;

            for (i = 0; i < sig.length; i++) {
                if (hash.hasOwnProperty(sig[i])) {
                    return hash[sig[i]];
                }
            }
        },
        selectOption: function (pageSize) {
            if (pageSize !== undefined) {
                var selectedOption = this.$el.find("select option[value='" + pageSize + "']");
                selectedOption.siblings().removeAttr('selected');
                selectedOption.attr('selected', 'selected');
            }
        },
        updatePageSizeClick: function (event) {
            this.clicks++;
            if (this.clicks === 2) {
                var pageSize = $(event.target).find("option:selected").val();
                if (pageSize !== undefined) {
                    this.updatePageSize(pageSize);
                }
                this.clicks = 0;
            }
        },
        updatePageSize : function (pageSize) {
            var sig = this.model.get("sig");
            queryParameters.updateHash(this.updateSignaturesHash(sig, pageSize, {}));
        },
        updatePageSizeSelect : function (param) {
            var pageSize = param.currentTarget.value;
            this.updatePageSize(pageSize);
        }
    });


    api.init = function() {
        if ($("body").hasClass("on-page-editor") || initialized) {
            return;
        }

        queryModel = XA.component.search.query;
        queryParameters = XA.component.search.parameters;
        searchResultModels = XA.component.search.results.searchResultModels;

        var searchPageSize = $(".page-size");
        _.each(searchPageSize, function (elem) {
            var view = new SearchPageSizeView({el: $(elem), model: new SearchPageSizeModel()});
        });

        initialized = true;
    };

    api.getDefaultPageSizes = function() {
        var pageSizeComponents = $(".page-size"),
            data, 
            i, 
            defaultPageSize,
            searchResultsSignatures, 
            pageSize,
            result = [];

        if (pageSizeComponents.length > 0) {
            for (i = 0; i < pageSizeComponents.length; i++) {
                pageSize = $(pageSizeComponents[i]);
                data = pageSize.data();                
                searchResultsSignatures = data.properties.searchResultsSignature.split(',')
                defaultPageSize = data.properties.defaultPageSize;
                result.push({
                    signatures: searchResultsSignatures,
                    defaultPageSize: defaultPageSize !== '' ? defaultPageSize : pageSize.find("select option:first-child").val()
                });
            }
            return result;
        }        

        return -1;
    }

    return api;

}(jQuery, document));

XA.register('searchPageSize', XA.component.search.pageSize);