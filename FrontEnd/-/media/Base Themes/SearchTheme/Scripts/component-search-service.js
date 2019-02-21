XA.component.search.service = (function ($, document) {

    'use strict';

    var searchResultsModels = XA.component.search.results.searchResultModels,
        urlHelperModel = XA.component.search.url,
        queryModel = XA.component.search.query,
        queryParameters = XA.component.search.parameters,
        apiModel = XA.component.search.ajax,
        SearchServiceModel;

    SearchServiceModel = Backbone.Model.extend({
        defaults: {
        },
        initialize: function () {
            var that = this;
            XA.component.search.vent.on('orderChanged', function (data) {
                that.getData(data);
            });
        },
        getData: function (overrideProps) {
            if (searchResultsModels.length > 0) {
                this.getSearchResultsData(overrideProps);
            } else {
                this.getEndpointAndSearch(overrideProps);
            }
        },
        getSearchResultsData: function(overrideProps) {
            var hash = queryModel.parseHashParameters(window.location.hash),
                that = this,
                defaultSortOrder,
                offsetSignature,
                searchResultsDefaultPageSize,
                searchResultsDefaultVariant,
                signature,
                mergedProps,
                url;

            _.each(searchResultsModels, function (searchModel) {
                signature = searchModel.get('dataProperties').sig !== null
                    ? encodeURIComponent(searchModel.get('dataProperties').sig)
                    : "";
                offsetSignature = signature !== '' ? signature + "_e" : "e";

                if (!searchModel.get('dataProperties').autoFireSearch) {
                    //If hash is not present or no creteria with search model signature present in hash, then do not fire search
                    if (!hash || !queryModel.isSignaturePresentInHash(hash, signature)) {
                        return;
                    }
                }

                defaultSortOrder = searchModel.get('dataProperties').defaultSortOrder;
                searchResultsDefaultPageSize = searchModel.get('dataProperties').p;
                searchResultsDefaultVariant = searchModel.get('dataProperties').v;

                //if we have singleRequestMode with signature that we are getting data just for that signature
                if (typeof overrideProps !== "undefined" &&
                    overrideProps.hasOwnProperty("singleRequestMode") &&
                    overrideProps["singleRequestMode"] != signature) {
                    return;
                }

                //if there is no page size param in the query but we have page size component on the page then take
                //default page size or first (if default isn't set)
                hash = that.getDefaultDefaultPageSize(signature, searchResultsDefaultPageSize, hash);

                //this is the case that we have facet that is filtering items by language
                //when someone is filtering by certain language, and will remove that filter later on then without
                //this if statement search results language parameter was not consider - because l="" from hash object
                //will override default language from dataProperties
                if (hash.hasOwnProperty('l') && hash.l === '') {
                    hash.l = searchModel.get('dataProperties').l;
                }

                mergedProps = $.extend({}, searchModel.get('dataProperties'), hash);
                mergedProps = $.extend(mergedProps, overrideProps);

                if (mergedProps.hasOwnProperty('loadMore')) {
                    searchModel.set('loadMore', true);
                    delete mergedProps.loadMore;

                    var newOffset =  searchModel.get('loadMoreOffset') + mergedProps.p;
                    searchModel.set('loadMoreOffset', newOffset);
                    mergedProps[offsetSignature] = newOffset;
                }
                else if(searchModel.get('loadMoreOffset')){ //if results reload is caused by variant change, loadMoreOffset should be considered in request, so after changing variant, the same number of results will be visible
                    if(mergedProps.p !== 0){
                        mergedProps.p = mergedProps.p * (1 + searchModel.get('loadMoreOffset') / mergedProps.p);
                        mergedProps[offsetSignature] = 0;
                        searchModel.set('loadMoreOffset', 0);
                    }
                    delete mergedProps.variantChanged;
                }

                mergedProps = that.getSortOrder(signature, mergedProps, defaultSortOrder);
                mergedProps = that.setVariant(signature, mergedProps, searchResultsDefaultVariant);

                url = urlHelperModel.createSearchUrl(mergedProps, signature);
                if (!url) {
                    return;
                }

                if (!searchModel.checkBlockingRequest()) {
                    searchModel.blockRequests(true);
                    XA.component.search.vent.trigger('results-loading', searchModel.cid);
                    apiModel.getData({
                        callback: function(data) {
                            data.Signature = data.Signature !== null ? data.Signature : '';
                            XA.component.search.vent.trigger('results-loaded', {
                                dataCount: data.Count,
                                data: data.Results,
                                pageSize : data.Signature !== '' && mergedProps.hasOwnProperty(data.Signature + '_p') ? mergedProps[data.Signature + '_p'] : mergedProps.p,
                                offset : data.Signature !== '' && mergedProps.hasOwnProperty(data.Signature + '_e') ? mergedProps[data.Signature + '_e'] : mergedProps.e,
                                searchResultsSignature: data.Signature,
                                loadMore : searchModel.get('loadMore')
                            });
                        },
                        url: url
                    });
                }
            });
        },

        getSearchData: function(overrideProps) {
            var hash = queryModel.parseHashParameters(window.location.hash),
                mergedProps = $.extend({}, hash, mergedProps, overrideProps),
                signatures = [],
                firstSortingOption,
                url,
                i;


            mergedProps = this.getDefaultDefaultPageSize("", 0, mergedProps);


            //if there is no sort param in the query but we have sort results component on the page then take first sorting option
            if (!mergedProps.hasOwnProperty('o') && typeof XA.component.search.sort !== 'undefined') {
                firstSortingOption = XA.component.search.sort.getFirstSortingOption();
                if (firstSortingOption !== -1) {
                    mergedProps.o = firstSortingOption;
                }
            }

            //currently we are takign endpoint from Map component (atm it's only which can handle showing of search results)
            mergedProps.endpoint =  XA.component.map.getSearchEndpoint();
            //the same with signature
            signatures = XA.component.map.getSignatures();

            for (i = 0; i < signatures.length; i++) {
                mergedProps.sig = signatures[i];
                url = urlHelperModel.createSearchUrl(mergedProps, signatures[i]);

                if (!url) {
                    return;
                }

                apiModel.getData({
                    callback: function(data) {
                        data.Signature = data.Signature !== null ? data.Signature : '';
                        XA.component.search.vent.trigger('results-loaded', {
                            dataCount: data.Count,
                            data: data.Results,
                            pageSize : mergedProps.p,
                            offset : 0,
                            searchResultsSignature: data.Signature
                        });
                    },
                    url: url
                });
            }
        },
        getEndpointAndSearch: function() {
            if (typeof (XA.component.map.getSearchEndpoint()) !== 'undefined') {
                this.getSearchData();
            } else {
                setTimeout(this.getEndpointAndSearch.bind(this), 100);
            }
        },
        getSortOrder: function(signature, mergedProps, searchResultsDefaultSortOrder) {
            var firstSortingOption = XA.component.search.sort.getFirstSortingOption(signature),
                paramName = signature !== '' ? signature + '_o' : 'o',
                obj = {},
                defaultSortOrder = "",
                defaultSigned = {};


            if (firstSortingOption !== -1) {
                defaultSortOrder = firstSortingOption;
            } else if (searchResultsDefaultSortOrder !== "") {
                defaultSortOrder = searchResultsDefaultSortOrder;
            }

            if (!mergedProps.hasOwnProperty(paramName) && defaultSortOrder !== "") {
                delete mergedProps.defaultSortOrder;
                delete mergedProps.o;

                mergedProps[paramName] = defaultSortOrder;
                obj[paramName] = defaultSortOrder;
            }
            defaultSigned[paramName] = defaultSortOrder;
            queryParameters.registerDefault(defaultSigned);

            return mergedProps;
        },


        getDefaultDefaultPageSize: function (signature, searchResultsDefaultPageSize, hash) {
            var defaultPageSizes = XA.component.search.pageSize.getDefaultPageSizes(),
                paramName = signature !== "" ? signature + "_p" : "p",
                pageSizes;

            if (!hash.hasOwnProperty(paramName) && typeof XA.component.search.pageSize !== 'undefined') {
                if (defaultPageSizes !== -1) {
                    pageSizes = defaultPageSizes.filter(function (x) {
                        return x.signatures.indexOf(signature) !== -1;
                    });
                    if (pageSizes.length > 0) {
                        hash[paramName] = pageSizes[0].defaultPageSize;
                    }
                }
            }

            if (!hash.hasOwnProperty(paramName) && searchResultsDefaultPageSize != 0) {
                hash[paramName] = searchResultsDefaultPageSize;
            }

            var defaultParam = {};
            defaultParam[paramName] = searchResultsDefaultPageSize;
            queryParameters.registerDefault(defaultParam);
            return hash;
        },
        setVariant: function (signature, mergedProps, searchResultsDefaultVariant) {
            var variantMappings = XA.component.search.variantFilter.getVariantMappings(signature),
                paramName = signature !== "" ? signature + "_v" : "v",
                variantIndex;

            if (signature === "" && $.isEmptyObject(variantMappings)) {
                //there is no signature and no variant selector on the page so do nothing - mergedPropes should have v param
            } else if (signature === "" && mergedProps.hasOwnProperty(paramName) && variantMappings.hasOwnProperty(mergedProps[paramName])) {
                //there is no signature, but there is variant selector on the page and it was clicked
                mergedProps[paramName] = variantMappings[mergedProps[paramName]].id;
            } else if (signature === "" && variantMappings.hasOwnProperty(0)) {
                //there is no signature, but there is variant selector on the page so take first variant from it
                mergedProps[paramName] = variantMappings[0].id;
            } else {
                delete mergedProps.v;
                if (mergedProps.hasOwnProperty(paramName)) {
                    //variant swither was clicked
                    mergedProps[paramName] = variantMappings[mergedProps[paramName]].id;
                } else {
                    //if there is variant selector on the page, take first variant form the component, otherwhise use search results default variant
                    if (variantMappings.hasOwnProperty(0)) {
                        mergedProps[paramName] = variantMappings[0].id;
                    } else {
                        mergedProps[paramName] = searchResultsDefaultVariant;
                    }
                }
            }

            var defaultVariant = {};
            defaultVariant[paramName] = 0;
            queryParameters.registerDefault(defaultVariant);

            return mergedProps;
        }
    });

    return new SearchServiceModel();

}(jQuery, document));
