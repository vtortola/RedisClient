simpleqa = window.simpleqa || {}
simpleqa.scene = simpleqa.scene || {}

simpleqa.scene.question = function ($) {

    simpleqa.scenebase.authenticated($);

    var validation = simpleqa.validation($);
    var markdown = simpleqa.feature.markdown($);

    // performs an ajax upvote/downvote in question and answers
    var vote = function ($vote) {
        var up = $vote.is('.vote-up');
        var $element = $vote.closest('#question-header, .answer');
        var url = $element.data('vote-url');
        var voteClass = up ? "upvoted" : "downvoted";

        var data = {
            upvote: up
        };
        data[$('html').data('xsrf-key')] = $('html').data('xsrf-value');

        $.ajax({
            url: url,
            method: 'post',
            data: data,
            dataType: 'json',

            // for using JSON: Still have to work out the XSRF token validation
            //data: JSON.stringify(data),
            //contentType: "application/json; charset=utf-8"
        })
        .done(function (response) {
            var $votes = $element.find('.votes');
            $votes.text(response.Votes)
            $element.find('.vote').addClass(voteClass);
        });
    }

    var postAnswerAndReplace = function ($form, $answer) {
        $answer.prepend("<div class='glyphicon glyphicon-refresh loading answer-loading'></>");
        $.ajax({
            url: $form.attr('action'),
            type: 'post',
            data: $form.serialize()
        })
        .done(function (response) {
            $answer.replaceWith(response);
        });
    }

    // load answer edit controls and replace readonly view
    var makeAnswerEditable = function ($editButton) {
        var url = $editButton.data('edit-answer');

        var $answer = $editButton.closest('.answer');
        var $answerContent = $answer.find('.answer-content');
        $answerContent.hide();
        $answerContent.after("<div class='glyphicon glyphicon-refresh loading answer-loading'></>");

        $.ajax({
            url: url,
            type: 'get'
        })
        .done(function (response) {

            $answer.find(".answer-loading").remove();
            $answerContent.after(response);

            var $form = $answer.find('form');

            validation.enableNonVisibleValidation($form);
            markdown.run('.markdown-editable');

            $answer.on('click', function (e) {
                    var $target = $(e.target);
                    if ($target.is('button.cancel-editing')) {
                        e.preventDefault = true;
                        e.stopPropagation = true;

                        $answerContent.show();
                        $answer
                            .find('.answer-editing')
                            .remove();
                    }
                    else if ($target.is('button.post-editing')) {
                        e.preventDefault = true;
                        e.stopPropagation = true;

                        postAnswerAndReplace($form, $answer);
                    }
                });
        });
    }

    var $question = $('#question');
    if (!$question.length)
        return;

    validation.enableNonVisibleValidation($('form'));
    markdown.run('.markdown-editable');


    $question.on('click', function (e) {
        var $target = $(e.target);
        if ($target.is('button.vote-up, button.vote-down')) {
            vote($target);
        }
        else if ($target.is('button.edit-answer')) {
            makeAnswerEditable($target);
        }
    });

    simpleqa.action.visitQuestion($).execute($question.data('visit'));
}