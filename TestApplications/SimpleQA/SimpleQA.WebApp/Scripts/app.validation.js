simpleqa = window.simpleqa || {}

simpleqa.validation = function ($) {
    return {
        enableNonVisibleValidation: function (selector) {
            $(selector).each(function () {
                var $form = $(this);
                var valdata = validatorSettings = $.data($form[0], 'validator');
                if (!valdata)
                    $.validator.unobtrusive.parse($form);
                valdata = validatorSettings = $.data($form[0], 'validator');

                if (valdata)
                    valdata.settings.ignore = '';
            });
        }
    };
}