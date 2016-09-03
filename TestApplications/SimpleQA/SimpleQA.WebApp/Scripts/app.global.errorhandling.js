simpleqa = window.simpleqa || {}
simpleqa.global = simpleqa.global || {}

simpleqa.global.errorhandling = function ($) {
        var $genericErrorModal = $('#generic-error');
        var lastError = 'Unexpected error';

        $genericErrorModal.on('show.bs.modal', function (event) {
            $genericErrorModal.find('.modal-body').text(lastError)
        });

        $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
            lastError = thrownError;
            $genericErrorModal.modal('show');
        });
}