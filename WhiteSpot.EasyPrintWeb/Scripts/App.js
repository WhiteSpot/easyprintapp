var container;

function checkAll() {
    var selectAll = $('#select-all');

    // Iterate each checkbox
    $(':checkbox').each(function () {
        this.checked = selectAll.is(':checked');
    });
}

function printAllInProgress() {
    $('#PrintAllButton').attr('disabled', 'disabled');

    container.addClass('hcf-processing');
}

function autoResize() {
    if (window.parent == null)
        return;

    var params = $.deparam.querystring();  // Fetch querystring params object
    var height = $(".app-container").height() + 50;
    var width = $(".app-container").width();
    var message = "<Message senderId=" + params.SenderId + " >" + "resize(" + width + "," + height + ")</Message>";

    window.parent.postMessage(message, params.SPHostUrl);
}

$(document).ready(function () {
    container = $('.app-container');

    autoResize();
});
