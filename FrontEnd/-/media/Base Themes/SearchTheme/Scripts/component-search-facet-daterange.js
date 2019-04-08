XA.component.search.facet.daterange = (function ($, document) {

    var api = {}, queryModel, urlHelperModel, initialized = false, toUrlDate, fromUrlDate;

    toUrlDate = function(date) {
        return date !== null && date !== "" ? $.datepicker.formatDate('yymmdd', date) : "";
    };

    fromUrlDate = function(dateString) {
        var y = dateString.substr(0,4),
            m = dateString.substr(4,2) - 1,
            d = dateString.substr(6,2),
            D = new Date(y,m,d);

        if (dateString === "") {
            return;
        }

        if (D.getFullYear() == y && D.getMonth() == m && D.getDate() == d)
            return D;
        else 
            throw "Invalid date: " + dateString;
    };

    var FacetDateRangeModel = Backbone.Model.extend({
        defaults: {                   
            dataProperties: {},
            sig: []
        }
    });

    var FacetDateRangeView = XA.component.search.baseView.extend({
        initialize : function () {
            this.properties = this.$el.data().properties;
            if(this.model){
                this.model.set({dataProperties: this.properties});
                this.model.set("sig", this.translateSignatures(this.properties.searchResultsSignature, this.properties.f));
            }
            this.model.on("change", this.render, this);

            XA.component.search.vent.on("hashChanged", this.updateComponent.bind(this));
        },
        events : {
            'change .startDate' : 'updateFacet',
            'change .endDate' : 'updateFacet',
            'click .bottom-remove-filter, .clear-filter' : 'clearFilter'
        },        
        render: function(){
            var fromDateDefaultOffset = parseInt(this.model.get('dataProperties').fromDateDefaultOffset),
                toDateDefaultOffset = parseInt(this.model.get('dataProperties').toDateDefaultOffset),
                fromDateFormat = this.model.get('dataProperties').fromDateDisplayFormat,
                toDateFormat = this.model.get('dataProperties').toDateDisplayFormat,
                fromDateMonthsShown = this.model.get('dataProperties').fromDateMonthsShown,
                toDateMonthsShown = this.model.get('dataProperties').toDateMonthsShown,
                fromDatePastDays = this.model.get('dataProperties').fromDatePastDays,
                toDateFutureDays = this.model.get('dataProperties').toDateFutureDays,
                fromDateVisible = this.model.get('dataProperties').fromDateVisible,
                toDateVisible = this.model.get('dataProperties').toDateVisible,                
                $fromDate = this.$el.find('.startDate'),
                $toDate = this.$el.find('.endDate'),
                hashObj = queryModel.parseHashParameters(window.location.hash),
                sig = this.model.get('sig'),
                dates, i;

            if (toDateFormat) {
                toDateFormat = toDateFormat.replace(/yy/g, "y");
            }
            if (fromDateFormat) {
                fromDateFormat = fromDateFormat.replace(/yy/g, "y");
            }

            if (fromDateVisible) {
                $fromDate.datepicker({
                    dateFormat: fromDateFormat,
                    changeMonth: fromDateMonthsShown,
                    changeYear: fromDateMonthsShown,
                    minDate: fromDatePastDays ? (fromDateDefaultOffset != '' ? -1 * fromDateDefaultOffset : new Date(1900, 1 , 1)) : new Date()
                });
            }
        
            if (toDateVisible) {
                $toDate.datepicker({
                    dateFormat: toDateFormat,
                    changeMonth: toDateMonthsShown,
                    changeYear: toDateMonthsShown,
                    maxDate: toDateFutureDays ? (toDateDefaultOffset != '' ? toDateDefaultOffset : new Date(2100, 1 , 1)) : new Date()
                });
            }

            for (i = 0; i < sig.length; i++) {
                if (hashObj.hasOwnProperty(sig[i]) && hashObj[sig[i]] != '') {
                    dates = hashObj[sig[i]].split("|");
                    $fromDate.datepicker("setDate", fromUrlDate(dates[0]));
                    $toDate.datepicker("setDate", fromUrlDate(dates[1]));
                }
            }
        },
        updateFacet : function(param) {
            var $facetClose = this.$el.find('.facet-heading > span'),
                $fromDate = this.$el.find('.startDate'),
                $toDate = this.$el.find('.endDate'),
                fromDate = $fromDate.length > 0 ? $fromDate.datepicker("getDate") : null,
                toDate = $toDate.length > 0 ? $toDate.datepicker("getDate") : null,
                sig = this.model.get('sig');

            queryModel.updateHash(this.updateSignaturesHash(sig, toUrlDate(fromDate) + "|" + toUrlDate(toDate), {}));
            $facetClose.addClass('has-active-facet');
        },
        clearFilter: function(param) {
            var properties = this.model.get('dataProperties'),
                $facetClose = this.$el.find('.facet-heading > span'),
                hash = queryModel.parseHashParameters(window.location.hash),
                sig = this.model.get('sig'),
                i;

            $facetClose.removeClass('has-active-facet');

            for (i = 0; i < sig.length; i++) {
                delete properties[sig[i]];
            }
            queryModel.updateHash(this.updateSignaturesHash(sig, "", hash));

            this.model.set({dataProperties : properties});

            this.$el.find('.startDate').val("");
            this.$el.find('.endDate').val("");
        },
        updateComponent: function (hash) {
            var $fromDate = this.$el.find('.startDate'),
                $toDate = this.$el.find('.endDate'),
                sig = this.model.get('sig'),
                facetPart,
                dates,
                i;

            for (i = 0; i < sig.length; i++) {
                facetPart = sig[i].toLowerCase();
                if (hash.hasOwnProperty(facetPart) && hash[facetPart] !== '') {
                    dates = hash[facetPart].split("|");
                    if(dates[0] !== ""){
                        this.handleDate($fromDate, dates[0]);    
                    }
                    if(dates[1] !== ""){
                        this.handleDate($toDate, dates[1]);    
                    }
                } else {
                    this.clearFilter();
                }
            }
        },
        handleDate: function (control, value) {
            var $facetClose = this.$el.find('.facet-heading > span');
            if (control.length !== 0 && toUrlDate(control.datepicker('getDate')) !== value) {
                control.datepicker("setDate", fromUrlDate(value));
                $facetClose.addClass('has-active-facet');
            } else if (value === "") {
                control.datepicker('setDate', null);
            }
        }
    });    


    api.init = function() {
        if($("body").hasClass("on-page-editor") || initialized){
            return;
        }

        queryModel = XA.component.search.query;
        urlHelperModel = XA.component.search.url;

        var facetDateRangesList = $(".facet-date-range");
        _.each(facetDateRangesList, function(elem){
            var model = new FacetDateRangeModel(),
                view = new FacetDateRangeView({el: $(elem), model: model});
            view.render();
        });

        initialzied = true;
    };

    return api;

}(jQuery, document));

XA.register('facetDateRange', XA.component.search.facet.daterange);