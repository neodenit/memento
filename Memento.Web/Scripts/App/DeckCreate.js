$(function () {
    $('input[name=ControlMode]').change(function () {
        if ($('input[name=ControlMode]:checked').val() === 'Automatic') {
            if ($('input[name=DelayMode]:checked').val() === 'Combined') {
                $('#SmoothMode').prop('checked', true);
            }

            $('#Combined').addClass('disabled');
            $('#CombinedMode').prop('disabled', true);
        } else if ($('input[name=ControlMode]:checked').val() === 'Manual') {
            $('#Combined').removeClass('disabled');
            $('#CombinedMode').prop('disabled', false);
        }
    });
});