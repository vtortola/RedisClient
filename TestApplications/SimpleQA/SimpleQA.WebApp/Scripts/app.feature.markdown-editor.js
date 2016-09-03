simpleqa = window.simpleqa || {}
simpleqa.feature = simpleqa.feature || {}

simpleqa.feature.markdown = function ($) {
	var setMarkdownControl = function ($editable) {
		var simplemde = new SimpleMDE({
			element: $editable[0],
			forceSync: true,
			spellChecker: false,
			status: false,
			hideIcons: ["preview", "side-by-side", "fullscreen", "guide"],
			lineWrapping: false,
			parsingConfig: {
				allowAtxHeaderWithoutSpace: true
			},
			renderingConfig: {
				singleLineBreaks: false
			}
		});

		var $preview = $editable.closest('form').find('div.preview-content');
		if ($preview.length) {
			var converter = new showdown.Converter();
			$preview.html(converter.makeHtml($editable.val()));
			simplemde.codemirror.on("change", function () {
				$preview.html(converter.makeHtml($editable.val()));
			});
		}
	}

	return {
		run : function (selector) {
			$(selector).each(function (index) {
				var $this = $(this);

				if (!$this.is(':visible'))
					return;

				setMarkdownControl($this);
			});
		}
	};
}