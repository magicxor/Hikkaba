// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

var navigationFn = {
    goToSection: function (id) {
        $("html, body").animate({
            scrollTop: $("#" + id).offset().top
        }, 0);
    }
};

$(function () {
    $("time.time-localizable").each(function () {
        var el = $(this);
        var dt = moment(el.attr("datetime"));
        el.text(dt.format("YYYY-MM-DD ddd HH:mm:ss"));
    });

    // if js is enabled and post form exists, prevent href=... and insert >>post id to form
    var inputId = "new-post-message-input";
    if ($("#" + inputId).length) {
        $(".post-id-link").attr("onclick", "return false;");
        $(".post-id-link").click(function () {
            var el = $(this);
            writeLineToInput("new-post-message-input", ">>" + el.text(), "");
            writeSelectionLineToInput("new-post-message-input");
            navigationFn.goToSection("new-post-message-input");
        });
    }

    $(".datetimepicker-enabled").each(function () {
        const dtDateOnly = 'YYYY-MM-DD';
        const dtFull = dtDateOnly + ' HH:mm';
        var el = $(this);
        el.datetimepicker({
            format: dtFull,
            minDate: moment().format(dtDateOnly),
            defaultDate: moment().format(dtDateOnly)
        });
    });
});

function getSelectionText() {
    var text = "";
    if (window.getSelection) {
        text = window.getSelection().toString();
    } else if (document.selection && document.selection.type !== "Control") {
        text = document.selection.createRange().text;
    }
    return text;
}

function writeSelectionLineToInput(inputId) {
    var textArea = document.getElementById(inputId);

    if (typeof (textArea) !== "undefined") {
        var selectionText = getSelectionText();
        if (selectionText) {
            textArea.value += "[quote]" + getSelectionText() + "[/quote]\n";
        }
    }
}

function writeLineToInput(inputId, text) {
    var textArea = document.getElementById(inputId);

    if (typeof (textArea) !== "undefined") {
        textArea.value += text + "\n";
    }
}

function wrapText(inputId, openTag, closeTag) {
    var textArea = document.getElementById(inputId);

    if (typeof (textArea.selectionStart) !== "undefined") {
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
