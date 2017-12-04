function login(componentid) {
    var logincontrol = jQuery("." + componentid).first();
    var usernameField = logincontrol.find("#loginEmail");
    var passwordField = logincontrol.find("#loginPassword");
    jQuery.ajax(
        {
            url: "/api/accounts/Login",
            method: "POST",
            data: {
                email: usernameField.val(),
                password: passwordField.val()
            },
            success: function (data) {
                if (data.RedirectUrl != null && data.RedirectUrl != undefined) {
                    window.location.assign(data.RedirectUrl);
                } else {
                    var body = logincontrol.find(".login-body");
                    var parent = body.parent();
                    body.remove();
                    parent.html(data);
                }
            }
        });
}