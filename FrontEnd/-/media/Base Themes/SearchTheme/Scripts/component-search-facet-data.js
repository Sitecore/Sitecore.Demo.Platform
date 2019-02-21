XA.component.search.facet.data = (function ($, document) {

    var FacetDataModel = Backbone.Model.extend({
        defaults: {
        },
        initialize: function () {
        },
        getInitialFacetData: function () {
            var facetRequestData = this.getFacetRequestData(),
                data = facetRequestData.data,
                facetNames = [],
                language;

            for (var signature in data) {
                if (data.hasOwnProperty(signature)) {
                    //here we doesn't have any params after hash in the url co we are getting all data at once
                    facetNames = facetNames.concat(data[signature].normalFiltering);
                    facetNames = facetNames.concat(data[signature].partialFiltering);

                    //get language settings form Search Results rendering
                    language = this.getSearchResultsLanguage(signature);

                    if (facetNames.length > 0) {
                        XA.component.search.ajax.getData({
                            callback: function (data) {
                                XA.component.search.vent.trigger("facet-data-loaded", data);
                            },
                            url: XA.component.search.url.createMultiFacetUrl({
                                endpoint: facetRequestData.endpoint,
                                s: facetRequestData.s,
                                l: language
                            }, facetNames, signature)
                        });
                    }
                }
            }
        },
        filterFacetData: function (hashObj) {
            var facetRequestData = this.getFacetRequestData(hashObj),
                data = facetRequestData.data,
                requestData,
                language,
                scope;

            for (var signature in data) {
                if (data.hasOwnProperty(signature)) {
                    //get language settings form Search Results rendering
                    language = this.getSearchResultsLanguage(signature);
                    //get scope settings form Search Results rendering
                    scope = this.getSearchResultsScope(signature);

                    //make one request for data for facet controls with all hash params
                    if (data[signature].normalFiltering.length > 0) {
                        requestData = $.extend({ endpoint: facetRequestData.endpoint, s: scope, l: language}, hashObj);
                        XA.component.search.ajax.getData({
                            callback: function (data) {
                                XA.component.search.vent.trigger("facet-data-filtered", data);
                            },
                            url: XA.component.search.url.createMultiFacetUrl(requestData, data[signature].normalFiltering, signature)
                        });
                    }

                    //make as many requests as many controls which require partial filtering we have
                    //we will take all params from url hash without control facet name so controls won't collapse
                    if (data[signature].partialFiltering.length > 0) {
                        _.each(data[signature].partialFiltering, function (facetName) {
                            var hash = $.extend({}, hashObj);

                            //ensure that we will remove facet (despite it's lower case or not) from hash object in case of partial filtering  
                            delete hash[signature !== "" ? signature + "_" + facetName : facetName];
                            delete hash[signature !== "" ? signature + "_" + facetName.toLowerCase() : facetName.toLowerCase()];

                            requestData = $.extend({ endpoint: facetRequestData.endpoint, s: scope, l: language}, hash);

                            XA.component.search.ajax.getData({
                                callback: function (data) {
                                    XA.component.search.vent.trigger("facet-data-partial-filtered", data);
                                },
                                url: XA.component.search.url.createMultiFacetUrl(requestData, [facetName], signature)
                            });
                        });
                    }
                }
            }
        },
        getFacetRequestData: function (hashObj) {
            var that = this, data = [], requestData = {}, facetControl, control, endpoint, facetName;

            for (facetControl in XA.component.search.facet) {
                control = XA.component.search.facet[facetControl];
                if (typeof (control.getFacetDataRequestInfo) === "function") {
                    data = control.getFacetDataRequestInfo();
                    _.each(data, function (controlData) {
                        facetName = controlData.signature !== "" ? controlData.signature + "_" + controlData.facetName : controlData.facetName;
                        
                        if (!requestData.hasOwnProperty(controlData.signature)) {
                            that.initRequestObject(requestData, controlData);
                        }

                        if (!controlData.filterWithoutMe || (hashObj !== undefined && !hashObj.hasOwnProperty(facetName) && !hashObj.hasOwnProperty(facetName.toLowerCase()))) {
                            //if the control doesn't require partial filtering or control facet name isn't in the hash add it to "one request" list
                            requestData[controlData.signature].normalFiltering.push(controlData.facetName);
                            endpoint = controlData.endpoint;
                        } else {
                            requestData[controlData.signature].partialFiltering.push(controlData.facetName);
                            endpoint = controlData.endpoint;
                        }
                    });
                }
            }

            return { 
                endpoint: endpoint, 
                data: requestData
            };
        },
        initRequestObject: function (requestData, controlData) {
            requestData[controlData.signature] = {};
            requestData[controlData.signature].normalFiltering = [];
            requestData[controlData.signature].partialFiltering = [];
        },
        getSearchResultsLanguage: function(signature) {
            var searchResultsModel = this.getSearchResultsModelBySignature(signature);
            if (typeof searchResultsModel !== "undefined") {
                return searchResultsModel.get("dataProperties").l;
            }
            return "";
        },
        getSearchResultsScope: function (signature) {
            var searchResultsModel = this.getSearchResultsModelBySignature(signature);
            if (typeof searchResultsModel !== "undefined") {
                return searchResultsModel.get("dataProperties").s;
            }
            return "";
        },
        getSearchResultsModelBySignature: function(signature) {
            var searchResultModels = XA.component.search.results.searchResultModels,
                model = searchResultModels.filter(function (element) {
                    return element.get("dataProperties").sig === signature;
                })[0];

            return model;
        }
    });

    return new FacetDataModel();

}(jQuery, document));