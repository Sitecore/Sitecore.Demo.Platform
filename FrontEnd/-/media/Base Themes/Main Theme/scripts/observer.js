XA.component.observer = (function($){
	var pub = {},
		initialized = false,
		body = $("body");

	function initObserver() {
		if((typeof(Sitecore)!== "undefined") && !initialized && body.hasClass("on-page-editor")){
			Sitecore.PageModes.ChromeManager.chromesReseted.observe(function(){	
				XA.init();
			});
			initialized = true;
		}
	}

	pub.init = function(){
		initObserver();
	}

	return pub;

}(jQuery));

XA.register("observer", XA.component.observer);