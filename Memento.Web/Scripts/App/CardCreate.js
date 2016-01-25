$(function () {
    $('#AddCloze').click(function () {
        var element = $('#Text');
        var domElement = element.get(0);

        var start = domElement.selectionStart;
        var end = domElement.selectionEnd;

        if (start === end) {
            $('#customErrorMessage').removeClass('hidden');
        } else {
            $('#customErrorMessage').addClass('hidden');

            var text = element.val();

            var startPart = text.substring(0, start);
            var middlePart = text.substring(start, end);
            var endPart = text.substring(end, text.length);

            var label = getNextLabel(text, middlePart);

            if (label) {
                var newText = startPart + '{{' + label + '::' + middlePart + '}}' + endPart;

                element.val(newText);
            }
        }

        return false;
    });

    var getCurrentClozePattern = function (clozeName) {
        var currentPattern = '{{' + clozeName + '::((?:(?!}}).)+?)}}';
        return currentPattern;
    };

    var maxClozeNum = 100;

    var getNextLabel = function (text, answer) {
        var existingLabel = getExistingLabel(text, answer);

        if (existingLabel) {
            return existingLabel;
        } else {
            for (var i = 1; i <= maxClozeNum; i++) {
                var label = 'c' + i;

                var regexText = getCurrentClozePattern(label);

                if (!text.match(regexText)) {
                    return label;
                }
            }
        }
    };

    var getExistingLabel = function (text, answer) {
        var pattern = '{{((?:(?!}}).)+?)::' + answer + '}}';

        var regex = new RegExp(pattern);

        var match = regex.exec(text);

        return match ? match[1] : null;
    };
});