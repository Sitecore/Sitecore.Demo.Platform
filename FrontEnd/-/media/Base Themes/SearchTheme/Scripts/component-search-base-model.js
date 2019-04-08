XA.component.search.baseModel = (function ($, document) {
    
    return Backbone.Model.extend({
		sortFacetArray: function(sortOrder, facetArray) {
			switch (sortOrder) {
				case 'SortByCount': {
					facetArray.sort(function(a, b) { return a.Count < b.Count });
					break;
				}
				case 'SortByNames':
				default: {
					//no need to sort by names as values are sorted that way by default
					break;
				}
			}
		}
	});

}(jQuery, document));

XA.register('searchBaseModel', XA.component.search.baseModel);
