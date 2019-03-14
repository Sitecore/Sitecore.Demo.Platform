XA.component.search.ajax = (function ($, document) {
    var ApiModel, getPrameterByName;

    ApiModel = Backbone.Model.extend({
        getData: function (properties) {
            var siteName = this.getPrameterByName("sc_site"),
                url = typeof properties.excludeSiteName !== "undefined" && properties.excludeSiteName
                    ? properties.url
                    : XA.component.search.url.createSiteUrl(properties.url, siteName);
            Backbone.ajax({
                dataType: "json",
                url: url,
                success: function(data){
                    properties.callback(data);
                }
            });
        },
        getPrameterByName: function (name, url) {
            if (!url) url = window.location.href;
            name = name.replace(/[\[\]]/g, "\\$&");
            var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
                results = regex.exec(url);
            if (!results) return null;
            if (!results[2]) return '';
            return decodeURIComponent(results[2].replace(/\+/g, " "));
        } 
    });

    return new ApiModel();

}(jQuery, document));