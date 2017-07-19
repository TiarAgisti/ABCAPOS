$(function () {
    var action = $("#hdnAction").val();
    $("[name='lnkAddNewGrid1']").hide();
    $('#Form1').attr("action", "/BookingSales/" + action);

    addTooltipSO();
});

function addTooltipSO() {
    var noOfRows = $("#soGrid tbody tr").length;

    for (x = 0; x < noOfRows; x++) {
        var soID = $("#soDetails_" + x + "_ID").val();
        var soCode = $("#soDetails_" + x + "_Code").text();
        var soLink = "/SalesOrder/Detail?key=" + soID;

        $("#soDetails_" + x + "_Code").replaceWith('<a href="' + soLink + '" style="color: #505abc;">' + soCode + '</a>');
    }

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

function recalcSOTotals() {
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
            var newStockQty = oldStockQty / rate;
            var newAvailQty = oldAvailQty / rate;
            var newPrice = (oldPrice / priceRate) * rate;
            var newStockQtyStr = newStockQty.toLocaleString();
            var newAvailQtyStr = newAvailQty.toLocaleString();
            var newPriceStr = newPrice.toLocaleString();
            $("#Details_" + index + "_Price").val(newPrice);
            calc(index);
        }
    });
}
