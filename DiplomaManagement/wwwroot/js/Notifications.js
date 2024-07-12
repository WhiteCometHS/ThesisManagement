document.addEventListener('DOMContentLoaded', function () {
    var MessageElement = document.getElementById('info-message');
    if (MessageElement) {
        setTimeout(function () {
            var alert = new bootstrap.Alert(MessageElement);
            alert.close();
        }, 5000);
    }
});