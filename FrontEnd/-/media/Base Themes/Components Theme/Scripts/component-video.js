/* global XAContext:false, MediaElementPlayer:false */
XA.component.video = (function ($, document) {

    var api = {};

    function checkSize(video) {
        var videoWidth = video.width();
        video.removeClass("video-small hide-controls");

        if ((videoWidth < 481) && (videoWidth >= 321)) {
            video.addClass("video-small");
        } else if (videoWidth < 321) {
            video.addClass("hide-controls");
        }
    }

    function initVideo(video, properties) {
        var content = video.find("video");

        if (!content.length) {
            return;
        }

        if (!movieTracker) {
            var movieTracker = XAContext.Tracking.Movies($);
        }

        var callback = function (mediaElement) {
            mediaElement.movieName = 'Movie';
            movieTracker.register({
                name: 'Movie',
                api: mediaElement,
                trackerId: 'mejs',
                completedTime: null
            });

            $(mediaElement).on("ended", function () {
                if (properties.fromPlaylist) {
                    $(properties.playlist).trigger("change-video");
                }
            });
            $(mediaElement).closest('.component-content').find('.video-init').hide();
        }

        $.extend(properties, {
            "plugins": ['youtube', 'flash', 'silverlight'],
            "silverlightName": 'silverlightmediaelement.xap',
            "classPrefix": "mejs-",
            "success": callback,
            stretching: 'auto',
            pluginPath: '../other/'
        });

        content.each(function (key) {
            var $elem = $(content[key]),
                _this = this;
            if ($elem.attr('poster')) {
                var initBtn = $elem.parent().find('.video-init');
                $elem.add(initBtn).on('click', function () {
                    properties.success = (function (arg) {
                        return function () {
                            try {
                                arguments[0].load()
                                arguments[0].play()
                            } catch (e) {
                                /* eslint-disable */
                                console.warn('Error while loading video');
                                /* eslint-enable  */
                            }

                            return arg.apply(this, arguments)
                        }
                    })(properties.success)
                    initBtn.hide();
                    new MediaElementPlayer(_this, properties);
                })
            } else {
                new MediaElementPlayer(content[key], properties);
            }

        })
        return;
    }

    api.initVideoFromPlaylist = function (video, playlist) {
        var properties = $(video).data("properties");

        $.extend(properties, {
            "fromPlaylist": true,
            "playlist": playlist
        });

        return initVideo(video, properties);
    }

    api.initInstance = function (component, prop) {
        var $component = $(component)
        initVideo($component, prop);
        checkSize($component);

        $(window).resize(function () {
            checkSize($component);
        });
    }

    api.init = function () {
        if (XA.component.hasOwnProperty("playlist")) {
            XA.component.playlist.init();
        }

        var video = $(".video.component:not(.initialized)");

        video.each(function () {
            var properties = $(this).data("properties");

            api.initInstance(this, properties);

            $(this).addClass("initialized");
        });

        $(document).on('mozfullscreenchange', function () {
            setTimeout(function () {
                $(window).resize();
            }, 200); //mozilla bug fix
        });
    };

    return api;
}(jQuery, document));

XA.register("video", XA.component.video);