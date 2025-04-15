// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

window.navigationFn = {
  goToSection: function (id) {
    $('html, body').animate(
      {
        scrollTop: $('#' + id).offset().top,
      },
      0,
    );
  },
};

window.getSelectionText = function getSelectionText() {
  return window.getSelection ? window.getSelection().toString() : '';
};

window.writeSelectionLineToInput = function writeSelectionLineToInput(inputId) {
  const textArea = document.getElementById(inputId);
  if (typeof textArea !== 'undefined') {
    const selectionText = getSelectionText();
    if (selectionText) {
      textArea.value += '[quote]' + getSelectionText() + '[/quote]\n';
    }
  }
};

window.writeLineToInput = function writeLineToInput(inputId, text) {
  const textArea = document.getElementById(inputId);
  if (typeof textArea !== 'undefined') {
    textArea.value += text + '\n';
  }
};

window.wrapText = function wrapText(inputId, openTag, closeTag) {
  const textArea = document.getElementById(inputId);
  if (typeof textArea.selectionStart !== 'undefined') {
    const begin = textArea.value.substring(0, textArea.selectionStart);
    const selection = textArea.value.substring(textArea.selectionStart, textArea.selectionEnd);
    const end = textArea.value.substring(textArea.selectionEnd);
    textArea.value = begin + openTag + selection + closeTag + end;
  }
};

window.insertTag = function insertTag(inputId, markupButtonId) {
  switch (markupButtonId) {
    case 'm-bold':
      wrapText(inputId, '[b]', '[/b]');
      break;
    case 'm-italic':
      wrapText(inputId, '[i]', '[/i]');
      break;
    case 'm-underline':
      wrapText(inputId, '[u]', '[/u]');
      break;
    case 'm-strikeout':
      wrapText(inputId, '[s]', '[/s]');
      break;
    case 'm-quote':
      wrapText(inputId, '[quote]', '[/quote]');
      break;
    case 'm-spoiler':
      wrapText(inputId, '[spoiler]', '[/spoiler]');
      break;
    case 'm-code':
      wrapText(inputId, '[code]', '[/code]');
      break;
    case 'm-subscript':
      wrapText(inputId, '[sub]', '[/sub]');
      break;
    case 'm-superscript':
      wrapText(inputId, '[sup]', '[/sup]');
      break;
    default:
      break;
  }
};

$(function () {
  // convert UTC datetime to local datetime
  $('time.time-localizable').each(function () {
    const thisElement = $(this);
    const dateTimeControl = moment(thisElement.attr('datetime'));
    thisElement.text(dateTimeControl.format('YYYY-MM-DD ddd HH:mm:ss'));
  });

  // if js is enabled and post form exists, prevent href=... and insert >>post id to form
  const inputId = 'Message';
  if ($('#' + inputId).length) {
    const postIdLink = $('.post-id-link');
    postIdLink.attr('onclick', 'return false;');
    postIdLink.click(function () {
      // check if input is empty
      const textArea = document.getElementById(inputId);
      if (typeof textArea !== 'undefined') {
        const textAreaValue = textArea.value;
        if (textAreaValue.length > 0) {
          // check if the last character is not a new line
          const lastChar = textAreaValue.charAt(textAreaValue.length - 1);
          if (lastChar !== '\n') {
            textArea.value += '\n';
          }
        }
      }

      const thisElement = $(this);
      writeLineToInput(inputId, '>>' + thisElement.text(), '');
      writeSelectionLineToInput(inputId);
      navigationFn.goToSection(inputId);

      // focus on the input
      if (typeof textArea !== 'undefined') {
        textArea.focus();
      }
    });
  }
});

const processedForms = new Set();
const allUtcInputs = document.querySelectorAll('.datetimepicker-utc');

const handleSubmit = (event) => {
  const formElement = event.target; // The form that was submitted

  // Find all relevant inputs *within this specific form*
  const localInputsInForm = formElement.querySelectorAll('.datetimepicker-utc');

  localInputsInForm.forEach((localInput) => {
    const originalName = localInput.name;
    if (!originalName) {
      // Skip if the input has no name (might happen if script runs unexpectedly)
      return;
    }

    const localValue = localInput.value;

    if (localValue) {
      const dateObject = new Date(localValue);

      if (!isNaN(dateObject.getTime())) {
        // Date is valid: convert, create hidden input, swap name
        const utcString = dateObject.toISOString();

        const utcInput = document.createElement('input');
        utcInput.type = 'hidden';
        utcInput.value = utcString;
        utcInput.name = originalName;

        formElement.appendChild(utcInput); // Add hidden input to the form
        localInput.removeAttribute('name'); // Remove name from original input

        console.log(
          `JS Info: Created hidden input named "${utcInput.name}" with UTC value "${utcString}". Original input "${originalName}" name removed.`,
        );
      } else {
        // Date is invalid: do nothing, let the original input submit
        console.warn(`JS Warning: Invalid date for field named "${originalName}". Submitting original value.`);
      }
    } else {
      // Input is empty: do nothing, let the original input submit
      console.log(`JS Info: Field "${originalName}" is empty. Submitting as is.`);
    }
  });
  // Form submission continues after this function finishes
};

// Iterate through all found UTC inputs to identify their forms
allUtcInputs.forEach((input) => {
  const form = input.closest('form'); // Find the parent form of the input

  // If the input is inside a form, and we haven't processed this form yet
  if (form && !processedForms.has(form)) {
    // Add the single submit event listener to this form
    form.addEventListener('submit', handleSubmit);
    // Mark this form as processed so we don't add the listener again
    processedForms.add(form);
    console.log(`JS Setup: Attached UTC date handler to form#${form.id || '[no id]'}.`);
  }
});
