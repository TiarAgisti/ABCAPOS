
function sendJSON(url, data, callback, overrideMethod, failCallback) {
    var method = "GET";
    var dataString = null;
    if (data != null) {
        method = "POST";
        dataString = JSON.stringify(data)
    }
    if (overrideMethod != null) {
        method = overrideMethod;
    }
    return jQuery.ajax({
        'type': method,
        'url': url,
        'contentType': 'application/json',
        'data': dataString,
        'dataType': 'json',
        'success': callback,
        'error': function () {
            if (failCallback != null) {
                failCallback();
            }
        },
        statusCode: {
            401: function () {
                //kickUser();
            }
        }
    });
};
$.urlParam = function (name) {
    var results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(window.location.href);
    if (results == null) {
        return null;
    }
    else {
        return results[1] || 0;
    }
}
