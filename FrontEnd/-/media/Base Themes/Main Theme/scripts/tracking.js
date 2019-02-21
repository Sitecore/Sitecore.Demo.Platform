/// <reference path="jquery-1.7.1-vsdoc.js" />
/// <reference path="ga.js" />
var movieTracker;

var XAContext = XAContext || {};
XAContext.Domain = XAContext.Domain || {};

XAContext.Domain.TrackingOptions = function () {
    ///<summary></summary>
    ///<field name="category" type="String" />
    ///<field name="event" type="String" />
    ///<field name="label" type="String" />
    ///<field name="data" type="String" />
    return {
        category: '',
        event: '',
        label: '',
        data: ''
    };
};

XAContext.Domain.TrackingTypes = function () {
    return {
        view: 'page-view',
        event: 'event'
    };
};

XAContext.Domain.TrackingType = function (type, custom, method, label) {
    ///<summary></summary>
    return {
        type: type || '',
        method: method || 'async',
        custom: custom || false,
        label: label || ''
    };
};

XAContext.Tracking = function ($) {
    /// <param name="$" type="jQuery" />

    window._gaq = window._gaq || [];

    var config =
    {
        accountId: '',
        domainName: '',
        trackInitalView: false,
        attributes:
        {
            prefix: 'data-',
            type: 'tracking-type',
            data: 'tracking-info',
            method: 'tracking-method'
        },
        schema: '',
        selectors: ['.live', '.track'],
        types:
        {
            view: 'page-view',
            event: 'event'
        },
        googleMethods:
        {
            trackPageView: '',
            trackEvent: '_trackEvent'
        }
    };

    function evalElement(elem) {
        ///<summary>Obtain tracking info from given DOM element.</summary>
        ///<param name="elem" domElement="true" />
        ///<returns type="XAContext.Domain.TrackingType">Tracking details object</returns>

        var $elem = $(elem);
        var type = XAContext.Domain.TrackingType();
        if ($elem.filter(':input').length > 0) {
            type.label = $elem.val();
        }
        //for nested inputs and links need to change context to parent div
        if (!($elem.hasClass('track') || $elem.hasClass('live'))) {
            $elem = $elem.closest('div.track');
        }
        type.type = $elem.attr(config.attributes.prefix + config.attributes.type);
        type.custom = $elem.attr(config.attributes.prefix + config.attributes.data);
        type.method = $elem.attr(config.attributes.prefix + config.attributes.method);

        return type;
    }

    function evalFields(data, label) {
        ///<summary>Parses tracking field and returns tracking details</summary>
        ///<param name="data" type="String" />
        ///<returns type="XAContext.Domain.TrackingType">Tracking details object</returns>

        var infos = data ? data.split(',') : false;
        var result = XAContext.Domain.TrackingOptions();
        result.category = infos[0];
        result.event = infos[1];
        result.label = label.length == 0 ? infos[2] : (infos[2] == undefined ? label : (infos[2] + '_' + label));
        result.data = infos[3];
        return result;
    }

    function trackView(trackingDetails) {
        ///<summary>Schedules a track view to be send to Google.</summary>
        ///<param name="trackingDetails" type="XAContext.Domain.TrackingOptions" />
        ///<returns type="undefined" />

        if (XAContext.Tracking.Blacklist != null && XAContext.Tracking.Blacklist.isForbidden != null
            && XAContext.Tracking.Blacklist.isForbidden(window.location.href)) { return; }

        var url = trackingDetails.data || window.location.pathname;
        ///<field name="url" type="String" />
        url = url.toLowerCase();
        var qmIndex = url.indexOf('?');
        if (qmIndex > -1) {
            if (url.charAt(qmIndex - 1) != '/') { url = url.replace('?', '/?'); }
        }
        var hashIndex = url.indexOf('#');
        if (hashIndex > -1) {
            if (url.charAt(hashIndex - 1) != '/') { url = url.replace('#', '/#'); }
            if (url.charAt(url.length - 1) != '/') { url = url + "/"; }
            if (url.charAt(url.length - 1) != '#!' && url.indexOf('#!/') == -1) { url = url.replace("#!", "#!/"); }
        }
        else if (url.charAt(url.length - 1) != '/') {
            url = url + '/';
        }
        //do {
        url = url.replace('//', '/');
        //} while (url.indexOf('//') > -1); should replace, replace ALL of the occurences? http://www.w3schools.com/jsref/jsref_replace.asp

        _gaq.push(['_trackPageview', url]);
    }

    var pageTracker;

    function trackEvent(trackingDetails, sync) {
        ///<summary>Schedules a track event to be send to Google.</summary>
        ///<param name="trackingDetails" type="XAContext.Domain.TrackingOptions" />
        ///<param name="sync" type="Boolean" />
        ///<returns type="undefined" />

        //XAContext.Logger.log("Track event: " + trackingDetails.category + ' - ' + trackingDetails.event + ' - ' + trackingDetails.label);
        if (sync) {
            _gat._getTrackerByName()._trackEvent(trackingDetails.category,
            trackingDetails.event,
            trackingDetails.label,
            trackingDetails.data);
        }
        else {
            _gaq.push([config.googleMethods.trackEvent,
            trackingDetails.category,
            trackingDetails.event,
            trackingDetails.label,
            trackingDetails.data]);
        }
    }

    function dispatch(info, details) {
        ///<param name="info" type="XAContext.Domain.TrackingType" />
        ///<returns type="undefined" />

        if (info.type == config.types.event && details) {
            trackEvent(details, info.method == "sync");
        }
        else if (info.type == config.types.view) {
            trackView(details);
        }
    }

    function manageTracking(e) {

        //1. determine type
        var info = evalElement(e.currentTarget);
        //2. fill details
        var details = info.type == config.types.event ? evalFields(info.custom, info.label) : { data: e.currentTarget[info.custom] };
        //3. perform tracking
        dispatch(info, details);
    }

    function init() {
        //if (config.accountId === '') {
        //    return;
        //}

        ///<summary>Initializes Google tracking and binds all active elements.</summary>

        for (var i = 0; i < config.selectors.length; i++) {
            $("a" + config.selectors[i]).click(manageTracking);
            $("div" + config.selectors[i] + " a").click(manageTracking);
            $("div" + config.selectors[i] + " input[type=submit]").click(manageTracking);
        }

        //_gaq.push(['_setAccount', config.accountId]);
        if (config.trackInitalView) {
            _gaq.push(['_trackPageview']);
        }

        var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
        ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
        var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
    }

    $(document).ready(init);

    var module = {};

    module.track = function (type, details, sync) {
        ///<summary>Allows custom tracking by explicit tracking call.</summary>
        ///<param name="details" type="XAContext.Domain.TrackingOptions">Tracking data.</param>
        ///<param name="type" type="String">Tracking type, can be either view or event.</param>
        ///<returns type="undefined" />

        var type = XAContext.Domain.TrackingType(type, null, sync ? "sync" : "async");
        dispatch(type, details);
    };
    module.view = function (path) {
        ///<summary>Allows page view tracking explicitely.</summary>
        ///<param name="path" type="String">Path to track.</param>
        ///<returns type="undefined" />

        //$(window).load(function ()
        //{
        var type = XAContext.Domain.TrackingType(config.types.view);
        trackView({ data: path });
        //});
    };
    module.event = function (data, sync) {
        ///<summary>Allows events tracking explicitely.</summary>
        ///<param name="data" type="String">Data containing: category, action and label</param>
        ///<param name="sync" type="String">Whether method should be synchronous</param>
        ///<returns type="undefined" />

        //$(window).load(function ()
        //{
        var type = XAContext.Domain.TrackingType(config.types.event);
        trackEvent(data, sync);
        //});
    };
    module.setAccounts = function (accounts) {
        ///<param name="accounts" type="String">Accounts (comma separated).</param>
        config.accountId = ('' + accounts).split(',')[0];
        _gaq.push(['_setAccount', config.accountId]);
    };
    module.setDomainName = function (domainName) {
        ///<param name="domainName" type="String">Domain name.</param>
        if (domainName !== '') {
            config.domainName = domainName;
            _gaq.push(['_setDomainName', config.domainName]);
        }
    };
    return module;
} (jQuery);