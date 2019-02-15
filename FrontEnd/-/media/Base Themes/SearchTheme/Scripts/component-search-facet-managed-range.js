XA.component.search.facet.managedrange = (function ($, document) {

    var api = {},
        urlHelperModel,
        queryModel,
        apiModel,
        initialized = false;

    var FacetManagedRangeModel = Backbone.Model.extend({
        defaults: {
            dataProperties: {},
            sig: []
        }
    });

    var FacetManagedRangeView = XA.component.search.baseView.extend({
        initialize: function () {
            this.properties = this.$el.data().properties;

            if (this.model) {
                this.model.set({ dataProperties: this.properties });
                this.model.set("sig", this.translateSignatures(this.properties.searchResultsSignature, this.properties.f));
            }

            if (this.$el.find(".filterButton").length === 0) {
                //if there won't be filter button then we still have to have possibility to use manual ranges
                this.$el.find('.manualRangeMin').on('blur', this.textBoxChange.bind(this, []));
                this.$el.find('.manualRangeMax').on('blur', this.textBoxChange.bind(this, []));
            }
            this.model.on("change", this.render, this);

            XA.component.search.vent.on("hashChanged", this.updateComponent.bind(this));
        },
        events: {
            'click .faceLink': 'linkClick',
            'click .facetRadio': 'radioClick',
            'click .facetCheckbox': 'checkBoxClick',
            'click .filterButton': 'filter',
            'click .bottom-remove-filter, .clear-filter': 'clearFilter',
            'keyup .manualRangeMin, .manualRangeMax': 'configureKeyCodes'
        },
        configureKeyCodes : function (e) {
            if (e.keyCode == 13) {
                this.filter();
            }
        },
        updateHash: function (foundRangeControls) {
            var sig = this.model.get('sig'),
                ranges = [];

            _.each(foundRangeControls, function (range) {
                var $range = $(range);
                ranges.push($range.data().minvalue + "|" + $range.data().maxvalue);
            });

            this.$el.find('.manualRangeMin').val('');
            this.$el.find('.manualRangeMax').val('');

            queryModel.updateHash(this.updateSignaturesHash(sig, ranges.join(','), {}));
        },
        render: function () {
            var hashObj = queryModel.parseHashParameters(window.location.hash),
                facetClose = this.$el.find(".facet-heading > span"),
                sig = this.model.get('sig'),
                inst = this,
                ranges, 
                i;

            for (i = 0; i < sig.length; i++) {
                if (hashObj.hasOwnProperty(sig[i]) && hashObj[sig[i]] !== '') {
                    ranges = hashObj[sig[i]].split(",");

                    facetClose.addClass('has-active-facet');
                    _.each(ranges, function (range) {
                        var r = range.split('|'),
                            minValue = r[0],
                            maxValue = r[1],
                            selector,
                            $component;

                        if (minValue === "" && maxValue === "") {
                            return;
                        } else if (minValue !== "" && maxValue !== "") {
                            selector = ".facetCheckbox[data-minvalue='" + minValue + "'][data-maxvalue='" + maxValue + "'], .facetRadio[data-minvalue='" + minValue + "'][data-maxvalue='" + maxValue + "']";
                        } else if (minValue !== "" && maxValue === "") {
                            selector = ".facetCheckbox[data-minvalue='" + minValue + "'], .facetRadio[data-minvalue='" + minValue + "']";
                        } else if (minValue === "" && maxValue !== "") {
                            selector = ".facetCheckbox[data-maxvalue='" + maxValue + "'], .facetRadio[data-maxvalue='" + maxValue + "']";
                        }

                        $component = $(selector);

                        if ($component.length > 0) {
                            $component.attr("checked", "checked");
                        } else {
                            inst.$el.find('.manualRangeMin').val(r[0]);
                            inst.$el.find('.manualRangeMax').val(r[1]);
                        }
                    });
                }
            }
        },
        radioClick: function (param) {
            //fix for radio buttons groups in the ASP.NET repeater control
            var radio = $(param.currentTarget);
            $('.facetRadio').attr("name", radio.attr("name"));
            this.updateHash(radio);
        },
        checkBoxClick: function (param) {
            var checkbox = $(param.currentTarget);
            if (checkbox.is(":checked")) {
                this.$el.find('.manualRangeMin').val("");
                this.$el.find('.manualRangeMax').val("");
            }
        },
        linkClick: function (param) {
            var $link = $(param.currentTarget),
                hashValue = $link.data().minvalue + "|" + $link.data().maxvalue;

            this.$el.find(".facetCheckbox[data-shortid!=" + $link.data().shortid + "]").removeAttr("checked");
            this.$el.find(".facetCheckbox[data-shortid=" + $link.data().shortid + "]").attr("checked", "checked");
            this.$el.find('.manualRangeMin').val("");
            this.$el.find('.manualRangeMax').val("");

            queryModel.updateHash(this.updateSignaturesHash(sig, hashValue, {}));
        },
        filter: function () {
            var multipleSelection = this.model.get('dataProperties').multipleSelection,
                checkedCheckBoxes = this.$el.find('.facetCheckbox:checked'),
                checkedRadioButton = this.$el.find('.facetRadio:checked'),
                minRange = this.$el.find('.manualRangeMin'),
                maxRange = this.$el.find('.manualRangeMax'),
                sig = this.model.get('sig'),
                maxRange, minRange, minValue, maxValue;

            //get values from text boxes
            minValue = minRange.length > 0 && minRange.val() !== "" ? minRange.val() : "";
            maxValue = maxRange.length > 0 && maxRange.val() !== "" ? maxRange.val() : "";

            if (minValue !== "" || maxValue !== "") {
                //clear all selected radio buttons and checkboxes
                this.$el.find(".facetRadio").removeAttr("checked");
                this.$el.find(".facetCheckbox").removeAttr("checked");

                //auto update hash after text box change just in case that there is no filter button
                queryModel.updateHash(this.updateSignaturesHash(sig, minValue + "|" + maxValue, {}));
            } else if (checkedCheckBoxes.length > 0 || checkedRadioButton.length > 0) {
                if (multipleSelection) {
                    this.updateHash(checkedCheckBoxes);
                } else {
                    this.radioClick({ currentTarget: checkedRadioButton });
                }
            }
        },
        clearFilter: function () {
            var minRange = this.$el.find('.manualRangeMin'),
                maxRange = this.$el.find('.manualRangeMax'),
                properties = this.model.get('dataProperties'),
                dataProperties = this.$el.data('properties'),
                facetClose = this.$el.find('.facet-heading > span'),
                hash = queryModel.parseHashParameters(window.location.hash),
                sig = this.model.get('sig'),
                updatedProperties,
                i;

            for (i = 0; i < sig.length; i++) {
                delete properties[sig[i]];
                hash[sig[i]] = "";
            }
            queryModel.updateHash(hash);

            this.model.set({ dataProperties: properties });

            this.$el.find('.facetCheckbox').removeAttr("checked");
            this.$el.find('.facetRadio').removeAttr("checked");

            facetClose.removeClass('has-active-facet');

            if (minRange.length > 0) {
                minRange.val(minRange.data().defaultvalue);
            }
            if (maxRange.length > 0) {
                maxRange.val(maxRange.data().defaultvalue);
            }
        },
        textBoxChange: function () {
            var minRange = this.$el.find('.manualRangeMin'),
                maxRange = this.$el.find('.manualRangeMax'),
                minValue = minRange.length > 0 && minRange.val() !== "" ? minRange.val() : "",
                maxValue = maxRange.length > 0 && maxRange.val() !== "" ? maxRange.val() : "",
                sig = this.model.get('sig');

            //clear all selected radio buttons and checkboxes
            this.$el.find(".facetRadio").removeAttr("checked");
            this.$el.find(".facetCheckbox").removeAttr("checked");            

            if (minValue !== "" || maxValue !== "") {
                //auto update hash after text box change just in case that there is no filter button
                queryModel.updateHash(this.updateSignaturesHash(sig, minValue + "|" + maxValue, {}));
            } else {
                //clear all radio buttons and checkbox hash parameters
                queryModel.updateHash(this.updateSignaturesHash(sig, "", {}));            
            }
        },
        updateComponent: function (hash) {
            var sig = this.model.get('sig'),
                facetPart,
                i;
            for (i = 0; i < sig.length; i++) {
                facetPart = sig[i].toLowerCase();
                if (hash.hasOwnProperty(facetPart)) {
                    this.render();
                } else {
                    this.clearFilter();
                }
            }
        }
    });

    api.init = function () {
        if ($("body").hasClass("on-page-editor") || initialized) {
            return;
        }

        queryModel = XA.component.search.query;
        apiModel = XA.component.search.ajax;
        urlHelperModel = XA.component.search.url;

        var facetManagedRangesList = $(".facet-managed-range");
        _.each(facetManagedRangesList, function (elem) {
            var model = new FacetManagedRangeModel(),
                view = new FacetManagedRangeView({ el: $(elem), model: model });
            view.render();
        });

        initialzied = true;
    };

    return api;

} (jQuery, document));

XA.register('managedrange', XA.component.search.facet.managedrange);
