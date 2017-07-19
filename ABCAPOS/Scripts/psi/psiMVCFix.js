/* 
* for better user experience
* all javascript plugins should be placed below html and css code
* all javascript should be placed inside document ready below html and css code
**/
function objDelayTextAnimation(obj, offset, delay, finishingText, waitingclass, href) {
	obj.removeAttr('href');
	obj.text('Wait ' + (offset / delay) + 's...');
	offset -= delay;
	setTimeout(function () {
		if (offset > 0) {
			obj.fadeOut(100).fadeIn(500);
			objDelayTextAnimation(obj, offset, delay, finishingText, waitingclass, href);
		} else {
			obj.fadeOut(100).fadeIn(500);
			obj.text(finishingText);
			obj.removeClass('disabled');
			obj.attr('href', href);
		}
	}, delay);
}
function thousandSeparator(value) {
    return value.replace(/(\d)(?=(\d{3})+\.)/g, "$1,");
}
function setBtnCreateShadow() {
	btnCreate = $('#btnCreate');
	$('#btnCreateShadow').attr('onclick', btnCreate.attr('onclick'));
	btnCreate.removeAttr('onclick');
}
function getListOfDetailItemNo(objIDToSearch){
	objItemNo = [];
	$('[id^=' + objIDToSearch + ']').each(function () {
		obj = $(this);
		ItemNo = obj.attr('id').split('_')[1];
		ItemNoExist = false;
		for (i = 0; i < objItemNo.length; i++)
			if (objItemNo[i] == ItemNo)
				ItemNoExist = true;
		if (!ItemNoExist)
			objItemNo.push(ItemNo);
	});
	return objItemNo;
}
function hideDetailsColumn(objPreText, objPostText, affectedValues, affectedObjPostText) {
    affectedValues = (affectedValues == null) ? [] : affectedValues;
    var listItemNo = getListOfDetailItemNo(objPreText);
    for (var i = 0; i < listItemNo.length; i++) {
        var obj = $('#' + objPreText + listItemNo[i] + objPostText);
        for (var j = 0; j < affectedValues.length; j++)
            if (affectedValues[j].length > 0) {
                if (obj.text() == affectedValues[j] || obj.val() == affectedValues[j])
                    $('#' + objPreText + listItemNo[i] + affectedObjPostText).hide();
            } else {
                if (obj.text().length == 0 && obj.text().length == 0)
                    $('#' + objPreText + listItemNo[i] + affectedObjPostText).hide();
            }
    }
}
function validateDetailsReference(objIDToSearch, preText, postText, emptyText, errorPostText) {
	objItemNo = getListOfDetailItemNo(objIDToSearch);
	validation = true;
	for (i = 0; i < objItemNo.length; i++) {
		obj = $('#' + preText + objItemNo[i] + postText);
		var objError = $('#' + preText + objItemNo[i] + errorPostText);
		if (obj.val() == emptyText) {
			validation = false;
			objError.css('border-color', 'red');
			objError.css('background-color', 'yellow');
		} else {
			objError.css('border-color', '');
			objError.css('background-color', '');
		}
	}
	return validation;
}
function menuLinkScroller() {
    var index = 0;
    var scrollerName = "rmenulinkscroller_";
    var scrollerClass = "rmenulink";
    $("li>a").each(function () {
        if ($(this).parent().children("ul").length > 0) {
            index += 1;
            $(this).before("<span id='" + scrollerName + index + "'>scroll</span>");
            $("#" + scrollerName + index).addClass(scrollerClass);
        }
    });
    for (var i = 1; i <= index; i++) {
        $("#" + scrollerName + i).click(function () {
            $(this).parent().children("ul").slideToggle();
        });
    }
}
$(document).ready(function () {
    console.log("%cStop!", "color:red; font-size:60px;");
    console.log("%cThis is a browser feature intended for developers.", "color:black; font-size:30px;");
    menuLinkScroller();
    setBtnCreateShadow();
	/* prevent submitForm twice */
	$("#btnCreate").click(function () {
		var btnCreate = $(this);
		var anyEmptyDetails = false;
		detailsValidatorMember = (detailsValidatorMember instanceof Array) ? detailsValidatorMember : [];
		for (var i = 0; i < detailsValidatorMember.length; i++) {
		    var detailsValidator = detailsValidatorMember[i].split('|');
		    var objSearchText = detailsValidator[0]; //searchable text
		    var preText = detailsValidator[1]; //id pretext
		    var postText = detailsValidator[2]; //id posttext
		    var emptyText = detailsValidator[3]; //object default value
		    var errorPostText = detailsValidator[4]; //posttext of object to show error
		    if (!validateDetailsReference(objSearchText, preText, postText, emptyText, errorPostText)) {
		        anyEmptyDetails = true;
		    }
		}
		if (!anyEmptyDetails) {
			if (!btnCreate.hasClass('disabled')) {
				btnCreate.addClass('disabled');
				objDelayTextAnimation(btnCreate, 10000, 1000, btnCreate.text(), 'disabled', btnCreate.attr('href'));
				$('#btnCreateShadow').click();
			}
		} else {
			alert("masih ada detail yang kosong");
		}
	});
});