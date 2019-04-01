XA.component.search.radiusFilter = function ($, document) {
    var api = {},
        initializeComponent,
        RadiusFilterModel,
        RadiusFilterView,
        queryModel,
        urlHelperModel;


    RadiusFilterModel = Backbone.Model.extend({
        defaults : {
            properties : [],
            selected : {},
            sig : []
        }
    });

    RadiusFilterView = XA.component.search.baseView.extend({
        initialize : function() {
            var radiusElements = _.map(this.$el.find("li[data-value][data-title]"), function(r){return $(r);}),
                properties = this.$el.data("properties");
                
            _.each(radiusElements, function(r) { r.addClass("radius-button");});

            this.model.set({ properties: properties, selected : {}});
            this.model.bind("change", this.render,this);
            this.model.set("sig", this.translateSignatures(properties.searchResultsSignature, properties.f));

            XA.component.search.vent.on("hashChanged", this.hashChanged.bind(this));
            this.render();
        },
        events : {
            "click li": "radiusClick",
            'click .bottom-remove-filter, .clear-filter': 'deselect'
        },
        
        render : function() {
            var _this = this, selected;
            
            selected = this.model.get("selected");
            _this.$el.find(".selected").removeClass("selected");
            
            if(typeof(selected) !== "undefined" && selected.length){
                selected.addClass("selected");    
            }
            else {
                selected = _this.$el.find("[data-value='-1']");
                if(selected.length && selected.length >= 1){
                    $(selected[0]).addClass("selected");
                }
            }
        },        
        updateHash : function(newValue){
            var sig = this.model.get("sig");
            newValue = newValue == -1 ? "" : newValue;
            queryModel.updateHash(this.updateSignaturesHash(sig, newValue, {}));
        },

        radiusClick : function(args) {
            var _this = this,
                selected = $(args.currentTarget);
                
            _this.updateHash(selected.data("value"));  
            _this.model.set({selected : selected});
        },
        
        deselect : function(args) {
            var _this = this;
            
            _this.updateHash("");
            _this.model.set({selected : undefined});            
        },
        
        hashChanged : function(hash){
            var _this = this,
                sig = this.model.get("sig"),
                facetPart,
                value,
                selected,
                i;

            for (i = 0; i < sig.length; i++) {
                facetPart = sig[i].toLowerCase();
                if (hash.hasOwnProperty(facetPart)) {
                    value = hash[facetPart];
                    value = value === "" ? -1 : value;
                    selected = _this.$el.find("[data-value='"+value+"']");
                    if(selected.length && selected.length >= 1){
                        _this.model.set({selected : $(selected[0])});
                    }
                    else {
                        _this.model.set({selected : undefined});
                    }
                }
            }
        }

    });

    initializeComponent = function(component) {
        var model = new RadiusFilterModel();
        var view = new RadiusFilterView({ el: component[0], model: model });
        component.addClass("initialized");
    }

    api.init = function() {
        if ($("body").hasClass("on-page-editor")) {
            return;
        }

        var components = $(".radius-filter:not(.initialized)");
        _.each(components, function(component){
            initializeComponent($(component));
        });
        
        queryModel = XA.component.search.query;
        urlHelperModel = XA.component.search.url;
    };
    
    return api;
}(jQuery, document);
XA.register("radiusFilter", XA.component.search.radiusFilter);