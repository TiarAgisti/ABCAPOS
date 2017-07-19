String.prototype.padLeft = function (paddingChar, length) {

    var s = new String(this);

    if ((this.length < length) && (paddingChar.toString().length > 0)) {
        for (var i = 0; i < (length - this.length) ; i++)
            s = paddingChar.toString().charAt(0).concat(s);
    }

    return s;
};
function onchangeDate() {
    var terms = $('#TermOfPayment').val();
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
    //var dateStr = $("#txtDate").val().split("/");
    //var date = new Date(dateStr[2], dateStr[0] - 1, dateStr[1]);

    //var valueAdd = 0;

    //if (terms == 2)
    //    valueAdd = 15;
    //else if (terms == 3)
    //    valueAdd = 30;
    //else if (terms == 4)
    //    valueAdd = 60;
    //else if (terms == 5)
    //    valueAdd = 90;
    //else if (terms == 6)
    //    valueAdd = 14;
    //else if (terms == 7)
    //    valueAdd = 45;
    //else if (terms == 8)
    //    valueAdd = 75;
    //else
    //    valueAdd = 0;

    //date.setDate(date.getDate() + valueAdd);
    //var Month = date.getMonth() + 1;
    //var futDate = Month.toString().padLeft("0", 2) + "/" + date.getDate().toString().padLeft("0", 2) + "/" + date.getFullYear();
    ////alert(futDate);
    //$('#txtDueDate').val(futDate);

}

function checkCode(id) {
    var code = $("#txtCode").val();
    console.log("checkCode called for: " + code + " " + id);
    $.ajax({
        type: "POST",
        async: false,
        url: "/Webservice.asmx/CheckPurchaseBillCode",
        data: JSON.stringify({ billCode: code, ID: id }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var exists = (data.d);
            console.log("ajax called");
            if (exists) {
                alert("Warning: " + code + " already exists!");
                $("#txtCode").val('');
            }
        }
    });
}


function calc(index) {
    var assetPrice = parseFloat($("#hdn_AssetPrice_" + index).val());
    var discount = parseFloat($("#hdn_Discount_" + index).val());
    console.log("price= " + assetPrice);

    var qty = parseFloat($("#Details_" + index + "_StrQuantity").val().replaceAll(',', ''));
    //var qtyPD = parseFloat($("#Details_"+ index + "_CreatedPDQuantity").text().replaceAll(',', ''));
    console.log("qty= " + qty);

    var taxType = $("#Details_" + index + "_TaxType").val();

    if (isNaN(assetPrice))
        assetPrice = 0;
    if (isNaN(discount))
        discount = 0;

    if (isNaN(qty))
        qty = 0;

    if (isNaN(taxType))
        taxType = 0;

    //if (qty > qtyPD)
    //    $("#Details_" + index + "_StrQuantity").val(qtyPD);
        


    var totalAmount = qty * assetPrice;
    //alert(totalAmount.toLocaleString());
    $("#Details_" + index + "_TotalAmount").text(totalAmount.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,"));

    var totalPPN = 0;

    if (taxType == 2)
        totalPPN = (totalAmount - discount) * 0.1;
    else
        totalPPN = 0;

    $("#Details_" + index + "_TotalPPN").text(totalPPN.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,"));

    var total = (totalAmount - discount) + totalPPN;

    var strTotal = total.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,");
    console.log(strTotal);

    $("#Details_" + index + "_Total").text(strTotal);
    $("#hdn_Total_" + index).val(strTotal);

    var rowCount = $('#Grid1 tbody tr').length;

    var total2 = 0;
    var totaldisc = 0;
    var totalTax = 0;
    var subTotal = 0;
    for (x = 0; x < rowCount; x++) {
        var st = parseFloat($("#Details_" + x + "_Total").text().replaceAll(',', ''));
        var sdisc = parseFloat($("#Details_" + x + "_Discount").text().replaceAll(',', ''))
        var sttax = parseFloat($("#Details_" + x + "_TotalPPN").text().replaceAll(',', ''));
        var stsub = parseFloat($("#Details_" + x + "_TotalAmount").text().replaceAll(',', ''));
        total2 += st;
        totaldisc += sdisc;
        totalTax += sttax;
        subTotal += stsub;
    }
    var strTotal2 = total.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,");
    var strDisc = totaldisc.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,")
    var strTotalTax = totalTax.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,");
    var strTotalSub = subTotal.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,");
    //$('#txtGridGrandTotal').text(strTotal2);
    $('#txtGridDiscount').text(strDisc);
    $('#txtGridPPN').text(strTotalTax);
    $('#txtGridTotal').text(strTotalSub);
    /**/
    $('#txtGridGrandTotal').text((parseFloat(strTotalSub.replaceAll(",", "")) - parseFloat(strDisc.replaceAll(",", "")) + parseFloat(strTotalTax.replaceAll(",", ""))).toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,"));
}

function reversecalc(index) {
    var totalGross = $("#hdn_Total_"+ index).val().replaceAll(',', '');
    console.log("totalGross= " + totalGross);

    var qty = parseFloat($("#Details_" + index + "_StrQuantity").val().replaceAll(',', ''));
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

    $("#Details_" + index + "_TotalAmount").text(totalAmount.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,"));
    $("#Details_" + index + "_TotalPPN").text(totalPPN.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,"));
    $("#hdn_AssetPrice_"+ index).val(price);

    var rowCount = $('#Grid1 tbody tr').length;

    var total = 0;
    var totalTax = 0;
    var subTotal = 0;
    for (x = 0; x < rowCount; x++) {
        var st = parseFloat($("#hdn_Total_" + x ).val().replaceAll(',', ''));
        var sttax = parseFloat($("#Details_" + x + "_TotalPPN").text().replaceAll(',', ''));
        var stsub = parseFloat($("#Details_" + x + "_TotalAmount").text().replaceAll(',', ''));
        total += st;
        totalTax += sttax;
        subTotal += stsub;
    }
    var strTotal = total.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,");
    var strTotalTax = totalTax.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,");
    var strTotalSub = subTotal.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,");
    $('#txtGridGrandTotal').text(strTotal);
    $('#txtGridPPN').text(strTotalTax);
    $('#txtGridTotal').text(strTotalSub);
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
                console.log(oriName);

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
    calc(0);
}