simpleqa = window.simpleqa || {}
simpleqa.scenebase = simpleqa.scene || {}

simpleqa.scenebase.anonymous = function ($) {
    simpleqa.feature.login($).run('#login-form');
};