/* global mejs:false */
XA.component.playlist = (function($) {

    var api = {};

    function Playlist(playlist, properties) {
        this.properties = properties;
        this.playlist = playlist;
        this.activeVideo = 0;
        this.playlistItems = 0;
    }

    Playlist.prototype.createNewSources = function(source, videoContainer) {
        var newSource;

        var sourceBuilder = function(path) {
            var newSource = $("<source>"),
                type;

            if (path.match(/\.(mp4)$/)) {
                type = "video/mp4";
            } else if (path.match(/\.(webm)$/)) {
                type = "video/webm";
            } else if (path.match(/\.(ogv)$/)) {
                type = "video/ogg";
            } else {
                type = "video/youtube";
            }

            newSource.attr({
                "type": type,
                "src": path
            });

            return newSource;
        };

        if (source instanceof Array) {
            for (var i = 0; i < source.length; i++) {
                newSource = sourceBuilder(source[i]);
                videoContainer.find("video").append(newSource);
            }
        } else {
            newSource = sourceBuilder(source);
            videoContainer.find("video").append(newSource);
        }
    };

    Playlist.prototype.replaceSource = function(itemIndex, loadFromEvent) {
        var inst = this,
            videoContainer,
            videoClone,
            newSrc = inst.properties.sources[itemIndex].src,
            sources,
            videoId,
            videoContainerHeight = 0;

        $(inst.properties.playlistId).each(function() {
            videoContainer = $(this);
            videoId = inst.properties.playlistId;

            if ((videoContainer.is(videoId) && (newSrc.length))) {
                videoContainer.addClass("show");

                sources = videoContainer.find("source");
                sources.remove();
                inst.createNewSources(newSrc, videoContainer);
                videoContainer.find("video").attr({
                    src: ""
                }).show();

                var autoplayVideo = false;
                if (loadFromEvent) {
                    if (inst.properties.autoPlaySelected) {
                        autoplayVideo = true;
                    }
                } else {
                    if (inst.properties.autoPlay) {
                        autoplayVideo = true;
                    }
                }

                if (autoplayVideo) {
                    videoContainer.find("video").attr({
                        autoplay: ""
                    });
                }

                videoClone = videoContainer.find("video").clone();
                videoContainerHeight = videoContainer.height();
                videoContainer.css({
                    "height": videoContainerHeight
                });

                var id = videoContainer.find(".mejs-container").attr("id");
                if (id) {
                    $("#" + id).remove();
                    delete mejs.players[id];
                    videoContainer.find(".component-content").append(videoClone);
                }

                XA.component.video.initVideoFromPlaylist(videoContainer, inst.playlist);
                videoContainer.css({
                    "height": "auto"
                });
            }
        });
    };

    Playlist.prototype.loadPlaylistVideo = function() {
        var inst = this,
            playlistItems = $(inst.playlist).find(".playlist-item"),
            activeListItem;

        inst.playlistItems = playlistItems.length;
        var loadVideoFromPlaylist = function(loadFromEvent) {
            inst.replaceSource(inst.activeVideo, loadFromEvent);
            activeListItem = playlistItems.eq(inst.activeVideo);
            activeListItem.addClass("active");
            activeListItem.siblings().removeClass("active");
        }

        loadVideoFromPlaylist();
        $(inst.playlist).on("change-video", function(event, properties) {
            var loadNewVideo = false;

            if (properties) {
                if (properties.hasOwnProperty("back")) {
                    inst.activeVideo--;
                    if (inst.activeVideo < 0) {
                        inst.activeVideo = 0;
                    } else {
                        loadNewVideo = true;
                    }
                } else {
                    inst.activeVideo++;
                    if (inst.activeVideo === inst.playlistItems) {

                        if (inst.properties.repeatAfterAll) {
                            inst.activeVideo = 0;
                            loadNewVideo = true
                        } else {
                            inst.activeVideo = inst.playlistItems - 1;
                        }
                    } else {
                        loadNewVideo = true;
                    }
                }

            } else {
                if (inst.properties.playNext) {
                    if ((inst.activeVideo + 1) <= inst.playlistItems) {
                        inst.activeVideo++;

                        if (inst.activeVideo === inst.playlistItems) {
                            inst.activeVideo = 0;

                            if (inst.properties.repeatAfterAll) {
                                loadNewVideo = true;
                            }
                        } else {
                            inst.actiVideo--;
                            loadNewVideo = true;
                        }
                    }
                }
            }

            if (loadNewVideo) {
                loadVideoFromPlaylist(true);
            }
        });
    };

    Playlist.prototype.attachEvents = function() {
        var inst = this,
            link = $(inst.playlist).find(".playlist-section"),
            navItems = $(inst.playlist).find(".playlist-nav a"),
            playlistItem;

        link.on("click", function(event) {
            event.preventDefault();

            playlistItem = $(this).parents(".playlist-item");
            var itemIndex = playlistItem.index();

            if (itemIndex !== inst.activeVideo) {
                playlistItem.addClass("active");
                playlistItem.siblings().removeClass("active");
                inst.replaceSource(itemIndex, true);
                inst.activeVideo = itemIndex;
            }
        });

        navItems.on("click", function(event) {
            event.preventDefault();
            var properties = {};

            if ($(this).parent().hasClass("playlist-prev")) {
                properties.back = true;
            }

            $(inst.playlist).trigger("change-video", properties);
        });

    };

    api.initInstance=function(component,prop) {
        var playlist;
        $(prop.playlistId).addClass("initialized"); //prevent video init in component-video.js
        if (prop.sources.length) {
            playlist = new Playlist(component, prop);
            playlist.loadPlaylistVideo();
            playlist.attachEvents();
        }
    }

    api.init = function() {
        var playlists = $(".playlist.component:not(.initialized)"),
            properties;

        playlists.each(function() {
            properties = $(this).data("properties");
            api.initInstance(this,properties);
            $(this).addClass("initialized");
        });
    };

    return api;
}(jQuery, document));

XA.register("playlist", XA.component.playlist);