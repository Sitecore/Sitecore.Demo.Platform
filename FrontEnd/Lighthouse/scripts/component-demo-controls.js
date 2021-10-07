(function($) {
	var refreshPanel = function (reloadPage, checked) {
		var panel = $("#experiencedata");
		$.ajax({
			url: "/api/Demo/TogglePersonalization",
			method: "get",
			data: {
				TogglePersonalization: checked,
			},
			cache: false,
			success: function (data) {
				panel.replaceWith(data);
				$("#sidebar").find('.panel-collapse').hide();
				if (reloadPage) {
					window.location.href = window.location.href;
				}
			}
		});
	};

	// change event may modified to other events
	$("#TogglePersonalization").change(function () {

		// check if checkbox is being checked
		if ($("#TogglePersonalization").is(":checked")) {
			refreshPanel(true, true);
		}
		else {
			refreshPanel(true, false);
		}
	});
})(jQuery);