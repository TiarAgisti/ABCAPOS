$(function () {
    $("[name='lnkAddNewGrid1']").hide();

    $("#btnVoid").live("click", function (e) {
        e.preventDefault();
        $(" <div></div>")
            .addClass("dialog")
            .attr("id", $(this).attr("data-dialog-id"))
            .appendTo("body").dialog({
                title: "Void Remarks",
                close: function () {
                    $(this).remove();
                },
                modal: true,

                height: 180,
                width: 700,
                left: 0
            }).load(this.href);
    });

    $("#btnCloseVoidForm").live("click", function (e) {
        e.preventDefault();
        $(this).closest(".dialog").dialog("close");
    });

    addTooltip();
    setGridButtonVisivility();
    //setDeleteRowBugFix();

    //$('#txtExchangeRate').prop('readonly', true);
});

function setDeleteRowBugFix() {
    var noOfRows = $("#Grid1 tbody tr").length;

    for (x = 0; x < noOfRows; x++) {
        $('#btnDeleteDetails_' + x).attr('onclick', "deleteRow('btnDeleteDetails_2', 'Details');");
        //getStockQtyHidden(x);
    }
}

function addTooltip() {
    var noOfRows = $("#deliveryGrid tbody tr").length;

    for (x = 0; x < noOfRows; x++) {
        var doID = $("#deliveryDetails_" + x + "_ID").val();
        var doCode = $("#deliveryDetails_" + x + "_Code").text();
        var doLink = "/PurchaseDelivery/Detail?key=" + doID;

        $("#deliveryDetails_" + x + "_Code").replaceWith('<a href="' + doLink + '" style="color: #505abc;">' + doCode + '</a>');
    }

    var noOfRows = $("#billGrid tbody tr").length;

    for (x = 0; x < noOfRows; x++) {
        var billID = $("#billDetails_" + x + "_ID").val();
        var billCode = $("#billDetails_" + x + "_Code").text();
        var billLink = "/PurchaseBill/Detail?key=" + billID;

        $("#billDetails_" + x + "_Code").replaceWith('<a href="' + billLink + '" style="color: #505abc;">' + billCode + '</a>');
    }
}

function onSelectVendor(data) {
    $("#hdnSupplierID").val(data.ID);
    $("#txtVendorName").val(data.Name);
    $("#hdnCurrencyID").val(data.CurrencyID);

    sendJSON("/WebService.asmx/RetrieveRate", { "currencyID": data.CurrencyID }, rateListener);

    function rateListener(data2) {
        //alert(JSON.stringify(data2.d.Display));
        $("#lblCurrencyName").text(data2.d.Display);

        sendJSON("/WebService.asmx/RetrieveLastExchangeRate", { "vendorID": data.ID }, exchangeRateListener);

        function exchangeRateListener(data3) {
            //alert('exchangeRate =' + data3.d.ExchangeRate);
            if (data3.d == null) {
                $("#txtExchangeRate").val(data2.d.Value);
                var valueIdr = parseFloat(data2.d.Value);
                if (valueIdr == 1)
                    $('#txtExchangeRate').prop('readonly', true);
                else
                    $('#txtExchangeRate').prop('readonly', false);
            }
            else {
                $("#txtExchangeRate").val(data3.d.ExchangeRate);
            }
        }

    }
}

function onSelectEmployee(data) {
    $("#hdnEmployeeID").val(data.ID);
    $("#lblEmployeeName").text(data.Name);
}


function getSupplierID() {
    return $("#hdnSupplierID").val();
}

function getCustomerID() {
    return $("#WarehouseID").val();
}
function getWarehouseID() {
    return $("#hdnWarehouseID").val();
}

function adjustPaymentLine(index) {
    var amountDue = parseFloat($("#Details_" + index + "_AmountDue").val().replaceAll(',', ''));
    console.log("amountDue = " + amountDue);
    var discount = parseFloat($("#Details_" + index + "_DiscountTaken").val().replaceAll(',', ''));
    console.log("discount = " + discount);
    var payment = parseFloat($("#Details_" + index + "_AmountStr").val().replaceAll(',', ''));
    console.log("payment = " + payment);


    payment = amountDue - discount;
    console.log("new payment = " + payment);

    var paymentStr = payment.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,");
    $("#Details_" + index + "_AmountStr").val(paymentStr);

    calcPaymentAmount(index);
}

function adjustDiscountLine(index) {
    var amountDue = parseFloat($("#Details_" + index + "_AmountDue").val().replaceAll(',', '').replaceAll('.', ','));
    var discount = parseFloat($("#Details_" + index + "_DiscountTaken").val().replaceAll(',', '').replaceAll('.', ','));
    var payment = parseFloat($("#Details_" + index + "_Amount").val().replaceAll(',', '').replaceAll('.', ','));

    if (payment + discount > amountDue) {
        discount = amountDue - payment;
    }

    var discountStr = discount.toLocaleString('en-US');
    $("#Details_" + index + "_DiscountTaken").val(discount);

    calcPaymentAmount(index);
}


function calcPaymentAmount(index) {

    var rowCount = $('#Grid1 tbody tr').length;
    var total = 0;
    for (x = 0; x < rowCount; x++) {
        var st = parseFloat($("#Details_" + x + "_AmountStr").val().replaceAll(',', ''));
        if (isNaN(st)) {
            st = 0;
        }
        //if (st = '') {
        //    st = 0;
        //    parseFloat($("#Details_" + x + "_AmountStr").val(0));
        //}
        console.log("amount " + x + ":" + st);
        total += st;
    }
    var strTotal = total.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,");
    $('#txtAmountHelp').val(strTotal);
}

function calc(index) {
    var assetPrice = parseFloat($("#Details_" + index + "_AssetPrice").val().replaceAll('.', '').replaceAll(',', '.'));

    var qty = parseFloat($("#Details_" + index + "_Quantity").val().replaceAll(',', '.'));


    var taxType = $("#Details_" + index + "_TaxType").val();

    if (isNaN(assetPrice))
        assetPrice = 0;

    if (isNaN(qty))
        qty = 0;

    if (isNaN(taxType))
        taxType = 0;

    var totalAmount = qty * assetPrice;
    //alert(totalAmount.toLocaleString().replaceAll('.', ','));
    $("#Details_" + index + "_TotalAmount").text(totalAmount.toLocaleString().replaceAll('.', ','));

    var totalPPN = 0;
    if (taxType == 2)
        totalPPN = totalAmount * 0.1;

    $("#Details_" + index + "_TotalPPN").text(totalPPN.toLocaleString().replaceAll('.', ','));

    var total = totalAmount + totalPPN;

    var strTotal = total.toLocaleString().replaceAll('.', ',');

    $("#Details_" + index + "_Total").text(strTotal);

    var rowCount = $('#Grid1 tbody tr').length;

    var total = 0;
    for (x = 0; x < rowCount; x++) {
        var st = parseFloat($("#Details_" + x + "_Total").text().replaceAll(',', '').replaceAll('.', ','));
        total += st;
    }

    var strTotal = total.toLocaleString().replaceAll('.', ',');
    $('#txtGridTotal').text(strTotal);
}

function calcReceipt(index) {
    //var qtyPO = parseFloat($("#Details_" + index + "_QtyPO").text());
    //var qtyReceive = parseFloat($("#Details_" + index + "_QtyReceive").text());
    var strQty = parseFloat($("#Details_" + index + "_StrQuantity").val());

    if (isNaN(strQty))
        strQty = 0;

    if (strQty == 0) {
        alert('Penerimaan harus lebih dari nol');
    }
    //if ((qtyPO - qtyReceive) < strQty) {
    //    $("#Details_" + index + "_StrQuantity").val(qtyPO - qtyReceive);
    //}

}

function setGridButtonVisivility() {
    var noOfRows = $("#Grid1 tbody tr").length;
    var hasDO = $("#hdnHasDO").val();

    if (hasDO == "True")
        $("#btnAddNew").hide();

    for (x = 0; x < noOfRows; x++) {
        if (hasDO == "True") {
            $("#btnDeleteDetails_" + x).hide();
        }
    }
}

function changeCurrency() {
    if ($('[name=Currency]:checked').val() == 2) {
        var currencyValue = parseFloat($('#hdnConversionValue').val());
        $('#txtExchangeRate').val(currencyValue.toFixed(0).replace('.', ','));
    }
    else
        $('#txtExchangeRate').val('1,00');

}

//working
function deleteRow(deleteButtonID, dataContainerName) {
    //alert(deleteButtonID + " " + dataContainerName);
    var button = $("#" + deleteButtonID);

    var deletedIndex = button.parent().parent().index();
    //console.log("deletedIndex = " + deletedIndex);
    //var CodeInRows = $("#Details_" + deletedIndex + "_ProductCode").val();

    var gridID = button.parent().parent().parent().parent().attr('id');
    //console.log("gridID = " + gridID);

    var noOfRows = $("#" + gridID + " tbody tr").length;
    //console.log("#" + gridID + " tbody tr" + ", therefore");
    //console.log("noOfRows = " + noOfRows);

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

    //var oldCode = $('#hdnProductReference').val();
    //removeProductCode(oldCode, CodeInRows);
    //showGridProduct();

    //oldCode = $('#hdnDiamondReference').val();
    //removeDiamondCode(oldCode, CodeInRows);
    //showGridDiamond();
    calc(0);
}