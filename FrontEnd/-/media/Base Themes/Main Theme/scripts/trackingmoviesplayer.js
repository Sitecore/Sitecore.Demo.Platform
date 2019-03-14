/// <reference path="chivas.base.js" />
/// <reference path="chivas.domain.js" />
/// <reference path="chivas.tracking.js" />
/// <reference path="jquery-1.7.1-vsdoc.js" />
/// <reference path="ga.js" />

XAContext.Tracking.Movies.Player = function ($, options)
{
    var api = {},
        defaults = {
            'name': null,
            'completedTime': null,
            'getDuration': function () { },
            'getCurrentTime': function () { }
        },
        config = $.extend({}, defaults, options);

    config.completedTime = isNaN(config.completedTime) ? defaults.completedTime : config.completedTime;

    api.getName = function ()
    {
        return config.name;
    };

    api.getCompletedTime = function ()
    {
        return config.completedTime;
    };

    api.getCurrentTime = function ()
    {
        return config.getCurrentTime();
    };

    api.getDuration = function ()
    {
        return config.getDuration();
    };

    api.init = function (playerManager)
    {
    };

    return api;
};