/// <reference path="jquery-1.7.1-vsdoc.js" />
/// <reference path="ga.js" />

var XAContext = XAContext || {};

XAContext.Tracking = function ($) {
    /// <param name="$" type="jQuery" />

    function init() {
        var movieTracker = XAContext.Tracking.Movies($);
        var youtubeVideoHandlers = XAContext.Tracking.Movies.YouTubePlayer.Handlers($, { customEvents: ['onPlayerHidden', 'onPlayerVisible'] });

        //var browserVersion = parseInt($.browser.version, 10);
        //var isIElt9 = $.browser.msie && browserVersion < 9;
        //var isIE9 = $.browser.msie && browserVersion == 9;
    }

    $(document).ready(init);
} (jQuery);