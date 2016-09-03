simpleqa = window.simpleqa || {}
simpleqa.feature = simpleqa.feature || {}

simpleqa.feature.usernotification = function ($) {
    var $dropdown = $('#authenticated-notifications');
    var $incoming = $('#authenticated-notifications-incoming');
    var $empty = $('#authenticated-notifications-icon');
    var $itemlist = $('#authenticated-notifications-items');
    var $button = $('#authenticated-notifications-button');

    var user = $button.data('user-inbox');
    var url = $button.data('user-inbox-url');
    var pending = true;
    var pendingCount = $incoming.text() ? Number($incoming.text()) : 0;

    $dropdown.on('hidden.bs.dropdown', function (e) {
        $incoming.text('0');
        $empty.removeClass('nodisplay');
        $incoming.addClass('nodisplay');
        pendingCount = 0;
        pending = false;
    })

    var replacer = simpleqa.action.replaceHtml($);
    $dropdown.on('shown.bs.dropdown', function (e) {
        if (pending)
            replacer.execute($itemlist, url);
    })

    var subscriber = simpleqa.action.subscription($);
    subscriber.subscribe(user, function (message) {
        
        pendingCount += 1;
        $incoming.text(pendingCount);
        $incoming.removeClass('nodisplay');
        $empty.addClass('nodisplay');

        pending = true;
    });
};