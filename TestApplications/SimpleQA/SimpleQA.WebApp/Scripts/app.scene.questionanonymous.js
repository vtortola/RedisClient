simpleqa = window.simpleqa || {}
simpleqa.scene = simpleqa.scene || {}

simpleqa.scene.questionanonymous = function ($) {

    simpleqa.scenebase.anonymous($);

    var $question = $('#question');

    if (!$question.length)
        return;

    simpleqa.action.visitQuestion($).execute($question.data('visit'));
};