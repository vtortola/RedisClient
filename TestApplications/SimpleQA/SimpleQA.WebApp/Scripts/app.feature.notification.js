simpleqa = window.simpleqa || {}
simpleqa.feature = simpleqa.feature || {}

simpleqa.feature.notification = function ($) {
    var $dialog = $('#sa-notification');
    var $message = $dialog.find('.sa-notification-message');
    var $close = $dialog.find('button.close');
    var opened = false;

    $close.click(function (e) {
        $dialog.removeClass('sa-notification-visible');
        opened = false;
    });

    return {
        notify: function (message) {

            if (opened)
                return;

            $message.text(message);

            $dialog.addClass('sa-notification-visible');
            opened = true;
        }
    };
};