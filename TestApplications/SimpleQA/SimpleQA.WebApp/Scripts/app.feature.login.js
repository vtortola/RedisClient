simpleqa = window.simpleqa || {}
simpleqa.feature = simpleqa.feature || {}

simpleqa.feature.login = function ($) {

    var regex = /[a-z0-9]/i;

    var filterContent = function ($control, length) {
        var current = $control.val();
        var final = '';
        var illegal = false;
        for (var i = 0; i < current.length; i++) {
            if (i == length) {
                illegal = true;
                break;
            }
            else if (current[i].match(regex))
                final += current[i];
            else
                illegal = true;
        }
        if (illegal) {
            $control.val(final);
        }
        return illegal;
    }

    var limitValue = function ($control, length) {
        $control.keyup(function () { filterContent($control, length); });
    };

    return {
        run: function (selector) {
            var $loginForm = $(selector);
            if (!$loginForm.length)
                return;

            var $username = $('#username');
            var $password = $('#password');

            limitValue($username, 20);
            limitValue($password, 20);

            $loginForm.submit(function (e) {
                if (!$username.val() || !$password.val()) {
                    e.preventDefault();
                    return false;
                }
                return true;
            })

        }
    }
};

