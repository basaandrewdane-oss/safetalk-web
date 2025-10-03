app.service("DashboardService", function ($http, ApiHelper) {

    this.getDashboardStats = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Dashboard/GetDashboardStats")
        )
    }
})