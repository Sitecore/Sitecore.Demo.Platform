/// <reference path="chivas.base.js" />
/// <reference path="chivas.domain.js" />
/// <reference path="chivas.tracking.js" />
/// <reference path="chivas.tracking.player.js" />
/// <reference path="chivas.tracking.playerManager.js" />
/// <reference path="jquery-1.7.1-vsdoc.js" />
/// <reference path="ga.js" />

XAContext.Tracking.Movies.MediaElementJsPlayer = function ($, context)
{
    var api = XAContext.Tracking.Movies.Player($, {
            'name': context.name,
            'completedTime': context.completedTime,
            'getDuration': function ()
            {
                return context.api ? context.api.duration : null;
            },
            'getCurrentTime': function ()
            {
                return context.api ? context.api.currentTime : null;
            }
        }),
        name = api.getName(),
        manager = null,
        mediaElement = context.api,
        stopped = true,
        eventName = 'createTouch' in document ? "touchstart" : "click";

    function play()
    {
        if (mediaElement.movieName === name)
        {
            manager.play();
        }
        stopped = false;
    }

    function pause()
    {
        if (mediaElement.movieName === name && !stopped &&
            mediaElement.currentTime && mediaElement.duration > mediaElement.currentTime)
        {
            manager.pause();
        }
    }

    function update()
    {
        if (mediaElement.movieName === name)
        {
            manager.update();
        }
    }

    function stop()
    {
        if (mediaElement.movieName === name) { manager.stop(); stopped = true; }
    }

    function ended()
    {
        if (mediaElement.movieName === name) { stopped = true; }
    }

    api.init = function (playerManager)
    {
        manager = playerManager;
        if (mediaElement != null)
        {
            if (context.$elem && context.$elem.length)
            {
                context.$elem.bind(eventName, stop);
            }
            mediaElement.addEventListener("play", play, false);
            mediaElement.addEventListener("pause", pause, false);
            //mediaElement.addEventListener("progress", update, false);
            mediaElement.addEventListener("seeked", update, false);
            mediaElement.addEventListener("timeupdate", update, false);
            mediaElement.addEventListener("ended", ended, false);
        }
    };

    return api;
};