$(function () {
    if ($("#hdnMode").val() == "Create") {
        onChangeVenue();
    }
    else if ($("#hdnMode").val() == "Update" || $("#hdnMode").val() == "Detail") {
        //        var splitDP = $("#rbSplitDP_0").is(':checked');

        if ($("#hdnMode").val() == "Update") {
            var noOfDP = $("#NoOfDP").val();
        }
        else if ($("#hdnMode").val() == "Detail") {
            var noOfDP = $("#hdnNoOfDP").val();
        }

        var taxType = 2;

        if ($("#rbTaxType_0").is(':checked'))
            taxType = 2;
        else if ($("#rbTaxType_1").is(':checked'))
            taxType = 1;
                    
        if (taxType == 1)
            setVisible(false, noOfDP);
        else
            setVisible(true, noOfDP);
    }

    loadHall();
});

function setVisible(isShowTax, noOfDP) {
    if (isShowTax) {
        $("#trTotalBeforeTax").show();
        $("#trTax").show();
    }
    else {
        $("#trTotalBeforeTax").hide();
        $("#trTax").hide();
    }

    $("#lblDPIAmount").parent().parent().hide();
    $("#txtDPIAmount").parent().parent().hide();

    $("#lblDPIIAmount").parent().parent().hide();
    $("#txtDPIIAmount").parent().parent().hide();

    $("#lblDPIIIAmount").parent().parent().hide();
    $("#txtDPIIIAmount").parent().parent().hide();

    $("#lblDPIVAmount").parent().parent().hide();
    $("#txtDPIVAmount").parent().parent().hide();

    $("#lblDPVAmount").parent().parent().hide();
    $("#txtDPVAmount").parent().parent().hide();

    $("#lblDPVIAmount").parent().parent().hide();
    $("#txtDPVIAmount").parent().parent().hide();

    if (noOfDP > 0) {
        $("#lblDPIAmount").parent().parent().show();
        $("#txtDPIAmount").parent().parent().show();
    }

    if (noOfDP > 1) {
        $("#lblDPIIAmount").parent().parent().show();
        $("#txtDPIIAmount").parent().parent().show();
    }

    if (noOfDP > 2) {
        $("#lblDPIIIAmount").parent().parent().show();
        $("#txtDPIIIAmount").parent().parent().show();
    }

    if (noOfDP > 3) {
        $("#lblDPIVAmount").parent().parent().show();
        $("#txtDPIVAmount").parent().parent().show();
    }

    if (noOfDP > 4) {
        $("#lblDPVAmount").parent().parent().show();
        $("#txtDPVAmount").parent().parent().show();
    }

    if (noOfDP > 5) {
        $("#lblDPVIAmount").parent().parent().show();
        $("#txtDPVIAmount").parent().parent().show();
    }
}

//function initializePrice() {
//    //    var splitDP = $("#rbSplitDP_0").is(':checked');
//    var eventDate = $("#txtEventStartDate").val();
//    var bookingFeeDueDate = $("#txtBookingFeeDueDate").val();
//    var discountValue = $("#txtDiscountValue").val();
//    var discountPercentage = $("#txtDiscountPercentage").val();
//    var noOfDP = $("#NoOfDP").val();
//    var venuePrice = $("#hdnVenuePrice").val();
//    var venueDiscountPercentage = $("#txtVenueDiscountPercentage").val();
//    var venueDiscountValue = $("#txtVenueDiscountValue").val();
//    var buffetQty = $("#txtPaxQty").val();
//    var sittingBuffetQty = $("#txtSittingBuffetQty").val();
//    var buffetPrice = $("#txtBuffet").val();
//    var sittingBuffetPrice = $("#txtSittingBuffet").val();
//    var foodStallPrice = $("#txtFoodStall").val();
//    var decorationPrice = $("#txtDecoration").val();
//    var entertainmentPrice = $("#txtEntertainment").val();

//    var taxType = 1;

//    if ($("#rbTaxType_0").is(':checked'))
//        taxType = 2;
//    else if ($("#rbTaxType_2").is(':checked'))
//        taxType = 3;

//    if (taxType == 1)
//        setVisible(false, noOfDP);
//    else
//        setVisible(true, noOfDP);

//    $.ajax({
//        url: "/Service.asmx/InitializeMiceBookingAmount",
//        data: "{ 'eventDate': '" + eventDate + "', 'discountValue': '" + discountValue + "', 'discountPercentage': '" + discountPercentage + "', 'bookingFeeDueDate': '" + bookingFeeDueDate + "', 'noOfDP': '" + noOfDP + "', 'taxType': '" + taxType + "', 'venuePrice': '" + venuePrice + "', 'venueDiscountPercentage': '" + venueDiscountPercentage + "', 'venueDiscountValue': '" + venueDiscountValue + "', 'buffetQty': '" + buffetQty + "', 'sittingBuffetQty': '" + sittingBuffetQty + "', 'buffetPrice': '" + buffetPrice + "', 'sittingBuffetPrice': '" + sittingBuffetPrice + "', 'foodStallPrice': '" + foodStallPrice + "', 'decorationPrice': '" + decorationPrice + "', 'entertainmentPrice': '" + entertainmentPrice + "' }",
//        dataType: "json",
//        type: "POST",
//        contentType: "application/json; charset=utf-8",
//        dataFilter: function (data) { return data; },
//        success: function (data) {

//            var settlementDueDate = new Date(parseInt(data.d.SettlementDueDate.substr(6)));

//            $("#hdnVenuePrice").val(data.d.VenuePrice);
//            $("#lblVenuePrice").text(data.d.VenuePrice);
//            $("#lblVenuePrice").formatNumber({ format: "#,###.00", locale: "id" });

//            $("#hdnPackagePrice").val(data.d.Price);
//            $("#lblPackagePrice").text(data.d.Price);
//            $("#lblPackagePrice").formatNumber({ format: "#,###.00", locale: "id" });

//            $("#lblTotalDiscount").text(data.d.TotalDiscount);
//            $("#lblTotalDiscount").formatNumber({ format: "#,###.00", locale: "id" });
//            $("#lblGrandTotal").text(data.d.GrandTotal);
//            $("#lblGrandTotal").formatNumber({ format: "#,###.00", locale: "id" });
//            $("#lblServiceChargeValue").text(data.d.ServiceChargeValue);
//            $("#lblServiceChargeValue").formatNumber({ format: "#,###.00", locale: "id" });
//            $("#hdnServiceCharge").val(data.d.ServiceCharge);
//            $("#lblTaxValue").text(data.d.TaxValue);
//            $("#lblTaxValue").formatNumber({ format: "#,###.00", locale: "id" });
//            $("#lblGrandTotalBeforeTax").text(data.d.GrandTotalBeforeTax);
//            $("#lblGrandTotalBeforeTax").formatNumber({ format: "#,###.00", locale: "id" });

//            $("#hdnSettlementDueDate").val(settlementDueDate.getDate() + '/' + (settlementDueDate.getMonth() + 1) + "/" + settlementDueDate.getFullYear());
//            $("#txtSettlementDueDate").val(settlementDueDate.getDate() + '/' + (settlementDueDate.getMonth() + 1) + "/" + settlementDueDate.getFullYear());
//            $("#lblSettlementDueDate").text(settlementDueDate.getDate() + '/' + (settlementDueDate.getMonth() + 1) + "/" + settlementDueDate.getFullYear());
//            $("#txtSettlementAmount").val(data.d.SettlementAmount);
//            $("#lblSettlementAmount").text(data.d.SettlementAmount);

//            if (noOfDP > 0) {
//                var dpIDueDate = new Date(parseInt(data.d.DPIDueDate.substr(6)));
//                $("#txtDPIAmount").val(data.d.DPIAmount);
//                $("#hdnDPIAmount").val(data.d.DPIAmount);
//                $("#txtDPIDueDate").val(dpIDueDate.getDate() + '/' + (dpIDueDate.getMonth() + 1) + "/" + dpIDueDate.getFullYear());
//            }

//            if (noOfDP > 1) {
//                var dpIIDueDate = new Date(parseInt(data.d.DPIIDueDate.substr(6)));
//                $("#txtDPIIAmount").val(data.d.DPIIAmount);
//                $("#hdnDPIIAmount").val(data.d.DPIIAmount);
//                $("#txtDPIIDueDate").val(dpIIDueDate.getDate() + '/' + (dpIIDueDate.getMonth() + 1) + "/" + dpIIDueDate.getFullYear());
//            }
//            else if (noOfDP > 2) {
//                var dpIIIDueDate = new Date(parseInt(data.d.DPIIIDueDate.substr(6)));
//                $("#txtDPIIIAmount").val(data.d.DPIIIAmount);
//                $("#hdnDPIIIAmount").val(data.d.DPIIIAmount);
//                $("#txtDPIIIDueDate").val(dpIIIDueDate.getDate() + '/' + (dpIIIDueDate.getMonth() + 1) + "/" + dpIIIDueDate.getFullYear());
//            }

//            if (noOfDP > 3) {
//                var dpIVDueDate = new Date(parseInt(data.d.DPIVDueDate.substr(6)));
//                $("#txtDPIVAmount").val(data.d.DPIVAmount);
//                $("#hdnDPIVAmount").val(data.d.DPIVAmount);
//                $("#txtDPIVDueDate").val(dpIVDueDate.getDate() + '/' + (dpIVDueDate.getMonth() + 1) + "/" + dpIVDueDate.getFullYear());
//            }

//            if (noOfDP > 4) {
//                var dpVDueDate = new Date(parseInt(data.d.DPVDueDate.substr(6)));
//                $("#txtDPVAmount").val(data.d.DPVAmount);
//                $("#hdnDPVAmount").val(data.d.DPVAmount);
//                $("#txtDPVDueDate").val(dpVDueDate.getDate() + '/' + (dpVDueDate.getMonth() + 1) + "/" + dpVDueDate.getFullYear());
//            }

//            if (noOfDP > 5) {
//                var dpVIDueDate = new Date(parseInt(data.d.DPVIDueDate.substr(6)));
//                $("#txtDPVIAmount").val(data.d.DPVIAmount);
//                $("#hdnDPVIAmount").val(data.d.DPVIAmount);
//                $("#txtDPVIDueDate").val(dpVIDueDate.getDate() + '/' + (dpVIDueDate.getMonth() + 1) + "/" + dpVIDueDate.getFullYear());
//            }
//        }
//    });
//}

function onChangePrice() {
    var eventDate = $("#txtEventStartDate").val();
    var bookingFeeDueDate = $("#txtBookingFeeDueDate").val();
    var discountValue = $("#txtDiscountValue").val();
    var discountPercentage = $("#txtDiscountPercentage").val();
    var refundableDeposit = $("#txtRefundableDeposit").val();
    var useServiceCharge = $("#rbUseServiceCharge_0").is(':checked');
    var bookingFeePrice = $("#txtBookingFee").val();
    var noOfDP = $("#NoOfDP").val();
    var venueID = $("#VenueID").val();
    var venueDiscountPercentage = $("#txtVenueDiscountPercentage").val();
    var venueDiscountValue = $("#txtVenueDiscountValue").val();
    var buffetQty = $("#txtPaxQty").val();
    var sittingBuffetQty = $("#txtSittingBuffetQty").val();
    var buffetPrice = $("#txtBuffet").val();
    var sittingBuffetPrice = $("#txtSittingBuffet").val();
    var foodStallPrice = $("#txtFoodStall").val();
    var decorationPrice = $("#txtDecoration").val();
    var entertainmentPrice = $("#txtEntertainment").val();

    var taxType = 2;

    if ($("#rbTaxType_0").is(':checked'))
        taxType = 2;
    else if ($("#rbTaxType_1").is(':checked'))
        taxType = 1;

    if (taxType == 1)
        setVisible(false, noOfDP);
    else
        setVisible(true, noOfDP);

    $.ajax({
        url: "/Service.asmx/CountMiceBookingAmount",
        data: "{ 'eventDate': '" + eventDate + "', 'discountValue': '" + discountValue + "', 'discountPercentage': '" + discountPercentage + "', 'refundableDeposit': '" + refundableDeposit + "', 'bookingFeeDueDate': '" + bookingFeeDueDate + "', 'bookingFeePrice': '" + bookingFeePrice + "', 'useServiceCharge': '" + useServiceCharge + "', 'noOfDP': '" + noOfDP + "', 'taxType': '" + taxType + "', 'venueID': '" + venueID + "', 'venueDiscountPercentage': '" + venueDiscountPercentage + "', 'venueDiscountValue': '" + venueDiscountValue + "', 'buffetQty': '" + buffetQty + "', 'sittingBuffetQty': '" + sittingBuffetQty + "', 'buffetPrice': '" + buffetPrice + "', 'sittingBuffetPrice': '" + sittingBuffetPrice + "', 'foodStallPrice': '" + foodStallPrice + "', 'decorationPrice': '" + decorationPrice + "', 'entertainmentPrice': '" + entertainmentPrice + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataFilter: function (data) { return data; },
        success: function (data) {
            var settlementDueDate = new Date(parseInt(data.d.SettlementDueDate.substr(6)));

            $("#hdnVenuePrice").val(data.d.VenuePrice);
            $("#lblVenuePrice").text(data.d.VenuePrice);
            $("#lblVenuePrice").formatNumber({ format: "#,###.00", locale: "id" });

            $("#hdnPackagePrice").val(data.d.Price);
            $("#lblPackagePrice").text(data.d.Price);
            $("#lblPackagePrice").formatNumber({ format: "#,###.00", locale: "id" });

            $("#lblTotalDiscount").text(data.d.TotalDiscount);
            $("#lblTotalDiscount").formatNumber({ format: "#,###.00", locale: "id" });
            $("#lblGrandTotal").text(data.d.GrandTotal);
            $("#lblGrandTotal").formatNumber({ format: "#,###.00", locale: "id" });
            $("#lblServiceChargeValue").text(data.d.ServiceChargeValue);
            $("#lblServiceChargeValue").formatNumber({ format: "#,###.00", locale: "id" });
            $("#hdnServiceCharge").val(data.d.ServiceCharge);
            $("#lblTaxValue").text(data.d.TaxValue);
            $("#lblTaxValue").formatNumber({ format: "#,###.00", locale: "id" });
            $("#lblGrandTotalBeforeTax").text(data.d.GrandTotalBeforeTax);
            $("#lblGrandTotalBeforeTax").formatNumber({ format: "#,###.00", locale: "id" });

            $("#hdnSettlementDueDate").val(settlementDueDate.getDate() + '/' + (settlementDueDate.getMonth() + 1) + "/" + settlementDueDate.getFullYear());
            $("#txtSettlementDueDate").val(settlementDueDate.getDate() + '/' + (settlementDueDate.getMonth() + 1) + "/" + settlementDueDate.getFullYear());
            $("#lblSettlementDueDate").text(settlementDueDate.getDate() + '/' + (settlementDueDate.getMonth() + 1) + "/" + settlementDueDate.getFullYear());
            $("#txtSettlementAmount").val(data.d.SettlementAmount);
            $("#lblSettlementAmount").text(data.d.SettlementAmount);

            if (noOfDP > 0) {
                var dpIDueDate = new Date(parseInt(data.d.DPIDueDate.substr(6)));
                $("#txtDPIAmount").val(data.d.DPIAmount);
                $("#hdnDPIAmount").val(data.d.DPIAmount);
                $("#txtDPIDueDate").val(dpIDueDate.getDate() + '/' + (dpIDueDate.getMonth() + 1) + "/" + dpIDueDate.getFullYear());
            }

            if (noOfDP > 1) {
                var dpIIDueDate = new Date(parseInt(data.d.DPIIDueDate.substr(6)));
                $("#txtDPIIAmount").val(data.d.DPIIAmount);
                $("#hdnDPIIAmount").val(data.d.DPIIAmount);
                $("#txtDPIIDueDate").val(dpIIDueDate.getDate() + '/' + (dpIIDueDate.getMonth() + 1) + "/" + dpIIDueDate.getFullYear());
            }

            if (noOfDP > 2) {
                var dpIIIDueDate = new Date(parseInt(data.d.DPIIIDueDate.substr(6)));
                $("#txtDPIIIAmount").val(data.d.DPIIIAmount);
                $("#hdnDPIIIAmount").val(data.d.DPIIIAmount);
                $("#txtDPIIIDueDate").val(dpIIIDueDate.getDate() + '/' + (dpIIIDueDate.getMonth() + 1) + "/" + dpIIIDueDate.getFullYear());
            }

            if (noOfDP > 3) {
                var dpIVDueDate = new Date(parseInt(data.d.DPIVDueDate.substr(6)));
                $("#txtDPIVAmount").val(data.d.DPIVAmount);
                $("#hdnDPIVAmount").val(data.d.DPIVAmount);
                $("#txtDPIVDueDate").val(dpIVDueDate.getDate() + '/' + (dpIVDueDate.getMonth() + 1) + "/" + dpIVDueDate.getFullYear());
            }

            if (noOfDP > 4) {
                var dpVDueDate = new Date(parseInt(data.d.DPVDueDate.substr(6)));
                $("#txtDPVAmount").val(data.d.DPVAmount);
                $("#hdnDPVAmount").val(data.d.DPVAmount);
                $("#txtDPVDueDate").val(dpVDueDate.getDate() + '/' + (dpVDueDate.getMonth() + 1) + "/" + dpVDueDate.getFullYear());
            }

            if (noOfDP > 5) {
                var dpVIDueDate = new Date(parseInt(data.d.DPVIDueDate.substr(6)));
                $("#txtDPVIAmount").val(data.d.DPVIAmount);
                $("#hdnDPVIAmount").val(data.d.DPVIAmount);
                $("#txtDPVIDueDate").val(dpVIDueDate.getDate() + '/' + (dpVIDueDate.getMonth() + 1) + "/" + dpVIDueDate.getFullYear());
            }

            $("#Grid1Template").val(data.d.MiceBookingDetailTemplate);
        }
    });
}

function updatePrice() {
    var eventDate = $("#txtEventStartDate").val();
    var bookingFee = $("#txtBookingFee").val();
    var bookingFeeDueDate = $("#txtBookingFeeDueDate").val();
    var discountValue = $("#txtDiscountValue").val();
    var discountPercentage = $("#txtDiscountPercentage").val();
    var refundableDeposit = $("#txtRefundableDeposit").val();
    var serviceChargePercentage = $("#hdnServiceCharge").val();
    var noOfDP = $("#NoOfDP").val();
    var venuePrice = $("#hdnVenuePrice").val();
    var venueDiscountPercentage = $("#txtVenueDiscountPercentage").val();
    var venueDiscountValue = $("#txtVenueDiscountValue").val();
    var buffetQty = $("#txtPaxQty").val();
    var sittingBuffetQty = $("#txtSittingBuffetQty").val();
    var buffetPrice = $("#txtBuffet").val();
    var sittingBuffetPrice = $("#txtSittingBuffet").val();
    var foodStallPrice = $("#txtFoodStall").val();
    var decorationPrice = $("#txtDecoration").val();
    var entertainmentPrice = $("#txtEntertainment").val();

    var taxType = 1;

    if ($("#rbTaxType_0").is(':checked'))
        taxType = 2;
    else if ($("#rbTaxType_2").is(':checked'))
        taxType = 3;

    if (taxType == 1)
        setVisible(false, noOfDP);
    else
        setVisible(true, noOfDP);

    $.ajax({
        url: "/Service.asmx/UpdateMiceBookingAmount",
        data: "{ 'eventDate': '" + eventDate + "', 'discountValue': '" + discountValue + "', 'discountPercentage': '" + discountPercentage + "', 'refundableDeposit': '" + refundableDeposit + "', 'bookingFeeDueDate': '" + bookingFeeDueDate + "', 'bookingFee': '" + bookingFee + "', 'serviceChargePercentage': '" + serviceChargePercentage + "', 'noOfDP': '" + noOfDP + "', 'taxType': '" + taxType + "', 'venuePrice': '" + venuePrice + "', 'venueDiscountPercentage': '" + venueDiscountPercentage + "', 'venueDiscountValue': '" + venueDiscountValue + "', 'buffetQty': '" + buffetQty + "', 'sittingBuffetQty': '" + sittingBuffetQty + "', 'buffetPrice': '" + buffetPrice + "', 'sittingBuffetPrice': '" + sittingBuffetPrice + "', 'foodStallPrice': '" + foodStallPrice + "', 'decorationPrice': '" + decorationPrice + "', 'entertainmentPrice': '" + entertainmentPrice + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataFilter: function (data) { return data; },
        success: function (data) {
            var settlementDueDate = new Date(parseInt(data.d.SettlementDueDate.substr(6)));

            $("#hdnPackagePrice").val(data.d.Price);
            $("#lblPackagePrice").text(data.d.Price);
            $("#lblPackagePrice").formatNumber({ format: "#,###.00", locale: "id" });

            $("#lblTotalDiscount").text(data.d.TotalDiscount);
            $("#lblTotalDiscount").formatNumber({ format: "#,###.00", locale: "id" });
            $("#lblGrandTotal").text(data.d.GrandTotal);
            $("#lblGrandTotal").formatNumber({ format: "#,###.00", locale: "id" });
            $("#lblServiceChargeValue").text(data.d.ServiceChargeValue);
            $("#lblServiceChargeValue").formatNumber({ format: "#,###.00", locale: "id" });
            $("#lblTaxValue").text(data.d.TaxValue);
            $("#lblTaxValue").formatNumber({ format: "#,###.00", locale: "id" });
            $("#lblGrandTotalBeforeTax").text(data.d.GrandTotalBeforeTax);
            $("#lblGrandTotalBeforeTax").formatNumber({ format: "#,###.00", locale: "id" });

            $("#hdnSettlementDueDate").val(settlementDueDate.getDate() + '/' + (settlementDueDate.getMonth() + 1) + "/" + settlementDueDate.getFullYear());
            $("#txtSettlementDueDate").val(settlementDueDate.getDate() + '/' + (settlementDueDate.getMonth() + 1) + "/" + settlementDueDate.getFullYear());
            $("#lblSettlementDueDate").text(settlementDueDate.getDate() + '/' + (settlementDueDate.getMonth() + 1) + "/" + settlementDueDate.getFullYear());
            $("#txtSettlementAmount").val(data.d.SettlementAmount);
            $("#lblSettlementAmount").text(data.d.SettlementAmount);

            if (noOfDP > 0) {
                var dpIDueDate = new Date(parseInt(data.d.DPIDueDate.substr(6)));
                $("#txtDPIAmount").val(data.d.DPIAmount);
                $("#hdnDPIAmount").val(data.d.DPIAmount);
                $("#txtDPIDueDate").val(dpIDueDate.getDate() + '/' + (dpIDueDate.getMonth() + 1) + "/" + dpIDueDate.getFullYear());
            }

            if (noOfDP > 1) {
                var dpIIDueDate = new Date(parseInt(data.d.DPIIDueDate.substr(6)));
                $("#txtDPIIAmount").val(data.d.DPIIAmount);
                $("#hdnDPIIAmount").val(data.d.DPIIAmount);
                $("#txtDPIIDueDate").val(dpIIDueDate.getDate() + '/' + (dpIIDueDate.getMonth() + 1) + "/" + dpIIDueDate.getFullYear());
            }

            if (noOfDP > 2) {
                var dpIIIDueDate = new Date(parseInt(data.d.DPIIIDueDate.substr(6)));
                $("#txtDPIIIAmount").val(data.d.DPIIIAmount);
                $("#hdnDPIIIAmount").val(data.d.DPIIIAmount);
                $("#txtDPIIIDueDate").val(dpIIIDueDate.getDate() + '/' + (dpIIIDueDate.getMonth() + 1) + "/" + dpIIIDueDate.getFullYear());
            }

            if (noOfDP > 3) {
                var dpIVDueDate = new Date(parseInt(data.d.DPIVDueDate.substr(6)));
                $("#txtDPIVAmount").val(data.d.DPIVAmount);
                $("#hdnDPIVAmount").val(data.d.DPIVAmount);
                $("#txtDPIVDueDate").val(dpIVDueDate.getDate() + '/' + (dpIVDueDate.getMonth() + 1) + "/" + dpIVDueDate.getFullYear());
            }

            if (noOfDP > 4) {
                var dpVDueDate = new Date(parseInt(data.d.DPVDueDate.substr(6)));
                $("#txtDPVAmount").val(data.d.DPVAmount);
                $("#hdnDPVAmount").val(data.d.DPVAmount);
                $("#txtDPVDueDate").val(dpVDueDate.getDate() + '/' + (dpVDueDate.getMonth() + 1) + "/" + dpVDueDate.getFullYear());
            }

            if (noOfDP > 5) {
                var dpVIDueDate = new Date(parseInt(data.d.DPVIDueDate.substr(6)));
                $("#txtDPVIAmount").val(data.d.DPVIAmount);
                $("#hdnDPVIAmount").val(data.d.DPVIAmount);
                $("#txtDPVIDueDate").val(dpVIDueDate.getDate() + '/' + (dpVIDueDate.getMonth() + 1) + "/" + dpVIDueDate.getFullYear());
            }
        }
    });
}

function loadHall() {
    var count = 1;
    var isEnabled = true;

    if ($("#hdnMode").val() == "Detail")
        isEnabled = false;

    $('#Hall input').remove();
    $('#Hall label').remove();

    $.ajax({
        url: "/Service.asmx/RetrieveAllHall",
        data: "",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataFilter: function (data) { return data; },
        success: function (data) {
            $.map(data.d, function (item) {
                var attr = "";

                if (!isEnabled) {
                    attr += "disabled = \"disabled\" ";
                }

                if ($("#" + item.ID).val() != undefined) {
                    attr += "checked= \"checked\" ";
                }

                var html = "<input type=\"checkbox\" name=\"Hall" + count + "\"  id=\"" + item.ID + "\" value=\"" + item.ID + "\"" + attr + "\ /><label>" + item.Description + "</label>";

                $('#Hall').append($(html));
                count++;
            })
        }
    });
}

function onSelectCustomer(data) {
    $("#lblCustomerName").text(data.FullName);
    $("#lblCustomerAddress1").text(data.Address1);
    $("#lblCustomerAddress2").text(data.Address2);
    $("#lblCustomerAddress3").text(data.Address3);
    $("#lblCustomerCityDescription").text(data.CityDescription);
    $("#lblCustomerPostCode").text(data.PostCode);
    $("#lblCustomerEmail").text(data.Email);
    $("#lblCustomerFax").text(data.Fax);
    $("#lblCustomerContact").text(data.Contact);
    $("#lblCustomerContact2").text(data.Contact2);
}

function onBookingDateChanged() {
    var bookingDate = $("#txtDate").val();
    $("#txtBookingFeeDueDate").val(bookingDate);
}

function onChangeVenue() {
    var venueID = $("#VenueID").val();

    $.ajax({
        url: "/Service.asmx/RetrieveVenue",
        data: "{ 'venueID': '" + venueID + "' }",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataFilter: function (data) { return data; },
        success: function (data) {
            $("#lblVenuePrice").text(data.d.Price);
            $("#lblVenuePrice").formatNumber({ format: "#,###.00", locale: "id" });
            $("#hdnVenuePrice").val(data.d.Price);

            onChangePrice();
        }
    });
}

function addDateGridRow() {
    var rowCount = $('#Grid1 tbody > tr').length;
    var newRowNumber = rowCount;

    if (newRowNumber < 0)
        newRowNumber = 0;

    var template = $('#Grid1Template').val();

    template = template.replace(/@index/g, newRowNumber);

    if (newRowNumber > 0)
        $("#Grid1 tbody > tr:last").after(template);
    else
        $("#Grid1 tbody").append(template);

    initNumeric();
    initDatePicker();

}