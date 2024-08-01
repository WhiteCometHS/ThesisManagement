document.addEventListener('DOMContentLoaded', function () {
    var toastElement = document.getElementById('liveToast');

    if (toastElement) {
        var toast = new bootstrap.Toast(toastElement);
        toast.show();
    }
    

/*    var MessageElement = document.getElementById('info-message');
    if (MessageElement) {
       
        setTimeout(function () {
            var alert = new bootstrap.Alert(MessageElement);
            alert.close();
        }, 5000);
    }*/
});