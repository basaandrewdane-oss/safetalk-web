app.controller('ReportsController', ["$scope", "$timeout", "reportsService", function ($scope, $timeout, reportsService) {

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
        if ($.fn.DataTable.isDataTable('#patientsTable')) {
            $('#patientsTable').DataTable().destroy();
        }
        var getPatientHistory = reportsService.getPatientHistory();
        getPatientHistory.then(function (result) {
            $scope.patientHistory = result.data;
            $timeout(function () {
                $('#patientsTable').DataTable({
                    responsive: true,
                    drawCallback: function () {
                        $('#patientsTable_length select').formSelect();
                    }
                })
            })
        }, function () {
            alert('Error in getting patient history');
        });
    }

    $scope.loadHistory = function (patientID) {
        if ($.fn.DataTable.isDataTable('#appointmentsTable')) {
            $('#historyTable').DataTable().destroy();
        }
        reportsService.getPatientHistory(patientID).then(function (result) {
            $scope.patientHistory = result.data;
            $scope.selectedPatient = result.data.length > 0 ? result.data[0] : null;
            $timeout(function () {
                $('#historyTable').DataTable({
                    responsive: true,
                    drawCallback: function () {
                        $('#historyTable_length select').formSelect();
                    }
                })
            })
        }, function () {
            alert('Error in getting patient history');
        });
    };

    $scope.backToPatients = function () {
        $scope.selectedPatient = null;
        $scope.getPatientHistory();
    };

    $scope.selectedDoctor = null;

    $scope.getDoctorHistory = function () {
        if ($.fn.DataTable.isDataTable('#doctorsTable')) {
            $('#doctorsTable').DataTable().destroy();
        }
        var getDoctortHistory = reportsService.getDoctorHistory();
        getDoctortHistory.then(function (result) {
            $scope.doctorHistory = result.data;
            $timeout(function () {
                $('#doctorsTable').DataTable({
                    responsive: true,
                    drawCallback: function () {
                        $('#doctorsTable_length select').formSelect();
                    }
                })
            })
        }, function () {
            alert('Error in getting doctor history');
        });
    }

    $scope.loadDoctorHistory = function (doctorID) {
        if ($.fn.DataTable.isDataTable('#doctorHistoryTable')) {
            $('#doctorHistoryTable').DataTable().destroy();
        }
        reportsService.getDoctorHistory(doctorID).then(function (result) {
            $scope.doctorHistory = result.data;
            $scope.selectedDoctor = result.data.length > 0 ? result.data[0] : null;
            $timeout(function () {
                $('#doctorHistoryTable').DataTable({
                    responsive: true,
                    drawCallback: function () {
                        $('#doctorHistoryTable_length select').formSelect();
                    }
                })
            })
        }, function () {
            alert('Error in getting doctor history');
        });
    }

    $scope.backToDoctors = function () {
        $scope.selectedDoctor = null;
        $scope.getDoctorHistory();
    };

    $scope.getMissedAppointments = function () {
        if ($.fn.DataTable.isDataTable('#patientMissed') && $.fn.DataTable.isDataTable('#doctorMissed')) {
            $('#patientMissed').DataTable().destroy();
            $('#doctorMissed').DataTable().destroy();
        }
        reportsService.getMissedAppointments().then(function (result) {
            if (result.success) {
                $scope.missedAppointments = result.data;
                $timeout(function () {
                    $('#patientMissed').DataTable({
                        responsive: true,
                        drawCallback: function () {
                            $('#patientMissed_length select').formSelect();
                        }
                    })
                    $('#doctorMissed').DataTable({
                        responsive: true,
                        drawCallback: function () {
                            $('#doctorMissed_length select').formSelect();
                        }
                    })
                })
            }
        }).catch(function () {
            alert('Error in getting missed appointments report');
        });
    }

}])