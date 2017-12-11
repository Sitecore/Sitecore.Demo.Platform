function login(componentid) {
    var logincontrol = jQuery("." + componentid).first();
    var usernameField = logincontrol.find("#loginEmail");
    var passwordField = logincontrol.find("#loginPassword");
    var returnUrlField = logincontrol.find("#ReturnUrl");
    jQuery.ajax(
        {
            url: "/api/accounts/_Login",
            method: "POST",
            data: {
                email: usernameField.val(),
                password: passwordField.val(),
                returnUrl: returnUrlField.val()
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
};
function register(componentid) {
    var logincontrol = jQuery("." + componentid).first();
    var usernameField = logincontrol.find("#registerEmail");
    var passwordField = logincontrol.find("#registerPassword");
    var confirmPasswordField = logincontrol.find("#registerConfirmPassword");
    var returnUrlField = logincontrol.find("#ReturnUrl");
    jQuery.ajax(
        {
            url: "/api/accounts/Register",
            method: "POST",
            data: {
                email: usernameField.val(),
                password: passwordField.val(),
                confirmPassword: confirmPasswordField.val(),
                returnUrl: returnUrlField.val()
            },
            success: function (data) {
                if (returnUrlField.val() != null && returnUrlField.val() != undefined) {
                    window.location.href = returnUrlField.val();
                } else {
                    var body = logincontrol.find("componentid");
                    var parent = body.parent();
                    body.remove();
                    parent.html(data);
                }
            }
        });
}