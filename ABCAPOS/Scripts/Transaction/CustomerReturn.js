$(function () {
    $("[name='lnkAddNewGrid1']").hide();

    $("#btnVoid").live("click", function (e) {
        e.preventDefault();
        $(" <div></div>")
                .addClass("dialog")
                .attr("id", $(this).attr("data-dialog-id"))
                .appendTo("body").dialog({
                    title: $(this).attr("Alasan Void"),
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

    setGridButtonVisivility();

    addTooltip();
});

function addTooltip() {
    var noOfRows = $("#receiptGrid tbody tr").length;

    for (x = 0; x < noOfRows; x++) {
        var doID = $("#receiptDetails_" + x + "_ID").val();
        var doCode = $("#receiptDetails_" + x + "_Code").text();
        var doLink = "/ReturnReceipt/Detail?key=" + doID;

        $("#receiptDetails_" + x + "_Code").replaceWith('<a href="' + doLink + '" style="color: #505abc;">' + doCode + '</a>');
    }

    var noOfRows = $("#creditGrid tbody tr").length;

    for (x = 0; x < noOfRows; x++) {
        var creditID = $("#creditDetails_" + x + "_ID").val();
        var creditCode = $("#creditDetails_" + x + "_Code").text();
        var creditLink = "/CreditMemo/Detail?key=" + creditID;

        $("#creditDetails_" + x + "_Code").replaceWith('<a href="' + creditLink + '" style="color: #505abc;">' + creditCode + '</a>');
    }



}

function calculateCredit(index) {
    var remainingCredit = $('#hdnCeilingAmount').val();
    console.log("Remaining: " + remainingCredit);
    var rowCount = $('#Grid1 tbody tr').length;
    var totalApply = 0;

    for (x = 0; x < rowCount; x++) {
        var st = parseFloat($("#Details_" + x + "_Amount").val());
        console.log("row " + x + " value = " + st);
        totalApply += st;
        console.log("totalApply = " + totalApply);
    }
    remainingCredit -= totalApply;
    console.log("Remaining now: " + remainingCredit);
    $('#txtCreditRemaining').val(remainingCredit.toLocaleString());
}

function calcReceipt(index) {
    //var qtyPO = parseFloat($("#Details_" + index + "_QtyPO").text());
    //var qtyReceive = parseFloat($("#Details_" + index + "_QtyReceive").text());
    var strQty = parseFloat($("#Details_" + index + "_Quantity").val().replace(',',''));

    if (isNaN(strQty))
        strQty = 0;

    if (strQty == 0) {
        alert('Penerimaan harus lebih dari nol');
    }
    //if ((qtyPO - qtyReceive) < strQty) {
    //    $("#Details_" + index + "_StrQuantity").val(qtyPO - qtyReceive);
    //}

}

function changeCurrency() {
    if ($('[name=Currency]:checked').val() == 2) {
        var currencyValue = parseFloat($('#hdnConversionValue').val());
        //alert(currencyValue);
        $('#txtExchangeRate').val(currencyValue.toFixed(2).replace('.', ','));
    }
    else
        $('#txtExchangeRate').val('1,00');

}

function getCustomerID() {
    return $("#hdnCustomerID").val();
}

function getWarehouseID() {
    //alert($("#hdnWarehouseID").val());
    return $("#WarehouseID").val();
}

function getDOWarehouseID() {
    return $("#hdnWarehouseID").val();
}

function onSelectCustomer(data) {
    $("#hdnCustomerID").val(data.ID);
    $("#txtCustomerName").val(data.Name);
    $("#lblSalesReference").text(data.SalesRepName);
    $("#txtContactPerson").val(data.ContactPerson);
    $("#txtShipTo").val(data.Address);
    $("#hdnPriceLevelID").val(data.PriceLevelID);
    $("#hdnPriceLevelName").val(data.PriceLevelName);
    $("#TermsOfPaymentID").val(data.TermsID)
}

function onSelectEmployee(data) {
    $("#hdnEmployeeID").val(data.ID);
    $("#lblEmployeeName").text(data.Name);
}

function onSelectQuotation1(data) {
    $("#hdnReferenceID1").val(data.ID);
}

function onSelectQuotation2(data) {
    $("#hdnReferenceID2").val(data.ID);
}

function onSelectQuotation3(data) {
    $("#hdnReferenceID3").val(data.ID);
}

function onSelectQuotation4(data) {
    $("#hdnReferenceID4").val(data.ID);
}

function onSelectQuotation5(data) {
    $("#hdnReferenceID5").val(data.ID);
}

function onSelectQuotation6(data) {
    $("#hdnReferenceID6").val(data.ID);
}

function trimBin(index, warehouseID) {
    var productID = $("#Details_" + index + "_ProductID").val();
    console.log("warehouseID = " + warehouseID);
    console.log("productID = " + productID);
    $.ajax({
        type: "POST",
        async: false,
        url: "/Webservice.asmx/RetrieveBinByProductIDWarehouseIDInverse",
        data: "{productID: " + productID + ", warehouseID: " + warehouseID + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            $.each(data.d, function (key, val) {
                $("#Details_" + index + "_BinID option[value='" + val.ID + "']").remove();
            });
        }
    });
}

function trimUnitOption(index) {
    var productID = $("#Details_" + index + "_ProductID").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "/Webservice.asmx/GetUnitsByProductIDInversed",
        data: "{productID: " + productID + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            $.each(data.d, function (key, val) {
                $("#Details_" + index + "_ConversionID option[value='" + val.ID + "']").remove();
            });
        }
    });
}

function onSelectProduct(data, index) {
    $.ajax({
        type: "POST",
        async: true,
        url: "/Webservice.asmx/GetUnitsByProductID",
        data: "{productID: " + data.ID + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (validUnit) {
            $("#Details_" + index + "_ConversionID").empty();
            $.each(validUnit.d, function (key, val) {
                var option = document.createElement('option');
                option.text = val.Name;
                option.value = val.ID;
                $("#Details_" + index + "_ConversionID").append(option);
            });
            var ddl = document.getElementById("Details_" + index + "_ConversionID");
            var opts = ddl.options.length;
            for (var i = 0; i < opts; i++) {
                if (ddl.options[i].value == data.SaleUnitID) {
                    ddl.options[i].selected = true;
                    break;
                }
            }
            //alert(opts);
            //alert(JSON.stringify(data));
            $("#Details_" + index + "_ProductID").val(data.ID);
            $("#Details_" + index + "_ProductName").text(data.ProductName);
            // the following line is for Credit Memos
            $("#Details_" + index + "_ProductName").val(data.ProductName);
            $("#Details_" + index + "_StockQtyHidden").val(data.StockQty);
            $("#Details_" + index + "_StockAvailableHidden").val(data.StockAvailable);
            $("#Details_" + index + "_PriceHidden").val(data.SellingPrice);
            getSellingUnitRate(index, data.SaleUnitID);
            unitChange(index);
            //alert(data.SellingPrice.toFixed(0).replaceAll('.', ','));   
            //alert($("#hdnPriceLevelName").val());
            var priceLevelID = $("#hdnPriceLevelID").val();
            var ddl2 = document.getElementById("Details_" + index + "_PriceLevelID");
            var opts2 = ddl2.options.length;
            for (var i = 0; i < opts2; i++) {
                if (ddl2.options[i].value == priceLevelID) {
                    ddl2.options[i].selected = true;
                    break;
                }
            }
            console.log("onSelectProduct, priceLevel = " + priceLevelID);
            console.log("item price = " + data.SellingPrice);
            //priceLevelChange(index);
            //alert(data.PriceLevelID);
            //alert($("#Details_" + index + "_PriceLevelID").val());
            //$("#Details_" + index + "_PriceLevelName").text(priceLevelName);
            //$("#Details_" + index + "_AssetPrice").val(data.AssetPrice);
        }
    });
    setPriceLevelReadOnly(index);

    //alert(index);

    //var priceLevelID = $("#hdnPriceLevelID").val();
    //var priceLevelName = $("#hdnPriceLevelName").val();

    //$("#Details_" + index + "_PriceLevelID").val(priceLevelID);
}

function priceLevelChange(index) {
    var productID = $("#Details_" + index + "_ProductID").val();
    var priceLevelID = $('#Details_' + index + '_PriceLevelID').val();
    var taxTypeID = $('#Details_' + index + '_TaxType').val();
    $.ajax({
        type: "POST",
        async: false,
        url: "/Webservice.asmx/GetProductByIDPriceLevelIDTaxTypeID",
        data: "{productID: " + productID + ", priceLevelID: " + priceLevelID + ", taxType: " + taxTypeID + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            $("#Details_" + index + "_PriceHidden").val(data.d.SellingPrice);
            console.log("priceLevelChange, price = " + data.d.SellingPrice);
            unitChange(index);
            //calc(index);
        }
    });

    setPriceLevelReadOnly(index);

}

function setPriceLevelReadOnly(index) {
    if ($('#Details_' + index + '_PriceLevelID').val() == 15)
        $('#Details_' + index + '_Price').prop('readonly', false);
    else
        $('#Details_' + index + '_Price').prop('readonly', true);
}

function getSellingUnitRate(index, unitDetailID) {
    $.ajax({
        type: "POST",
        async: false,
        url: "/Webservice.asmx/GetUnitRateByID",
        data: "{unitDetailID: " + unitDetailID + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var rate = (data.d);
            $("#Details_" + index + "_SaleUnitRateHidden").val(rate);
            console.log("Selling Unit ID = " + unitDetailID + ", rate = " + rate);
        }
    });
}

function unitChange(index) {
    var unitDetailID = $("#Details_" + index + "_ConversionID").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "/Webservice.asmx/GetUnitRateByID",
        data: "{unitDetailID: " + unitDetailID + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var oldStockQty = $("#Details_" + index + "_StockQtyHidden").val();
            var oldAvailQty = $("#Details_" + index + "_StockAvailableHidden").val();
            var oldPrice = $("#Details_" + index + "_PriceHidden").val();
            var priceRate = $("#Details_" + index + "_SaleUnitRateHidden").val();
            var rate = (data.d);
            console.log("unitChange rate = " + data.d);
            var newStockQty = oldStockQty / rate;
            var newAvailQty = oldAvailQty / rate;
            var newPrice = (oldPrice / priceRate) * rate;
            console.log("unitChange price = " + newPrice);
            var newStockQtyStr = newStockQty.toLocaleString();
            var newAvailQtyStr = newAvailQty.toLocaleString();
            var newPriceStr = newPrice.toLocaleString();
            $("#Details_" + index + "_StockQty").text(newStockQtyStr);
            $("#Details_" + index + "_StockAvailable").text(newAvailQtyStr);
            $("#Details_" + index + "_Price").val(newPrice);
            calc(index);
        }
    });
}

function populateQuotation() {
    var referenceID1 = $("#hdnReferenceID1").val();
    var referenceID2 = $("#hdnReferenceID2").val();
    var referenceID3 = $("#hdnReferenceID3").val();
    var referenceID4 = $("#hdnReferenceID4").val();
    var referenceID5 = $("#hdnReferenceID5").val();
    var referenceID6 = $("#hdnReferenceID6").val();
    var title = $("#txtTitle").val();
    var contactPerson = $("#txtContactPerson").val();
    var shipTo = $("#txtShipTo").val();

    window.location.href = "/Quotation/Create?contactPerson=" + contactPerson + "&title=" + title + "&shipTo=" + shipTo + "&referenceID1=" + referenceID1 + "&referenceID2=" + referenceID2 + "&referenceID3=" + referenceID3 + "&referenceID4=" + referenceID4 + "&referenceID5=" + referenceID5 + "&referenceID6=" + referenceID6;
}

function onSelectBin(data, index) {
    $("#Details_" + index + "_BinID").val(data.ID);
}

function calc(index) {
    var price = parseFloat($("#Details_" + index + "_Price").val().replaceAll(',', ''));
    console.log("price= " + price);

    var qty = parseFloat($("#Details_" + index + "_Quantity").val().replaceAll(',', ''));
    console.log("qty= " + qty);

    var taxType = $("#Details_" + index + "_TaxType").val();

    if (isNaN(price))
        price = 0;

    if (isNaN(qty))
        qty = 0;

    if (isNaN(taxType))
        taxType = 0;

    var totalAmount = qty * price;
    $("#Details_" + index + "_TotalAmount").text(totalAmount.toFixed(2).toLocaleString('en-us'));

    var totalPPN = 0;
    if (taxType == 2)
        totalPPN = totalAmount * 0.1;

    $("#Details_" + index + "_TotalPPN").text(totalPPN.toFixed(2).toLocaleString('en-us'));

    var total = totalAmount + totalPPN;

    var strTotal = total.toFixed(2).toLocaleString('en-us');
    console.log("total string =" + strTotal);

    $("#Details_" + index + "_Total").val(total.toFixed(0));

    var rowCount = $('#Grid1 tbody tr').length;

    var total = 0;
    var totalTax = 0;
    var subTotal = 0;
    for (x = 0; x < rowCount; x++) {
        var st = parseFloat($("#Details_" + x + "_Total").val().replaceAll(',', '').replaceAll('.', ','));
        var sttax = parseFloat($("#Details_" + x + "_TotalPPN").text().replaceAll(',', '').replaceAll('.', ','));
        var stsub = parseFloat($("#Details_" + x + "_TotalAmount").text().replaceAll(',', '').replaceAll('.', ','));
        total += st;
        totalTax += sttax;
        subTotal += stsub;
    }
    var strTotal = total.toLocaleString().replaceAll('.', ',');
    var strTotalTax = totalTax.toLocaleString().replaceAll('.', ',');
    var strTotalSub = subTotal.toLocaleString().replaceAll('.', ',');
    $('#txtGridGrandTotal').text(strTotal);
    $('#txtGridPPN').text(strTotalTax);
    $('#txtGridTotal').text(strTotalSub);
}

function reversecalc(index) {
    var totalGross = $("#Details_" + index + "_Total").val();
    console.log("totalGross= " + totalGross);

    var qty = parseFloat($("#Details_" + index + "_Quantity").val().replaceAll(',', '.'));
    console.log("qty= " + qty);

    var taxType = $("#Details_" + index + "_TaxType").val();

    var price = 0;
    var totalPPN = 0;

    if (isNaN(totalGross))
        totalGross = 0;

    if (isNaN(qty))
        qty = 0;

    if (isNaN(taxType))
        taxType = 0;

    if (taxType == 2) {
        totalPPN = totalGross / 1.1 * 0.1;
    }
    else
        totalPPN = 0

    var totalAmount = totalGross - totalPPN;
    price = totalAmount / qty;

    /*total = amount + ppn
    ppn = amount * 0.1
    total = amount + amount * 0.1
    total = 1.1(amount)
    amount = total / 1.1
    ppn = total / 1.1 * 0.1
    */

    $("#Details_" + index + "_TotalAmount").text(totalAmount.toFixed(2).toLocaleString('en-us'));
    $("#Details_" + index + "_TotalPPN").text(totalPPN.toFixed(2).toLocaleString('en-us'));
    $("#Details_" + index + "_Price").val(price);

    var rowCount = $('#Grid1 tbody tr').length;

    var total = 0;
    var totalTax = 0;
    var subTotal = 0;
    for (x = 0; x < rowCount; x++) {
        var st = parseFloat($("#Details_" + x + "_Total").val().replaceAll(',', '').replaceAll('.', ','));
        var sttax = parseFloat($("#Details_" + x + "_TotalPPN").text().replaceAll(',', '').replaceAll('.', ','));
        var stsub = parseFloat($("#Details_" + x + "_TotalAmount").text().replaceAll(',', '').replaceAll('.', ','));
        total += st;
        totalTax += sttax;
        subTotal += stsub;
    }
    var strTotal = total.toLocaleString().replaceAll('.', ',');
    var strTotalTax = totalTax.toLocaleString().replaceAll('.', ',');
    var strTotalSub = subTotal.toLocaleString().replaceAll('.', ',');
    $('#txtGridGrandTotal').text(strTotal);
    $('#txtGridPPN').text(strTotalTax);
    $('#txtGridTotal').text(strTotalSub);
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

//function sendJSON(url, data, callback, overrideMethod, failCallback) {
//    var method = "GET";
//    var dataString = null;
//    if (data != null) {
//        method = "POST";
//        dataString = JSON.stringify(data)
//    }
//    if (overrideMethod != null) {
//        method = overrideMethod;
//    }
//    return jQuery.ajax({
//        'type': method,
//        'url': url,
//        'contentType': 'application/json',
//        'data': dataString,
//        'dataType': 'json',
//        'success': callback,
//        'error': function () {
//            if (failCallback != null) {
//                failCallback();
//            }
//        },
//        statusCode: {
//            401: function () {
//                //kickUser();
//            }
//        }
//    });
//};

var server = "localhost:8002";
//var server = "md.psinformatika.com";
var salt = "d50ebaae784f428f91b467d06c75220c";
//var server = "192.168.2.53";
var getServer = function () {
    return "http://" + server;
}