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

    setGridButtonVisivility();
});

function checkCode(id) {
    var code = $("#txtCode").val();
    console.log("CheckCode called for: " + code + " " + id);
    $.ajax({
        type: "POST",
        async: false,
        url: "/Webservice.asmx/CheckResiPaymentCode",
        data: JSON.stringify({ resipaymentCode: code }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var exists = (data.d);
            console.log("ajax called");
            if (exists) {
                alert("Warning: " + code + " already exists");
                $("#txtCode").val('');
            }
        }
    });
}

function setDeleteRowBugFix() {
    var noOfRows = $("#Grid1 tbody tr").length;

    for (x = 0; x < noOfRows; x++) {
        $('#btnDeleteDetails_' + x).attr('onclick', "deleteRow('btnDeleteDetails_2', 'Details');");
        //getStockQtyHidden(x);
    }
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

function adjustPaymentLineResi(index) {
    var amountDue = parseFloat($("#ResiDetails_" + index + "_AmountDue").val().replaceAll(',', ''));
    console.log("amountDueResi = " + amountDue);
    var discount = parseFloat($("#ResiDetails_" + index + "_DiscountTaken").val().replaceAll(',', ''));
    console.log("Discount Taken = " + discount);
    var payment = parseFloat($("#ResiDetails_" + index + "_AmountStr").val().replaceAll(',', ''));
    console.log("payment = " + payment);

    payment = amountDue - discount;
    console.log("new payment =" + payment);

    var paymentStr = payment.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,");
    $("#ResiDetails_" + index + "_AmountStr").val(paymentStr);

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

function adjustDiscountLineResi(index) {
    var amountDue = parseFloat($("#ResiDetails_" + index + "_AmountDue").val().replaceAll(',', '').replaceAll('.', ','));
    var discount = parseFloat($("#ResiDetails_" + index + "_DiscountTaken").val().replaceAll(',', '').replaceAll('.', ','));
    var payment = parseFloat($("#ResiDetails_" + index + "_Amount").val().replaceAll(',', '').replaceAll('.', ','));

    if (payment + discount > amountDue) {
        discount = amountDue - payment;
    }

    var discountStr = discount.toLocaleString('en-US');
    $("#ResiDetails_" + index + "_DiscountTaken").val(discount);

    calcPaymentAmount(index);
}

function calcPaymentAmount(index) {

    var rowCount = $('#Grid1 tbody tr').length;
    var rowResiCount = $('#gridResi tbody tr').length;
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

    for (i = 0; i < rowResiCount; i++) {
        var stResi = parseFloat($("#ResiDetails_" + i + "_AmountStr").val().replaceAll(',', ''));
        if (isNaN(stResi)) {
            stResi = 0;
        }

        total += stResi;
    }

    var strTotal = total.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,");
    $('#txtAmountHelp').val(strTotal);
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
}