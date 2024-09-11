const ctx = document.getElementById('thesisStatus');
const promoters = document.getElementById('promoters');

new Chart(ctx, {
    type: 'polarArea',
    data: {
        labels: ['Red', 'Blue', 'Yellow'],
        datasets: [{
            label: '# of Votes',
            data: [5, 12, 3],
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
        labels: ['1','2','3','4','5','6','7','8','9','10','11','12','13','14','15'],
        datasets: [{
            label: 'Promoters',
            data: [65, 59, 80, 81, 56, 55, 40, 33, 47, 56, 23, 32, 15, 44, 57],
            backgroundColor: [
                'rgba(255, 99, 132, 1)',
                'rgba(255, 159, 64,1)',
                'rgba(255, 205, 86, 1)',
                'rgba(75, 192, 192, 1)',
                'rgba(54, 162, 235, 1)',
                'rgba(153, 102, 255, 1)',
                'rgba(255, 99, 132, 1)',
                'rgba(255, 159, 64,1)',
                'rgba(255, 205, 86, 1)',
                'rgba(75, 192, 192, 1)',
                'rgba(54, 162, 235, 1)',
                'rgba(153, 102, 255, 1)',
                'rgba(255, 99, 132, 1)',
                'rgba(255, 159, 64,1)',
                'rgba(255, 205, 86, 1)',
            ],
        }]
    },
    options: {
        responsive: true,
    }
});