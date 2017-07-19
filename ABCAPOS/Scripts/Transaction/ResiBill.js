String.prototype.padLeft = function (paddingChar, length) {

    var s = new String(this);

    if ((this.length < length) && (paddingChar.toString().length > 0)) {
        for (var i = 0; i < (length - this.length) ; i++)
            s = paddingChar.toString().charAt(0).concat(s);
    }

    return s;
};
function onchangeDate() {
    var terms = $('#TermOfPaymentID').val();
    $.ajax({
        type: "POST",
        async: false,
        url: "/Webservice.asmx/RetreiveTermsOfPayment",
        data: "{topID: " + terms + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var top = (data.d);
            var valueAdd = (top.Terms);
            console.log("Terms = " + top.Terms);
            var dateStr = $("#txtDate").val().split("/");
            var date = new Date(dateStr[2], dateStr[0] - 1, dateStr[1]);

            date.setDate(date.getDate() + valueAdd);
            var Month = date.getMonth() + 1;
            var futDate = Month.toString().padLeft("0", 2) + "/" + date.getDate().toString().padLeft("0", 2) + "/" + date.getFullYear();
            $('#txtDueDate').val(futDate);
        }
    });
    
}

function recalcTotals() {
    var rowCount = $('#Grid1 tbody tr').length;

    var total = 0;

    for (x = 0; x < rowCount; x++) {
        var st = parseFloat($("#Details_" + x + "_Amount").text().replaceAll(',', ''));
        console.log("Amount=" + st);
        total += st;
    }

    var strTotal = thousandSeparator(total.toFixed(2));
    $('#txtGridGrandTotal').text(strTotal);
}

function deleteRow(deleteButtonID, dataContainerName) {
    var button = $("#" + deleteButtonID);

    var deletedIndex = button.parent().parent().index();

    var gridID = button.parent().parent().parent().parent().attr('id');

    var noOfRows = $("#" + gridID + " tbody tr").length;

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
    recalcTotals();
}

function AddUrlResi() {
    var mode = $("#hdnMode").val();
    console.log("mode = " + mode);
    //if (mode == "Detail" || mode == "Create") {
    var noOfRows = $("#Grid1 tbody tr").length;
    for (index = 0; index < noOfRows; index++) {
        var ID = $("#Details_" + index + "_ResiID").val();
        console.log("ID = " + ID);
        var Code = $("#Details_" + index + "_ResiCode").text();

        var Link = "/Resi/Detail?key=" + ID;
        $("#Details_" + index + "_ResiCode").replaceWith('<a href="' + Link + '">' + Code + '</a>');
    }
}

function AddUrlResiPayment() {
    var noOfRows = $("#paymentGrid tbody tr").length;

    for (x = 0; x < noOfRows; x++) {
        var paymentID = $("#paymentDetails_" + x + "_HeaderID").val();
        var paymentCode = $("#paymentDetails_" + x + "_PaymentCode").text();
        var paymentLink = "/ResiPayment/Detail?key=" + paymentID;

        $("#paymentDetails_" + x + "_PaymentCode").replaceWith('<a href="' + paymentLink + '" style="color: #505abc;">' + paymentCode + '</a>');
    }
}
