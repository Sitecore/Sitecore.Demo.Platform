XA.component.search.url = (function ($, document) {

    var clearIdData = function(data) {
        data = data + "";
        var parts = data.split(','), urlData = [], details;
        _.each(parts, function (part) {
            details = part.split('|');
            if (_.contains(part, '|') && details.length === 3) {
                urlData.push(details[1] + '|' + details[2]);
            } else {
                urlData.push(part);
            }
        });
        return urlData.join(',');
    };


    var UrlHelperModel = Backbone.Model.extend({
        createSearchUrl: function(dataProp, signature) {
            var url = this.setEndpoint(dataProp.endpoint);

            url += (dataProp.l) ? "&l=" + dataProp.l : "";  //language
            url += (dataProp.s) ? "&s=" + dataProp.s : "";  //scope
            url += (dataProp.itemid) ? "&itemid=" + dataProp.itemid : "";
            url += this.getFacetParams(dataProp, signature);
            url = this.fixUrl(url);

            return url;
        },
        createRedirectSearchUrl: function(redirectUrl, query, signature, targetSignature) {
            var originalHash = XA.component.search.query.get("hashObj"),
                updatedQuery = {},
                hash = {},
                index = 0,
                clearParam,
                paramSignature,
                param,
                url;

            if (signature !== "") {
                //if signature is provided then we have to filter just those params which have the same signature
                for (param in originalHash) {
                    clearParam = param.substring(param.indexOf('_') + 1);
                    paramSignature = param.substr(0, param.indexOf('_'));
                    if (paramSignature === signature) {
                        hash[param] = originalHash[param];
                    }
                }
            } else {
                hash = originalHash;
            }

            query = $.extend({}, hash, query);

            if (targetSignature !== "") {
                //if target signature is provided we have to replace signature with target signature in all params
                for (param in query) {
                    clearParam = param.substring(param.indexOf('_') + 1);
                    paramSignature = param.substr(0, param.indexOf('_'));
                    if (paramSignature === signature) {
                        updatedQuery[targetSignature + "_" + clearParam] = query[param];
                    } else if (paramSignature === targetSignature) {
                        updatedQuery[param] = query[param];
                    }
                }
            } else {
                updatedQuery = query;
            }

            url = this.setEndpoint(redirectUrl + "#");

            _.each(updatedQuery, function(item, key) {
                url += (index === 0 ? "" : "&") + key + "=" + item;
                index++;
            });

            return url;
        },
        createPredictiveSearchUrl: function(endpoint, dataProp, query){
            var url = this.setEndpoint(endpoint);

            url += "?q=" + encodeURIComponent(query);
            url += "&v=" + dataProp.v;
            url += "&p=" + dataProp.p;
            url += (dataProp.l) ? "&l=" + dataProp.l : "";
            url += (dataProp.s) ? "&s=" + dataProp.s : "";
            url += (dataProp.itemid) ? "&itemid=" + dataProp.itemid : "";

            return url;
        },
        createFacetUrl: function (dataProp, query) {
            var url = this.setEndpoint(dataProp.endpoint);

            url += "?f=" + dataProp.f.toLowerCase();
            url += (dataProp.s) ? "&s=" + dataProp.s : "";
            url += (dataProp.l) ? "&l=" + dataProp.l : "";
            url += (query) ? "&q=" + encodeURIComponent(query) : "";

            url += this.getFacetParams(dataProp);

            return url;
        },
        createMultiFacetUrl: function (dataProp, facetList, sig) {
            var url = this.setEndpoint(dataProp.endpoint);

            url += "?f=" + facetList.join(',').toLowerCase();
            url += (dataProp.s) ? "&s=" + dataProp.s : "";
            url += (dataProp.l) ? "&l=" + dataProp.l : "";
            url += (dataProp.q) ? "&q=" + encodeURIComponent(dataProp.q) : "";

            url += this.getFacetParams(dataProp, sig);

            url += "&sig=" + encodeURIComponent(sig);

            return url;
        },
        clearUrlParams: function(dataProperties, properties) {
            var facetName = dataProperties.f.toLowerCase(),
                hash = {};

            delete properties[facetName];
            delete hash[facetName];
            XA.component.search.query.updateHash(hash);

            return properties;
        },
        getFacetParams: function(dataProp, searchResultsSignature) {
            var url = "",
                clearFacetName,
                facetSignature,
                skipParams = ['endpoint', 'l', 's', 'e', 'f', 'sig', 'itemid'], //params which will be skipped while adding facet params
                specialParams = ['g', 'o', 'q', 'p', 'e', 'v']; //params which are not facets but can have signature


            if (dataProp.hasOwnProperty("sig")) {
                url += "&sig=" + encodeURIComponent(dataProp["sig"]);
            }

            for (facet in dataProp) {
                if (skipParams.indexOf(facet) === -1 && facet && dataProp[facet]) {
                    clearFacetName = facet.substring(facet.indexOf('_') + 1);
                    facetSignature = facet.substr(0, facet.indexOf('_'));
                    if (searchResultsSignature === facetSignature && specialParams.indexOf(clearFacetName) === -1) {
                        url += "&" + clearFacetName + "=" + encodeURI(clearIdData(dataProp[facet]));
                    }
                }
            }

            //get params which are not facets but can have signature
            url = this.getSpecialParams(dataProp, searchResultsSignature, specialParams, url);

            return url;
        },
        getSpecialParams: function(dataProp, searchResultsSignature, specialParams, url) {
            var param,
                paramValue,
                clearParamName,
                paramSignature;

            for (param in dataProp) {
                clearParamName = param.substring(param.indexOf('_') + 1);
                paramSignature = param.substr(0, param.indexOf('_'));
                if (specialParams.indexOf(clearParamName) !== -1) {
                    paramValue = typeof dataProp[param] !== "undefined" ? dataProp[param] : "";
                    if (searchResultsSignature === paramSignature) {
                        url += "&" + clearParamName + "=" + paramValue;
                    }
                }
            }

            return url;
        },
        createGetPoiContentUrl: function (dataProp, poiId, poiVariantId) {
            var url = this.setEndpoint(dataProp.endpoint);
            url += "/" + poiVariantId + "/" + poiId;
            return url;
        },
        createGetGeoPoiContentUrl: function (dataProp, poiId, poiVariantId, coordinates, units) {
            var url = this.setEndpoint(dataProp.endpoint);
            url += "/" + poiVariantId + "/" + poiId + "/" + coordinates + "/" + units;
            return url;
        },
        createSiteUrl: function(url, siteName) {
            if (typeof siteName !== "undefined" && siteName != null && siteName != "") {
                return url + "&site=" + siteName;
            }
            return url;
        },
        setEndpoint: function(endpoint) {
            var url = window.location.origin;

            if (endpoint.indexOf(url) !== -1) {
                return endpoint;
            }

            return url += endpoint;
        },
        fixUrl: function (url) {
            var index;

            url = url.replace(/[?]/g, "&");
            index = url.indexOf("&");
            url = url.substr(0, index) + "?" + url.substr(index + 1)

            return url;
        }
    });

    var urlHelperModel = new UrlHelperModel();

    return urlHelperModel;

}(jQuery, document));
