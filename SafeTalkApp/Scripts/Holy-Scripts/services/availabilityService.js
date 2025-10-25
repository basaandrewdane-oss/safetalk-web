app.service("AvailabilityService", function ($http, ApiHelper) {
    this.getAvailability = function () {
        return ApiHelper.handleApiResponse(
            $http.get('/Availability/GetAvailability')
        );
    };

    this.saveAvailability = function (availabilities) {
        return ApiHelper.handleApiResponse(
            $http.post('/Availability/SaveAvailability', availabilities)
        );
    };
});
