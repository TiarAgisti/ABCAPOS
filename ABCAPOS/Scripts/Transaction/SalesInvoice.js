
function onSelectEmployee(data) {
    $("#hdnEmployeeIDAsDireksi").val(data.ID);
    $("#txtEmployeeNameAsDireksi").val(data.Name);
}

function calcAmount(index) {
    var rowCount = $('#Grid1 tbody tr').length;
    var total = 0;
    for (x = 0; x < rowCount; x++) {
        var taxType = $("#Details_" + x + "_TaxType").val();
        var st = parseFloat($("#Details_" + x + "_GrossAmount").val());
        if (taxType == 2) {
            var totAmount = st / 1.1;
            var totPPN = st - totAmount;

            $("#Details_" + x + "_TotalAmount").text(totAmount.toFixed(2).toLocaleString('en-US'));
            $("#Details_" + x + "_TotalPPN").text(totPPN.toFixed(2).toLocaleString('en-US'));
        }
        else {
            var totPPN = 0;
            $("#Details_" + x + "_TotalAmount").text(st.toFixed(2).toLocaleString('en-US'));
            $("#Details_" + x + "_TotalPPN").text(totPPN.toFixed(2).toLocaleString('en-US'));
        }
        total += st;
    }
    
    var strTotal = total.toLocaleString('en-US');
    $('#txtGridTotal').text(strTotal);
}
    
function calc(index) {
    
    var price = parseFloat($("#Details_" + index + "_Price").val());
    console.log("price= " + price);

    var qty = parseFloat($("#Details_" + index + "_StrQuantity").val());
    console.log("qty= " + qty);
    
    var taxType = $("#Details_" + index + "_TaxType").val();

    if (isNaN(price))
        price = 0;

    if (isNaN(qty))
        qty = 0;

    if (isNaN(taxType))
        taxType = 0;

    var totalAmount = qty * price;
    $("#Details_" + index + "_TotalAmount").text(totalAmount.toFixed(2).toLocaleString('en-US'));
    
    var totalPPN = 0;
    if (taxType == 2)
        totalPPN = totalAmount * 0.1;

    $("#Details_" + index + "_TotalPPN").text(totalPPN.toFixed(2).toLocaleString('en-US'));
    var total = totalAmount + totalPPN;

    var strTotal = total.toFixed(2).toLocaleString('en-US');
    console.log(strTotal);

    $("#Details_" + index + "_GrossAmount").val(strTotal);

    var rowCount = $('#Grid1 tbody tr').length;

    var total = 0;
    for (x = 0; x < rowCount; x++) {
        var st = parseFloat($("#Details_" + x + "_GrossAmount").val());
        total += st;
    }

    var strTotal = total.toLocaleString('en-US');
    $('#txtGridTotal').text(strTotal);

    recalcTotals();
}

function recalcTotals() {
    var rowCount = $('#Grid1 tbody tr').length;
    var shipping = parseFloat($('#txtGridShipping').text().replaceAll(',', ''));
    var GrandTotal = 0;

    var total = 0;
    var totalTax = 0;
    var subTotal = 0;

    if(isNaN(shipping))
        shipping = 0;

    for (x = 0; x < rowCount; x++) {
        var st = parseFloat($("#Details_" + x + "_GrossAmount").val());
        var sttax = parseFloat($("#Details_" + x + "_TotalPPN").text().replaceAll(',', ''));
        var stsub = parseFloat($("#Details_" + x + "_TotalAmount").text().replaceAll(',', ''));
        total += st;
        totalTax += sttax;
        subTotal += stsub;
        //alert(subTotal + ' ' + totalTax + ' ' + total);
    }

    GrandTotal = total + shipping;
    console.log(GrandTotal);

    var strTotal = thousandSeparator(GrandTotal.toFixed(2));
    console.log("GrandTotal="+strTotal);

    //var strGrandTotal = strTotal.toLocaleString('en-US');
    //console.log("strGrandTotal=" + strGrandTotal);

    var strTotalTax = totalTax.toLocaleString('en-US');
    console.log(strTotalTax);

    var strTotalSub = subTotal.toLocaleString('en-US');
  

    $('#txtGridGrandTotal').text(strTotal);
    $('#txtGridPPN').text(strTotalTax);
    $('#txtGridTotal').text(strTotalSub);
}

function AddUrlResi() {
    var noOfRows = $("#resiGrid tbody tr").length;
    for (index = 0; index < noOfRows; index++) {
        var ID = $("#ResiDetails_" + index + "_ResiID").val();
        var Code = $("#ResiDetails_" + index + "_ResiCode").text();

        var Link = "/Resi/Detail?key=" + ID;
        $("#ResiDetails_" + index + "_ResiCode").replaceWith('<a href="' + Link + '">' + Code + '</a>');
    }
}

function reversecalc(index) {
    var totalGross = $("#Details_" + index + "_GrossAmount").val();
    console.log("totalGross= " + totalGross);
    
    var qty = parseFloat($("#Details_" + index + "_StrQuantity").val().replaceAll(',', '.'));
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
        totalPPN = 0;

    var totalAmount = totalGross - totalPPN;
    price = totalAmount / qty;

    /*total = amount + ppn
    ppn = amount * 0.1
    total = amount + amount * 0.1
    total = 1.1(amount)
    amount = total / 1.1
    ppn = total / 1.1 * 0.1
    */
    //alert(price + ' ' + totalAmount + ' ' + totalPPN);
    $("#Details_" + index + "_TotalAmount").text(totalAmount.toFixed(2).toLocaleString('en-us'));
    $("#Details_" + index + "_TotalPPN").text(totalPPN.toFixed(2).toLocaleString('en-us'));
    $("#Details_" + index + "_Price").text(price);
    //alert(price);
    recalcTotals();
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
    //calc(0);
    recalcTotals();
}