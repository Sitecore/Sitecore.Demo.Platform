XA.component.search.baseView = (function ($, document) {
    
    return Backbone.View.extend({
		initialize: function () {
	    },
		translateSignatures: function (rawSignature, f) {
            var signatures, i;

		    f = f.toLowerCase();

			if (typeof rawSignature === "undefined" || rawSignature === null) {
				return [f];
			}

	        signatures = rawSignature.split(',');

	        if (rawSignature === "") {
	            return [f];
	        } else {
	            for (i = 0; i < signatures.length; i++) {
	                signatures[i] = signatures[i] + "_" + f;
	            }
	            return signatures;
	        }
	    },
	    updateSignaturesHash: function(sig, value, hash) {
	    	for (var i = 0; i < sig.length; i++) {
                hash[sig[i]] = value;
            }
            return hash;
	    }
	});

}(jQuery, document));

XA.register('searchBaseView', XA.component.search.baseView);
