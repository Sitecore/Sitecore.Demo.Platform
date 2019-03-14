XA.component.search.query = (function ($, document) {

	var QueryModel = Backbone.Model.extend({
        defaults: {
            hash: "",
            hashObj: {}
        },
        initialize: function () {
            var inst = this,
                hash = window.location.hash;

            if (hash.length) {
                this.set({hash: hash});
                this.createHashObj();
            }
        },
        createHashObj: function () {
            this.set({hashObj: this.parseHashParameters(this.get("hash"))});
        },
        parseHashParameters: function (aUrl) {
            if (aUrl === null || aUrl === "") {
                return {};
            }

            aUrl = aUrl || window.location.hash;
            var vars = {};
            var hashes = aUrl.slice(aUrl.indexOf('#') + 1).split('&');

            hashes = hashes.filter(function(x) {
                return x !== "";
            });

            for (var i = 0; i < hashes.length; i++) {
                var hash = hashes[i].split('='),
                    facetPart = hash[0].toLowerCase();
                if (hash.length > 1)
                {
                    vars[decodeURIComponent(facetPart)] = decodeURIComponent(hash[1].replace("+", " "));
                }
                else
                {
                    vars[decodeURIComponent(facetPart)] = null;
                }
            }
            return vars;
        },
        updateHash: function(newHash, targetUrl) {
            var inst = this,
                hashStr = "#",
                hashObj = this.parseHashParameters(window.location.hash);

            _.each(newHash, function(item, key){
                if (item !== "") {
                    hashObj[key] = item;
                } else {
                    delete hashObj[key];
                }
            
            });

            if((targetUrl === "#") || (targetUrl == undefined)){
                targetUrl = window.location.pathname;
            }

            var i = 0;
            _.each(hashObj, function(item, key){
                if(i > 0){
                    hashStr += "&";
                }
                i++;
                hashStr += key + "=" + encodeURIComponent(item).replace("%7C", "|");
            });

            Backbone.history.navigate(hashStr, {trigger: true});

            if (hashStr.length) {
                this.set({hash: hashStr});
                this.createHashObj();
            }
        },
        isSignaturePresentInHash: function(hash, signature) {
            var hashKeys = Object.keys(hash),
                isSignatureFoundInHash = false;

            for (var len = hashKeys.length, i = 0; i < len; i++) {
                if (hashKeys[i].startsWith(signature)) {
                    isSignatureFoundInHash = true;
                    break;
                }
            }

            return isSignatureFoundInHash;
        }
	});

    return new QueryModel();

}(jQuery, document));