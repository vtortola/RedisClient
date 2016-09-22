simpleqa = window.simpleqa || {}
simpleqa.global = simpleqa.global || {}

simpleqa.global.errorhandling = function ($) {
        var $genericErrorModal = $('#generic-error');
        var lastError = 'Unexpected error';
        var visible = false;

        $genericErrorModal.on('show.bs.modal', function (event) {
            visible = true;
            $genericErrorModal.find('.modal-body').text(lastError)
        });

        $genericErrorModal.on('hide.bs.modal', function (event) {
            visible = false;
        });

        // removing modal caches
        $('body').on('hidden.bs.modal', '.modal', function () {
            $(this).removeData('bs.modal');
        });

        $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
            lastError = thrownError;
            if (visible) {
                $genericErrorModal.find('.modal-body').text(lastError);
            }
            else {
                $('.modal').modal('hide'); // close opened modals
                $genericErrorModal.modal('show');
            }
        });
}