app.service("HomeService", function ($http, ApiHelper) {

    this.getDoctors = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Home/GetDoctors")
        );
    }
})