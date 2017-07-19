function populateInvoices() {
    var customerID = $("#hdnCustomerID").val();
    
    var id = $("#hdnID").val();
    var mode = $("#hdnMode").val();

    if (mode == "Create") {
        var dateFrom = $("#txtDateFrom").val();
        var dateTo = $("#txtDateTo").val();
        var date = $("#txtDate").val();
        window.location.href = "/MultipleInvoicing/Create?customerID=" + customerID + "&dateFrom=" + dateFrom + "&dateTo=" + dateTo + "&date=" + date;
    }
    if (mode == "Update") {
        var dateFrom = $("#hdnDateFrom").val();
        var dateTo = $("#hdnDateTo").val();
        window.location.href = "/MultipleInvoicing/Update?key=" + id + "&customerID=" + customerID + "&dateFrom=" + dateFrom + "&dateTo=" + dateTo;
    }
}

function populateItems() {
    var multiInvoiceID = $('#hdnID').val();
    var mode = $("#hdnMode").val();
    var noOfRows = $("#Grid1 tbody tr").length;
    var intCount = 0;
    for (x = 0; x < noOfRows; x++) {
        var invoiceID = $('#Details_' + x + '_InvoiceID').val();
        if (mode == "Update") {
            $.ajax({
                type: "POST",
                async: true,
                url: "/Webservice.asmx/GetAllMultipleItemByInvoiceID",
                data: "{multiInvoiceID: " + multiInvoiceID + ", invoiceID: " + invoiceID + "}",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    $.each(data.d, function (key, val1) {
                        addItemGridRow();
                        $("#ItemDetails_" + intCount + "_InvoiceID").val(val1.InvoiceID);
                        $("#ItemDetails_" + intCount + "_InvoiceDetailItemNo").val(val1.InvoiceDetailItemNo);
                        $("#ItemDetails_" + intCount + "_TaxType").text(val1.TaxType);
                        $("#ItemDetails_" + intCount + "_ProductID").val(val1.ProductID);
                        $("#ItemDetails_" + intCount + "_ProductName").text(val1.ProductName);
                        $("#ItemDetails_" + intCount + "_Quantity").text(val1.Quantity);
                        $("#ItemDetails_" + intCount + "_ConversionName").text(val1.ConversionName);
                        $("#ItemDetails_" + intCount + "_Price").text(val1.Price.toFixed(2).toLocaleString('en-US'));
                        $("#ItemDetails_" + intCount + "_TotalAmount").text(val1.TotalAmount.toFixed(2).toLocaleString('en-US'));
                        $("#ItemDetails_" + intCount + "_TotalPPN").text(val1.TotalPPN.toFixed(2).toLocaleString('en-US'));

                        intCount++;
                    });
                }
            });
        }
        
        $.ajax({
            type: "POST",
            async: true,
            url: "/Webservice.asmx/GetMultipleItemByInvoiceID",
            data: "{multiInvoiceID: " + multiInvoiceID + ", invoiceID: " + invoiceID + "}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                $.each(data.d, function (key, val1) {
                    addItemGridRow();
                    $("#ItemDetails_" + intCount + "_InvoiceID").val(val1.InvoiceID);
                    $("#ItemDetails_" + intCount + "_InvoiceDetailItemNo").val(val1.InvoiceDetailItemNo);
                    $("#ItemDetails_" + intCount + "_TaxType").text(val1.TaxType);
                    $("#ItemDetails_" + intCount + "_ProductID").val(val1.ProductID);
                    $("#ItemDetails_" + intCount + "_ProductName").text(val1.ProductName);
                    $("#ItemDetails_" + intCount + "_Quantity").text(val1.Quantity);
                    $("#ItemDetails_" + intCount + "_ConversionName").text(val1.ConversionName);
                    $("#ItemDetails_" + intCount + "_Price").text(val1.Price.toFixed(2).toLocaleString('en-US'));
                    $("#ItemDetails_" + intCount + "_TotalAmount").text(val1.TotalAmount.toFixed(2).toLocaleString('en-US'));
                    $("#ItemDetails_" + intCount + "_TotalPPN").text(val1.TotalPPN.toFixed(2).toLocaleString('en-US'));

                    intCount++;
                });
            }
        });
        
        $('#populateItems').prop('disable', true);
        
    }
    recalcInvItemTotals();
}

function recalcInvTotals() {
    var rowCount = $('#Grid1 tbody tr').length;

    var total = 0;
    var totalTax = 0;
    var subTotal = 0;
    for (x = 0; x < rowCount; x++) {
        var st = parseFloat($("#Details_" + x + "_GrandTotal").text().replaceAll(',', '').replaceAll('.', ','));
        var sttax = parseFloat($("#Details_" + x + "_TaxAmount").text().replaceAll(',', '').replaceAll('.', ','));
        var stsub = parseFloat($("#Details_" + x + "_Amount").text().replaceAll(',', '').replaceAll('.', ','));
        total += st;
        totalTax += sttax;
        subTotal += stsub;
    }

    var strTotal = total.toLocaleString().replaceAll('.', ',');
    var strTotalTax = totalTax.toLocaleString().replaceAll('.', ',');
    var strTotalSub = subTotal.toLocaleString().replaceAll('.', ',');
    $('#txtGridPPN').text(strTotalTax);
    $('#txtGridTotal').text(strTotalSub);
    $('#txtGridGrandTotal').text(strTotal);
}

function recalcInvItemTotals() {
    var rowCount = $('#ItemGrid tbody tr').length;
    
    var total = 0;
    var totalTax = 0;
    var subTotal = 0;
    for (x = 0; x < rowCount; x++) {
        var sttax = parseFloat($("#ItemDetails_" + x + "_TotalPPN").text().replaceAll(',', '').replaceAll('.', ','));
        var stsub = parseFloat($("#ItemDetails_" + x + "_TotalAmount").text().replaceAll(',', '').replaceAll('.', ','));
        totalTax += sttax;
        subTotal += stsub;
    }
    total = subTotal + totalTax;

    var strTotal = total.toLocaleString().replaceAll('.', ',');
    var strTotalTax = totalTax.toLocaleString().replaceAll('.', ',');
    var strTotalSub = subTotal.toLocaleString().replaceAll('.', ',');
    $('#txtGridPPNItem').text(strTotalTax);
    $('#txtGridTotalItem').text(strTotalSub);
    $('#txtGridGrandTotalItem').text(strTotal);
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

    recalcInvTotals();
    recalcInvItemTotals();
}