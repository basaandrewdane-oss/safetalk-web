app.controller("DashboardController", function ($scope, DashboardService) {

    // For user and doctor
    $scope.stats = {
        UpcomingAppointments: 0,
        ActiveConsultations: 0,
        CompletedConsultations: 0,
        Resources: 0,
        PendingCount: 0,
        ApprovedCount: 0,
        PaidCount: 0,
        CompletedCount: 0,
        MissedCount: 0
    }

    $scope.loadStats = function () {
        var getStats = DashboardService.getDashboardStats()
        getStats.then(function (result) {
            $scope.stats = result.data
            $scope.renderChart();
        })
        .catch(function (error) {
            console.error("Error loading dashboard stats:", error)
        })
    }

    $scope.renderChart = function () {
        const ctx = document.getElementById("appointmentChart").getContext("2d");

        if ($scope.chart) {
            $scope.chart.destroy(); // destroy old instance before re-rendering
        }

        $scope.chart = new Chart(ctx, {
            type: "doughnut",
            data: {
                labels: ["Pending", "Approved", "Paid", "Completed", "Missed"],
                datasets: [{
                    data: [
                        $scope.stats.PendingCount,
                        $scope.stats.ApprovedCount,
                        $scope.stats.PaidCount,
                        $scope.stats.CompletedCount,
                        $scope.stats.MissedCount
                    ],
                    backgroundColor: ["#ffcc80", "#80deea", "#b39ddb", "#a5d6a7", "#ef9a9a"]
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { position: "bottom" },
                    title: { display: true, text: "Appointment Status Overview" }
                }
            }
        });

        // Line chart
        const ctx2 = document.getElementById("consultationTrendChart").getContext("2d");
        if ($scope.chart2) {
            $scope.chart2.destroy();
        }

        const labels = $scope.stats.ConsultationTrends.map(x => x.Date);
        const data = $scope.stats.ConsultationTrends.map(x => x.Count);

        $scope.chart2 = new Chart(ctx2, {
            type: "line",
            data: {
                labels: labels,
                datasets: [{
                    label: "Completed Consultations",
                    data: data,
                    borderColor: "#5e35b1",
                    backgroundColor: "rgba(94, 53, 177, 0.2)",
                    tension: 0.3,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                scales: {
                    y: { beginAtZero: true, ticks: { stepSize: 1 } }
                },
                plugins: {
                    legend: { position: "bottom" },
                    title: { display: true, text: "Consultations Over the Last 7 Days" }
                }
            }
        });
    };

    // For admin
    $scope.loadAdminReports = function () {
        DashboardService.getAdminReports()
            .then(function (result) {
                var data = result.data;
                $scope.renderStatusChart(data.StatusCounts);
                $scope.renderTrendChart(data.AppointmentTrends);
                $scope.renderUserGrowthChart(data.UserGrowth);
                $scope.renderResourceChart(data.ResourceUploads);
            })
            .catch(function (error) {
                console.error("Error loading admin reports:", error);
            });
    };

    $scope.renderStatusChart = function (statusCounts) {
        const ctx = document.getElementById("adminStatusChart").getContext("2d");
        new Chart(ctx, {
            type: "doughnut",
            data: {
                labels: Object.keys(statusCounts),
                datasets: [{
                    data: Object.values(statusCounts),
                    backgroundColor: ["#ffcc80", "#80deea", "#b39ddb", "#a5d6a7", "#ef9a9a", "#ffa726"]
                }]
            },
            options: {
                plugins: { legend: { position: "bottom" } }
            }
        });
    };

    $scope.renderTrendChart = function (trends) {
        const ctx = document.getElementById("adminTrendChart").getContext("2d");
        new Chart(ctx, {
            type: "line",
            data: {
                labels: trends.map(x => x.Date),
                datasets: [{
                    label: "Appointments",
                    data: trends.map(x => x.Count),
                    borderColor: "#5e35b1",
                    backgroundColor: "rgba(94, 53, 177, 0.2)",
                    fill: true,
                    tension: 0.3
                }]
            },
            options: {
                plugins: { legend: { position: "bottom" } },
                scales: { y: { beginAtZero: true } }
            }
        });
    };

    $scope.renderUserGrowthChart = function (userGrowth) {
        const ctx = document.getElementById("userGrowthChart").getContext("2d");
        new Chart(ctx, {
            type: "bar",
            data: {
                labels: userGrowth.map(x => x.Month),
                datasets: [
                    {
                        label: "Patients",
                        data: userGrowth.map(x => x.PatientCount),
                        backgroundColor: "#26a69a"
                    },
                    {
                        label: "Doctors",
                        data: userGrowth.map(x => x.DoctorCount),
                        backgroundColor: "#7e57c2"
                    }
                ]
            },
            options: { plugins: { legend: { position: "bottom" } } }
        });
    };

    $scope.renderResourceChart = function (resources) {
        const ctx = document.getElementById("resourceChart").getContext("2d");
        new Chart(ctx, {
            type: "bar",
            data: {
                labels: resources.map(x => x.Month),
                datasets: [{
                    label: "Resources Uploaded",
                    data: resources.map(x => x.Count),
                    backgroundColor: "#42a5f5"
                }]
            },
            options: { plugins: { legend: { position: "bottom" } } }
        });
    };
})