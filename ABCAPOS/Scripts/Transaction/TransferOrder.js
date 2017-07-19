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
    var noOfRows = $("#deliveryGrid tbody tr").length;

    for (x = 0; x < noOfRows; x++) {
        var doID = $("#deliveryDetails_" + x + "_ID").val();
        var doCode = $("#deliveryDetails_" + x + "_Code").text();
        var doLink = "/TransferDelivery/Detail?key=" + doID;

        $("#deliveryDetails_" + x + "_Code").replaceWith('<a href="' + doLink + '" style="color: #505abc;">' + doCode + '</a>');
    }

    var noOfRows = $("#receiptGrid tbody tr").length;

    for (x = 0; x < noOfRows; x++) {
        var receiptID = $("#receiptDetails_" + x + "_ID").val();
        var receiptCode = $("#receiptDetails_" + x + "_Code").text();
        var receiptLink = "/TransferReceipt/Detail?key=" + receiptID;

        $("#receiptDetails_" + x + "_Code").replaceWith('<a href="' + receiptLink + '" style="color: #505abc;">' + receiptCode + '</a>');
    }
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
                if (ddl.options[i].value == data.StockUnitID) {
                    ddl.options[i].selected = true;
                    break;
                }
            }
            $("#Details_" + index + "_ProductID").val(data.ID);
            $("#Details_" + index + "_ProductName").text(data.ProductName);
            $("#Details_" + index + "_StockQtyHidden").val(data.StockQty);
            $("#Details_" + index + "_QtyAvailableHidden").val(data.StockAvailable);
            unitChange(index);
            $("#Details_" + index + "_TransferPrice").val(data.AssetPrice.toFixed(0));
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
            console.log("oldStockQty =" + oldStockQty);
            var oldAvailQty = $("#Details_" + index + "_QtyAvailableHidden").val();
            var rate = (data.d);
            var newStockQty = oldStockQty / rate;
            var newAvailQty = oldAvailQty / rate;
            var newStockQtyStr = newStockQty.toFixed(2);
            var newAvailQtyStr = newAvailQty.toFixed(2);
            console.log("newStockQtyStr =" + newStockQtyStr);
            $("#Details_" + index + "_StockQty").text(newStockQtyStr);
            $("#Details_" + index + "_QtyAvailable").text(newAvailQtyStr);
            //calc(index); NOTE: we're not changing price with Unit change
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

function getFromWarehouseID() {
    return $("#FromWarehouseID").val();
}

function onSelectEmployee(data) {
    $("#hdnStaffID").val(data.ID);
    $("#lblStaffName").text(data.Name);
}

function calc(index) {
    var price = parseFloat($("#Details_" + index + "_TransferPrice").val());
    var qty = parseFloat($("#Details_" + index + "_Quantity").val());

    if (isNaN(price))
        price = 0;

    if (isNaN(qty))
        qty = 0;

    var amount = qty * price;
    var strAmount = amount.toFixed(2);
    
    $("#Details_" + index + "_Amount").text(strAmount);

    var rowCount = $('#Grid1 tbody tr').length;

    var total = 0;
    for (x = 0; x < rowCount; x++) {
        var st = parseFloat($("#Details_" + x + "_Amount").text());
        total += st;
    }

    var shipCost = parseFloat($("#lblShippingCost").text());
    $('#hdnSubTotal').val(total);
    var strTotal = total.toFixed(2);
    $('#txtSubTotal').text(strTotal);
   
    updateGrandTotal();
}

function updateShipCost() {
    var shipCost = parseFloat($('#txtShippingCost').val());
    var strShipCost = shipCost.toFixed(2);
    $('#lblShippingCost').text(strShipCost);
    updateGrandTotal();
}

function updateGrandTotal() {
    var shipCost = parseFloat($("#lblShippingCost").text());
    var total = parseFloat($("#txtSubTotal").text());
    var grandTotal = total + shipCost;

    var strGrandTotal = grandTotal.toFixed(2);
    $('#hdnGrandTotal').val(grandTotal);
    $('#txtGrandTotal').text(strGrandTotal);
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

var server = "localhost:8002";
//var server = "md.psinformatika.com";
var salt = "d50ebaae784f428f91b467d06c75220c";
//var server = "192.168.2.53";
var getServer = function () {
    return "http://" + server;
}