$(document).ready(function () {
    if ($("#hdnMode").val() == "Create") {
        onChangeBooking();
    }
    else if ($("#hdnMode").val() == "Update") {
        onChangePrice();
    }
});

function getMiceBooking() {
    var bookingID = $("#MiceBookingID").val();

    if (bookingID != undefined) {
        $.ajax({
            url: "/Service.asmx/GetMiceBoooking",
            data: "{ 'bookingID': '" + bookingID + "' }",
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            dataFilter: function (data) { return data; },
            success: function (data) {
                $("#lblCustomerCode").text(data.d.CustomerCode);
                $("#lblCustomerName").text(data.d.CustomerName);
                $("#lblCustomerAddress1").text(data.d.CustomerAddress1);
                $("#lblCustomerAddress2").text(data.d.CustomerAddress2);
                $("#lblCustomerAddress3").text(data.d.CustomerAddress3);
                $("#lblCustomerCityDescription").text(data.d.CustomerCityDescription);
                $("#lblCustomerPostCode").text(data.d.CustomerPostCode);
                $("#lblCustomerEmail").text(data.d.CustomerEmail);
                $("#lblCustomerPhone").text(data.d.CustomerPhone);
                $("#lblCustomerFax").text(data.d.CustomerFax);
                $("#lblCustomerContact").text(data.d.CustomerContact);
                $("#lblCustomerContact2").text(data.d.CustomerContact2);

                var date = new Date(parseInt(data.d.EventStartDate.substr(6)));
                $("#lblEventName").text(data.d.EventName);
                $("#lblPackageName").text(data.d.PackageName);
                $("#lblEventStartDate").text(date.getDate() + '/' + (date.getMonth() + 1) + "/" + date.getFullYear());

                $("#lblPackageRemarks").text(data.d.PackageRemarks);

                //                if (data.d.EventType == 1) {
                //                    $("#rbEventType_0").attr('checked', true);
                //                }
                //                else {
                //                    $("#rbEventType_1").attr('checked', true);
                //                }
            }
        });
    }
}

function getMiceBookingDate() {
    var bookingID = $("#MiceBookingID").val();
    $('#MiceBookingItemNo option').remove();

    if (bookingID != undefined) {
        $.ajax({
            url: "/Service.asmx/GetMiceBoookingDate",
            data: "{ 'bookingID': '" + bookingID + "' }",
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            dataFilter: function (data) { return data; },
            success: function (data) {
                $.map(data.d, function (item) {
                    var eventDate = new Date(parseInt(item.StartDate.substr(6)));
                    val = item.ItemNo;
                    text = pad2(eventDate.getDate()) + '/' + pad2((eventDate.getMonth() + 1)) + "/" + eventDate.getFullYear();
                    var option = "<option value='" + val + "'>" + text + "</option>";
                    $('#MiceBookingItemNo').append(option);
                })
            }
        });
    }
}

function pad2(number) {
    return (number < 10 ? '0' : '') + number
}

function onChangeBooking() {
    getMiceBooking();
    getMiceBookingDate();
}

function onChangePrice() {
    if ($("#hdnMode").val() == "Create") {
        var bookingID = $("#MiceBookingID").val();
    }
    else {
        var bookingID = $("#lblMiceBookingID").text();
    }

    var additionalDecoration = $("#txtAdditionalDecorationPackage").val();
    var additionalEntertainment = $("#txtAdditionalEntertainmentPackage").val();

    var useDecorationAdd = $("#chkUseDecoration").is(':checked'); //use decoration (additional)
    var useEntertainmentAdd = $("#chkUseEntertainment").is(':checked'); //use entertainment (additional)

    if (bookingID != undefined) {
        $.ajax({
            url: "/Service.asmx/GetMiceBoooking",
            data: "{ 'bookingID': '" + bookingID + "' }",
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            dataFilter: function (data) { return data; },
            success: function (data) {
                if (useDecorationAdd == false) {
                    $("#txtAdditionalDecoration").val(0);
                    $("#spServiceChargeDecoration").hide();
                }
                else {
                    $("#spServiceChargeDecoration").show();
                }

                if (useEntertainmentAdd == false) {
                    $("#txtAdditionalEntertainment").val(0);
                    $("#spServiceChargeEntertainment").hide();
                }
                else {
                    $("#spServiceChargeEntertainment").show();
                }
            }
        });
    }
}

function getSelectedSupplier() {
    var supplierCode = $("#hdnFoodSupplierCode").val();

    if (supplierCode != undefined)
        return supplierCode;
    else
        return "";
}

function onSelectFoodSupplier(data) {
    $("#lblFoodSupplierName").text(data.Name);
}

function onSelectDecorationSupplier(data) {
    $("#lblDecorationSupplierName").text(data.Name);
}

function onSelectEntertainmentSupplier(data) {
    $("#lblEntertainmentSupplierName").text(data.Name);
}

function onChangeSCAdditionalPax() {
    var useSCAddPax = $("#chkUseServiceChargeAdditionalPax").is(':checked');

    if (useSCAddPax == true) {
        $("#hdnUseServiceChargeAdditionalPax").val(true);
    }
    else {
        $("#hdnUseServiceChargeAdditionalPax").val(false);
    }
}

function onChangeSCAdditionalSittingBuffet() {
    
    var useSCAddSittingBuffet = $("#chkUseServiceChargeAdditionalSittingBuffetQty").is(':checked');

    if (useSCAddSittingBuffet == true) {
        $("#hdnUseServiceChargeAdditionalSittingBuffetQty").val(true);
    }
    else {
        $("#hdnUseServiceChargeAdditionalSittingBuffetQty").val(false);
    }
}

function onChangeSCDecoration() {
    var useSCDecoration = $("#chkUseServiceChargeDecoration").is(':checked');

    if (useSCDecoration == true) {
        $("#hdnUseServiceChargeDecoration").val(true);
    }
    else {
        $("#hdnUseServiceChargeDecoration").val(false);
    }
}

function onChangeSCEntertainment() {
    var useSCEntertainment = $("#chkUseServiceChargeEntertainment").is(':checked');

    if (useSCEntertainment == true) {
        $("#hdnUseServiceChargeEntertainment").val(true);
    }
    else {
        $("#hdnUseServiceChargeEntertainment").val(false);
    }
}

function onChangeQty() {
    var addQty = $("#txtAdditionalPaxQty").val();
    var additionalQty = parseInt(addQty);

    var addSittingBuffetQty = $("#txtAdditionalSittingBuffetQty").val();
    var additionalSittingBuffetQty = parseInt(addSittingBuffetQty);

    var PaxQty = $("#hdnPaxQty").val();
    var pQty = parseInt(PaxQty);

    var sittingBuffetQty = $("#hdnSittingBuffetQty").val();
    var sQty = parseInt(sittingBuffetQty);

    var totalQty = pQty + additionalQty + sQty + additionalSittingBuffetQty;
    $("#lblTotalPaxQty").text(totalQty);

    if (additionalQty <= 0) {
        $("#chkUseServiceChargeAdditionalPax").hide();
    }
    else {
        $("#chkUseServiceChargeAdditionalPax").show();
    }

    if (additionalSittingBuffetQty <= 0) {
        $("#chkUseServiceChargeAdditionalSittingBuffetQty").hide();
    }
    else {
        $("#chkUseServiceChargeAdditionalSittingBuffetQty").show();
    }
}

function populateMiceBooking() {
    var bookingID = $("#MiceBookingID").val();

    var url = "Create?bookingID=" + bookingID;
    window.location.href = url;
}