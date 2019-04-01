/// <reference path="chivas.base.js" />
/// <reference path="chivas.domain.js" />
/// <reference path="chivas.tracking.js" />
/// <reference path="jquery-1.7.1-vsdoc.js" />
/// <reference path="ga.js" />

XAContext.Tracking.Movies.PlayerManager = function ($, player)
{
    var api = {},
        isPlayedTracked = false,
        isPauseTracked = false,
        isStopTracked = false,
        isCompletedTracked = false,
        onPlayHandlers = [],
        thresholds = [
            { number: '1. ', 'value': 0, 'isReached': false },
            { number: '2. ', 'value': 25, 'isReached': false },
            { number: '3. ', 'value': 50, 'isReached': false },
            { number: '4. ', 'value': 75, 'isReached': false },
            { number: '5. ', 'value': 100, 'isReached': false }
        ];

    function getReachedThreshold()
    {
        var duration = player.getDuration(),
            currentTime = player.getCurrentTime(),
            percentage = (duration && currentTime) ? Math.floor((currentTime * 100) / duration) : -1,
            threshold = -1;

        if (percentage < 25 && percentage >= 0) { threshold = 0; }
        else if (percentage >= 98) { threshold = 100; }
        else if (percentage >= 75) { threshold = 75; }
        else if (percentage >= 50) { threshold = 50; }
        else if (percentage >= 25) { threshold = 25; }

        return threshold;
    }

    function trackCompleteEvent()
    {
        var duration = player.getDuration(),
            currentTime = player.getCurrentTime(),
            completedTime = player.getCompletedTime(),
            name,
            completedPercentage;

        if (currentTime && duration && ((completedTime && currentTime >= completedTime)
            || (completedTime === null && getReachedThreshold() === 100)))
        {
            name = player.getName();
            //completedPercentage = Math.floor((completedTime * 100) / duration);
            XAContext.Tracking.track(XAContext.Domain.TrackingTypes().event, { category: 'movie', event: 'watched ' + name, label: '6. ' + name + ' completed' });
            //XAContext.Logger.log('movie - watched ' +name + ' - 6. ' + name + ' completed');
            isCompletedTracked = true;
        }
    }

    function trackWatchedEvent()
    {
        var reachedThreshold = getReachedThreshold(),
            threshold,
            i;

        for (i = 0; i < thresholds.length; i++)
        {
            threshold = thresholds[i];
            if (reachedThreshold >= threshold.value && !threshold.isReached)
            {
                threshold.isReached = true;
                var name = player.getName();
                XAContext.Tracking.track(XAContext.Domain.TrackingTypes().event, { category: 'movie', event: 'watched ' + name, label: threshold.number + name + ' ' + threshold.value });
                //XAContext.Logger.log('movie - watched ' + name + ' - ' + threshold.number + name + ' ' + threshold.value);
                //XAContext.Logger.log('debug: duration=' + player.getDuration() + ', currentTime=' + player.getCurrentTime());
            }
        }
    }

    function fireHandlers(handlers)
    {
        var i;
        for (i = 0; i < handlers.length; i++)
        {
            handlers[i](api);
        }
    }

    api.isPlayedTracked = function ()
    {
        return isPlayedTracked;
    };

    api.play = function ()
    {
        if (!isPlayedTracked)
        {
            XAContext.Tracking.track(XAContext.Domain.TrackingTypes().event, { category: 'movie', event: 'play', label: player.getName() });
            //XAContext.Logger.log('movie - play - ' + player.getName());
            isPlayedTracked = true;
        }
        fireHandlers(onPlayHandlers);
    };

    api.pause = function ()
    {
        if (!isPauseTracked)
        {
            XAContext.Tracking.track(XAContext.Domain.TrackingTypes().event, { category: 'movie', event: 'pause', label: player.getName() });
            //XAContext.Logger.log('movie - pause - ' + player.getName());
            isPauseTracked = true;
        }
    };

    api.stop = function ()
    {
        if (!isStopTracked)
        {
            XAContext.Tracking.track(XAContext.Domain.TrackingTypes().event, { category: 'movie', event: 'stop', label: player.getName() });
            //XAContext.Logger.log('movie - stop - ' + player.getName());
            isStopTracked = true;
        }
    };

    api.onPlay = function (handler)
    {
        onPlayHandlers.push(handler);
    };

    api.update = function ()
    {
        trackWatchedEvent();
        if (!isCompletedTracked)
        {
            trackCompleteEvent();
        }
    };

    api.getPlayer = function ()
    {
        return player;
    };

    return api;
};