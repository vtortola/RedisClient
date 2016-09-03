simpleqa = window.simpleqa || {}
simpleqa.feature = simpleqa.feature || {}

simpleqa.feature.tags = function ($) {

    // sets control configuration
    var setControl = function ($tags) {
        var tagsUrl = $tags.data('tags-url');
        $tags.tagsinput({
            typeahead: {
                source: function (query) {
                    return $.post({
                        url: tagsUrl,
                        data: { query: query },
                        dataType: 'json',
                        method: 'POST'
                    });
                }
            },
            maxTags: 5,
            trimValue: true,
            maxChars: 20,
            confirmKeys: [9, 44, 32]
        });
    }

    // prevents using enter to introduce tags
    var preventEnter = function ($tags) {
        $tags
            .tagsinput('input')
            .keydown(function (event) {
                if (event.keyCode === 10 || event.keyCode === 13)
                    event.preventDefault();
            });
    }

    // sets tags control internal input to empty after adding
    var setClearAfterAdding = function ($tags) {
        $tags.on('itemAdded', function (event) {
            setTimeout(function () { $('#Tags').tagsinput('input').val(''); }, 10);
        });
    }

    return {
        run: function (selector) {

            var $tags = $(selector)
            if (!$tags.length) {
                return;
            }

            setControl($tags);
            preventEnter($tags);
            setClearAfterAdding($tags);

            $tags.css('min-width', '100%');
        }
    };
}