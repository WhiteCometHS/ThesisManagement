function getRandomColor() {
    var r = Math.floor(Math.random() * 255);
    var g = Math.floor(Math.random() * 255);
    var b = Math.floor(Math.random() * 255);
    return `rgba(${r}, ${g}, ${b}, 1)`;
}

var backgroundColors = [];
for (var i = 0; i < barCounts.length; i++) {
    backgroundColors.push(getRandomColor());
}

const ctx = document.getElementById('thesisStatus');
const promoters = document.getElementById('promoters');

new Chart(ctx, {
    type: 'polarArea',
    data: {
        labels: polarAreaStatuses,
        datasets: [{
            label: polarAreaLabel,
            data: polarAreaCounts,
            backgroundColor: [
                'rgba(255, 99, 132, 1)',
                'rgba(54, 162, 235, 1)',
                'rgba(255, 206, 86, 1)'
            ],
        }]
    },
    options: {
        responsive: true,
    }
});

new Chart(promoters, {
    type: 'bar',
    data: {
        labels: barNames,
        datasets: [{
            label: barLabel,
            data: barCounts,
            backgroundColor: backgroundColors,
        }]
    },
    options: {
        responsive: true,
    }
});