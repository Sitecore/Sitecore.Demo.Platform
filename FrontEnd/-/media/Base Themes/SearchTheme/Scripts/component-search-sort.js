XA.component.search.sort = (function ($, document) {

    var api = {},
        initialized = false,
        queryModel,
        queryParameters,
        getSignature;

    var SortModel = Backbone.Model.extend({
        defaults: {
            updateOrder: false,
            sig: []
        }
    });

    var SortView = XA.component.search.baseView.extend({
        initialize : function () {
            var dataProperties = this.$el.data();

            if (this.model) {
                this.model.set("sig", this.translateSignatures(dataProperties.properties.sig, "o"));
            }

            XA.component.search.vent.on("hashChanged", this.updateComponent.bind(this));
        },
        events : {
            'click .sort-results-group a': 'sortSearchResultsLink',
            'change select': 'sortSearchResultsSelect'
        },
        sortSearchResultsLink: function (event) {
            event.preventDefault();
            this.sortSearchResults($(event.currentTarget).parent());
        },
        sortSearchResultsSelect: function (event) {
            this.sortSearchResults($(event.currentTarget[event.currentTarget.options.selectedIndex]));
        },
        sortSearchResults: function (element) {
            var attributes = element.data(),
                sig = this.model.get("sig"),
                attrString = attributes.facet + ',' + attributes.direction;

            if (attributes.direction !== "") {
                queryParameters.updateHash(this.updateSignaturesHash(sig, attrString, {}));
            } else {
                queryParameters.updateHash(this.updateSignaturesHash(sig, "", {}));
            }
        },
        updateComponent: function (hash) {
            var optionToSelect, sortData,
                sig = this.model.get("sig"),
                i;

            for (i = 0; i < sig.length; i++) {
                if (hash.hasOwnProperty(sig[i])){
                    sortData = hash[sig[i]].split(',');
                    optionToSelect = this.$el.find("[data-facet='" + sortData[0] + "'][data-direction='" + sortData[1] + "']");
                } else {
                    optionToSelect = this.$el.find("[data-facet][data-direction]:first");
                }
            }        

            this.$el.find("[data-facet][data-direction]").removeAttr("selected");
            optionToSelect.attr("selected", "selected");
        }
    });

    api.init = function () {
        if ($("body").hasClass("on-page-editor") || initialized){
            return;
        }

        queryModel = XA.component.search.query;
        queryParameters = XA.component.search.parameters;

        var sort = $(".sort-results");
        _.each(sort, function (elem) {
            var sortModel = new SortModel(),
                view = new SortView({el: $(elem), model: sortModel});
        });

        initialzied = true;
    };

    api.getFirstSortingOption = function(signature) {
        var sortResults = $(".sort-results"),
            firstSort,
            attributes,
            attrString,
            thisSignatures,
            data,
            i, j;

        for (i = 0; i < sortResults.length; i++) {        
            if (typeof(signature) !== "undefined") {
                data = $(sortResults[i]).data();
                thisSignatures = data.properties.sig.split(',');

                for (j = 0; j < thisSignatures.length; j++) {
                    if (thisSignatures[j] === signature) {
                        firstSort = $(sortResults[i]).find("[data-facet][data-direction]");
                        attributes = firstSort.data();
                        if (attributes.direction !== "") {
                            attrString = attributes.facet + ',' + attributes.direction;
                            return attrString;
                        }
                    }
                }
            }
        }

        return -1;
    };

    return api;

}(jQuery, document));

XA.register('searchSort', XA.component.search.sort);
