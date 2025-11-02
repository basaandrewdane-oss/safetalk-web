app.service('reportsService', ['$http', 'ApiHelper', function ($http, ApiHelper) {

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

    this.getDoctorHistory = function (doctorID) {
        let url = "/Reports/GetDoctorHistory";
        if (doctorID) {
            url += "?doctorID=" + doctorID;
        }
        return ApiHelper.handleApiResponse(
            $http.get(url)
        );
    };

    this.getMissedAppointments = function () {
        return ApiHelper.handleApiResponse(
            $http.get('/Reports/GetMissedAppointments')
        )
    }
}])