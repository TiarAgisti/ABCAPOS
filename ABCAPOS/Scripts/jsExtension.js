String.prototype.replaceAll = function (stringToFind, stringToReplace) {

    var temp = this;

    var index = temp.indexOf(stringToFind);

    while (index != -1) {

        temp = temp.replace(stringToFind, stringToReplace);

        index = temp.indexOf(stringToFind);

    }

    return temp;

}
