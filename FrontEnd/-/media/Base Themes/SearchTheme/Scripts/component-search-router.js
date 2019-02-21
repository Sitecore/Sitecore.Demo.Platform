XA.component.search.router = (function ($, document) {

    "use strict";

    var api = {},
        queryModel,
        initialized = false,
        Router;

    Router = Backbone.Router.extend({
        routes: {
            "*params": "checkUrl"
        },
        checkUrl: function (params) {
            var hashObj = queryModel.parseHashParameters(window.location.hash);

            XA.component.search.service.getData();

            if (!hashObj) {
                XA.component.search.facet.data.getInitialFacetData();
            } else {
                XA.component.search.facet.data.filterFacetData(hashObj);
            }

            XA.component.search.vent.trigger("hashChanged", hashObj);
        }
    });

    api.init = function () {
        if ($("body").hasClass("on-page-editor") || initialized) {
            return;
        }

        queryModel = XA.component.search.query;

        var router = new Router();

        if (!Backbone.History.started) {
            Backbone.history.start();
        }

        initialized = true;
    };

    return api;

}(jQuery, document));

XA.register('searchRouter', XA.component.search.router);
