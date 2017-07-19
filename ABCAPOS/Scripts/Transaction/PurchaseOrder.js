$(function () {
    var action = $("#hdnAction").val();
    $("[name='lnkAddNewGrid1']").hide();
    $('#Form1').attr("action", "/PurchaseOrder/" + action);

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
    setUnitChange();
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

function getUnitOnProduct(index) {
    var productID = $("#Details_" + index + "_ProductID").val();
    var conversionIDTemp = $("#hdn_ConversionID_" + index).val();
    console.log("ConversionIDTemp= " + conversionIDTemp);
    $.ajax({
        type: "POST",
        async: true,
        url: "/Webservice.asmx/GetUnitsByProductID",
        data: "{productID: " + productID + "}",
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
                if (ddl.options[i].value == conversionIDTemp) {
                    ddl.options[i].selected = true;
                    break;
                }
            }
        }
    });

}

function getStockQtyHidden(index) {
    var productCode = $("#Details_" + index + "_ProductCode").val();
    var warehouseID = getWarehouseID();
    var vendorID = getSupplierID();
    $.ajax({
        type: "POST",
        async: true,
        url: "/Webservice.asmx/RetrieveProduct",
        data: "{productCode: " + productID + ", customerID: " + warehouseID + ", vendorID:" + vendorID + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            $("#Details_" + index + "_StockQtyHidden").val(data.d.StockQty);
            console.log("stockQty = " + data.d.StockQty);
            $("#Details_" + index + "_StockAvailableHidden").val(data.d.StockAvailable);
        }
    });
}

function setUnitChange() {
    var noOfRows = $("#Grid1 tbody tr").length;
    var mode = $("#hdnMode").val();
    if (mode == "Create" || mode == "Update") {
        for (x = 0; x < noOfRows; x++) {
            getUnitOnProduct(x);
            //getStockQtyHidden(x);
        }
    }
    
}

function addTooltip() {
    var mode = $("#hdnMode").val();

    if (mode == "Detail" || mode == "Create") {
        var boID = $("#hdnBookingOrderID").val();
        var boCode = $("#lblBookingOrderCode").text();
        
        var boLink = "/BookingOrder/Detail?key=" + boID;

        $("#lblBookingOrderCode").replaceWith('<a href="' + boLink + '">' + boCode + '</a>');

    }

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


function onSelectProduct(data, index) {
    $.ajax({
        type: "POST",
        async: true,
        url: "/Webservice.asmx/GetUnitsByProductID",
        data: "{productID: " + data.ID + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (validUnit) {
            $("#Details_" + index + "_ConversionName").replaceWith("<select id='Details_" + index + "_ConversionID' name='Details[" + index + "].ConversionID' style='width:99%;' onchange='unitChange($(this).parent().parent().index())'></select>");
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
                if (ddl.options[i].value == data.PurchaseUnitID) {
                    //alert(data.PurchaseUnitID);
                    ddl.options[i].selected = true;
                    break;
                }
            }
            
            $("#Details_" + index + "_ProductID").val(data.ID);
            $("#Details_" + index + "_ProductName").val(data.ProductName);
            $("#Details_" + index + "_VendorName").text(data.VendorName);
            $("#Details_" + index + "_StockQtyHidden").val(data.StockQty);
            console.log("stockQty = " + data.StockQty);
            $("#Details_" + index + "_StockAvailableHidden").val(data.StockAvailable);
            unitChange(index);
         
            $("#Details_" + index + "_AssetPrice").val(data.AssetPrice.toFixed(0).replaceAll('.', ','));
        }
    });
}

function unitChange(index) {
    var unitDetailID = $("#Details_" + index + "_ConversionID").val();
    console.log("unitDetailID= " + unitDetailID);
    $.ajax({
        type: "POST",
        async: false,
        url: "/Webservice.asmx/GetUnitRateByID",
        data: "{unitDetailID: " + unitDetailID + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var oldStockQty = $("#Details_" + index + "_StockQtyHidden").val();
            console.log("oldStockQty= " + oldStockQty);

            var oldAvailQty = $("#Details_" + index + "_StockAvailableHidden").val();
            console.log("oldAvailQty= " + oldAvailQty);

            var rate = (data.d);
            var newStockQty = oldStockQty / rate;
            var newAvailQty = oldAvailQty / rate;
          
            var newStockQtyStr = newStockQty.toLocaleString();
            var newAvailQtyStr = newAvailQty.toLocaleString();
            
            $("#Details_" + index + "_StockQty").text(newStockQtyStr);
            $("#Details_" + index + "_StockAvailable").text(newAvailQtyStr);
            $("#Details_" + index + "_ConversionID").val(unitDetailID);
            
        }
    });
}

function onSelectBin(data, index) {
    $("#Details_" + index + "_BinID").val(data.ID);
}


function adjustPaymentLine(index) {
    var amountDue = parseFloat($("#Details_" + index + "_AmountDue").val().replaceAll(',', '').replaceAll('.', ','));
    console.log("amountDue = " + amountDue);
    var discount = parseFloat($("#Details_" + index + "_DiscountTaken").val().replaceAll(',', '').replaceAll('.', ','));
    console.log("discount = " + discount);
    var payment = parseFloat($("#Details_" + index + "_Amount").val().replaceAll(',', '').replaceAll('.', ','));
    console.log("payment = " + payment);

    if (payment + discount > amountDue) {
        payment = amountDue - discount;
        console.log("new payment = " + payment);
    }

    var paymentStr = payment.toLocaleString('en-US');
    $("#Details_" + index + "_Amount").val(payment);

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
        var st = parseFloat($("#Details_" + x + "_Amount").val().replaceAll(',', '').replaceAll('.', ','));
        total += st;
    }
    var strTotal = total.toLocaleString('en-US');
    $('#txtAmountHelp').val(strTotal);
}

function recalcTotals() {
    var rowCount = $('#Grid1 tbody tr').length;

    var total = 0;
    var discount = 0;
    var totalTax = 0;
    var subTotal = 0;
    for (x = 0; x < rowCount; x++) {
        var st = parseFloat($("#Details_" + x + "_Total").val().replaceAll(',', ''));
        var stdiscount = parseFloat($("#Details_" + x + "_Discount").val());
        var sttax = parseFloat($("#Details_" + x + "_TotalPPN").text().replaceAll(',', '').replaceAll('.', ','));
        var stsub = parseFloat($("#Details_" + x + "_TotalAmount").text().replaceAll(',', '').replaceAll('.', ','));
        total += st;
        discount += stdiscount;
        totalTax += sttax;
        subTotal += stsub;
    }
    var strTotal = total.toLocaleString('en-us')
    var strDiscount = discount.toLocaleString('en-us')
    var strTotalTax = totalTax.toLocaleString('en-us')
    var strTotalSub = subTotal.toLocaleString('en-us')
    $('#txtGridGrandTotal').text(strTotal);
    $('#txtGridDiscount').text(strDiscount);
    $('#txtGridPPN').text(strTotalTax);
    $('#txtGridTotal').text(strTotalSub);
}

function calc(index) {
    var assetPrice = parseFloat($("#Details_" + index + "_AssetPrice").val());
    var discount = parseFloat($("#Details_" + index + "_Discount").val());
    
    var qty = parseFloat($("#Details_" + index + "_Quantity").val());
    

    var taxType = $("#Details_" + index + "_TaxType").val();

    if (isNaN(assetPrice))
        assetPrice = 0;
    if (isNaN(discount))
        discount = 0;

    if (isNaN(qty))
        qty = 0;

    if (isNaN(taxType))
        taxType = 0;

    var totalAmount = qty * assetPrice;
    $("#Details_" + index + "_TotalAmount").text(totalAmount.toLocaleString('en-US'));

    var totalPPN = 0;
    if (taxType == 2)
        totalPPN = (totalAmount - discount) * 0.1;

    $("#Details_" + index + "_TotalPPN").text(totalPPN.toLocaleString('en-US'));

    var total =(totalAmount - discount) + totalPPN;
    
    var strTotal = total.toFixed(2).toLocaleString('en-US');

    var strdiscount = discount.toFixed(2).toLocaleString('en-US');
    //alert(strdiscount);
    
    $("#Details_" + index + "_Total").val(strTotal);
    $("#hdn_Discount_" + index).val(strdiscount);
    $("#hdn_AssetPrice_" + index).val(assetPrice);

    recalcTotals();
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
    $("#Details_" + index + "_AssetPrice").val(price);

    recalcTotals();
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
    var currencyID = $("#CurrencyID").val();

    $.ajax({
        type: "POST",
        async: false,
        url: "/Webservice.asmx/RetrieveCurrency",
        data: "{currencyID: " + currencyID + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            $("#txtExchangeRate").val(data.d.Value);
        }
    });
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

function onchangeLocation() {
    var warehouseID = $("#WarehouseID").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "/Webservice.asmx/RetrieveWarehouse",
        data: "{warehouseID: " + warehouseID + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var loc = (data.d);
            $("#txtEmailTo").val(loc.Email);
            console.log("Email To = " + loc.Email);
            //var rate = (data.d);
            //$("#Details_" + index + "_SaleUnitRateHidden").val(rate);
            //console.log("Selling Unit ID = " + unitDetailID + ", rate = " + rate);
        }
    });
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
    recalcTotals();
}