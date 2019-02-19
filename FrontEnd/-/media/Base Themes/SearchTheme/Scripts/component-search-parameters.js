XA.component.search.parameters = (function ($, document) {

    var api = {},
        queryModel,
        searchResultModels,
        initialized = false,
        defaults = {};


    api.init = function() {
        if ($("body").hasClass("on-page-editor") || initialized) {
            return;
        }

        queryModel = XA.component.search.query;
        searchResultModels = XA.component.search.results.searchResultModels;
        initialized = true;
    };

    api.registerDefault = function(hash){
        _.each(hash, function(item, key) {
            defaults[key] = hash[key].toString();
        });
    };

    api.updateHash = function (newHash) {
        _.each(newHash, function(item, key){
            if (defaults[key] === newHash[key].toString()) {
                newHash[key] = "";
            }
        });

        queryModel.updateHash(newHash);
    }

    return api;

}(jQuery, document));

XA.register('searchParameters', XA.component.search.parameters);