simpleqa = window.simpleqa || {}
simpleqa.action = simpleqa.action || {}

simpleqa.action.subscription = function ($) {
    
    window.pushConnectionSubscriptions = window.pushConnectionSubscriptions || {};
    window.pushConnectionCache = window.pushConnectionCache || [];

    var subscribe = function (topic, callback) {
        var subscription = window.pushConnectionSubscriptions[topic];
        if (!subscription) {
            subscription = window.pushConnectionSubscriptions[topic] = [];
            subscription.push(callback);
        }
    }

    var publish = function (topic, message) {
        var subscription = window.pushConnectionSubscriptions[topic];
        if (!subscription)
            return;

        for (var i = 0; i < subscription.length; i++) {
            var callback = subscription[i];
            if (!callback)
                continue;
            callback(message);
        }
    }

    var send = function (message) {
        if (window.pushConnectionCache)
            window.pushConnectionCache.push(message);
        else
            window.pushConnection.send(JSON.stringify(message));
    }

    return {
        connect : function(url){
            if (!window.pushConnection && url) {
                window.pushConnection = new WebSocket(url);

                window.pushConnection.onmessage = function (message) {
                    var obj = JSON.parse(message.data);
                    publish(obj.Topic, obj);
                };

                window.pushConnection.onopen = function () {
                    var local = window.pushConnectionCache;
                    window.pushConnectionCache = null;
                    for (var i = 0; i < local.length; i++) {
                        send(local[i]);
                    }
                }
            }
        },
        subscribe: function (topic, callback) {
            subscribe(topic, callback);
            send({
                topic: topic,
                action: 'subscribe'
            });
        }
    }
};