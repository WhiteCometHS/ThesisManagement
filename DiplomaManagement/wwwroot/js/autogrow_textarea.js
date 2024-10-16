document.addEventListener("DOMContentLoaded", function () {
    const textareas = document.querySelectorAll('.autogrow-textarea');

    textareas.forEach(textarea => {
        const maxRows = textarea.getAttribute('data-max-rows') || 10;
        textarea.addEventListener('input', function () {
            let lineHeight = parseInt(window.getComputedStyle(this).lineHeight, 10);
            let rows = Math.floor(this.scrollHeight / lineHeight);

            if (rows <= maxRows) {
                this.style.overflowY = 'hidden';
                this.rows = rows;
            } else {
                this.rows = maxRows;
                this.style.overflowY = 'auto';
            }
        });
    });
});