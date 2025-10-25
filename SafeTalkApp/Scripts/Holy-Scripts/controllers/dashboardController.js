app.controller("DashboardController", function ($scope, DashboardService) {

    $scope.stats = {
        UpcomingAppointments: 0,
        ActiveConsultations: 0,
        CompletedConsultations: 0,
        Resources: 0,
        PendingCount: 0,
        ApprovedCount: 0,
        PaidCount: 0,
        CompletedCount: 0,
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
                labels: ["Pending", "Approved", "Paid", "Completed"],
                datasets: [{
                    data: [
                        $scope.stats.PendingCount,
                        $scope.stats.ApprovedCount,
                        $scope.stats.PaidCount,
                        $scope.stats.CompletedCount
                    ],
                    backgroundColor: ["#ffcc80", "#80deea", "#b39ddb", "#a5d6a7"]
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
})