$(document).ready(function () {
    var mode = $("#hdnMode").val();

    if (mode == "Detail" || mode == "Update")
        recalcTotals();
});
function onSelectExpedition(data) {
    $("#hdnExpeditionID").val(data.ID);
    $("#txtExpeditionName").val(data.Name);
}
function onSelectCustomer(data) {
    var expeditionID = $("#hdnExpeditionID").val();
    var resiCode = $("#txtCode").val();
    console.log(resiCode);
    window.location.href = "/Resi/Create?customerID=" + data.ID + "&expeditionID=" + expeditionID + "&resiCode=" + resiCode;
}

function populateDeliveryOrders() {
    var customerID = $("#hdnCustomerID").val();
    var expeditionID = $("#hdnExpeditionID").val();
    var id = $("#hdnID").val();
    var mode = $("#hdnMode").val();

    if (mode == "Create") {
        var date = $("#txtDate").val();
        //console(date);
        var dateFrom = $("#txtDateFrom").val();
        var dateTo = $("#txtDateTo").val();
        var resiCode = $("#txtCode").val();

        window.location.href = "/Resi/Create?customerID=" + customerID + "&expeditionID=" + expeditionID + "&dateFrom=" + dateFrom + "&dateTo=" + dateTo + "&date=" + date + "&resiCode=" + resiCode;
    }

    if (mode == "Update") {
        var dateFrom = $("#hdnDateFrom").val();
        var dateTo = $("#hdnDateTo").val();
        window.location.href = "/Resi/Update?key=" + id + "&customerID=" + customerID + "&dateFrom=" + dateFrom + "&dateTo=" + dateTo;
    }
}

function calc(index) {
    var qty = parseFloat($("#ResiPriceDetails_" + index + "_Qty").val());
    var price = parseFloat($("#ResiPriceDetails_" + index + "_Price").val());

    if (isNaN(qty))
        qty = 0;

    if (isNaN(price))
        price = 0;

    var amount = qty * price;
    $("#ResiPriceDetails_" + index + "_TotalAmount").text(amount.toLocaleString('en-US'));
    $("#ResiPriceDetails_" + index + "_Qty").val(qty);
    $("#ResiPriceDetails_" + index + "_Price").val(price)
    recalcTotals();
}

function recalcTotals() {
    var rowCount = $('#ItemGrid tbody tr').length;

    var total = 0;

    for (x = 0; x < rowCount; x++) {
        var st = parseFloat($("#ResiPriceDetails_" + x + "_TotalAmount").text().replaceAll(',', ''));
        total += st;
    }

    var strTotal = total.toLocaleString('en-US');
    $('#txtGridTotalItem').text(strTotal);
}

function AddUrlDeliveryOrder() {
    var noOfRows = $("#Grid1 tbody tr").length;
    for (index = 0; index < noOfRows; index++) {
        var ID = $("#Details_" + index + "_DeliveryOrderID").val();
        var Code = $("#Details_" + index + "_DeliveryOrderCode").text();
        var Link = "/DeliveryOrder/Detail?key=" + ID;
        $("#Details_" + index + "_DeliveryOrderCode").replaceWith('<a href="' + Link + '">' + Code + '</a>');
    }
}

function AddUrlPayment() {
    var noOfRowsPayment = $("#billGrid tbody tr").length;

    for (indexPayment = 0; indexPayment < noOfRowsPayment; indexPayment++) {
        var resiPaymentID = $("#ResiBillDetails_" + indexPayment + "_HeaderID").val();
        var resiPaymentCode = $("#ResiBillDetails_" + indexPayment + "_ResiBillCode").text();
        var resiPaymentLink = "/ResiBill/Detail?key=" + resiPaymentID;
        $("#ResiBillDetails_" + indexPayment + "_ResiBillCode").replaceWith('<a href="' + resiPaymentLink + '">' + resiPaymentCode + '</a>');
    }
}

function AddUrlInvoice() {
    var noOfRowsInv = $("#invoiceGrid tbody tr").length;
    for (indexInv = 0; indexInv < noOfRowsInv; indexInv++) {
        var invID = $("#InvoiceResiDetails_" + indexInv + "_InvoiceID").val();
        var invCode = $("#InvoiceResiDetails_" + indexInv + "_InvoiceCode").text();
        var invLink = "/Invoice/Detail?key=" + invID;
        $("#InvoiceResiDetails_" + indexInv + "_InvoiceCode").replaceWith('<a href="' + invLink + '">' + invCode + '</a>');
    }
}


function checkCode(id) {
    var code = $("#txtCode").val();
    console.log("checkCode called for: " + code + " " + id);
    $.ajax({
        type: "POST",
        async: false,
        url: "/Webservice.asmx/CheckResiCode",
        data: JSON.stringify({ resiCode: code}),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var exists = (data.d);
            console.log("ajax called");
            if (exists) {
                alert("Warning: " + code + " already exists!");
                $("#txtCode").val('');
            }
        }
    });
}

function deleteRow(deleteButtonID, dataContainerName) {
    var button = $("#" + deleteButtonID);

    var deletedIndex = button.parent().parent().index();

    var gridID = button.parent().parent().parent().parent().attr('id');

    var noOfRows = $("#" + gridID + " tbody tr").length;

    button.parent().parent().remove();

    for (x = deletedIndex + 1; x < noOfRows; x++) {
        $("[name^='" + dataContainerName + "[" + x + "]'").each(
            function () {
                //access to form element via $(this)                
                var oriName = $(this).attr('name');

                if (oriName == null)
                    return;

                var newName = oriName.replace("[" + x + "]", "[" + (x - 1) + "]");
                $(this).attr("name", newName);
            }
        );

        $("[id*=_" + x + "]").each(
            function () {

                var oriID = $(this).attr('id');

                if (oriID == null)
                    return;

                if (oriID.indexOf(dataContainerName) < 0)
                    return;

                var endOfReplacementIndex = oriID[oriID.indexOf("_" + x) + ("_" + x).length];
                if (endOfReplacementIndex < oriID.length && oriID[endOfReplacementIndex] != '_')
                    return;

                var newID = oriID.replace("_" + x, "_" + (x - 1));
                $(this).attr("id", newID);

                if (newID.indexOf('btnDelete') == 0) {
                    var newDeleteFunction = "deleteRow('btnDelete" + dataContainerName + "_" + (x - 1) + "', '" + dataContainerName + "')";
                    $(this)[0].setAttribute('onclick', newDeleteFunction);
                }
            }
        );

    }
    recalcTotals();
}

