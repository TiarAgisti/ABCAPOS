$(function () {
    var action = $("#hdnAction").val();
    $("[name='lnkAddNewGrid1']").hide();
    $('#Form1').attr("action", "/BookingOrder/" + action);

    addTooltipPO();
});

function addTooltipPO() {
    var noOfRows = $("#poGrid tbody tr").length;

    for (x = 0; x < noOfRows; x++) {
        var poID = $("#poDetails_" + x + "_ID").val();
        var poCode = $("#poDetails_" + x + "_Code").text();
        var poLink = "/PurchaseOrder/Detail?key=" + poID;

        $("#poDetails_" + x + "_Code").replaceWith('<a href="' + poLink + '" style="color: #505abc;">' + poCode + '</a>');
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
            //unitChange(index);
            //var assetPriceInDollar = data.AssetPriceInDollar.toFixed(2).replaceAll('.', ',');
            //$("#Details_" + index + "_AssetPriceInDollar").val(assetPriceInDollar);
            //alert(data.AssetPrice.toFixed(0).replaceAll('.', ','));
            $("#Details_" + index + "_AssetPrice").val(data.AssetPrice.toFixed(0).replaceAll('.', ','));
        }
    });
}

function unitChange(index) {
}
