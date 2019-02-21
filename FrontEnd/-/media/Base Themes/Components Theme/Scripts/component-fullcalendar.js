/* global googleCalendarApiKey:true */
XA.component.calendar = (function($) {

    function GetEvents(selector, options) {
        this.data = options.data;
        this.selector = selector;
        this.options = options;
        this.events = [];

        this.checkSource();
    }

    GetEvents.prototype.checkSource = function() {
        var inst = this;

        switch (inst.options.dataType) {
            case "json":
                inst.getJson();
                new InitCalendar(inst.selector, inst.options, inst.events);
                break;
            case "gcalendar":
                new InitCalendar(inst.selector, inst.options, inst.events);
                break;
        }

    };

    GetEvents.prototype.getJson = function() {
        var inst = this,
            date, dateEnd, allDay = false,
            tempObj = [];


        $.each(inst.data, function() {
            date = new Date(this.eventStart);
            dateEnd = new Date(this.eventEnd);

            if (date === dateEnd) {
                allDay = true;
            }

            tempObj = {
                title: this.eventName,
                start: date,
                end: dateEnd,
                eventDescription: this.eventDescription,
                eventLink: this.eventLink,
                eventClass: this.eventClass
            };

            inst.events.push(tempObj);
        });

    };


    /*--------------------------------------------*/

    function InitCalendar(selector, options, events) {
        var inst = this,
            prevNext = "",
            title = "",
            calendarTypes = "";

        if (options.dataType === "gcalendar") {
            googleCalendarApiKey = options.calendarApiKey;
            events = options.calendarId;
        } else {
            googleCalendarApiKey = null;
        }

        options.showPrevNext ? prevNext = "prev, next" : "";
        options.showMonthCaptions ? title = "title" : "";

        for (var i in options.calendarTypes) {
            if (options.calendarTypes[i] === "day") {
                options.calendarTypes[i] = "basicDay";
            } else if (options.calendarTypes[i] === "week") {
                options.calendarTypes[i] = "basicWeek";
            }
        }

        if (options.calendarTypes.length > 1) {
            calendarTypes = options.calendarTypes.join();
        }

        $(selector).fullCalendar({
            monthNames: options.localization.monthNames,
            monthNamesShort: options.localization.monthNamesShort,
            dayNames: options.localization.dayNames,
            dayNamesShort: options.localization.dayNamesShort,
            nextDayThreshold:'00:00',

            buttonText: {
                agendaDay: 'agenda day',
                agendaWeek: 'agenda week'
            },

            header: {
                left: prevNext,
                center: title,
                right: calendarTypes
            },

            googleCalendarApiKey: googleCalendarApiKey,
            events: events,
            renderEvent: false,
            eventRender: function(event, element) {
                if ((options.compactView) && (options.dataType === "json")) {
                    $(element).css("display", "none");
                } else {
                    if (options.dataType === "json") {
                        inst.attachTooltip(event, element, false);
                    }
                }

                element.addClass(event.eventClass);
            },
            eventAfterAllRender: function() {
                if ((options.compactView) && (options.dataType === "json")) {
                    inst.renderCompactCalendarEvents(selector, events);
                }
            }
        });
    }

    InitCalendar.prototype.attachTooltip = function(event, element, compactCalendar) {
        var $tooltip,
            tooltipContent;

        $(element).on("mouseenter", function() {
            tooltipContent = "";
            $(".calendar-tooltip").fadeOut();
            $(".calendar-tooltip").remove();

            if (compactCalendar) {
                tooltipContent = "";
                $.each(event, function() {
                    tooltipContent += "<div class='compact-event'>" +
                        "<span class='title'>" + this.title + "</span>" +
                        "<span class='description'>" + this.eventDescription + "</span>" +
                        "<span class='link'><a href='" + this.eventLink + "'>Link</a></span></div>";
                });
            } else {
                tooltipContent = "<span class='description'>" + event.eventDescription + "</span>" +
                    "<span class='link'>" + event.eventLink + "</span>";
            }
            $tooltip = $("<div class='calendar-tooltip'><div class='arrow'>" +
                "</div><div class='events'>" + tooltipContent + "</div></div>");
            $("body").append($tooltip);


            $tooltip.css({
                "left": $(this).offset().left + $(this).width() / 2 - 80
            });
            $tooltip.css({
                "top": $(this).offset().top + $(this).height() / 2 + 5
            });

            var timeout;
            $(this).unbind("mouseleave");
            $(this).on("mouseleave", function() {
                timeout = setTimeout(function() {
                    $tooltip.fadeOut(function() {
                        $(this).remove();
                    });
                }, 300);

                $tooltip.unbind("mouseenter");
                $tooltip.on("mouseenter", function() {
                    clearTimeout(timeout);
                });
            });


            $tooltip.unbind("mouseleave");
            $tooltip.on("mouseleave", function() {
                $(this).fadeOut(function() {
                    $(this).remove();
                });

            });

        });
    };

    //attach events for single days - compact calendar*/
    InitCalendar.prototype.renderCompactCalendarEvents = function(selector, events) {
        var inst = this,
            currentDay,
            currentDate,
            currentEvent,
            dc, mc, yc,
            d, m, y, de, me, ye, he,
            startDate,
            endDate,
            dayEvents = [];

        $(selector).find(".fc-day").each(function() {
            currentDay = this;

            currentDate = new Date($(this).data("date"));
            dc = currentDate.getDate();
            mc = currentDate.getMonth();
            yc = currentDate.getFullYear();

            dayEvents = [];
            $.each(events, function() {
                currentEvent = this;
                startDate = new Date(this.start);
                d = startDate.getDate();
                m = startDate.getMonth();
                y = startDate.getFullYear();

                if (this.end) {
                    endDate = new Date(this.end);
                    de = endDate.getDate();
                    me = endDate.getMonth();
                    ye = endDate.getFullYear();
                    he = endDate.getHours();
                }

                if ((yc >= y) && (yc <= ye) && (mc >= m) && (mc <= me) && (dc >= d) && (dc <= de)) {
                    if ((yc == ye) && (mc == me) && (dc == de) && (he < 9)) {
                        // If last day and hour < 9 do nothing
                    } else {
                        $(currentDay).addClass("selected-day");
                        dayEvents.push(currentEvent);
                    }
                }
            });

            if (dayEvents.length) {
                inst.attachTooltip(dayEvents, currentDay, true);
            }
        });

    };


    function resizeCalendar(selector) {
        $(selector).fullCalendar("render");
    }


    var pub = {};

    pub.initInstance=function(component,prop) {
        var selector = "#" + component.find(".event-calendar-inner").attr("id");
        if ((prop.compactView) && (prop.dataType === "json")) {
            $(this).addClass("compact-mode");
        }

        new GetEvents(selector, prop);

        $(window).resize(function() {
            resizeCalendar(selector);
        });

    }


    pub.init = function() {
        $(".event-calendar:not(.initialized)").each(function() {
            var properties = $(this).data("properties");
            pub.initInstance($(this),properties);
            $(this).addClass("initialized");
        });
    };

    return pub;

})(jQuery, document);

XA.register("calendar", XA.component.calendar);