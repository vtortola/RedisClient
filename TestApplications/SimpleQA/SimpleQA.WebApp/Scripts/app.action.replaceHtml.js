simpleqa = window.simpleqa || {}
simpleqa.action = simpleqa.action || {}

simpleqa.action.replaceHtml = function ($) {
    return {
        execute: function ($container, url) {
            return $.ajax({
                url: url,
                type: 'post',
                dataType: 'html'
            }).done(function(response){
                $container.html(response);
            });
        }
    }
};
