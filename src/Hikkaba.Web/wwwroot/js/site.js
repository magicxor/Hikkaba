// Write your Javascript code.

$(function () {
    $("time.time-localizable").each(function () {
        var el = $(this);
        var dt = moment(el.attr("datetime"));
        el.text(dt.format("DD.MM.YYYY ddd HH:mm:ss"));
    });
})