simpleqa = window.simpleqa || {}
simpleqa.action = simpleqa.action || {}

simpleqa.action.visitQuestion = function ($) {
    return {
        execute: function (url) {
            return $.ajax({
                url: url,
                type: 'post',
                dataType: 'json'
            });
        }
    }
};
