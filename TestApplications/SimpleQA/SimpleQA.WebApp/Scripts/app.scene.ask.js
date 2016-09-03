simpleqa = window.simpleqa || {}
simpleqa.scene = simpleqa.scene || {}

simpleqa.scene.ask = function ($) {
    simpleqa.scenebase.authenticated($);
    simpleqa.validation($).enableNonVisibleValidation('form');
    simpleqa.feature.tags($).run('#Tags');
    simpleqa.feature.markdown($).run('.markdown-editable');
};