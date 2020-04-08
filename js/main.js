$( document ).ready(function() {
    jQuery.get('/sprint.json', function(data) {
        $('#codeCompleteDate').text(data.codeCompleteDate);
        $('#codeFreezeDate').text(data.codeFreezeDate);
        $('#endDate').text(data.endDate);
        $('#sprint').text(data.sprint);
        $('#startDate').text(data.startDate);

        $('body').addClass('status-' + data.status);
    });
});
