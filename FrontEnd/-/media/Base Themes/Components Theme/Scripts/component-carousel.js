/**
 * XAContext
 *
 * XA.component.carousels module.
 *
 * Exposes register(selector, options) method, which initializes Carousel
 * for each elements that match the provided selector, with given options.
 * Element used as a carousel must contain a <ul class="slides"> tag.
 * That tag have to be filled with <li> elements. Each <li> element will be considered as a slide.
 * For more details read the documentation of a register(selector, options) method.
 *
 * @requires jQuery JavaScript Library v1.4.4 or higher
 */
/* global Hammer:true */
XA.component.carousels = (function($) {
    var pub = {},
        NavigationBuilder = null,
        SliderInitializer = null;

    /**
     * Timer class saves time stamp and counts time difference between
     * current time (update() method) and saved time stamp.
     * @constructor
     */
    function Timer() {
        /**
         * Time difference between last update() call and last set() call
         *
         * @member Timer
         */
        this.elapsed = 0;
        /**
         * Time stamp
         *
         * @member Timer
         */
        this.stamp = null;
    }

    /**
     *  Counts time difference between current time and saved time stamp
     *
     * @member Timer
     */
    Timer.prototype.update = function() {
        if (this.stamp !== null) {
            this.elapsed += this.newStamp() - this.stamp;
        }
    };

    /**
     *  Saves new time stamp
     *
     * @member Timer
     */
    Timer.prototype.set = function() {
        this.stamp = this.newStamp();
    };

    /**
     *  Counts time difference between current time and saved time stamp
     *
     * @member Timer
     * @return stamp of a current time
     * @type Number
     */
    Timer.prototype.newStamp = function() {
        return new Date().valueOf();
    };

    /**
     *  Resets elapsed time.
     *
     * @member Timer
     */
    Timer.prototype.reset = function() {
        this.elapsed = 0;
    };
    /**
     * End of Timer class
     */

    /**
     * TimeIndicator class extends Timer class
     * @constructor
     * @param {Object} $     a jQuery object
     * @param {XA.component.carousels.IndicatorViews.View} view  a time indicator view.
     * @param {Number} timeout   a view timeout
     * @base Timer
     */
    function TimeIndicator($, view, timeout) {
        /**
         * A time indicator view.
         *
         * @member TimerIndicator
         */
        this.view = view;

        this.view.init(timeout);
    }
    TimeIndicator.prototype = new Timer();
    TimeIndicator.constructor = Timer;

    /**
     * Plays the attached view.
     *
     * @member TimerIndicator
     */
    TimeIndicator.prototype.play = function() {
        this.view.play();
    };

    /**
     * Pauses the attached view.
     *
     * @member TimerIndicator
     */
    TimeIndicator.prototype.pause = function() {
        this.view.pause();
    };

    /**
     * Resets elapsed time and resets the attached view.
     *
     * @member TimerIndicator
     */
    TimeIndicator.prototype.reset = function() {
        Timer.prototype.reset.call(this);
        this.view.reset();
    };

    /**
     * Counts time difference between current time and saved time stamp.
     * and updates the attached view
     *
     * @member TimerIndicator
     */
    TimeIndicator.prototype.update = function() {
        Timer.prototype.update.call(this);
        this.view.update(this.elapsed);
    };
    /**
     * end of TimeIndicator class
     */

    /**
     * NavigationBuilder object creates navigation items
     */
    NavigationBuilder = (function($) {
        function createItem(settings) {
            var text = settings.text,
                $navItem = null;

            settings.text = '';
            settings.href = '#';

            $navItem = $('<a>', settings);
            $('<span>', {
                'text': text
            }).appendTo($navItem);

            return $navItem;
        }

        return {
            /**
             * Returns the element(s) that match(es) the provided selector or null
             *
             * @param {String} selector a selector used for searching
             * @return a jQuery object containing element(s) that match(es) the provided selector or null if no element match
             * @type Object
             */
            getElement: function(selector) {
                var $element = $(selector);

                if ($element.length === 0) {
                    $element = null;
                }

                return $element;
            },
            /**
             * Creates navigation items and appends them to $container.
             *
             * @param {Object} $container   navigation items container (jQuery object)
             * @param {String} label   navigation items label template
             * @param {Number} count    number of navigation items to create
             */
            createItems: function($container, label, count) {
                var i,
                    pattern = /#\{index\}/g,
                    settings;

                for (i = 0; i < count; i++) {
                    settings = {
                        'text': label.replace(pattern, i + 1)
                    };
                    createItem(settings).appendTo($container);
                }
            },
            /**
             * Creates or selects 'previous'/'next' navigation item.
             * Mode of action depends on settings object:
             * var settings = {
             *     isContainer: Boolean value,
             *     label: String value,
             *     selector: String value,
             *     method: String value
             * };
             *
             * If settings.selector === null or if settings.selector !== null and settings.isContainer == true
             * navigation item is created and attached to $element.first() (using settings.method).
             * If settings.selector !== null and settings.isContainer == false
             * $element.first() is selected as navigation item
             *
             * @param {Object} settings settings of navigation item
             * @param {Object} itemSettings   html attributes of navigation item
             * @param {Object} $element    navigation items container (jQuery object)
             * @return The 'prev'/'next' navigation item
             * @type Object
             */
            createPrevNextItem: function(settings, itemSettings, $element) {
                var $item = null,
                    isSelectorExists = settings.selector !== null;


                if (isSelectorExists) {
                    $element = this.getElement(settings.selector);
                }

                if ($element !== null) {
                    if (settings.isContainer || !isSelectorExists) {
                        $item = createItem($.extend({}, {
                            'text': settings.label
                        }, itemSettings));
                        ($element.first())[settings.method]($item);
                    } else {
                        $item = $element.first();
                    }
                }

                return $item;
            }
        };
    }($));
    /**
     * end of NavigationBuilder object
     */

    /**
     * Navigation class holds navigation items.
     * @constructor
     */
    function Navigation($, $wrapper, options) {
        var defaults = {
                'isEnabled': true,
                'hasPrevNextItems': true,
                'item': {
                    'label': '#{index}',
                    'selector': null
                },
                'prevItem': {
                    'label': '<',
                    'selector': null,
                    'isContainer': false
                },
                'nextItem': {
                    'label': '>',
                    'selector': null,
                    'isContainer': false
                },
                'slidesCount': 0
            },
            settings = $.extend(true, {}, defaults, options);

        /**
         * jQuery object
         *
         * @member Navigation
         */
        this.$ = $;
        /**
         * Navigation items (one for each slide if navigation is enabled).
         *
         * @member Navigation
         */
        this.$items = $();
        /**
         * Navigation 'next' item (if prev/next items are enabled).
         *
         * @member Navigation
         */
        this.$nextItem = $();
        /**
         * Navigation 'previous' item (if prev/next items are enabled).
         *
         * @member Navigation
         */
        this.$prevItem = $();
        /**
         * Selected item (one from this.$items or null).
         *
         * @member Navigation
         */
        this.$selectedItem = null;

        //initialization
        if (settings.isEnabled) {
            var self = this;
            setTimeout(function() {
                self.prepareItems(settings.item, settings.slidesCount, $wrapper);
                //select first item on init
                self.selectItem(0)
            }, 1000);

            $wrapper.find('.nav').trigger('navigation-created', this);


        }
    }

    /**
     * CSS classm which will be used for mark selected item
     *
     * @member Navigation
     */
    Navigation.prototype.selectedItemClass = 'active';

    /**
     * Mark navigation item with given index as selected.
     *
     * @member Navigation
     * @param {Number} index     index of navigation item
     */
    Navigation.prototype.selectItem = function (index) {
        if (this.$selectedItem !== null) {
            this.$selectedItem.removeClass(this.selectedItemClass);
        }
        this.$selectedItem = this.$items.eq(index).addClass(this.selectedItemClass);
    };

    /**
     * Creates or selects navigation items container and fills it with navigation items.
     * If navigation items container is not empty, child elements are adopted as navigation items.
     * Mode of action depends on settings object:
     * var settings = {
     *     'label': '#{index}',
     *     'selector': null
     * };
     * If settings.selector === null navigation item container is created.
     * If settings.selector !== null element that match settings.selector is adapted as navigagation items container.
     *
     * @member Navigation
     * @param {Object} settings settings of navigation items
     * @param {Number} count   number of navigation items
     * @param {Object} $wrapper    wrapper of navigation items container (jQuery object)
     * @return The container filled with navigation items.
     * @type Object
     */
    Navigation.prototype.prepareItems = function(settings, count, $wrapper) {
        var $container = null;

        if (settings.selector !== null) {
            $container = NavigationBuilder.getElement(settings.selector);
        } else {
            $container = $('<div/>', {
                'class': 'nav'
            });
            $container.appendTo($wrapper);
        }

        if ($container !== null) {
            if ($container.children().length === 0) {
                NavigationBuilder.createItems($container, settings.label, count);
            }
            this.$items = $container.children().slice(0, count);
        }

        return $container;
    };

    /**
     * Creates or selects navigation 'previous' and 'next' navigation items (@see NavigationBuilder.createPrevNextItem()).
     * If navigation items container is not empty, child elements are adopted as navigation items.
     *
     * @member Navigation
     * @param {Object} settings settings of navigation
     * @param {Object} $container    navigation items container (jQuery object)
     */
    Navigation.prototype.preparePrevNextItems = function(settings, $container) {
        var $item;

        settings.prevItem.method = 'prepend';
        $item = NavigationBuilder.createPrevNextItem(settings.prevItem, {
            'class': 'prev'
        }, $container);
        this.$prevItem = ($item === null) ? this.$prevItem : $item;

        settings.nextItem.method = 'append';
        $item = NavigationBuilder.createPrevNextItem(settings.nextItem, {
            'class': 'next'
        }, $container);
        this.$nextItem = ($item === null) ? this.$nextItem : $item;
    };
    /**
     * end of Navigation class
     */

    /**
     * SliderContext class is binded to a Slider class instance
     * and holds its properties.
     * @constructor
     */
    function SliderContext() {
        /**
         * Flag, which determines if slide can or can not be changed (for user request)
         *
         * @member SliderContext
         */
        this.canChangeSlide = true;
        /**
         * Default Slider setting object (@see Slider)
         *
         * @member SliderContext
         */
        this.defaults = null;
        /**
         * Navigation object (@see Navigation)
         *
         * @member SliderContext
         */
        this.navigation = null;
        /**
         * Slider object, which this context is attacehd to.
         *
         * @member SliderContext
         */
        this.owner = null;
        /**
         * Flag, which determines if change of a slide can or can not be scheduled
         *
         * @member SliderContext
         */
        this.preventScheduling = false;
        /**
         * Slider setting object
         *
         * @member SliderContext
         */
        this.settings = null;
        /**
         * Instance of Timer class (@see Timer), which is used to store how much
         * time is left to change the slide
         *
         * @member SliderContext
         */
        this.slideTimer = new Timer();
        /**
         * Instance of TimeIndicator class (@see TimeIndicator)
         *
         * @member SliderContext
         */
        this.timeIndicator = null;
        /**
         * Timers array.
         *
         * @member SliderContext
         */
        this.timers = [];
        /**
         * Last timeout identifier.
         *
         * @member SliderContext
         */
        this.timeoutId = null;
        /**
         * Transition (@see XA.component.carousels.Transitions.Transition) used while changing slides
         *
         * @member SliderContext
         */
        this.transition = null;
        /**
         * Transition settings object (@see XA.component.carousels.Transitions.TransitionSettings)
         *
         * @member SliderContext
         */
        this.transitionSettings = new XA.component.carousels.Transitions.TransitionSettings();
        /**
         * jQuery object which contains all slides
         *
         * @member SliderContext
         */
        this.$slides = null;
        /**
         * jQuery object containing element that wrapps slides
         *
         * @member SliderContext
         */
        this.$wrapper = null;
        /**
         * Reference to method, which should change current slide
         *
         * @member SliderContext
         */
        this.changeCurrentSlide = null;
        /**
         * Reference to method, which should return jQuery object with current slide
         *
         * @member SliderContext
         */
        this.getCurrentSlide = null;

        this.timers.push(this.slideTimer);
    }
    /**
     * end of SliderContext class
     */

    /**
     * Slider class manages changing of slides.
     *
     * Options object:
     * {
     *     'navigation': Object value, //navigation settings object
     *     'timeout': Number value, //time between slides changing
     *     'transition': String value, //name of transition class (@see XA.component.carousels.Transitions.Transition)
     *     'isPauseEnabled': Boolean value, //flag, which determines if pause feature is enabled
     *     'timeIndicator': {
     *         'isEnabled': Boolean value, //flag, which determines if time indicator is enabled
     *         'selector': String value, //selector of time indicator html element
     *         'view': String value, //name of view class (@see XA.component.carousels.IndicatorViews.View
     *         'options': Object value //object with options, which will be passed to a time indicator view
     *     }
     * }
     * @constructor
     * @param {Object} $     the jQuery object
     * @param {SliderContext} context    instance of SliderContext class; must be separate for each instance (@see SliderContext)
     * @param {Object} options    Slider options object
     */
    function Slider($, context, options) {
        var defaults = {
            'navigation': {},
            'timeout': 10000,
            'transition': 'BasicTransition',
            'isPauseEnabled': true,
            'timeIndicator': {
                'isEnabled': false,
                'selector': null,
                'view': 'View',
                'options': {}
            }
        };

        /**
         * Instance of SliderContext class (@see SliderContext)
         *
         * @member Navigation
         */
        this.context = context;

        context.owner = this;
        context.defaults = defaults;
        context.settings = $.extend(true, {}, defaults, options);
        context.transitionSettings.$slides = context.$slides;
    }

    /**
     * Executes method with given methodName on each Timer in context.timers (@see Timer)
     *
     * @member Slider
     * @param {String} methodName   name of a method, which will be executed on timers
     * @param {SliderContext} context    current context
     */
    Slider.prototype.executeOnTimers = function(methodName, context) {
        var i = null,
            timer = null,
            timers = context.timers;

        for (i = 0; i < timers.length; i++) {
            timer = timers[i];
            if (typeof(timer[methodName]) === 'function') {
                timer[methodName]();
            }
        }
    };

    /**
     * Deschedules change of a slide, by clearing timeout previously set by setTiemout() function.
     *
     * @member Slider
     * @param {SliderContext} context    current context
     */
    Slider.prototype.descheduleSlide = function(context) {
        if (context.timeoutId !== null) {
            clearTimeout(context.timeoutId);
            context.timeoutId = null;
        }
    };

    /**
     * Schedules change of a slide, by setting timeout (using setTiemout() function) if can.
     * If can't, tries again after short period of time.
     *
     * @member Slider
     * @param {SliderContext} context    current context
     */
    Slider.prototype.scheduleSlide = function(context) {
        var owner = context.owner;

        owner.descheduleSlide(context);

        if (!context.preventScheduling && context.$slides.length > 1) {
            owner.executeOnTimers('set', context);

            if (context.canChangeSlide) {
                if (context.timeIndicator !== null) {
                    context.timeIndicator.play();
                }
                context.timeoutId = setTimeout(function() {
                    owner.changeCurrentSlideBy(1, context);
                }, context.settings.timeout - context.slideTimer.elapsed);
            } else {
                setTimeout(function() {
                    owner.scheduleSlide(context);
                }, 100);
            }
        }
    };

    /**
     * Changes current slide, by offset performing context.transition
     * Any offset value is save (if it is an integer).
     *
     * @member Slider
     * @param {Number} offset    the offset to an index of a new slide
     * @param {SliderContext} context    current context
     */
    Slider.prototype.changeCurrentSlideBy = function(offset, context) {
        var settings = context.transitionSettings,
            $currentSlide = context.getCurrentSlide(),
            $slides = context.$slides;

        if ((offset % $slides.length) !== 0) {
            context.canChangeSlide = false;

            settings.offset = offset;
            settings.$currentSlide = $currentSlide;
            settings.$nextSlide = $slides.eq(($currentSlide.index() + offset) % $slides.length);

            context.changeCurrentSlide(settings.$nextSlide);
            context.transition.perform(context.transitionSettings, offset);
            context.owner.executeOnTimers('reset', context);
        }


        var $carousel = $(context.$wrapper).parents(".carousel");
        $carousel.trigger("slide-changed");
    };

    /**
     * Starts automatic slides changing
     *
     * @member Slider
     */
    Slider.prototype.run = function() {
        this.executeOnTimers('reset', this.context);
        this.scheduleSlide(this.context);
    };

    /**
     * Cancel the default action of event and changes slide manually.
     *
     * @member Slider
     * @param {Object} event     the event
     * @param {Number} offset    the offset to an index of a new slide
     * @param {SliderContext} context    current context
     */
    Slider.prototype.onChangeCurrentSlide = function(event, offset, context) {
        var owner = context.owner;

        event.preventDefault();
        if (context.canChangeSlide) {
            owner.descheduleSlide(context);
            owner.changeCurrentSlideBy(offset, context);
        }
    };

    /**
     * Selects navigation item with given index.
     *
     * @member Slider
     * @param {Number} index     the index of navigation item
     */
    Slider.prototype.selectNavigationItem = function(index) {
        this.context.navigation.selectItem(index);
    };
    /**
     * end of Slider class
     */

    /**
     * SliderInitializer object set up instances of Slider class
     */
    SliderInitializer = (function($) {
        var DIRECTION = {
            PREV: -1,
            NEXT: 1
        };

        /**
         * Attaches events to navigation items.
         *
         * @param {SliderContext} context   a context of a Slider class instance
         */
        function attachNavigationEvents(context) {
            var owner = context.owner,
                $navLinks = context.$wrapper.find(".nav-items"),
                $prevItemTxt = context.$wrapper.find('.prev-text'),
                $nextItemTxt = context.$wrapper.find('.next-text'),
                $carousel = context.$wrapper.parents(".carousel");

            context.navigation.$items.each(function (index) {
                var $slide = context.$slides.eq(index);

                $(this).click(function (event) {
                    owner.onChangeCurrentSlide(event, $slide.index() - context.getCurrentSlide().index(), context);
                });
            });

            $prevItemTxt.click(function (event) {
                switchSlide(event, context, DIRECTION.PREV);
            });

            $nextItemTxt.click(function (event) {
                switchSlide(event, context, DIRECTION.NEXT);
            });

            context.navigation.$prevItem.click(function (event) {
                switchSlide(event, context, DIRECTION.PREV);
            });

            context.navigation.$nextItem.click(function (event) {
                switchSlide(event, context, DIRECTION.NEXT);
            });

            $navLinks.on("keydown", function (event) {
                switch (event.keyCode) {
                    case 37: //Letf arrow
                        switchSlide(event, context, DIRECTION.PREV);
                        $(this).parent().find(".active").focus();
                        break;
                    case 39: //Right arrow
                        switchSlide(event, context, DIRECTION.NEXT);
                        $(this).parent().find(".active").focus();
                        break;
                    case 13: //Enter
                        focusFirstFocusableElement(owner.context.getCurrentSlide());
                }
            });

            $carousel.on("slide-changed", function(event){
                var $activeLink = $navLinks.find(".active");
                $activeLink.attr('tabindex', '0');
                
                if($navLinks.find(document.activeElement).length){
                    $activeLink.focus();
                }

                $navLinks.find(">div").not('.active').attr('tabindex', '-1');
            });
        }

        /**
         * Switch slides back and foreward
         *
         * @param {Event} event              an event which caused switch slide action
         * @param {SliderContext} context    a context of a Slider class instance
         * @param {number} direction         switch slide direction. -1 - switch to prev slide. 1 - switch to the next slide
         */
        function switchSlide(event, context, direction) {
            var owner = context.owner,
                navLinks$ = context.$wrapper.find(".nav-items");

            navLinks$.find(".active").attr('tabindex', '-1');
            owner.onChangeCurrentSlide(event, direction, context);
            navLinks$.find(".active").attr('tabindex', '0');
        }

        /**
         * Focus the first focusable element inside container
         *
         * @param {Container} container  a container element where the first focusable element will be focused
         */
        function focusFirstFocusableElement(container) {
            var focusableElementsString = 'a[href], area[href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), button:not([disabled]), iframe, object, embed, [tabindex="0"], [contenteditable]';

            // Find all focusable children
            var focusableElements = container.find(focusableElementsString);

            // If there are focusable elements - select the first one
            if (focusableElements.length !== 0) {
                var firstTabStop = focusableElements[0];

                // Focus first
                firstTabStop.focus();
            }
        }

        /**
         * Attaches pause event to navigation wrapper.
         *
         * @param {SliderContext} context   a context of a Slider class instance
         */
        function attachPauseEvent(context) {
            var owner = context.owner,
                $element = context.$wrapper,
                $navLinks = $element.find(".nav a");

            var callbackMouseenter = function() {
                context.preventScheduling = true;
                owner.descheduleSlide(context);

                owner.executeOnTimers('update', context);
                if (context.timeIndicator !== null) {
                    context.timeIndicator.pause();
                }
            }

            $element.mouseenter(callbackMouseenter);
            $navLinks.on("focus", callbackMouseenter);

            var callbackMouseLeave = function() {
                context.preventScheduling = false;
                owner.scheduleSlide(context);
            }

            $element.mouseleave(callbackMouseLeave);
            $navLinks.on("blur", callbackMouseLeave);
        }

        /**
         * Creates time indicator view
         *
         * @param {SliderContext} context   a context of a Slider class instance
         */
        function createIndicator(context) {
            var settings = context.settings,
                timeIndicator = null,
                ViewConstructor = XA.component.carousels.IndicatorViews[settings.timeIndicator.view],
                view = null;

            if (ViewConstructor !== null) {
                view = new ViewConstructor(settings.timeIndicator.selector, settings.timeIndicator.options);
                timeIndicator = new TimeIndicator($, view, settings.timeout);
                context.timeIndicator = timeIndicator;
                context.timers.push(timeIndicator);
            }
        }

        /**
         * Attaches events to navigation.
         *
         * @param {SliderContext} context   a context of a Slider class instance
         */
        function initializeNavigation(context) {
            context.settings.navigation.slidesCount = context.$slides.length;
            context.navigation = new Navigation($, context.$wrapper, context.settings.navigation);

            setTimeout(function() {
                attachNavigationEvents(context);
            }, 1000);


            if (context.settings.isPauseEnabled) {
                attachPauseEvent(context);
            }
        }

        /**
         * Initializes transition for a given context.
         *
         * @param {SliderContext} context   a context of a Slider class instance
         */
        function initializeTransition(context) {
            var TransitionConstructor = null,
                owner = context.owner;

            context.transitionSettings.callback = function() {
                context.canChangeSlide = true;

                if (context.preventScheduling) {
                    owner.executeOnTimers('reset', context);
                } else {
                    owner.scheduleSlide(context);
                }
            };

            TransitionConstructor = XA.component.carousels.Transitions[context.settings.transition];
            if ((TransitionConstructor === null) || (TransitionConstructor === undefined)) {
                TransitionConstructor = XA.component.carousels.Transitions[context.defaults.transition];
            }
            context.transition = new TransitionConstructor();
            context.transition.init(context.transitionSettings);
        }

        return {
            /**
             * Initializes Slider object with a given context.
             *
             * @param {SliderContext} context   a context of a Slider class instance
             */
            initialize: function(context) {
                initializeNavigation(context);
                initializeTransition(context);

                if (context.settings.timeIndicator.isEnabled) {
                    createIndicator(context);
                }
            }
        };
    }($));
    /**
     * end of SliderInitializer
     */

    /**
     * Carousel class displays one slide (some content) at the time.
     * Its behaviour depends on settings (passed to init() method).
     * For more details @see Slider, @see SliderInitializer, @see Navigation,
     * @see NavigationBuilder, @see XA.component.carousels.Transitions.Transition,
     * @see XA.component.carousels.IndicatorViews.View
     *
     * @constructor
     */
    function Carousel() {
        var context = this;

        /**
         * Instance of a Slider class (@see Slider).
         *
         * @member Carousel
         */
        this.slider = null;
        /**
         * Displayed slide (jQuery object).
         *
         * @member Carousel
         */
        this.$currentSlide = null;
        /**
         * All slides (jQuery object).
         *
         * @member Carousel
         */
        this.$slides = null;

        /**
         * Changes current slide to a $newSlide.
         *
         * @member Carousel
         * @param {Object} $newSlide  the new slide (jQuery) object.
         */
        this.changeCurrentSlide = function($newSlide) {
            context.$currentSlide = $newSlide;
            context.slider.selectNavigationItem($newSlide.index());
        };

        /**
         * Returns current slide.
         *
         * @member Carousel
         * @return current slide (jQuery) object.
         * @type Object
         */
        this.getCurrentSlide = function() {
            return context.$currentSlide;
        };

        this.data = {};
    }

    /**
     * Resets the carousel.
     *
     * @member Carousel
     */
    Carousel.prototype.reset = function() {
        this.$slides.each(function() {
            $(this).hide();
        });

        this.changeCurrentSlide(this.$slides.first());
        this.$currentSlide.show();
    };

    /**
     * Initializes the carousel.
     * Wrapps carousel content with <div class="wrapper" /> element,
     * selects slides (<li /> elements) from <ul class="slides" /> element,
     * initializes Slider class inctance, resets the carousel and start automatic slides changing.
     *
     * @member Carousel
     * @param {Object} $container     jQuery object with a main html element of the carousel
     * @param {Object} options     carousel options object
     */
    Carousel.prototype.init = function($container, options) {
        var $wrapper = $('<div/>', {
                'class': 'wrapper'
            }),
            sliderContext = null;

        $wrapper.append($container.children().detach());
        $container.append($wrapper);
        this.$slides = $container.find('.slides li.slide');

        sliderContext = new SliderContext();
        sliderContext.changeCurrentSlide = this.changeCurrentSlide;
        sliderContext.getCurrentSlide = this.getCurrentSlide;
        sliderContext.$slides = this.$slides;
        sliderContext.$wrapper = $wrapper;

        this.slider = new Slider($, sliderContext, options);
        SliderInitializer.initialize(sliderContext);

        this.reset();
        this.slider.run();
    };
    /**
     * end of Carousel class
     */
   

    /* Carousel touch support */
    Carousel.prototype.swipeSlide = function() {
        var self = this,
            $wrapper = this.slider.context.$wrapper,
            hammer = new Hammer($wrapper[0]);
            hammer.get('pan').set({ direction: Hammer.DIRECTION_ALL });

        hammer.on("swipeleft", function(e) {
            self.slider.context.owner.onChangeCurrentSlide(e, 1, self.slider.context);
        });

        hammer.on("swiperight", function(e) {
            self.slider.context.owner.onChangeCurrentSlide(e, -1, self.slider.context);
        });
    };
    
    Carousel.prototype.maxSlideInfoHeight = function(){
        var maxHeight = this.$slides[0].querySelector('.slide-info').offsetHeight;
        for (var i = 1; i < this.$slides.length; i++){
            var currSlideInfoHeight = this.$slides[i].querySelector('.slide-info').offsetHeight
            if (maxHeight <currSlideInfoHeight ){
                maxHeight = currSlideInfoHeight;
            }
        }
        return maxHeight;
    }

    Carousel.prototype.resizeWrapper = function() {
            var slideInfoHeight = this.maxSlideInfoHeight(),
                wrapper = this.slider.context.$wrapper;
            wrapper.find('.slide-info').css({
                'min-height': slideInfoHeight
            });
        
    };

    pub.initInstance = function(component){
              var properties = component.data("properties"),
                component = component.find(".carousel-inner"),
                id = component.attr("data-id"),
                self = component.eq(0),
                querySlideIndex = Number.parseInt(XA.queryString.getQueryParam(component.closest('.component.carousel').attr("id") || "carousel")),
                validQueryParam = typeof querySlideIndex !== "undefined" && !Number.isNaN(querySlideIndex) && querySlideIndex < component.find(".sxa-numbers").length,
                context, settings, $currentSlide, $slides;

            if (properties) {
                if (!properties.navigation) {
                    properties.navigation = {
                        item: {},
                        prevItem: {},
                        nextItem: {},
                        isEnabled: validQueryParam ? false: true
                    };
                }
                properties.navigation.item.selector = "[data-id='" + id + "'] .nav-items"; //properties.navigation.item.selector;
                properties.navigation.prevItem.selector = "#" + id + " .nav";
                properties.navigation.nextItem.selector = "#" + id + " .nav";

                var carousel = new Carousel($);
                carousel.init(component, properties);

                $(document).ready(function() {
                    carousel.resizeWrapper();
                });

                if (validQueryParam) {
                    context = carousel.slider.context,
                    settings = context.transitionSettings,
                    $currentSlide = context.getCurrentSlide(),
                    $slides = context.$slides;

                    context.canChangeSlide = false;
                    settings.$currentSlide = $currentSlide;
                    settings.$nextSlide = $($slides[querySlideIndex]);

                    context.changeCurrentSlide(settings.$nextSlide);
                    context.transition.perform(context.transitionSettings);
                    context.owner.executeOnTimers('reset', context);

                    context.navigation.prepareItems(properties.navigation.item, $slides.length, $(context.$wrapper));
                    context.navigation.selectItem(querySlideIndex);
                } else {
                    carousel.swipeSlide();   
                }
            }
    }


    pub.init = function () {
        $(".carousel:not(.initialized)").each(function () {
            pub.initInstance($(this))

            $(this).addClass("initialized");
        });
    };

    return pub;
}(jQuery));
/**
 * end of XA.component.carousels module.
 */


XA.register("carousels", XA.component.carousels);



XA.component.carousels.Transitions = (function() {
    var pub = {};

    /**
     * TransitionSettings class holds data needed for changing slides
     * @constructor
     */
    function TransitionSettings() {}
    /**
     * Current slide (currently displayed) jQuery object
     *
     * @member TransitionSettings
     */
    TransitionSettings.prototype.$currentSlide = null;
    /**
     * Next slide jQuery object
     *
     * @member TransitionSettings
     */
    TransitionSettings.prototype.$nextSlide = null;
    /**
     * All slides (jQuery object)
     *
     * @member TransitionSettings
     */
    TransitionSettings.prototype.$slides = null;
    /**
     * Offset between $currentSlide and $nextSlide
     *
     * @member TransitionSettings
     */
    TransitionSettings.prototype.offset = 0;
    /**
     * Callback function which must be called after animation.
     *
     * @member TransitionSettings
     */
    TransitionSettings.prototype.callback = null;
    /**
     * end of TransitionSettings class
     */
    pub.TransitionSettings = TransitionSettings;

    /**
     * Transition class is a base class of all transitions.
     *
     * Important: settings.callback() (@see TransitionSettings) have to be called at the end of execution the perform() method.
     *
     * @constructor
     */
    function Transition() {}
    Transition.prototype = new Transition();
    /**
     * Initializes a transition with given settings (@see TransitionSettings).
     * 
     * @member Transition
     * @param {TransitionSettings} settings  transition settings
     */
    /* eslint-disable */
    Transition.prototype.init = function(settings) {
        //implementation using settings
    };
    /* eslint-enable */
    /**
     * Returns a factor associated with settings.offset (@see TransitionSettings), which
     * can be usefull for some animations.
     *
     * @member Transition
     * @param {TransitionSettings} settings  transition settings
     * @return The factor; 1 if offset is higher than 0, -1 otherwise
     * @type Number
     */
    Transition.prototype.factor = function(settings) {
        return (settings.offset > 0) ? 1 : -1;
    };
    /**
     * This method is called used by carousels to change slides.
     *
     * Important: settings.callback() (@see TransitionSettings) have to be called at the end of this method execution.
     *
     * @member Transition
     * @param {TransitionSettings} settings  transition settings
     */
    Transition.prototype.perform = function(settings) {
        settings.callback();
    };
    /**
     * End of Transition class
     */
    pub.Transition = Transition;

    /**
     * BasicTransition class changes slides without any animation.
     * Do not remove this class, becaouse carousels will use it by default,
     * if requested transition cannot be found.
     * @constructor
     */
    function BasicTransition() {}
    BasicTransition.prototype = new Transition();
    BasicTransition.constructor = BasicTransition;
    /**
     * @member BasicTransition
     */
    BasicTransition.prototype.perform = function(settings) {
        settings.$currentSlide.hide();
        settings.$nextSlide.show();

        settings.callback();
    };
    /**
     * End of BasicTransition class
     */
    pub.BasicTransition = BasicTransition;

    /**
     * FadeInTransition class changes slides by fading in new slide.
     * This class can be removed if effect is not used.
     * @constructor
     */
    function FadeInTransition() {}
    FadeInTransition.prototype = new Transition();
    FadeInTransition.constructor = FadeInTransition;
    /**
     * @member FadeInTransition
     */
    FadeInTransition.prototype.init = function() {
        /*settings.$slides.css({
         'position': 'absolute',
         'top': '0px',
         'left': '0px'
         });*/
    };
    /**
     * @member FadeInTransition
     */
    FadeInTransition.prototype.perform = function(settings) {
        var zIndexAttrib = 'z-index',
            $parent = settings.$slides.parent(),
            $nextSlide = settings.$nextSlide;

        $parent.css(zIndexAttrib, 0);
        $nextSlide.css(zIndexAttrib, 1);
        settings.$slides.css({
            'position': 'relative'
        });

        settings.$nextSlide.css({
            'position': 'absolute',
            'top': '0',
            'bottom': '0',
            'left': '0',
            'right': '0'
        });


        $nextSlide.fadeIn(function() {
            settings.$currentSlide.hide();

            settings.$nextSlide.css({
                'position': 'relative',
                'left': '0',
                'transform': 'none'
            });

            $nextSlide.css(zIndexAttrib, '');
            $parent.css(zIndexAttrib, '');

            settings.callback();
        });
    };
    /**
     * End of FadeInTransition class
     */
    pub.FadeInTransition = FadeInTransition;

    /**
     * SlideHorizontallyTransition class changes slides by sliding them horizontally.
     * This class can be removed if effect is not used.
     * If offset (difference between index of currently displayed slide and index of slide that will be shown)
     * is greater than 0, slides are sliding to the left. Otherwise (offset is lower than 0), slides are sliding
     * to the left.
     * When slide change is raised automatically, slides are always sliding to right (offset == 1).
     * If user raises slides change, slides are sliding:
     * - to left, if offset is greater than 0 (slide with higher index will be shown or user clicks on 'next' navigation item),
     * - to right, if offset is lower than 0 (slide with lower index will be shown or user clicks on 'prev' navigation item).
     * @constructor
     */
    function SlideHorizontallyTransition() {}
    SlideHorizontallyTransition.prototype = new Transition();
    SlideHorizontallyTransition.constructor = SlideHorizontallyTransition;
    /**
     * @member SlideHorizontallyTransition
     */
    SlideHorizontallyTransition.prototype.onAnimationComplete = function(settings, $parent) {
        settings.$currentSlide.hide();
        settings.$nextSlide.css({
            'top': '',
            'bottom': '',
            'left': '',
            'position': '',
            'width': ''
        });

        $parent.css({
            'left': ''
        });

        $parent.css('margin-left', '');

        settings.callback();
    };
    /**
     * @member SlideHorizontallyTransition
     */
    SlideHorizontallyTransition.prototype.perform = function (settings, offset) {
        var factor = this.factor(settings),
            onAnimationComplete = this.onAnimationComplete,
            width = settings.$currentSlide.width(),
            $parent = settings.$slides.parent();

        settings.$nextSlide.css({
            'top': '0',
            'bottom': '0',
            'left': '100%',
            'position': 'absolute',
            'width': '100%'
        });
        if (offset > 0) {
            settings.$nextSlide.css({
                'left': '100%',
            })
        } else {
            settings.$nextSlide.css({
                'left': '-100%',
            })
        }
        settings.$nextSlide.show();
        $parent.animate({
            'left': -factor * width + 'px',
        }, function() {
            onAnimationComplete(settings, $parent);
        });
    };
    /**
     * End of SlideHorizontallyTransition class
     */
    pub.SlideHorizontallyTransition = SlideHorizontallyTransition;

    /**
     * SlideVerticallyTransition class changes slides by sliding them vertically.
     * This class can be removed if this effect is not used.
     * If offset (difference between index of currently displayed slide and index of slide that will be shown)
     * is greater than 0, slides are sliding from up to down. Otherwise (offset is lower than 0), slides are sliding
     * from down to up.
     * When slide change is raised automatically, slides are always sliding from up to down (offset == 1).
     * If user raises slides change, slides are sliding:
     * - from up to down, if offset is greater than 0 (slide with higher index will be shown or user clicks on 'next' navigation item),
     * - from down to up, if offset is lower than 0 (slide with lower index will be shown or user clicks on 'prev' navigation item).
     * @constructor
     */
    function SlideVerticallyTransition() {}
    SlideVerticallyTransition.prototype = new Transition();
    SlideVerticallyTransition.constructor = SlideVerticallyTransition;
    /**
     * @member SlideVerticallyTransition
     */
    SlideVerticallyTransition.prototype.onAnimationComplete = function(settings, $parent) {
        settings.$currentSlide.hide();
        settings.$nextSlide.css({
            'top': '',
            'bottom': '',
            'left': '',
            'right': '',
            'position': '',
            'width': ''
        });

		$parent.css('top', '');
        $parent.css('margin-top', '');

        settings.callback();
    };
    /**
     * @member SlideVerticallyTransition
     */
    SlideVerticallyTransition.prototype.perform = function (settings, offset) {
        var factor = this.factor(settings),
            onAnimationComplete = this.onAnimationComplete,
            height = settings.$currentSlide.height(),
            $parent = settings.$slides.parent();

        settings.$nextSlide.css({
            'left': '0',
            'right': '0',
            'position': 'absolute',
            'width': '100%'
        });
        if (offset > 0) {
            settings.$nextSlide.css({
                'top': '-100%',
                'bottom': '100%',
            })
        } else {
            settings.$nextSlide.css({
                'top': '100%',
                'bottom': '-100%',
            })
        }
        settings.$nextSlide.show();

        $parent.animate({
            'top': factor * height
        }, function() {
            onAnimationComplete(settings, $parent);
        });

    };
    /**
     * SlideVerticallyTransition class
     */
    pub.SlideVerticallyTransition = SlideVerticallyTransition;

    return pub;
}(jQuery));
/**
 * end of XA.component.carousels.Transitions module.
 */

/**
 * XAContext
 *
 * XA.component.carousels.IndicatorViews module.
 *
 * Exposes View class, which is a base class of all 'time indicator' views used by carousels (@see XA.component.carousels, @see View).
 * Also exposes implementation of these 'time indicator' views:
 * - ProgressBarView - shows the progress of time, that looks like a progress bar,
 * - RotatorView - shows the progress of time with the filling up ring.
 * Carousel takes the class constructor by name from this module (it is given in options object of carousel; @see XA.component.carousels).
 * If there is a need to implement the new 'time indicator' view, create a new class,
 * by inheriting from View class and add it to this module. During implementation of a new 'time indicator'
 * view the XA.component.carousels.IndicatorsView.Tools namespace may be usefull. It contains:
 * - isCssSupported(props) function, which checks if any of CSS properties is supported (props - array of properties),
 * - isTransformSupported() function, which checks if CSS 3 'transform' is supported.
 *
 * Example view implementation (opacity increases with a progress of the time):
 * (function() {
 *     var View = XA.component.carousels.IndicatorViews.View;
 *
 *     function SampleView (selector, options) {
 *         //base constructor have to be called
 *         View.call(this, selector);
 *     }
 *
 *     SampleView.prototype = new View();
 *     SampleView.constructor = SampleView;
 *
 *     SampleView.prototype.init = function (timeout) {
 *         //base method have to be called
 *         View.prototype.init.call(this, timeout);
 *
 *         //initialization code - not needed in this example
 *     };
 *
 *     SampleView.prototype.reset = function () {
 *         //base method have to be called
 *         View.prototype.reset.call(this);
 *
 *         //reseting 'time indicator' view
 *         this.$indicator.stop(true);
 *         this.$indicator.css('opacity', 0);
 *     };
 *
 *     SampleView.prototype.pause = function () {
 *         //stoping 'time indicator' view animation
 *         this.$indicator.stop(true);
 *     };
 *
 *     SampleView.prototype.play = function () {
 *         //starting/resuming 'time indicator' view animation
 *         this.$indicator.animate({
 *             'opacity': 1
 *         }, this.timeout - this.timeElapsed, 'linear');
 *     };
 *
 *     XA.component.carousels.IndicatorViews.SampleView = SampleView;
 * }());
 *
 */
XA.component.carousels.IndicatorViews = (function($) {
    var pub = {},
        supportElementStyle = null;

    /**
     * Checks if any of CSS properties is supported.
     *
     * @param {Array} props     array of properties to be checked
     * @return true if any of CSS properties is supported, false otherwise
     * @type Boolean
     */
    function isCssSupported(props) {
        var i,
            isSupported = false;

        if (supportElementStyle === null) {
            supportElementStyle = document.createElement('supportElement').style;
        }

        for (i = 0; i < props.length; i++) {
            if (supportElementStyle[props[i]] !== undefined) {
                isSupported = true;
                break;
            }
        }

        return isSupported;
    }

    /**
     * Checks if CSS 3 'transform' is supported.
     *
     * @return true if CSS 3 'transform' is supported, false otherwise
     * @type Boolean
     */
    function isTransformSupported() {
        return isCssSupported(['transformProperty', 'WebkitTransform', 'MozTransform', 'OTransform', 'msTransform']);
    }

    /**
     * XA.component.carousels.IndicatorViews.Tools namespace
     *
     This namespace can be removed if RotatorView class is not used (@see RotatorView)
     */
    pub.Tools = {};
    pub.Tools.isCssSupported = isCssSupported;
    pub.Tools.isTransformSupported = isTransformSupported;
    /**
     * End of namespace
     */

    /**
     * View class is a base class of all 'time indicator' views
     * @constructor
     * @param {String} selector a selector used for searching
     * @param {Object} options   View options object
     */
    /* eslint-disable */
    function View(selector, options) {
        /**
         * A jQuery object containing element, which will be animated to show pogress of the time.
         *
         * @member View
         */

        this.$indicator = $(selector);
    }
    /* eslint-enable */
    /**
     * Initializes the view.
     *
     * @member Viewion
     * @param {Number} timeout   a timeout to the end of whole indicator animation
     */
    View.prototype.init = function(timeout) {
        this.timeElapsed = 0;
        this.timeout = timeout;
    };
    /**
     * Resets the view
     *
     * @member View
     */
    View.prototype.reset = function() {
        this.timeElapsed = 0;
    };
    /**
     * Starts/Renews the view animation.
     *
     * @member View
     */
    View.prototype.play = function() {};
    /**
     * Pauses the view animation.
     *
     * @member View
     */
    View.prototype.pause = function() {};
    /**
     * Updates the time that elapsed since animation begin.
     *
     * @member View
     */
    View.prototype.update = function(timeElapsed) {
        this.timeElapsed = timeElapsed;
    };
    /**
     * end of View class
     */
    pub.View = View;

    /**
     * ProgressBarView class provides a logic for animate a 'time indicator',
     * that looks like progress bar.
     *
     * Options object:
     * {
     *     'opacity': Number value //between 0 and 1; default is 0.8
     * }
     *
     * This class can be removed if this kind of view is not used.
     *
     * @constructor
     * @param {String} selector a selector used for searching
     * @param {Object} options   View options object
     */
    function ProgressBarView(selector, options) {
        View.call(this, selector);

        var defaults = {
            'opacity': 0.8
        };

        this.settings = $.extend(true, {}, defaults, options);

        this.$container = this.$indicator;
        this.$indicator = this.$indicator.find('div');
    }
    ProgressBarView.prototype = new View();
    ProgressBarView.constructor = ProgressBarView;
    /**
     * @member ProgressBarView
     */
    ProgressBarView.prototype.init = function(timeout) {
        View.prototype.init.call(this, timeout);

        var opacity = this.settings.opacity;
        this.$container.css('opacity', opacity);
        this.$indicator.css('opacity', opacity);
    };
    /**
     * @member ProgressBarView
     */
    ProgressBarView.prototype.reset = function() {
        View.prototype.reset.call(this);

        this.$indicator.stop(true);
        this.$indicator.css('width', '0px');
    };
    /**
     * @member ProgressBarView
     */
    ProgressBarView.prototype.pause = function() {
        this.$indicator.stop(true);
    };
    /**
     * @member ProgressBarView
     */
    ProgressBarView.prototype.play = function() {
        this.$indicator.animate({
            'width': '100%'
        }, this.timeout - this.timeElapsed, 'linear');
    };
    /**
     * end of ProgressBarView class
     */
    pub.ProgressBarView = ProgressBarView;

    /**
     * RotatorView class provides a logic for animate a 'time indicator',
     * that shows the progress of time with the filling up ring
     *
     * Options object:
     * {
     *     'opacity': Number value //between 0 and 1; default is 0.5
     * }
     *
     * This class can be removed if this kind of view is not used.
     *
     * @constructor
     * @param {String} selector a selector used for searching
     * @param {Object} options   View options object
     */
    function RotatorView(selector, options) {
        View.call(this, selector);

        var defaults = {
            'opacity': 0.5
        };

        this.settings = $.extend(true, {}, defaults, options);
        this.$mask = this.$indicator.find('span.mask').first();
        this.$rotator = this.$indicator.find('span.rotator').first();

        function rotateCss(deg) {
            var value = 'rotate(' + deg + 'deg)';

            return {
                '-webkit-transform': value,
                '-moz-transform': value,
                '-o-transform': value,
                '-ms-transform': value,
                'transform': value
            };
        }

        function animateRotator($rotator, options) {
            var defaults = {
                    'fontSize': '180px',
                    'animateOptions': {
                        'step': function(now) {
                            $rotator.css(rotateCss(now));
                        },
                        'easing': 'linear'
                    }
                },
                settings = $.extend(true, {}, defaults, options);

            $rotator.animate({
                    'font-size': settings.fontSize
                },
                settings.animateOptions);
        }

        function phaseTwoAnimation($mask, $rotator, duration) {
            $rotator.addClass('half');
            $mask.addClass('half');

            animateRotator($rotator, {
                'fontSize': '360px',
                'animateOptions': {
                    'duration': duration
                }
            });
        }

        function phaseOneAnimation($mask, $rotator, duration, phaseTwoDuration) {
            animateRotator($rotator, {
                'animateOptions': {
                    'duration': duration,
                    'complete': function() {
                        phaseTwoAnimation($mask, $rotator, phaseTwoDuration);
                    }
                }
            });
        }

        this.resetCss = function() {
            this.$rotator.removeClass('half');
            this.$rotator.css(rotateCss(0));
            this.$rotator.css('font-size', '0px');

            this.$mask.removeClass('half');
        };

        this.playPhaseOne = function() {
            var duration = this.timeout / 2 - this.timeElapsed;
            phaseOneAnimation(this.$mask, this.$rotator, duration, this.timeout / 2);
        };

        this.playPhaseTwo = function() {
            var duration = this.timeout - this.timeElapsed;
            phaseTwoAnimation(this.$mask, this.$rotator, duration);
        };
    }
    RotatorView.prototype = new View();
    RotatorView.constructor = RotatorView;
    /**
     * @member RotatorView
     */
    RotatorView.prototype.init = function(timeout) {
        View.prototype.init.call(this, timeout);

        if (isTransformSupported()) {
            this.$indicator.show();
            this.$mask.css('opacity', this.settings.opacity);
        }
    };
    /**
     * @member RotatorView
     */
    RotatorView.prototype.reset = function() {
        View.prototype.reset.call(this);

        this.$rotator.stop(true);
        this.resetCss();
    };
    /**
     * @member RotatorView
     */
    RotatorView.prototype.pause = function() {
        this.$rotator.stop(true);
    };
    /**
     * @member RotatorView
     */
    RotatorView.prototype.play = function() {
        var phase = this.playPhaseOne;
        if (this.timeElapsed >= this.timeout / 2) {
            phase = this.playPhaseTwo;
        }

        phase.call(this);
    };
    /**
     * end of RotatorView class
     */
    pub.RotatorView = RotatorView;

    return pub;
}(jQuery));
/**
 * end of XA.component.carousels.IndicatorViews module.
 */