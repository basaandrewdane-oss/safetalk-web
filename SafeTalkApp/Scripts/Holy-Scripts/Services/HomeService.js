app.service("HomeService", ['$http', 'ApiHelper', function ($http, ApiHelper) {

    this.getDoctors = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Home/GetDoctors")
        );
    }

    this.submitFeedback = function (data) {
        return ApiHelper.handleApiResponse(
            $http.post("/Home/SubmitFeedback", data)
        )
    }

    this.getTerms = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Home/GetTerms")
        )
    }
}])