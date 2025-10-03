app.controller("DashboardController", function ($scope, DashboardService) {

    $scope.stats = {
        appointments: 0,
        consultations: 0,
        resources: 0
    }

    $scope.loadStats = function () {
        var getStats = DashboardService.getDashboardStats()
        getStats.then(function (result) {
            $scope.stats = result.data
        })
        .catch(function (error) {
            console.error("Error loading dashboard stats:", error)
        })
    }
})