// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

window.navigationFn = {
    goToSection: function (id) {
        $("html, body").animate({
            scrollTop: $("#" + id).offset().top
        }, 0);
    }
};

window.getSelectionText = function getSelectionText() {
    let text = "";
    if (window.getSelection) {
        text = window.getSelection().toString();
    } else if (document.selection && document.selection.type !== "Control") {
        text = document.selection.createRange().text;
    }
    return text;
};

window.writeSelectionLineToInput = function writeSelectionLineToInput(inputId) {
    const textArea = document.getElementById(inputId);
    if (typeof (textArea) !== "undefined") {
        const selectionText = getSelectionText();
        if (selectionText) {
            textArea.value += "[quote]" + getSelectionText() + "[/quote]\n";
        }
    }
};

window.writeLineToInput = function writeLineToInput(inputId, text) {
    const textArea = document.getElementById(inputId);
    if (typeof (textArea) !== "undefined") {
        textArea.value += text + "\n";
    }
};

window.wrapText = function wrapText(inputId, openTag, closeTag) {
    const textArea = document.getElementById(inputId);
    if (typeof (textArea.selectionStart) !== "undefined") {
        const begin = textArea.value.substring(0, textArea.selectionStart);
        const selection = textArea.value.substring(textArea.selectionStart, textArea.selectionEnd);
        const end = textArea.value.substring(textArea.selectionEnd);
        textArea.value = begin + openTag + selection + closeTag + end;
    }
};

window.insertTag = function insertTag(inputId, markupButtonId) {
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
            wrapText(inputId, "[code]", "[/code]");
            break;
        case "m-subscript":
            wrapText(inputId, "[sub]", "[/sub]");
            break;
        case "m-superscript":
            wrapText(inputId, "[sup]", "[/sup]");
            break;
        default:
            break;
    }
};

$(function () {
    $("time.time-localizable").each(function () {
        const thisElement = $(this);
        const dateTimeControl = moment(thisElement.attr("datetime"));
        thisElement.text(dateTimeControl.format("YYYY-MM-DD ddd HH:mm:ss"));
    });

    // if js is enabled and post form exists, prevent href=... and insert >>post id to form
    const inputId = "Message";
    if ($("#" + inputId).length) {
        const postIdLink = $(".post-id-link");
        postIdLink.attr("onclick", "return false;");
        postIdLink.click(function () {
            const thisElement = $(this);
            writeLineToInput(inputId, ">>" + thisElement.text(), "");
            writeSelectionLineToInput(inputId);
            navigationFn.goToSection(inputId);
        });
    }

    $(".datetimepicker-enabled").each(function () {
        const dateFormatDateOnly = 'YYYY-MM-DD';
        const dateFormatFull = dateFormatDateOnly + ' HH:mm';
        const thisElement = $(this);
        thisElement.datetimepicker({
            format: dateFormatFull,
            minDate: moment().format(dateFormatDateOnly),
            defaultDate: moment().format(dateFormatDateOnly)
        });
    });
});
