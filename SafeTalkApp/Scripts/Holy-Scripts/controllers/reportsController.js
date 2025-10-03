app.controller('ReportsController', function ($scope, reportsService) {

    $scope.getConsultationReport = function () {
        var getConsultationReport = reportsService.getConsultationReport();
        getConsultationReport.then(function (result) {
            $scope.consultationReport = result.data;

            var labels = $scope.consultationReport.map(r => r.Label);
            var data = $scope.consultationReport.map(r => r.ConsultationCount);

            // Render chart
            var ctx = document.getElementById("consultationChart").getContext("2d");
            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Consultations',
                        data: data,
                        borderColor: 'rgba(75, 192, 192, 1)',
                        backgroundColor: 'rgba(75, 192, 192, 0.2)',
                        fill: true,
                        tension: 0.3, // smooth curve
                        pointRadius: 5,
                        pointBackgroundColor: 'rgba(75, 192, 192, 1)'
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: { display: true },
                        tooltip: { enabled: true }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                stepSize: 1,
                                callback: function (value) {
                                    return Number.isInteger(value) ? value : null;
                                }
                            }
                        }
                    }
                }
            });
        }, function () {
            alert('Error in getting consultation report');
        });
    }

    $scope.selectedPatient = null;

    $scope.getPatientHistory = function () {
        var getPatientHistory = reportsService.getPatientHistory();
        getPatientHistory.then(function (result) {
            $scope.patientHistory = result.data;
        }, function () {
            alert('Error in getting patient history');
        });
    }

    $scope.loadHistory = function (patientID) {
        reportsService.getPatientHistory(patientID).then(function (result) {
            $scope.patientHistory = result.data;
            $scope.selectedPatient = result.data.length > 0 ? result.data[0] : null;
        }, function () {
            alert('Error in getting patient history');
        });
    };

    $scope.backToPatients = function () {
        $scope.selectedPatient = null;
        $scope.getPatientHistory();
    };

})