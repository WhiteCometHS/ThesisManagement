document.addEventListener('DOMContentLoaded', function () {
    var toastElement = document.getElementById('liveToast');

    if (toastElement) {
        var toast = new bootstrap.Toast(toastElement);
        toast.show();
    }
});

function showToastNotification(toastModel) {
    $.ajax({
        url: '/Home/RenderToastNotification',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(toastModel),
        success: function (response) {
            var uniqueToastId = 'toast-' + new Date().getTime();
            var toastHtml = response.replace('liveToast', uniqueToastId);
            $('.toast-container').append(toastHtml);

            var toastElement = document.getElementById(uniqueToastId);
            if (toastElement) {
                var toast = new bootstrap.Toast(toastElement);
                toast.show();

                setTimeout(function () {
                    toast.hide();
                    $(toastElement).remove();
                }, 5000);
            }
        },
        error: function () {
            console.error("An error occurred while rendering the toast notification.");
        }
    });
}