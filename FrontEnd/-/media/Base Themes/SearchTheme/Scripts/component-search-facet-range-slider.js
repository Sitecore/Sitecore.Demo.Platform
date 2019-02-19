XA.component.search.facet.rangeslider = (function ($, document) {

    var api = {},
        urlHelperModel,
        queryModel,
        initialized = false;


    var SearchFacetRangeSliderModel = Backbone.Model.extend({
        defaults: {
            dataProperties: {},
            sig: [],
            timeStamp: ''
        }
    });

    var SearchFacetRangeSliderView = XA.component.search.baseView.extend({
        initialize : function () {
            var dataProperties = this.$el.data();
            this.properties = dataProperties.properties;

            if (this.model) {
                this.model.set({dataProperties: this.properties});
                this.model.set("sig", this.translateSignatures(this.properties.searchResultsSignature, this.properties.f));
            }
            this.model.on("change", this.updateSelectedValue, this);

            XA.component.search.vent.on("hashChanged", this.updateModel.bind(this));

            this.render();
        },
        events : {
            'click .bottom-remove-filter, .clear-filter' : 'removeFacet',
            'mouseup .ui-slider-handle' : 'updateModel'
        },
        render : function () {
            var self = this,
                sig = this.model.get('sig'),
                queryModel = XA.component.search.query,
                hashObj = queryModel.parseHashParameters(window.location.hash),
                minRangeValue = parseFloat(this.model.get('dataProperties').minRangeValue),
                maxRangeValue = parseFloat(this.model.get('dataProperties').maxRangeValue),
                changeStep = parseFloat(this.model.get('dataProperties').changeStep),
                $sliderValue = $('<div />').addClass('slider-value'),
                facetClose = this.$el.find('.facet-heading > span'),
                $slider = this.$el.find('.slider'),
                selectedMinValue,
                selectedMaxValue,
                i;

            for (i = 0; i < sig.length; i++) {
                if (!jQuery.isEmptyObject(_.pick(hashObj, sig[i]))) {
                    var values = _.values(_.pick(hashObj, sig[i]))[0];
                    if (values) {
                        selectedMinValue = values.split("|")[0];
                        selectedMaxValue = values.split("|")[1];
                        //add active class to group close button
                        facetClose.addClass('has-active-facet');
                    }
                } else {
                    selectedMinValue = parseFloat(this.model.get('dataProperties').selectedMinValue);
                    selectedMaxValue = parseFloat(this.model.get('dataProperties').selectedMaxValue);
                    facetClose.removeClass('has-active-facet');
                }
            }            

            if (isNaN(minRangeValue)) {
                minRangeValue = 0;
            }
            if (isNaN(maxRangeValue)) {
                maxRangeValue = 0;
            }
            if (isNaN(changeStep) || changeStep===0) {
                changeStep = 1;
            } 
            if (isNaN(selectedMinValue)) {
                selectedMinValue = 0;
            }
            if (isNaN(selectedMaxValue)) {
                selectedMaxValue = 0;
            }            

            $slider.slider({
                range: true,
                min: minRangeValue,
                max: maxRangeValue,
                step: changeStep,
                values: [selectedMinValue, selectedMaxValue],
                create: function (event, ui) {
                    $(".slider-value").remove();
                    $slider.after($sliderValue);
                    if (selectedMinValue !== 0 || selectedMaxValue !== 0) {
                        self.updateText(selectedMinValue, selectedMaxValue);
                    }                    
                },
                slide: function (event, ui) {
                    self.updateModel(self.updateSignaturesHash(sig, ui.values[0] + "|" + ui.values[1], {}));
               }
            });
                        
        },
        removeFacet : function () {
            var facet = this.$el,
                sig = this.model.get('sig'),
                $slider = this.$el.find('.slider'),
                $facetClose = facet.find('.facet-heading > span'),
                properties = this.model.get('dataProperties'),
                hash = queryModel.parseHashParameters(window.location.hash);

            $facetClose.removeClass('has-active-facet');

            properties.selectedMinValue = properties.minRangeValue;
            properties.selectedMaxValue = properties.maxRangeValue;

            //update slider text to default min and max range values
            this.updateText(properties.minRangeValue, properties.maxRangeValue);

            //set slider valued to default min and max range values
            $slider.slider("values", [properties.minRangeValue, properties.maxRangeValue]);

            this.model.set({dataProperties : properties});

            if (hash.hasOwnProperty(this.model.get('sig'))) {
                queryModel.updateHash(this.updateSignaturesHash(sig, "", hash));
            }
        },
        updateModel: function (hash) {
            var sig = this.model.get('sig'),
                facetPart,
                dataProperties, 
                values, 
                selectedMinValue, 
                selectedMaxValue,
                i;

            if (!hash) {
                hash = queryModel.parseHashParameters(window.location.hash);
            }

            for (i = 0; i < sig.length; i++) {
                facetPart = sig[i].toLocaleString();
                if (hash.hasOwnProperty(facetPart)) {
                    values = _.values(_.pick(hash, facetPart))[0];
                    if (values !== '') {
                        selectedMinValue = values.split("|")[0];
                        selectedMaxValue = values.split("|")[1];

                        if (typeof selectedMaxValue === "undefined" ||
                            typeof selectedMinValue === "undefined") {
                            continue;
                        }

                        dataProperties = this.model.get('dataProperties');
                        dataProperties.selectedMinValue = selectedMinValue;
                        dataProperties.selectedMaxValue = selectedMaxValue;
                        this.model.set('dataProperties', dataProperties);
                        this.model.set("timeStamp", (new Date()).getTime());
                    } else {
                        this.removeFacet();
                    }
                }
            }
        },
        updateSelectedValue: function() {
            var dataProperties = this.model.get('dataProperties'),
                facetMinValue = dataProperties.selectedMinValue,
                facetMaxValue = dataProperties.selectedMaxValue,
                $slider = this.$el.find('.slider'),
                sig = this.model.get('sig');

            $slider.slider("values", [facetMinValue, facetMaxValue]);
            this.updateText(facetMinValue, facetMaxValue);
            this.$el.find('.facet-heading > span').addClass('has-active-facet');

            queryModel.updateHash(this.updateSignaturesHash(sig, facetMinValue + "|" + facetMaxValue, {}));
        },
        updateText: function (facetMinValue, facetMaxValue) {
            var $sliderValueField = this.$el.find('.slider-value'),
                sliderText = this.model.get('dataProperties').formatingString;

            sliderText = sliderText.replace('{from}', facetMinValue).replace('{to}', facetMaxValue);
            $sliderValueField.html(sliderText);
        }
    });

    api.init = function () {
        queryModel = XA.component.search.query;
        urlHelperModel = XA.component.search.url;

        var searchFacetRangeSlider = $(".facet-range-selector");
        _.each(searchFacetRangeSlider, function (elem) {
            var searchFacetRangeSliderModel = new SearchFacetRangeSliderModel(),
                searchFacetRangeSliderView = new SearchFacetRangeSliderView({el: $(elem), model : searchFacetRangeSliderModel});
        });

        initialized = true;
    };

    return api;

}(jQuery, document));

XA.register('searchFacetRangeSlider', XA.component.search.facet.rangeslider);