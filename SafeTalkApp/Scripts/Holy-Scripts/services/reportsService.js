app.service('reportsService', function ($http, ApiHelper) {

    this.getConsultationReport = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Reports/GetConsultationReport")
        );
    }

    this.getPatientHistory = function (patientID) {
        let url = "/Reports/GetPatientHistory";
        if (patientID) {
            url += "?patientID=" + patientID;
        }
        return ApiHelper.handleApiResponse(
            $http.get(url)
        );
    };
})