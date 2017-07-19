var notificationObj;
var needRedir = true;

$(document).ready(function () {
    $("#hdnNotificationShown").val(false);

    getNotificationObj(); //set notif buffer

    generateNotificationHead();
});

function generateNotification(id, text, type, position, timeout, afterClose) {
    var n = noty({
        id: 'n_' + id,
        text: text,
        type: type,
        dismissQueue: true,
        layout: position,
        timeout: timeout,
        theme: 'defaultTheme',
        callback: {
            afterClose: afterClose
        }
    });
}

function generateNotificationHead() {
    generateNotification('Head', 'Show/Hide Notification', 'alert', 'topRight', false,
                          function () {
                              var isNotificationShown = $("#hdnNotificationShown").val();
                              if (isNotificationShown == 'false') {
                                  needRedir = true;
                                  showNotification();
                              }
                              else {
                                  needRedir = false;
                                  hideNotification();
                              }
                          });
}

function showNotification() {
    $("#hdnNotificationShown").val(true);
    $.noty.closeAll();

    generateNotificationHead();
    generateAll();
}

function hideNotification() {
    $("#hdnNotificationShown").val(false);
    $.noty.closeAll();

    generateNotificationHead();
}

function generateAll() {
    getNotificationObj();

    $.each(notificationObj, function (idx, obj) {
        if (obj.Count > 0)
            generateNotification(obj.ID, obj.Text, 'information', 'topRight', false,
                                 function () {
                                     notificationCallbackHandler(obj.Url);
                                 });
    });
}

function notificationCallbackHandler(url) {
    if (needRedir == true)
        window.open(url, "_self");
}

function getNotificationObj() {
    $.ajax({
        url: "/WebService.asmx/GetAllNotification",
        data: "{'isPurchaseOrderAllowed':'" + $("#isPurchaseOrderAllowed").val() + "', 'isSalesOrderAllowed':'" + $("#isSalesOrderAllowed").val() + "','isDeliveryOrderAllowed':'" + $("#isDeliveryOrderAllowed").val() + "','isInvoiceAllowed':'" + $("#isInvoiceAllowed").val() + "','isPaymentAllowed':'" + $("#isPaymentAllowed").val() + "'}",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataFilter: function (data) { return data; },
        success: function (data) {
            if (data.d != "") {
                var obj = JSON.parse(data.d);

                notificationObj = obj;
            }
            else
                console.log("get no obj!");
        }
    });
}