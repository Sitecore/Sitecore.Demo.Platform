(function ($) {
	$.fn.equalHeights = function () {
		var maxHeight = 0,
			$this = $(this);

		$this.css('height', 'auto');
		$this.each(function () {
			var height = $(this).innerHeight();
			if (height > maxHeight) {
				maxHeight = height;
			}
		});

		$this.height(maxHeight);
	};

	var equalizePromos = function(){
		$(".row .component .column-splitter .promo .field-title").equalHeights();
		$(".row .component .column-splitter .promo .field-promotext").equalHeights();
		$(".row .component .column-splitter .promo .field-promotext2").equalHeights();
		$(".row .component .column-splitter .promo .field-promotext3").equalHeights();
		$(".row .component .column-splitter .promo .field-introduction").equalHeights();
	}

	$(function () { equalizePromos(); });
	$(window).resize(function () { equalizePromos(); });
})(window.jQuery);
