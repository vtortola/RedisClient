simpleqa = window.simpleqa || {}
simpleqa.scenebase = simpleqa.scene || {}

simpleqa.scenebase.authenticated = function ($) {
    simpleqa.action.subscription($).connect($('#authenticated-control').data('push-notifications-url'));
    simpleqa.feature.autonotifications($);
    simpleqa.feature.usernotification($);
};