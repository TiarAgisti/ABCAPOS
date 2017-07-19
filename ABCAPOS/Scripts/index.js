function searchData() {
    var url = $("#hdnUrl").val();
    var filterBy = $("#FilterBy").val();
    var filterKey = $("#FilterKey").val();

    url += "?filterBy=" + filterBy + "&filterKey=" + filterKey;

    window.location.href = url;
}

function clearSearch() {
    $("#FilterKey").val("");
}

function goToPage(pageNumber) {
    var url = $("#hdnUrl").val();
    var filterBy = $("#FilterBy").val();
    var filterKey = $("#FilterKey").val();
    var sortParameter = $("#hdnSortParameter").val();

    var param = "?";

    if (filterKey != "")
        url += "filterBy=" + filterBy + "&filterKey=" + filterKey + "&";

    if (sortParameter != "")
        url += "sortParameter=" + sortParameter + "&";

    url += "pageIndex=" + pageNumber;

    window.location.href = url;
}