function submitForm(formName) {
    $("#" + formName).submit();
}

function submitForm(formName, actionName) {
    $("#" + formName).attr('action', actionName);
    $("#" + formName).submit();
}

function deleteRow(deleteButtonID, dataContainerName) {
    var button = $("#" + deleteButtonID);

    var deletedIndex = button.parent().parent().index();
    //console.log("deletedIndex = " + deletedIndex);

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
                //access to form element via $(this)
                //                var oriID = $(this).attr('id');

                //                if (oriID == null)
                //                    return;

                //                var newID = oriID.replace("_" + x, "_" + (x - 1));
                //                $(this).attr("id", newID);

                //                var newDeleteFunction = "deleteRow('btnDelete" + dataContainerName + "_" + (x - 1) + "', 'Details')";
                //                var deleteButtonID = "btnDelete" + dataContainerName + "_" + (x - 1);

                //                var btnDelete = $("#" + deleteButtonID);

                //                if (btnDelete != null) {
                //                    btnDelete.attr('onclick', newDeleteFunction);

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


    //var func = eval('onDeleteRow');
    //if ($.isFunction(func)) {
    //    onDeleteRow(deletedIndex);
    //}
}

function selectAllRows(gridID) {
    var checked = $("#chkSelectAllRows", "#" + gridID).attr("checked");

    $('.selectionCheckBoxContainer', "#" + gridID).each(function (index) {
        var checkBox = $(this).find(':checkbox');

        if (!checkBox.attr('disabled'))
            checkBox.attr('checked', checked);
    });
}

