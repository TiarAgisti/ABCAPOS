function isAutoApplyChange(index) {
    var isAutoApply = $("#Details_" + index + "_isAutoApply").is(':checked');
    //var qty = $("#Details_" + index + "_QtyHidden").val();
    var amountCheckedTotal = parseFloat($('#hdnAmountChecked').val().replaceAll(',', ''));
    if (isAutoApply == true) {
        var totalPayment = parseFloat($('#txtCreditRemaining').val().replaceAll(',', ''));
        var amountDue = parseFloat($("#Details_" + index + "_AmountDue").text().replaceAll(',', ''));
        var selisih = totalPayment - amountCheckedTotal;
        //alert(totalPayment + ' ' + amountCheckedTotal);
        //alert('amountDue:' + amountDue + ', selisih:' + selisih);
        if (selisih < amountDue) {
            $("#Details_" + index + "_Amount").val(selisih);
            $('#hdnAmountChecked').val(amountCheckedTotal + selisih);
        }
        else {
            $("#Details_" + index + "_Amount").val(amountDue);
            $('#hdnAmountChecked').val(amountCheckedTotal + amountDue);
        }
        //$("#Details_" + index + "_StrQuantity").val(qty);
    }
    else {
        var amountUnchecked = $("#Details_" + index + "_Amount").val();
        $("#Details_" + index + "_Amount").val("0.00");
        $('#hdnAmountChecked').val(amountCheckedTotal - amountUnchecked);
    }

}