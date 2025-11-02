app.service("AppointmentService", ['$http', 'ApiHelper', function ($http, ApiHelper) {

    this.getAppointmentStatus = function (appointmentID) {
        return ApiHelper.handleApiResponse(
            $http.get("/Appointment/GetAppointmentStatus", { params: { appointmentID } })
        );
    }

    // ===== User Appointment =====
    this.getDoctors = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Appointment/GetDoctors")
        );
    }

    this.getDoctorsAvailability = function (userID) {
        return ApiHelper.handleApiResponse(
            $http.get("/Appointment/GetDoctorsAvailability", { params: { userID } })
        )
    }

    this.bookAppointment = function (appointmentData) {
        return ApiHelper.handleApiResponse(
            $http.post("/Appointment/BookAppointment", appointmentData)
        )
    }

    this.checkSlotAvailability = function (payload) {
        return ApiHelper.handleApiResponse(
            $http.post("/Appointment/CheckSlotAvailability", payload)
        )
    }

    this.getPatientAppointments = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Appointment/GetPatientAppointments")
        );
    }

    this.cancelAppointment = function (appointmentID) {
        return ApiHelper.handleApiResponse(
            $http.post("/Appointment/CancelAppointment", { appointmentID: appointmentID })
        )
    }

    // ===== Doctor Appointment =====
    this.getDoctorAppointments = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Appointment/GetDoctorAppointments")
        )
    }

    this.approveAppointment = function (appointmentID) {
        return ApiHelper.handleApiResponse(
            $http.post("/Appointment/ApproveAppointment", { appointmentID: appointmentID })
        )
    }

    this.rejectAppointment = function (data) {
        return ApiHelper.handleApiResponse(
            $http.post("/Appointment/RejectAppointment", data)
        )
    }
}]);