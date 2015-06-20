$(function () {
    $('input[name=Type]').change(function () {
        if ($('input[name=Type]:checked').val() === 'Spaced') {
            $('#SRSDeckParams').slideDown();
        } else {
            $('#SRSDeckParams').slideUp();
        }
    });
});