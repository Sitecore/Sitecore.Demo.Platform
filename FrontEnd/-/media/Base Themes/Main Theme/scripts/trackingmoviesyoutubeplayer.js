/// <reference path="chivas.base.js" />
/// <reference path="chivas.domain.js" />
/// <reference path="chivas.tracking.js" />
/// <reference path="chivas.tracking.player.js" />
/// <reference path="chivas.tracking.playerManager.js" />
/// <reference path="chivas.tracking.youTubePlayer.handlers.js" />
/// <reference path="jquery-1.7.1-vsdoc.js" />
/// <reference path="ga.js" />

XAContext.Tracking.Movies.YouTubePlayer = function ($, context)
{
    var youTubePlayer = null,
        api = XAContext.Tracking.Movies.Player($, {
            'name': context.name,
            'completedTime': context.completedTime || 0,
            'getDuration': function ()
            {
                return youTubePlayer.getDuration();
            },
            'getCurrentTime': function ()
            {
                return youTubePlayer.getCurrentTime();
            }
        }),
        manager = null,
        videoId = context.videoId,
        intervalTrackingId = null,
        isTrackingEnabled = false,
        isPlayerVisible = false,
        isIE = context.isIE != null ? context.isIE : false,
        videoIdRegexp = /v=[^&]+/ig; ;

    function getYouTubeApiIE()
    {
        var youTubeApi = null,
            id = context.$container.children().first().attr('id'),
            players;

        if (id != null)
        {
            players = $.tubeplayer.getPlayers();
            youTubeApi = players[id] != null && $.isFunction(players[id].getVideoUrl) ? players[id] : null;
        }

        return youTubeApi;
    }

    function getYouTubeApi()
    {
        var youTubeApi = null,
            object = context.$container.tubeplayer('player');
        if ($.isFunction(object.getVideoUrl))
        {
            youTubeApi = object;
        }
        if (youTubeApi == null && isIE)
        {
            youTubeApi = getYouTubeApiIE();
        }

        return youTubeApi;
    }

    function currentVideoId()
    {
        var videoId = null,
            videoUrl = youTubePlayer.getVideoUrl();

        videoIdRegexp.compile(videoIdRegexp);

        videoId = videoIdRegexp.exec(videoUrl);
        videoId = videoId != null && videoId.length > 0 ? videoId[0] : null;
        if (videoId != null)
        {
            videoId = videoId.replace('v=', '');
        }

        return videoId;
    }

    function canExecute()
    {
        return youTubePlayer != null && videoId != null && videoId === currentVideoId();
    }

    function track(delegate)
    {
        if (!isTrackingEnabled) { return; }

        if (youTubePlayer == null)
        {
            youTubePlayer = getYouTubeApi();
        }

        if (canExecute())
        {
            delegate();
        }
    }

    function setUpdateInterval()
    {
        intervalTrackingId = setInterval(function ()
        {
            track(manager.update);
        }, 1000);
    }

    api.init = function (playerManager)
    {
        var handlers = context.handlers,
            onPlayerPlaying = 'onPlayerPlaying',
            handlerNames = [
                'onPlayerCued',
                onPlayerPlaying,
                'onPlayerPaused',
                'onPlayerBuffering',
                'onPlayerEnded'
            ];

        manager = playerManager;

        if (context.$container != null && handlers != null)
        {
            handlers.addHandlers(handlerNames, function ()
            {
                track(manager.update);
            });
            handlers.addHandler('onPlayerPaused', function ()
            {
                track(manager.pause);
            });
            handlers.addHandler(onPlayerPlaying, function ()
            {
                var oldValue = isTrackingEnabled;

                isTrackingEnabled = isTrackingEnabled ? true : !isTrackingEnabled && isPlayerVisible;

                track(manager.play);
            });
            handlers.addHandler('onPlayerVisible', function ()
            {
                isPlayerVisible = true;
                setUpdateInterval();
            });
            handlers.addHandler('onPlayerHidden', function onPlayerHidden()
            {
                isPlayerVisible = false;
                isTrackingEnabled = false;

                if (intervalTrackingId != null)
                {
                    clearInterval(intervalTrackingId);
                    intervalTrackingId = null;
                }

                youTubePlayer = null;
            });
            if (context.$elem && context.$elem.length)
            {
                context.$elem.bind('click', manager.stop);
            }
        }
    };

    return api;
};