// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function changeLanguage(culture, event) {
    event.preventDefault();
    var form = document.getElementById("selectLanguage");
    var cultureInput = document.getElementById("cultureInput");
    cultureInput.value = culture;
    form.submit();
}
