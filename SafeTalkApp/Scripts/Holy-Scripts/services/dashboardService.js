app.service("DashboardService", ['$http', 'ApiHelper', function ($http, ApiHelper) {

    this.getDashboardStats = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Dashboard/GetDashboardStats")
        )
    }

    this.getAdminReports = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Dashboard/GetAdminReports")
        );
    };
}])