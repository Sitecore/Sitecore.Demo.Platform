/// <reference path="chivas.base.js" />
/// <reference path="chivas.domain.js" />
/// <reference path="chivas.tracking.js" />
/// <reference path="chivas.tracking.player.js" />
/// <reference path="chivas.tracking.playerManager.js" />
/// <reference path="chivas.tracking.mejsPlayer.js" />
/// <reference path="jquery-1.7.1-vsdoc.js" />
/// <reference path="ga.js" />

XAContext.Tracking.Movies = function ($)
{
    var api = {},
        trackers = [
            {
                'id': 'mejs',
                'player': XAContext.Tracking.Movies.MediaElementJsPlayer
            },
            {
                'id': 'youTube',
                'player': XAContext.Tracking.Movies.YouTubePlayer
            }
        ],
        playerManagers = [],
        watchedAll = {
            'moviesCount': 2,
            'trackingOptions': 
            {
                'category': 'movie',
                'event': 'play',
                'label': 'both'
            },
            'names': []
        };

    function isRegistered(key)
    {
        return key != null && playerManagers[key] != null;
    }

    function isMovieWatched(movieName)
    {
        var i,
            result = false;

        for (i = 0; i < watchedAll.names.length; i++)
        {
            if (watchedAll.names[i] === movieName)
            {
                result = true;
                break;
            }
        }

        return result;
    }

    function trackMovieWatchedBoth(manager)
    {
        var movieName = manager.getPlayer().getName();

        if (!isMovieWatched(movieName))
        {
            watchedAll.names.push(movieName);
            if (watchedAll.names.length == watchedAll.moviesCount)
            {
                XAContext.Tracking.track(XAContext.Domain.TrackingTypes().event, watchedAll.trackingOptions);
                //XAContext.Logger.log('movie ' + watchedAll.trackingOptions.category + ' ' + watchedAll.trackingOptions.event + ' ' + watchedAll.trackingOptions.label);
            }
        }
    }

    function getTracker(trackerId)
    {
        var tracker = null,
            i;

        for (i = 0; i < trackers.length; i++)
        {
            if (trackers[i].id === trackerId)
            {
                tracker = trackers[i];
                break;
            }
        }

        return tracker;
    }

    api.register = function (context)
    {
        var tracker = getTracker(context.trackerId),
            playerManager,
            player;

        if (tracker != null && !isRegistered(context.name))
        {
            //XAContext.Logger.log('register movie tracker ' + context.name);
            player = tracker.player($, context);

            playerManager = XAContext.Tracking.Movies.PlayerManager($, player);
            player.init(playerManager);

            playerManagers[context.name] = playerManager;
        }
    };

    return api;
};
