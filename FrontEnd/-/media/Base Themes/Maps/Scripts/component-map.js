XA.component.map = (function ($, document) {

    "use strict";

    var api = {},
        maps = [],
        searchEndpoint,
        searchResultsSignatures = [],
        key,
        urlHelperModel,
        apiModel,
        queryModel,
        mapsConnector,
        scriptsLoaded = false,
        initialize,
        searchResults,
        pushMapData,
        resultsLoaded,
        myLocationChanged,
        myLocationData,
        mapViews = [],
        MapModel,
        MapView;

    resultsLoaded = function (data) {
        searchResults = data;
    };

    myLocationChanged = function (data) {
        myLocationData = data;
    };

    initialize = function () {
        var i, mapsCount = maps.length;
        for (i = 0; i < mapsCount; i++) {
            //crate Backbone.js views - one view per component on the page
            mapViews.push(new MapView({el: maps[i], model: new MapModel()}));
        }
        if (typeof (XA.component.search) !== "undefined") {
            pushMapData();
        }
    };

    pushMapData = function() {
        var notInitializedMaps,
            resultsExists = typeof searchResults !== "undefined",
            myLocationExists = typeof myLocationData !== "undefined";

        if (resultsExists || myLocationExists) {
            //in case that results were loaded before initialization of view and model, trigger another event with
            //previously loaded results
            //this will happen just once - while initialization
            //remember about checking if all maps are already rendered
            notInitializedMaps = mapViews.filter(function (view) {
                if (view.model.get("showed") === false) {
                    return true;
                }
                return false;
            });

            if (notInitializedMaps.length > 0) {
                setTimeout(pushMapData, 1000);
            } else {
                if (resultsExists) {
                    XA.component.search.vent.trigger("internal-results-loaded", searchResults);
                }
                if (myLocationExists) {
                    XA.component.search.vent.trigger("internal-my-location-coordinates-changed", myLocationData);
                }
            }
        }
    };

    MapModel = Backbone.Model.extend({
        defaults: {
            dataProperties: {},
            dynamicPoiList: [],
            showed: false,
            myLocation: ["", ""],
            id: null,
            loadMore: false
        },
        initialize: function() {
            var sig = this.get("dataProperties").searchResultsSignature,
                hash = queryModel.parseHashParameters(window.location.hash),
                param = typeof (sig) !== "undefined" && sig !== "" ? sig + "_g" : "g";

            if (typeof (XA.component.search) !== "undefined") {
                XA.component.search.vent.on("results-loaded", this.updateDynamicPoiList.bind(this));
                XA.component.search.vent.on("internal-results-loaded", this.updateDynamicPoiList.bind(this));
                XA.component.search.vent.on("my-location-coordinates-changed", this.changeMyLocation.bind(this));
                XA.component.search.vent.on("internal-my-location-coordinates-changed", this.changeMyLocation.bind(this));
            }

            if (hash.hasOwnProperty(param) && hash[param] !== "") {
                //if Location Filter component is on the page it could already fill "g" hash parameter
                this.set("myLocation", hash[param].split("|"));
            }
        },
        getPoiVariant: function (poiTypeId, poiVariantId) {
            var mapping = this.get("dataProperties").typeToVariantMapping,
                typeId;

            if (typeof poiTypeId !== "undefined" && poiTypeId != null) {
                typeId = "{" + poiTypeId.toUpperCase() + "}";
                if (mapping.hasOwnProperty(typeId)) {
                    return mapping[typeId];
                } else {
                    return poiVariantId;
                }
            }
            return poiVariantId;
        },
        updateDynamicPoiList: function(searchResults) {
            var dynamicPoiList = [],
                signature = this.get("dataProperties").searchResultsSignature,
                geolocationData = searchResults.data.filter(function (result) {
                    if (result.hasOwnProperty("Geospatial")) {
                        return true;
                    } else {
                        return false;
                    }
                }),
                i,
                poi;

            if (signature !== searchResults.searchResultsSignature) {
                return;
            }

            //save information that those POIs comes from load more action
            this.set("loadMore", searchResults.loadMore);

            for (i = 0; i < geolocationData.length; i++) {
                poi = geolocationData[i];

                //don't show POIs which doesn't have Latitude and Longitude
                if (poi.Geospatial.Latitude === 0 || poi.Geospatial.Longitude === 0) {
                    continue;
                }

                dynamicPoiList.push({
                    id: poi.Id,
                    type: "Dynamic",
                    title: poi.Name,
                    latitude: poi.Geospatial.Latitude,
                    longitude: poi.Geospatial.Longitude,
                    icon: poi.Geospatial.PoiIcon,
                    poiTypeId: poi.Geospatial.PoiTypeId,
                    poiVariantId: this.getPoiVariant(poi.Geospatial.PoiTypeId, poi.Geospatial.PoiVariantId)
                });
            }

            this.set("dynamicPoiList", dynamicPoiList);
        },
        changeMyLocation: function (myLocationData) {
            var sig = this.get("dataProperties").searchResultsSignature;
            if (sig === myLocationData.sig) {
                this.set("myLocation", myLocationData.coordinates);
            }
        }
    });

    MapView = Backbone.View.extend({
        initialize : function () {
            var dataProperties = this.$el.data(),
                properties = dataProperties.properties;

            if (this.model) {
                this.model.set({dataProperties: properties});
                this.model.set({id: this.$el.find(".map-canvas").prop("id")});
            }

            this.render();
            this.model.on("change:dynamicPoiList", this.renderDynamicPois, this);
            this.model.on("change:myLocation", this.updateMyLocationPoi, this);
            if (typeof (XA.component.search) !== "undefined") {
                XA.component.search.vent.on("center-map", this.handleCenterMap.bind(this));
            }
            this.updateMyLocationPoi();

        },
        render : function () {
            var that = this,
                showed = this.model.get("showed"),
                canvas = document.getElementById(this.model.get("id"));

            if (!showed && canvas !== null) {
                this.getCentralPoint(function (viewBounds, ctx) {
                    if (typeof viewBounds !== "undefined") {
                        var context = typeof ctx !== "undefined" ? ctx : this,
                            id = context.model.get("id"),
                            properties = context.model.get("dataProperties"),
                            mapOptions = {
                                canvasId: id,
                                zoom: typeof(properties.zoom) === "number" ? properties.zoom : context.parseZoom(properties.zoom, 15),
                                mode: properties.mode,
                                poiCount: properties.pois.length,
                                key: properties.key,
                                disableMapScrolling: properties.disableMapScrolling,
                                disableMapZoomOnScroll: properties.disableMapZoomOnScroll
                            };

                        mapsConnector.showMap(id, mapOptions, viewBounds);
                        context.renderPoiList(id, properties.pois);

                        context.model.set("showed", true);
                    }
                });
            }
        },
        renderDynamicPois: function() {
            var dataProperties = this.model.get("dataProperties"),
                dynamicPoiList = this.model.get("dynamicPoiList"),
                properties = this.model.get("dataProperties"),
                id = this.model.get("id"),
                i;

            if (!this.model.get("loadMore")) {
                mapsConnector.clearMarkers(id);
            }

            for (i = 0; i < dynamicPoiList.length; i++) {
                mapsConnector.renderDynamicPoi(id, dynamicPoiList[i], this.getGeoPoiContent.bind(this));
            }

            //if user want to see all POIs then after adding dynamic POIs we have to recalculate map center point
            if (dataProperties.centralPointMode === "MidOfPoi") {
                mapsConnector.updateMapPosition(this.model.get("id"), this.parseZoom(properties.zoom, 15));
            }
        },
        renderPoiList: function(mapId, poiList) {
            var i,
                poi,
                myLocation,
                poiCount = poiList.length,
                properties = this.model.get("dataProperties"),
                sig = properties.searchResultsSignature,
                hash = queryModel.parseHashParameters(window.location.hash),
                param = sig !== "" ? sig + "_g" : "g";

            for (i = 0; i < poiCount; i++) {
                poi = poiList[i];

                if (poi.Type === "MyLocation") {
                    myLocation = this.model.get("myLocation");
                    poi.Latitude = myLocation[0];
                    poi.Longitude = myLocation[1];

                    if (!hash.hasOwnProperty(param) || (hash.hasOwnProperty(param) && hash[param] === "")) {
                        //if there is no "g" param then it's mean that there is no Location Filter on the map
                        //in this case we have take coordinated from the browser to display My Location POI
                        this.getCurrentPosition(function (coordinates) {
                            this.model.set("myLocation", coordinates);
                        });
                    } else if (hash.hasOwnProperty(param)) {
                        this.model.set("myLocation", hash[param].split("|"));
                    }
                } else if (poi.Latitude === "" || poi.Longitude === "") {
                    continue;
                }

                mapsConnector.renderPoi(mapId, {
                    id: poi.Id.Guid,
                    title: poi.Title,
                    description: poi.Description,
                    latitude: poi.Latitude,
                    longitude: poi.Longitude,
                    icon: poi.PoiIcon,
                    html: poi.Html,
                    type: poi.Type
                });
            }
        },
        getPoiContent: function(poiId, poiTypeId, poiVariantId, renderPoiContentCallback) {
            var properties = this.model.get("dataProperties"),
                url = urlHelperModel.createGetPoiContentUrl({endpoint: properties.variantsEndpoint}, poiId, poiVariantId);

            apiModel.getData({
                callback: renderPoiContentCallback,
                url: url,
                excludeSiteName: true
            });
        },
        getGeoPoiContent: function (poiId, poiTypeId, poiVariantId, renderPoiContentCallback) {
            var myLocation = this.model.get("myLocation"),
                latitude = myLocation[0],
                longitude = myLocation[1],
                properties = this.model.get("dataProperties"),
                hash = queryModel.parseHashParameters(window.location.hash),
                units = hash.o,
                url = urlHelperModel.createGetGeoPoiContentUrl({ endpoint: properties.variantsEndpoint }, poiId, poiVariantId, latitude + "," + longitude, units);

            apiModel.getData({
                callback: renderPoiContentCallback,
                url: url,
                excludeSiteName: true
            });

        },
        getCentralPoint: function (callback) {
            var properties = this.model.get("dataProperties"),
                that = this;

            switch (properties.centralPointMode) {
                case "Auto": {
                    if (properties.centralPoint !== "") {
                        callback.call(that, [properties.latitude, properties.longitude]);
                    } else if (properties.pois.length > 0) {
                        callback.call(that, [properties.pois[0].Latitude, properties.pois[0].Longitude]);
                    } else {
                        this.getCurrentPosition(callback);
                    }
                    break;
                }
                case "MidOfPoi": {
                    callback.call(that, this.getPoisCentralPoint());
                    break;
                }
                case "Location":{
                    this.getCurrentPosition(callback);
                    break;
                }
            }
        },
        getCurrentPosition: function (callback) {
          var that = this;
          XA.component.locationService.detectLocation(
              function (location) {
                  callback.call(that, location, that);
              },
              function (errorMessage) {
                  callback.call(that, [0, 0], that);
                  console.log(errorMessage);
              }
          );
        },
        getPoisCentralPoint: function () {
            var i,
                poi,
                poisCoords = [],
                myLocation = this.model.get("myLocation"),
                properties = this.model.get("dataProperties"),
                count = properties.pois.length;

            for (i = 0; i < count; i++) {
                poi = properties.pois[i];
                if (poi.TemplateId.Guid === "7dd9ece5-9461-498d-8721-7cbea8111b5e") {
                    if (myLocation[0] !== "" && myLocation[1] !== "") {
                        poi.Latitude = myLocation[0];
                        poi.Longitude = myLocation[1];
                        this.model.set("dataProperties", properties);
                        poisCoords.push({latitude: poi.Latitude, longitude: poi.Longitude});
                    }
                } else {
                    poisCoords.push({latitude: poi.Latitude, longitude: poi.Longitude});
                }
            }

            return mapsConnector.getCentralPoint(poisCoords);
        },
        handleCenterMap: function (data) {
            var centerMap = this.model.get("dataProperties").centerOnFoundPoi === "1",
                animate = this.model.get("dataProperties").animateFoundPoi === "1";
            this.centerOnMap(data, centerMap, animate);
        },
        centerOnMap: function (data, centerMap, animate) {
            if (data.sig === this.model.get("dataProperties").searchResultsSignature) {
                mapsConnector.centerMap(this.model.get("id"), data, centerMap, animate);
            }
        },
        updateMyLocationPoi: function () {
            var properties = this.model.get("dataProperties"),
                myLocation = this.model.get("myLocation");

            //check if there is something in my location
            if (myLocation[0] !== "" && myLocation[1] !== 0) {
                //my location was changed so update position of My Location marker
                mapsConnector.updateMyPoiLocation(this.model.get("id"), myLocation, this.parseZoom(properties.zoom, 15));
            } else if (properties.latitude !== "" && properties.longitude !== "") {
                var data = {};
                data.sig = properties.searchResultsSignature;
                data.coordinates = [properties.latitude, properties.longitude];
                this.centerOnMap(data, true, false);
            }
        },
        parseZoom: function (str, defaultValue) {
            var retValue = defaultValue;
            if(str !== null) {
                if(str.length > 0) {
                    if (!isNaN(str)) {
                        retValue = parseInt(str);
                    }
                }
            }
            return retValue;
        }
    });

    api.init = function() {
        var i,
            mapElements = $(".map.component:not(.initialized)"),
            count = mapElements.length;

        if (typeof (XA.component.search) !== "undefined") {
            queryModel = XA.component.search.query;
            urlHelperModel = XA.component.search.url;
            apiModel = XA.component.search.ajax;

            //if the page was reloaded there could be situation that search results will load results faster then map initialization
            //so that we have to save them and pass to the models when they will be created
            XA.component.search.vent.on("results-loaded", resultsLoaded);
            //the same with "my location", sometimes Location Filter component can change location before map was initialized
            XA.component.search.vent.on("my-location-coordinates-changed", myLocationChanged);
        }
        mapsConnector = XA.connector.mapsConnector;

        if (count > 0) {
            for (i = 0; i < count; i++) {
                var $map = $(mapElements[i]);
                var properties = $map.data("properties");
                key = properties.key;
                searchEndpoint = properties.endpoint;
                searchResultsSignatures.push(properties.searchResultsSignature)
                $map.addClass("initialized");
                maps.push($map);
            }

            if (!scriptsLoaded) {
                mapsConnector.loadScript(key, XA.component.map.scriptsLoaded);
            } else {
                initialize();
            }
        }
    };

    api.                                                scriptsLoaded = function() {
        console.log("Maps api loaded");
        scriptsLoaded = true;
        initialize();
    };

    api.getSearchEndpoint = function() {
        return searchEndpoint;
    };

    api.getSignatures = function() {
        return searchResultsSignatures;
    }

    return api;

}(jQuery, document));

XA.register("map", XA.component.map);
