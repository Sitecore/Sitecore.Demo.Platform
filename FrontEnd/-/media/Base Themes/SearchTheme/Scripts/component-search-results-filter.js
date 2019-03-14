XA.component.search.facet.resultsfilter = (function ($, document) {

    var api = {},
        urlHelperModel,
        queryModel,
        apiModel,
        initialized = false;

    var FacetResultsFilterModel = XA.component.search.baseModel.extend({
        defaults: {
            template: "<div class='facet-search-filter'><% " +
                "_.forEach(facet.Values, function(value){" +
                "%><p class='facet-value' data-facetValue='<%= value.Name !== '' ? encodeURI(value.Name) : '_empty_' %>'>" +
                "<span><%= value.Name !== '' ? value.Name : emptyText %> " +
                "<span class='facet-count'>(<%= value.Count %>)</span>" +
                "</span>" +
                "</p><%" +
                " }); %>" +
                "</div>",

            templateMulti: "<div class='facet-search-filter'><% " +
                "_.forEach(facet.Values, function(value){" +
                "%><p class='facet-value' data-facetValue='<%= value.Name !== '' ? encodeURI(value.Name) : '_empty_' %>'>" +
                "<input type='checkbox' name='facetValue' />" +
                "<label for='facetName'><%= value.Name !== '' ? value.Name : emptyText %> " +
                "<span class='facet-count' data-facetCount='<%= value.Count %>'>(<%= value.Count %>)</span>" +
                "</label>" +
                "</p><%" +
                " }); %>" +
                "</div>",
            dataProperties: {},
            blockNextRequest: false,
            resultData: {},
            timeStamp: '',
            sig: []
        },
        initialize: function () {
            //event to get data at the begining or in case that there are no hash parameters in the url - one request for all controls
            XA.component.search.vent.on("facet-data-loaded", this.processData.bind(this));
            //if in the url hash we have this control facet name (someone clicked this control) then we have to listen for partial filtering
            XA.component.search.vent.on("facet-data-partial-filtered", this.processData.bind(this));
            //in case that we are not filtering by this control (not clicked)
            XA.component.search.vent.on("facet-data-filtered", this.processData.bind(this));
            //event after change of hash
            XA.component.search.vent.on("hashChanged", this.updateComponent.bind(this));

            this.set({ facetArray: [] });
        },
        toggleBlockRequests: function () {
            var state = this.get("blockNextRequest");
            this.set(this.get("blockNextRequest"), !state);
        },
        processData: function (data) {
            var inst = this,
                dataProperties = this.get('dataProperties'),
                sig = dataProperties.searchResultsSignature.split(','),
                sortOrder = dataProperties.sortOrder,
                i;

            if (data.Signature === null) {
                data.Signature = "";
            }


            for (i = 0; i < sig.length; i++) {
                if (data.Facets.length > 0 && (data.Signature === sig[i])) {
                    var facedData = _.find(data.Facets, function (f) {
                        return f.Key.toLowerCase() === inst.get('dataProperties').f.toLowerCase();
                    });
                    if (facedData !== undefined) {
                        this.sortFacetArray(sortOrder, facedData.Values);
                        inst.set({ resultData: facedData });
                    }
                }
            }
        },
        updateFacetArray: function (valuesString) {
            if (valuesString) {
                var values = valuesString.split(','),
                    array = this.get('facetArray');
                for (var i = 0; i < values.length; i++) {
                    array.push(values[i]);
                }
                this.set({ facetArray: _.unique(array) });
            }
        },
        updateComponent: function (hash) {
            var sig = this.get("sig");
            for (i = 0; i < sig.length; i++) {
                if (!hash.hasOwnProperty(sig[i])) {
                    this.set({ facetArray: [] });
                } else {
                    this.updateFacetArray(hash[sig[i]]);
                }
                //in some cases change of facetArray doesn't trigger model change event (why?) and view isn't updates
                //and because of that timeStamp is updated which properly triggers model change event
                this.set("timeStamp", (new Date()).getTime());
            }
        }
    });

    var FacetResultsFilterView = XA.component.search.baseView.extend({
        initialize: function () {
            var dataProperties = this.$el.data(),
                hash = queryModel.parseHashParameters(window.location.hash),
                properties = dataProperties.properties,
                signatures,
                i;


            if (dataProperties.properties.searchResultsSignature === null) {
                dataProperties.properties.searchResultsSignature = "";
            }

            signatures = this.translateSignatures(properties.searchResultsSignature, properties.f);

            this.model.set({ dataProperties: properties });
            this.model.set("sig", signatures);

            for (i = 0; i < signatures.length; i++) {
                if (!jQuery.isEmptyObject(_.pick(hash, signatures[i]))) {
                    var values = _.values(_.pick(hash, signatures[i]))[0];
                    this.model.updateFacetArray(values)
                }
            }

            this.model.on("change", this.render, this);
        },
        events: {
            'click .facet-value': 'updateFacet',
            'click .filterButton': 'updateFacet',
            'click .clear-filter': 'removeFacet',
            'click .bottom-remove-filter > button': 'removeFacet'
        },
        updateFacet: function (param) {
            var currentFacet = $(param.currentTarget),
                facetArray = this.model.get('facetArray'),
                properties = this.model.get('dataProperties'),
                facetClose = this.$el.find('.facet-heading > span'),
                facetGroup = currentFacet.parents('.component-content').find('.facet-search-filter'),
                facetName = properties.f.toLowerCase(),
                facetDataValue = currentFacet.data('facetvalue'),
                facetValue = typeof facetDataValue !== "undefined" ? decodeURIComponent(facetDataValue) : facetDataValue,
                sig = this.model.get('sig'),
                index,
                hash = {},
                i;

            if (properties.multi) {
                if (facetValue) {
                    if (currentFacet.is(':not(.active-facet)')) {
                        this.setActiveFacet(facetName, facetValue);
                        facetArray.push(facetValue);
                    } else {
                        currentFacet.removeClass('active-facet');

                        currentFacet.find('[type=checkbox]').prop('checked', false);
                        currentFacet.find('[type=checkbox] + label:after').css({ 'background': '#fff' });

                        index = facetArray.indexOf(facetValue);
                        if (index > -1) {
                            facetArray.splice(index, 1);
                        }

                        if (facetArray.length == 0) {
                            facetClose.removeClass('has-active-facet');
                        }
                    }
                    this.model.set({ facetArray: facetArray });
                }

                //is there any better way to check what action start method?
                if (currentFacet[0].type == "button") {
                    for (i = 0; i < sig.length; i++) {
                        hash[sig[i]] = _.uniq(facetArray, function (item) {
                            return JSON.stringify(item);
                        }).toString();
                    }
                    queryModel.updateHash(hash);
                }
            } else {
                if (facetValue) {
                    for (i = 0; i < sig.length; i++) {
                        hash[sig[i]] = facetValue;
                    }
                    facetGroup.data('active-facet', hash);
                    this.setActiveFacet(facetName, facetValue);
                    queryModel.updateHash(hash);
                }
            }

        },
        removeFacet: function (evt) {
            evt.preventDefault();

            var facets = this.$el,
                facetClose = facets.find('.facet-heading > span'),
                facetValues = facets.find('.facet-value'),
                sig = this.model.get('sig');

            queryModel.updateHash(this.updateSignaturesHash(sig, "", {}));

            facetClose.removeClass('has-active-facet');

            _.each(facetValues, function (single) {
                var $single = $(single);
                if ($single.hasClass('active-facet')) {
                    $single.removeClass('active-facet');
                    $single.find('[type=checkbox]').prop('checked', false);
                    $single.find('[type=checkbox] + label:after').css({ 'background': '#fff' });
                }
            });

            this.model.set({ facetArray: [] });
        },
        render: function () {
            var inst = this,
                resultData = this.model.get("resultData"),
                facetClose = this.$el.find('.facet-heading > span'),
                facetNames = this.model.get('dataProperties').f.split('|'),
                emptyValueText = this.model.get('dataProperties').emptyValueText,
                highlightThreshold = this.model.get('dataProperties').highlightThreshold,
                hash = queryModel.parseHashParameters(window.location.hash),
                sig = this.model.get('sig'),
                template, facetName, templateResult;

            //check if we're opening page from disc - if yes then we are in Creative Exchange mode
            if (window.location.href.startsWith("file://")) {
                return;
            }

            if (resultData !== undefined) {
                if (inst.model.get('dataProperties').multi === true) {
                    template = _.template(inst.model.get("templateMulti"));
                } else {
                    template = _.template(inst.model.get("template"));
                }
                templateResult = template({ facet: resultData, emptyText: emptyValueText });
            }

            inst.$el.find(".contentContainer").html(templateResult);

            //check url hash for facet's and run setActiveFacet method for each facet filter
            _.each(facetNames, function (val) {
                facetName = val.toLowerCase();
                for (var i = 0; i < sig.length; i++) {
                    if (!jQuery.isEmptyObject(_.pick(hash, sig))) {
                        var values = _.values(_.pick(hash, sig))[0];
                        if (values) {
                            inst.setActiveFacet(facetName, values);

                            //if this rendering is supporting multiple signatures that it's enough it we will mark active facet once
                            return;
                        }
                    }
                }
            });

            //highlight facets count greater than chosen threshold
            if (highlightThreshold) {
                this.handleThreshold(highlightThreshold);
            }

            //if no facet is selected remove previously highlighted cross icon (while back button)
            if (this.model.get("facetArray").length === 0) {
                facetClose.removeClass('has-active-facet');
            } else {
                facetClose.addClass('has-active-facet');
            }
        },
        setActiveFacet: function (facetGroupName, facetValueName) {
            var properties = this.model.get('dataProperties'),
                facetChildren = this.$el.find('p[data-facetvalue]'),
                facetClose = this.$el.find('.facet-heading > span'),
                inst = this,
                facetValue,
                values;

            facetValueName = facetValueName.toString().toLowerCase();
            facetValue = this.$el.find("[data-facetvalue]").filter(function () {
                return decodeURIComponent($(this).attr("data-facetvalue").toLowerCase()) === facetValueName;
            });


            if (typeof (facetValueName) !== "undefined" && facetValueName !== null) {
                values = facetValueName.split(',');
            } else {
                return;
            }

            if (values.length > 1) {
                properties.multi = true;
            }

            if (properties.multi) {
                //multi selection facet search results
                _.each(facetChildren, function (val) {

                    if (values.length > 1) {
                        for (var i = 0, l = values.length; i < l; i++) {
                            facetValue = inst.$el.find("[data-facetvalue]").filter(function () {
                                return $(this).attr('data-facetvalue').toLowerCase() === values[i];
                            });
                            if (val === facetValue[0]) {
                                $(val).addClass('active-facet');
                                $(val).find('[type=checkbox]').prop('checked', true);
                            }
                        }
                    }

                    if (val === facetValue[0]) {
                        $(val).addClass('active-facet');
                        $(val).find('[type=checkbox]').prop('checked', true);
                    }


                });
            } else {
                //single selection facet search results filter allow only one facet type be selected
                _.each(facetChildren, function (val) {
                    if (val !== facetValue[0]) {
                        $(val).removeClass('active-facet');
                        $(val).find('[type=checkbox]').prop('checked', false);
                        $(val).find('[type=checkbox] + label:after').css({ 'background': '#fff' });
                    } else {
                        $(val).addClass('active-facet');
                    }
                });
            }

            //add active class to group close button
            facetClose.addClass('has-active-facet');
        },
        handleThreshold: function (highlightThreshold) {
            var facets = this.$el.find('.facet-search-filter').children('p');

            _.each(facets, function (single) {
                var $facet = $(single),
                    $facetCount = $facet.find('.facet-count'),
                    facetCount = $facetCount.data('facetcount');

                if (facetCount > highlightThreshold) {
                    $facetCount.addClass('highlighted');
                }
            });
        }
    });

    api.init = function () {
        if ($("body").hasClass("on-page-editor") || initialized) {
            return;
        }

        queryModel = XA.component.search.query;
        apiModel = XA.component.search.ajax;
        urlHelperModel = XA.component.search.url;

        var facetResultsFilterList = $(".facet-single-selection-list");
        _.each(facetResultsFilterList, function (elem) {
            var model = new FacetResultsFilterModel(),
                view = new FacetResultsFilterView({ el: $(elem), model: model });
        });

        initialized = true;
    };

    api.getFacetDataRequestInfo = function () {
        var facetList = $(".facet-single-selection-list"),
            result = [];

        _.each(facetList, function (elem) {
            var properties = $(elem).data().properties,
                signatures = properties.searchResultsSignature.split(','),
                i;

            for (i = 0; i < signatures.length; i++) {
                result.push({
                    signature: signatures[i] === null ? "" : signatures[i],
                    facetName: properties.f,
                    endpoint: properties.endpoint,
                    s: properties.s,
                    filterWithoutMe: !properties.collapseOnSelection
                });
            }
        });

        return result;
    };

    return api;

}(jQuery, document));

XA.register('facetResultsFilter', XA.component.search.facet.resultsfilter);