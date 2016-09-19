simpleqa = window.simpleqa || {}
simpleqa.global = simpleqa.global || {}

simpleqa.global.errorhandling = function ($) {
        var $genericErrorModal = $('#generic-error');
        var lastError = 'Unexpected error';

        $genericErrorModal.on('show.bs.modal', function (event) {
            $genericErrorModal.find('.modal-body').text(lastError)
        });

        // removing modal caches
        $('body').on('hidden.bs.modal', '.modal', function () {
            $(this).removeData('bs.modal');
        });

        $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
            lastError = thrownError;
            $('.modal').modal('hide'); // close opened modals
            $genericErrorModal.modal('show');
        });
}