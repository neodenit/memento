$(function () {
    var correctSelection = function (target) {
        while (target.value.substr(target.selectionEnd - 1, 1) === ' ') {
            target.selectionEnd = target.selectionEnd - 1;
        }
    };

    $('textarea').select(function (e) {
        correctSelection(e.target);
    });
});
