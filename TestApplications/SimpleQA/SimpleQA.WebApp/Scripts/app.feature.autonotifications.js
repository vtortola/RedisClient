simpleqa = window.simpleqa || {}
simpleqa.feature = simpleqa.feature || {}

simpleqa.feature.autonotifications = function ($) {

    var notifier = simpleqa.feature.notification($);
    var subscriber = simpleqa.action.subscription($);
    
    $('[data-push-topic]').each(function (i, item) {
        var $item = $(item);
        var topic = $item.data('push-topic');
        var notificationText = $item.data('push-topic-message');

        subscriber.subscribe(topic, function (message) {
            notifier.notify(notificationText);
        });
    });
};