/// <reference path="jquery-1.7.1-vsdoc.js" />
/// <reference path="ga.js" />

XAContext.Tracking.Movies.YouTubePlayer.Handlers = function ($, options)
{
    var api = {},
        defaults = {
            'customEvents': []
        },
        config = $.extend({}, defaults, options),
        eventHandlers = {
            onPlay: null,
            onPause: null,
            onStop: null,
            onSeek: null,
            onPlayerUnstarted: null,
            onPlayerEnded: null,
            onPlayerPlaying: null,
            onPlayerPaused: null,
            onPlayerBuffering: null,
            onPlayerCued: null
        },
        dispatchers = {};

    $.each(config.customEvents, function (id, value)
    {
        eventHandlers[value] = null;
    });

    function execute(handlers, param, eventName)
    {
        var i;

        if (handlers !== null)
        {
            //XAContext.Logger.log('-- dispatch event: ' + eventName + ' count=' + handlers.length);
            for (i = 0; i < handlers.length; i++)
            {
                handlers[i](param);
            }
            //XAContext.Logger.log('-- done');
        }
    }

    function getDispatcher(eventName)
    {
        var dispatcher = dispatchers[eventName];

        if (dispatcher == null && eventHandlers[eventName] !== undefined)
        {
            dispatcher = function (param)
            {
                execute(eventHandlers[eventName], param, eventName);
            };
            dispatchers[eventName] = dispatcher;
        }

        return dispatcher;
    }

    api.dispatch = function (eventName)
    {
        var dispatcher = getDispatcher(eventName);

        if (dispatcher != null)
        {
            dispatcher();
        }
    }

    api.get = function (eventName)
    {
        var dispatcher = getDispatcher(eventName);

        return dispatcher != null ? dispatcher : $.noop;
    };

    api.addHandler = function (eventName, handler)
    {
        if (eventHandlers[eventName] !== undefined)
        {
            if (eventHandlers[eventName] === null)
            {
                eventHandlers[eventName] = [];
            }
            eventHandlers[eventName].push(handler);
        }
    };

    api.addHandlers = function (eventNames, handler)
    {
        var i;

        for (i = 0; i < eventNames.length; i++)
        {
            api.addHandler(eventNames[i], handler);
        }
    };

    api.detachHandlers = function ()
    {
        var eventName;

        for (eventName in eventHandlers)
        {
            eventHandlers[eventName] = null;
        }
    };

    return api;
};