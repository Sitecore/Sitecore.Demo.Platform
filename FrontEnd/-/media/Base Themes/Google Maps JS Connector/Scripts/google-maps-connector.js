XA.connector.mapsConnector = (function ($, document) {

    "use strict";

    var api = {},
        maps = [],
        markers = [],
        map,
        getMapType,
        createMarker,
        infoWindows = [],
        addMarker,
        callbacks = [],
        animationTimeout,
        loading = false;

    getMapType = function (options) {
        switch (options.mode) {
            case 'Roadmap': {
                return google.maps.MapTypeId.ROADMAP;
            }
            case 'Satellite': {
                return google.maps.MapTypeId.SATELLITE;
            }
            case 'Hybrid':{
                return google.maps.MapTypeId.HYBRID;
            }
            default: {
                return google.maps.MapTypeId.ROADMAP;
            }
        }
    };

    createMarker = function(mapId, data) {
        var marker;
        if (data.icon === null) {
            marker = new google.maps.Marker({
                position: new google.maps.LatLng(data.latitude, data.longitude),
                map: maps[mapId],
                title: data.title
            });
        } else {
            marker = new google.maps.Marker({
                position: new google.maps.LatLng(data.latitude, data.longitude),
                map: maps[mapId],
                title: data.title,
                icon: data.icon
            });
        }
        if (data.latitude === '' && data.longitude === '') {
            //if marker doesn't have coordinated then create it but doesn't show it on the map
            marker.setVisible(false);
        }
        return marker;
    };

    addMarker = function(mapId, marker, data) {
        //save POI id - this will be used while removing dynamic POIs from the map
        marker.id = data.id;

        if (markers[mapId]) {
            markers[mapId].push({ marker: marker, type: data.type});
        } else {
            markers[mapId] = [];
            markers[mapId].push({ marker: marker, type: data.type});
        }
    };

    api.loadScript = function(key, callback) {
        //save each callback and call them all when script will be loaded - protection for loading maps twice
        callbacks.push(callback);

        if (!loading) {
            loading = true;
            var script = document.createElement("script"),
                src = "https://maps.googleapis.com/maps/api/js?v=3.exp";
            script.type = "text/javascript";
            if (typeof key !== "undefined" && key !== "") {
                src += "&key=" + key + "&v=3.exp&signed_in=false";
            } else {
                src += "&signed_in=false";
            }
            src += "&libraries=places&callback=XA.connector.mapsConnector.scriptsLoaded";
            script.src = src;
            script.onload = function () {
                console.log("Google loader has been loaded, waiting for maps api");
            };
            document.body.appendChild(script);
        }
    };

    api.scriptsLoaded = function () {
        var i, length = callbacks.length;
        for (i = 0; i < length; i++) {
            callbacks[i].call();
        }
        loading = false;
    };

    api.showMap = function(mapId, options, viewBounds) {
        var mapOptions,
            listener,
            scrollwheel = options.disableMapZoomOnScroll !== "1",
            mapDragging = options.disableMapScrolling !== "1";

        if (viewBounds instanceof Array) {
                mapOptions = {
                    zoom: options.zoom,
                    scrollwheel: scrollwheel,
                    draggable: mapDragging,
                    center: new google.maps.LatLng(viewBounds[0], viewBounds[1]),
                    mapTypeId: getMapType(options)
                };
                map = new google.maps.Map(document.getElementById(options.canvasId), mapOptions);
        } else {
                mapOptions = {
                    scrollwheel: scrollwheel,
                    draggable: mapDragging,
                    mapTypeId: getMapType(options)
                };
                map = new google.maps.Map(document.getElementById(options.canvasId), mapOptions);
                map.fitBounds(viewBounds);
                if (options.poiCount < 2) {
                    listener = google.maps.event.addListener(map, "idle", function() { 
                        if (markers.length > 0 && markers[mapId].length < 2) {
                            map.setZoom(options.zoom); 
                            google.maps.event.removeListener(listener); 
                        }
                    });
                }
        }

        listener = google.maps.event.addListener(map, "zoom_changed", function(a, b, c, d) { 
            var zoom = map.getZoom();
            if (zoom < 1) {
                map.setZoom(1);
            }
        });

        maps[mapId] = map;
    };

    api.renderPoi = function(mapId, data) {
        var marker = createMarker(mapId, data),
            key = mapId + "#" + data.id;

        addMarker(mapId, marker, data);

        if (data.html !== "" && data.html !== null) {
            infoWindows[key] = new google.maps.InfoWindow({
                content: data.html
            });
        }

        if (typeof infoWindows[key] !== "undefined") {
            (function(currentKey, marker) {
                google.maps.event.addListener(marker, "click", function() {
                    for (var key in infoWindows) {
                        if (infoWindows.hasOwnProperty(key)) {
                            infoWindows[key].close();
                        }
                    }
                    infoWindows[currentKey].open(maps[mapId], marker);
                });
            })(key, marker);
        }
    };

    api.renderDynamicPoi = function(mapId, data, getPoiContent) {
        var marker = createMarker(mapId, data);            

        addMarker(mapId, marker, data);

        google.maps.event.addListener(marker, "click", function () {
            if (typeof (getPoiContent) === "function") {
                var poiId = data.id,
                    poiTypeId = data.poiTypeId,
                    poiVariantId = data.poiVariantId;

                if (poiVariantId == null) {
                    return;
                }

                getPoiContent(poiId, poiTypeId, poiVariantId, function (result) {
                    if (infoWindows[mapId]) {
                        infoWindows[mapId].close();
                    }
                    infoWindows[mapId] = new google.maps.InfoWindow({
                        content: result.Html
                    });
                    infoWindows[mapId].open(maps[mapId], marker);
                });
            }
        });
    };

    api.clearMarkers = function(mapId) {
        var mapMarkers;
        if (markers.hasOwnProperty(mapId)) {
            mapMarkers = markers[mapId];
            for (var i = 0; i < mapMarkers.length; i++) {
                if (mapMarkers[i].type === "Dynamic") {
                    mapMarkers[i].marker.setMap(null);
                }
            }
            markers[mapId] = mapMarkers.filter(function (markerData) {
                if (markerData.type === "Static" || markerData.type === "MyLocation") {
                    return true;
                } else {
                    return false;
                }
            });
        }
    };

    api.updateMapPosition = function (mapId) {
        var map = maps[mapId],
            marker,
            mapMarkers = [],
            bounds = new google.maps.LatLngBounds(),
            i;

        if (markers.hasOwnProperty(mapId)) {
            mapMarkers = markers[mapId];
        }

        for (i =0; i < mapMarkers.length; i++) {
            marker = mapMarkers[i].marker;
            bounds.extend(new google.maps.LatLng(marker.position.lat(), marker.position.lng()));
        }

        map.fitBounds(bounds);
    };

    api.centerMap = function(mapId, data, centerMap, animate) {
        var map = maps[mapId],
            mapMarkers = [],
            animatedMarker;

        if (centerMap) {
            map.setCenter(new google.maps.LatLng(data.coordinates[0], data.coordinates[1]));
        }

        if (animate) {
            if (markers.hasOwnProperty(mapId)) {
                mapMarkers = markers[mapId];
            }
            for (var i = 0; i < mapMarkers.length; i++) {
                mapMarkers[i].marker.setAnimation(null);
                if (mapMarkers[i].marker.id === data.id) {
                    animatedMarker = mapMarkers[i].marker;
                    mapMarkers[i].marker.setMap(null);
                }
            }
            if (typeof animatedMarker !== "undefined") {
                animatedMarker.setMap(map);
                animatedMarker.setAnimation(google.maps.Animation.BOUNCE);
                animationTimeout = setTimeout(function () {
                    animatedMarker.setMap(map);
                }, 2000);
            }
        }
    };

    api.getCentralPoint = function(poisCoords) {
        var i,
            poiCoords,
            count = poisCoords.length,
            bounds = new google.maps.LatLngBounds();

        for (i = 0; i < count; i++) {
            poiCoords = poisCoords[i];
            if (poiCoords.latitude !== "" && poiCoords.longitude !== "") {
                bounds.extend(new google.maps.LatLng(poiCoords.latitude, poiCoords.longitude));
            }
        }
        return bounds;
    };

    api.locationAutocomplete = function(queryParams, successCallback, failCallback){
        var autocomplete = new google.maps.places.AutocompleteService(),
            maxResults = queryParams.maxResults <= 0 ? 1 : queryParams.maxResults;
            autocomplete.getQueryPredictions({input : queryParams.text}, function(results){
                    var predictions = [], length;
                    if(results != null && results.length) {
                        length = results.length >= maxResults ? maxResults : results.length;

                        for (var i = 0; i < length; i++) {
                            predictions.push(_.extend(results[i], {text : results[i].description}));
                        }

                        successCallback(predictions);
                    }
                    else {
                        failCallback();
                    }
            });
    };

    api.addressLookup = function (queryParams, successCallback, failCallback) {
        var search,
            query;

        if(queryParams.hasOwnProperty("place_id")){ //places lookup
            search = new google.maps.places.PlacesService(typeof map !== "undefined" ? map : new google.maps.Map(document.createElement("div")));
            search.getDetails({
                placeId : queryParams["place_id"]
            }, function(results,status){
                if (status == google.maps.places.PlacesServiceStatus.OK) {
                    if(typeof(results) !== "undefined" && typeof(results.geometry.location) !== "undefined"){
                        successCallback([results.geometry.location.lat(),results.geometry.location.lng()]);
                        return;
                    }
                }
                failCallback();
            });
        }
        else { //standard lookup
            query = {
                address : queryParams.text
            };
            search = new google.maps.Geocoder();
            search.geocode(query, function(results,status){
                if (status == google.maps.GeocoderStatus.OK) {
                    if(typeof(results[0]) !== "undefined" && typeof(results[0].geometry.location) !== "undefined"){
                        successCallback([results[0].geometry.location.lat(),results[0].geometry.location.lng()]);
                        return;
                    }
                }
                failCallback();
            });
        }
    };

    api.updateMyPoiLocation = function(mapId, coordinates, zoom) {
        var map = maps[mapId],
            mapMarkers = [],
            myLocationMarker;

        if (markers.hasOwnProperty(mapId)) {
            mapMarkers = markers[mapId];
            myLocationMarker = mapMarkers.filter(function (marker) {
                if (marker.type === "MyLocation") {
                    return true;
                }
                return false;
            });
            if (myLocationMarker.length > 0) {                
                myLocationMarker[0].marker.setPosition(new google.maps.LatLng(coordinates[0], coordinates[1]));
                myLocationMarker[0].marker.setVisible(true);                
                this.updateMapPosition(mapId);
                if (typeof zoom !== "undefined") {
                    map.setZoom(zoom); 
                }
            }
        }
    };

    return api;

})(jQuery, document);
