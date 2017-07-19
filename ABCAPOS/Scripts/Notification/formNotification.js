$(function () {
    var successNotification = $("#hdnSuccessNotification").val();
    var errorNotification = $("#hdnErrorNotification").val();

    if (successNotification) {
        generateNotification('success', successNotification, 'information', 'top', false,
                         function () {
                         });
    }

    if (errorNotification) {
        generateNotification('error', errorNotification, 'error', 'top', false,
                         function () {
                         });
    }
});