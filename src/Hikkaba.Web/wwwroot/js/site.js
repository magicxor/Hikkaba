// Write your Javascript code.

$(function () {
    $("time.time-localizable").each(function () {
        var el = $(this);
        var dt = moment(el.attr("datetime"));
        el.text(dt.format("DD.MM.YYYY ddd HH:mm:ss"));
    });
});

function wrapText(inputId, openTag, closeTag) {
    var textArea = document.getElementById(inputId);

    if (typeof (textArea.selectionStart) != "undefined") {
        var begin = textArea.value.substr(0, textArea.selectionStart);
        var selection = textArea.value.substr(textArea.selectionStart, textArea.selectionEnd - textArea.selectionStart);
        var end = textArea.value.substr(textArea.selectionEnd);
        textArea.value = begin + openTag + selection + closeTag + end;
    }
}

function insertTag(inputId, markupButtonId) {
    switch (markupButtonId) {
        case "m-bold":
            wrapText(inputId, "[b]", "[/b]");
            break;
        case "m-italic":
            wrapText(inputId, "[i]", "[/i]");
            break;
        case "m-underline":
            wrapText(inputId, "[u]", "[/u]");
            break;
        case "m-strikeout":
            wrapText(inputId, "[s]", "[/s]");
            break;
        case "m-quote":
            wrapText(inputId, "[quote]", "[/quote]");
            break;
        case "m-spoiler":
            wrapText(inputId, "[spoiler]", "[/spoiler]");
            break;
        case "m-code":
            wrapText(inputId, "[pre]", "[/pre]");
            break;
        case "m-subscript":
            wrapText(inputId, "[sub]", "[/sub]");
            break;
        case "m-superscript":
            wrapText(inputId, "[sup]", "[/sup]");
            break;
        default:
    }
}