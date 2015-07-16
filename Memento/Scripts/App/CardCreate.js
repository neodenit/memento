$(function () {
    $('#AddCloze').click(function () {
        var element = $('#Text');
        var domElement = element.get(0);

        var start = domElement.selectionStart;
        var end = domElement.selectionEnd;

        var text = element.val();

        var startPart = text.substring(0, start);
        var middlePart = text.substring(start, end);
        var endPart = text.substring(end, text.length);

        var label = GetNextLabel(text);

        if (label) {
            var newText = startPart + '{{' + label + '::' + middlePart + '}}' + endPart;

            element.val(newText);
        }
    });

    var GetCurrentClozePattern = function (clozeName) {
        var currentPattern = "{{" + clozeName + "::((?:(?!}}).)+?)}}";
        return currentPattern;
    }

    var maxClozeNum = 100;

    var GetNextLabel = function (text) {
        for (var i = 1; i <= maxClozeNum; i++) {
            var label = 'c' + i;

            var regexText = GetCurrentClozePattern(label);

            if (!text.match(regexText))
            {
                return label;
            }
        }
    }
});