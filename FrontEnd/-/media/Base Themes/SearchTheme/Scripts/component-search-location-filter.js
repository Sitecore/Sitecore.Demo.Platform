XA.component.search.locationfilter = (function ($, document) {

    "use strict";

    var api = {},
        locationFilters = [],
        views = [],
        queryModel,
        urlHelperModel,
        initialized = false,
        scriptsLoaded = false,
        mapsConnector = XA.connector.mapsConnector,
        LocationFilterModel,
        LocationFilterView,
        initialize;

    initialize = function() {
        var i, view;
        for (i = 0; i < locationFilters.length; i++) {
            //crate Backbone.js views - one view per component on the page
            view = new LocationFilterView({el: locationFilters[i], model: new LocationFilterModel()});
        }
    };

    LocationFilterModel = Backbone.Model.extend({
        defaults: {
            dataProperties: {},
            sig: []
        },
        initAutocompleteEngine : function() {
            var _this = this,
                searchEngine;

            //initialize predictive only when number of predictions is non-zero
            if(_this.get("dataProperties").p > 0){
                    searchEngine = new Bloodhound({
                        datumTokenizer: Bloodhound.tokenizers.obj.whitespace("name"),
                        queryTokenizer: Bloodhound.tokenizers.whitespace,
                        limit :  _this.get("dataProperties").p,
                        remote : {
                            url : "-",
                            replace : function(){
                                return Date.now().toString();
                            },
                            transport : function(options, onSuccess, onError){
                                var queryParams =  _this.get("queryParams");
                                if(!queryParams.text){
                                    onSuccess([]);
                                    return;
                                }
                                mapsConnector.locationAutocomplete(queryParams,
                                    function (results) { //success
                                        var simplifiedResults = results.map(function(result) {
                                            if (result.hasOwnProperty("text")) {
                                                return result.text;
                                            }
                                            return result;
                                        });

                                        onSuccess(simplifiedResults);
                                    },
                                    function(){ //fail
                                        onError("Could not autocomplete");
                                    });
                            }
                        }
                });
                searchEngine.initialize();
                this.set({"searchEngine" : searchEngine});
            }
        }
    });

    LocationFilterView = XA.component.search.baseView.extend({
        initialize: function () {
            var inst = this,
                dataProperties = this.$el.data(),
                $textBox = this.$el.find(".location-search-box-input");

            if (dataProperties.properties.searchResultsSignature === null) {
                dataProperties.properties.searchResultsSignature = "";
            }

            this.model.set({dataProperties: dataProperties.properties});
            this.model.set({sig: dataProperties.properties.searchResultsSignature.split(',')});
            this.model.set({queryParams : {maxResults : dataProperties.p, text : ""}});
            this.model.initAutocompleteEngine();
            var autocompleteEngine = this.model.get("searchEngine");

            if(autocompleteEngine){
                $textBox.typeahead({
                    hint : true,
                    minLength : 2
                },
                {
                    source : autocompleteEngine.ttAdapter(),
                    templates : {
                        suggestion : function(data) {
                            return '<div class="suggestion-item">' + data + '</div>';
                        }
                    }
                }).on("typeahead:selected",function(args, selected){
                    inst.translateUserLocation(selected);
                    $textBox.typeahead("val", selected);
                });
            }


            this.addressLookup();
        },
        events: {
            "click .location-search-box-button": "addressLookup",
            "keypress .location-search-box-input": "searchTextChanges",
            "keyup .location-search-box-input" : "autocomplete"
        },

        addressLookup: function(e){
            var properties = this.model.get("dataProperties"),
                $textBox = this.$el.find(".location-search-box-input.tt-input"),
                lookupQuery = {
                    text : $textBox.length !== 0 ? $textBox.val() : this.$el.find(".location-search-box-input").val(),
                    maxResults : 1
                },
                hashObj;

            if (lookupQuery.text === "") {
                hashObj = this.createHashObject("", "");
                this.updateHash(hashObj, properties);
                return;
            }

            switch (properties.mode) {
                case "Location": {
                    //use browser to detect location
                    this.detectLocation();
                    break;
                }
                case "UserProvided": {
                    //take address entered by user and try to convert it to latitude and longitude
                    this.translateUserLocation(lookupQuery);
                    break;
                }
                case "Mixed": {
                    //use user address otherwise try to detect location by browser
                    if (typeof(lookupQuery.text) === "undefined" || lookupQuery.text === "") {
                        this.detectLocation();
                    } else {
                        this.translateUserLocation(lookupQuery);
                    }
                    break;
                }
            }
        },

        autocomplete : function(args){
            var $textBox,
                queryParams,
                properties = this.model.get("dataProperties");

            args.stopPropagation();
            if (args.keyCode === 13) {
                return;
            }


            $textBox = this.$el.find(".location-search-box-input.tt-input");

            queryParams = {
                text : $textBox.length !== 0 ? $textBox.val() : this.$el.find(".location-search-box-input").val(),
                maxResults : properties.p
            };
            this.model.set({queryParams : queryParams});
        },
        searchTextChanges: function(e) {
            e.stopPropagation();
            if (e.keyCode === 13) {
                this.addressLookup(e);
                return false;
            }
            return true;
        },
        translateUserLocation: function(lookupQuery) {
            var that = this,
                properties = this.model.get("dataProperties"),
                hashObj = {};

            if (lookupQuery === "") {
                return;
            }
            mapsConnector.addressLookup({ text: lookupQuery }, function(data) {
                hashObj = that.createHashObject(data[0] + "|" + data[1], properties.f + ",Ascending");
                that.updateHash(hashObj, properties);
            }, function () {
                console.error("Error while getting '" + lookupQuery + "' location");
            });
            that.$el.find(".location-search-box-input.tt-input").blur();
        },
        detectLocation: function() {
            var properties = this.model.get("dataProperties"),
                $textBox = this.$el.find(".location-search-box-input"),
                sig = this.model.get("sig"),
                hash = queryModel.parseHashParameters(window.location.hash),
                param,
                hashObj = {},
                that = this;

             XA.component.locationService.detectLocation(
                function (location) {
                    hashObj = that.createHashObject(location[0] + "|" + location[1], properties.f + ",Ascending");
                    that.updateHash.call(that, hashObj, properties);
                    if ($textBox.length > 0) {
                        $textBox.attr("placeholder", properties.myLocationText);
                    }
                },
                function (errorMessage) {
                    //do not update the hash in any way when then location isn't available
                    console.log(errorMessage);
                }
          );
        },
        updateHash: function (params, properties) {
            var sig = this.model.get("sig"),
                searchModels = typeof XA.component.search !== "undefined" ? XA.component.search.results.searchResultModels : [],
                i, j;

            //clear load more offset in each of search results with the same signature when location is changes
            //at the moment this is needed to clear offset but should be handle in search service in the future
            for (i = 0; i < searchModels.length; i++) {
                for (j = 0; j < sig.length; j++) {    
                    if (searchModels[i].get("dataProperties").sig === sig[j]) {
                        searchModels[i].set("loadMoreOffset", 0);
                    }
                }
            }

            queryModel.updateHash(params, properties.targetUrl);
            for (i = 0; i < sig.length; i++) {
                XA.component.search.vent.trigger("my-location-coordinates-changed", {
                    sig: sig[i],
                    coordinates: params[sig[i] !== "" ? sig[i] + "_g" : "g"].split("|")
                });
            }
        },
        createHashObject: function(g, o) {
            var sig = this.model.get("sig"),
                signature, 
                hashObj = {},
                i;

            for (i = 0; i < sig.length; i++) {
                signature = sig[i];
                hashObj[signature !== "" ? signature + "_g" : "g"] = g;
                hashObj[signature !== "" ? signature + "_o" : "o"] = o;
            }

            return hashObj;
        }
    });

    api.init = function() {
        if ($("body").hasClass("on-page-editor") || initialized) {
            return;
        }

        queryModel = XA.component.search.query;
        urlHelperModel = XA.component.search.url;

        var components = $(".location-filter:not(.initialized)");
        _.each(components, function(elem) {
            var $el = $(elem),
                properties = $el.data("properties");

            //collect all found components - we will use them later to create views
            locationFilters.push($el);

            //load google or bing scripts in order to properly use address lookup functionality but just
            //when we are not in Location mode (in this mode we are taking location form the browser)
            if (!scriptsLoaded && properties.mode !== "Location") {
                mapsConnector.loadScript(properties.key, XA.component.search.locationfilter.scriptsLoaded);
            } else {
                initialize();
            }

            $el.addClass("initialized");
        });
        initialized = true;
    };

    api.scriptsLoaded = function() {
        if (!scriptsLoaded) {
            console.log("Maps api loaded");
            scriptsLoaded = true;
            initialize();
        }
    };

    return api;

}(jQuery, document));

XA.register("locationfilter", XA.component.search.locationfilter);
