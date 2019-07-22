XA.component.search.facet.dropdown = (function ($, document) {

    "use strict";

    var api = {},
        urlHelperModel,
        queryModel,
        initialized = false;

    var FacetDropdownModel = XA.component.search.baseModel.extend({
        defaults: {
            template: "<% _.forEach(results, function(result){" +
                "%><option data-facetName='<%= result.Name !== '' ? result.Name : '_empty_' %>' <%= result.Selected !== undefined ? 'selected' : '' %> ><%= result.Name !== '' ? result.Name : emptyText %> (<%= result.Count %>)</option><%" +
                "}); %>",
            dataProperties: {},
            blockNextRequest: false,
            resultData: {},
            optionSelected: false,
            sig: []
        },
        initialize: function () {
            //event to get data at the begining or in case that there are no hash parameters in the url - one request for all controls
            XA.component.search.vent.on("facet-data-loaded", this.processData.bind(this));

            //event to get filtered data
            XA.component.search.vent.on("facet-data-filtered", this.processData.bind(this));

            //if in the url hash we have this control facet name (someone clicked this control) then we have to listen for partial filtering
            XA.component.search.vent.on("facet-data-partial-filtered", this.processData.bind(this));

            //event after change of hash
            XA.component.search.vent.on("hashChanged", this.updateComponent.bind(this));
        },
        toggleBlockRequests: function () {
            var state = this.get("blockNextRequest");
            this.set(this.get("blockNextRequest"), !state);
        },
        processData: function (data) {
            var inst = this,
                hashObj = queryModel.parseHashParameters(window.location.hash),
                sig = inst.get("sig"),
                dataProperties = this.get("dataProperties"),
                searchResultsSignature = dataProperties.searchResultsSignature.split(','),
                sortOrder = dataProperties.sortOrder,
                facet = dataProperties.f,
                facetItem,
                facedData,
                i, j;

            for (j = 0; j < searchResultsSignature.length; j++) {
                if (data.Facets.length > 0 && (data.Signature === searchResultsSignature[j]) || data.Signature === "" || data.Signature === null) {
                    facedData = _.find(data.Facets, function (f) {
                        return f.Key.toLowerCase() === facet.toLowerCase();
                    });

                    if (facedData === undefined) {
                        return;
                    }

                    for (i = 0; i < sig.length; i++) {
                        if (!jQuery.isEmptyObject(_.pick(hashObj, sig[i]))) {
                            if (hashObj[sig[i]] !== "") {
                                facetItem = _.where(facedData.Values, { Name: hashObj[sig[i]] });
                                if (facetItem.length === 0) {
                                    facetItem = _.where(facedData.Values, { Name: "" });
                                }
                                if (facetItem.length > 0) {
                                    facetItem[0].Selected = true;
                                    inst.optionSelected = true;
                                }
                            }
                        }
                    }

                    this.sortFacetArray(sortOrder, facedData.Values);
                    inst.set({ resultData: facedData.Values });
                }
            }
        },
        updateComponent: function (hash) {
            var sig = this.get("sig"), i, facetPart;

            for (i = 0; i < sig.length; i++) {
                facetPart = sig[i].toLowerCase();
                if (hash.hasOwnProperty(facetPart) && hash[facetPart] !== "") {
                    this.set({ optionSelected: true });
                } else {
                    this.set({ optionSelected: false });
                }
            }
        }
    });

    var FacetDropdownView = XA.component.search.baseView.extend({
        initialize: function () {
            var dataProperties = this.$el.data();
            this.properties = dataProperties.properties;

            if (this.model) {
                this.model.set({ dataProperties: this.properties });
                this.model.set("sig", this.translateSignatures(this.properties.searchResultsSignature, this.properties.f));
            }

            this.model.on("change", this.render, this);
        },
        events: {
            "change .facet-dropdown-select": "updateFacet",
            "click .bottom-remove-filter, .clear-filter": "clearFilter"
        },
        updateFacet: function (param) {
            var $selectedOption = this.$el.find(".facet-dropdown-select").find("option:selected"),
                facetName = $selectedOption.data("facetname"),
                sig = this.model.get("sig");

            if (facetName === "") {
                this.model.set({ optionSelected: false });
            } else {
                this.model.set({ optionSelected: true });
            }
            queryModel.updateHash(this.updateSignaturesHash(sig, facetName, {}));
        },
        render: function () {
            var inst = this,
                resultData = inst.model.get("resultData"),
                dropdown = this.$el.find(".facet-dropdown-select"),
                emptyValueText = inst.model.get('dataProperties').emptyValueText,
                facetClose = this.$el.find(".facet-heading > span"),
                notSelectedOption = dropdown.find("option:first"),
                notSelectedOptionEntry = $("<option />"),
                template = _.template(inst.model.get("template")),
                templateResult = template({ results: resultData, emptyText: emptyValueText });


            notSelectedOptionEntry.text(notSelectedOption.text());
            notSelectedOptionEntry.data("facetname", "");

            if (this.model.get("optionSelected")) {
                facetClose.addClass("has-active-facet");
            } else if (notSelectedOption.data("facetname") === "") {
                facetClose.removeClass("has-active-facet");
            }

            dropdown.empty().append(notSelectedOptionEntry).append(templateResult);
        },
        clearFilter: function () {
            var dropdown = this.$el.find(".facet-dropdown-select"),
                facetClose = this.$el.find(".facet-heading > span"),
                sig = this.model.get("sig");

            queryModel.updateHash(this.updateSignaturesHash(sig, "", {}));
            this.model.set({ optionSelected: false });
            facetClose.removeClass("has-active-facet");
            dropdown.val(dropdown.find("option:first").val());
        }
    });


    api.init = function () {
        if ($("body").hasClass("on-page-editor") || initialized) {
            return;
        }

        queryModel = XA.component.search.query;
        urlHelperModel = XA.component.search.url;

        var facetDropdownList = $(".facet-dropdown");
        _.each(facetDropdownList, function (elem) {
            var view = new FacetDropdownView({ el: $(elem), model: new FacetDropdownModel() });
        });

        initialized = true;
    };

    api.getFacetDataRequestInfo = function () {
        var facetDropdownList = $(".facet-dropdown"),
            result = [];

        _.each(facetDropdownList, function (elem) {
            var properties = $(elem).data().properties,
                signatures = properties.searchResultsSignature.split(','),
                i;

            for (i = 0; i < signatures.length; i++) {
                result.push({
                    signature: signatures[i] === null ? "" : signatures[i],
                    facetName: properties.f,
                    endpoint: properties.endpoint,
                    filterWithoutMe: true
                });
            }
        });

        return result;
    };

    return api;

}(jQuery, document));

XA.register("facetDropdown", XA.component.search.facet.dropdown);