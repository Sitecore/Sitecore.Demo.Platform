XA.component.archive = (function($) {
    var api = {};

    var toggleClick = function(args) {
        var groupHeader = $(args.target);
        groupHeader.siblings("ul").toggle();
        groupHeader.toggleClass('opened');
    };

    api.initInstance=function(component) {
        var toggles = component.find(".group-header");
        toggles.on("click", toggleClick);
    }

    api.init = function() {
        var archives = $(".sxa-archive:not(.initialized)"),
            archive;

        for (var i = 0, l = archives.length; i < l; i++) {
            archive = $(archives[i]);
            api.initInstance(archive)
            archive.addClass("initialized");
        }
    };

    return api;
})(jQuery, _);

XA.register("archive", XA.component.archive);