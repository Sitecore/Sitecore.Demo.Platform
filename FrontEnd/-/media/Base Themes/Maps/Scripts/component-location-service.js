XA.component.locationService = (function ($, document) {

    "use strict";

    var api = {},
        getCurrentLocation,
        getLocationOptions,
        detectBrowser,
        gettingLocation = false,
        errorCallbacks = [],
        successCallbacks = [],
        reportError;


    getCurrentLocation = function(success, error) {
        var i;

        errorCallbacks.push(error);
        successCallbacks.push(success);

        if (gettingLocation) {
            //process for getting current location has already been started, just subscribe for proper event and wait
            return;
        }
        gettingLocation = true;
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                function (position) {
                    for (i = 0; i < successCallbacks.length; i++) {
                        successCallbacks[i]([position.coords.latitude, position.coords.longitude]);
                    }
                    gettingLocation = false;
                },
                function (error) {
                    reportError("Error while detecting user location location");
                    gettingLocation = false;
                },
                getLocationOptions()
            );
        } else {
            reportError("Your browser doesn\'t support geolocation");
            gettingLocation = false;
        }
    };

    getLocationOptions = function() {
        var browser = detectBrowser();
        if (browser.indexOf("Chrome") !== -1) {
            return {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 0
            };
        } else {
            return {
                timeout: 1000,
                maximumAge: Infinity
            };
        }
    };

    detectBrowser = function() {
        var ua = navigator.userAgent,
            tem,
            M = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];

        if (/trident/i.test(M[1])) {
            tem=  /\brv[ :]+(\d+)/g.exec(ua) || [];
            return "IE " + (tem[1] || "");
        }
        if (M[1] === "Chrome") {
            tem = ua.match(/\b(OPR|Edge)\/(\d+)/);
            if(tem != null) {
                return tem.slice(1).join(" ").replace("OPR", "Opera");
            }
        }
        M= M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, "-?"];
        if ((tem= ua.match(/version\/(\d+)/i)) != null) {
            M.splice(1, 1, tem[1]);
        }
        return M.join(" ");
    };

    reportError = function(errorMessage) {
        var i;
        for (i = 0; i < errorCallbacks.length; i++) {
                if (typeof errorCallbacks[i] === "function") {
                    errorCallbacks[i](errorMessage);
                }
            }
    };

    api.detectLocation = function(success, error) {
        getCurrentLocation(success, error);
    };

    return api;

}(jQuery, document));

XA.register("locationService", ﻿XA.component.locationService);
