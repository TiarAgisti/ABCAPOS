$(function () {
    $("[name='lnkAddNewGrid1']").hide();

    $("#btnVoid").live("click", function (e) {
        e.preventDefault();
        console.log("void button pressed");
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

    //addTooltip();
});

function onSelectCustomer(data) {
    $("#hdnCustomerID").val(data.ID);
    $("#txtCustomerName").val(data.Name);
}

function getWarehouseID() {
    return $("#WarehouseID").val();
}

function onSelectEmployee(data) {
    $("#hdnStaffID").val(data.ID);
    $("#lblStaffName").text(data.Name);
}

function onSelectProduct(data, index) {
    $.ajax({
        type: "POST",
        async: false,
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
            $("#Details_" + index + "_QtyOnHandHidden").val(data.StockQty);
            $("#Details_" + index + "_QtyAvailableHidden").val(data.StockAvailable);
            unitChange(index);
        }
    });
    var warehouseID = getWarehouseID();
    $.ajax({
        type: "POST",
        async: false,
        url: "/Webservice.asmx/RetrieveBinProductWarehouse",
        data: "{productID: " + data.ID + ", warehouseID: " + warehouseID + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (bpw) {
            $("#Details_" + index + "_BinID").empty();
            var defaultBinID = 0;
            $.each(bpw.d, function (key, val) {
                var option = document.createElement('option');
                option.text = val.BinName;
                option.value = val.BinID;
                $("#Details_" + index + "_BinID").append(option);
                if (val.IsDefaultBin) {
                    defaultBinID = val.BinID;
                }
            });
            var ddl = document.getElementById("Details_" + index + "_BinID");
            var opts = ddl.options.length;
            for (var i = 0; i < opts; i++) {
                if (ddl.options[i].value == defaultBinID) {
                    ddl.options[i].selected = true;
                    break;
                }
            }
            $("#Details_" + index + "_ProductID").val(data.ID);
            $("#Details_" + index + "_ProductName").text(data.ProductName);
            $("#Details_" + index + "_QtyOnHandHidden").val(data.StockQty);
            $("#Details_" + index + "_QtyAvailableHidden").val(data.StockAvailable);
            unitChange(index);
        }
    });
}

function trimBin(index, warehouseID) {
    var productID = $("#Details_" + index + "_ProductID").val();
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
            var oldQty = $("#Details_" + index + "_QtyOnHandHidden").val();
            var oldQtyAvailable = $("#Details_" + index + "_QtyAvailableHidden").val();
            //alert(oldQtyAvailable);
            var rate = (data.d);
            var rateAvailable = (data.d);
            var convQtyOnHand = oldQty / rate;
            $("#Details_" + index + "_QtyOnHandOld").val(convQtyOnHand);
            var convQtyOnHandStr = convQtyOnHand.toFixed(5);
            $("#Details_" + index + "_QtyOnHand").text(convQtyOnHandStr);

            var convQtyAvailable = oldQtyAvailable / rateAvailable;
            //alert(convQtyAvailable);
            $("#Details_" + index + "_QtyAvailableOld").val(convQtyAvailable);
            var convQtyAvailableStr = convQtyAvailable.toFixed(5);
            $("#Details_" + index + "_QtyAvailable").text(convQtyAvailableStr);
            calc(index);
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

function calc(index) {
    var adjQty = parseFloat($("#Details_" + index + "_Quantity").val().replaceAll(',', '.'));
    var adjQtyAvail = parseFloat($("#Details_" + index + "_QuantityAvailable").val().replaceAll(',', '.'));
    var oldQty = parseFloat($("#Details_" + index + "_QtyOnHandOld").val().replaceAll(',', '.'));
    var oldQtyAvailable = parseFloat($("#Details_" + index + "_QtyAvailableOld").val().replaceAll(',', '.'))

    if (isNaN(adjQty))
        adjQty = 0;

    if (isNaN(adjQtyAvail))
        adjQtyAvail = 0;

    if (isNaN(oldQty))
        oldQty = 0;

    if (isNaN(oldQtyAvailable))
        oldQtyAvailable = 0;

    var newQty = oldQty + adjQty;
    $("#Details_" + index + "_QtyOnHandNew").val(newQty);

    var strNewQty = newQty.toFixed(5);
    $("#Details_" + index + "_NewQty").text(strNewQty);

    var newQtyAvailable = oldQtyAvailable + adjQtyAvail;
    $("#Details_" + index + "_QtyAvailableNew").val(newQtyAvailable);

    var strNewQtyAvailable = newQtyAvailable.toFixed(5);
    $("#Details_" + index + "_NewQtyAvailable").text(strNewQtyAvailable);
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