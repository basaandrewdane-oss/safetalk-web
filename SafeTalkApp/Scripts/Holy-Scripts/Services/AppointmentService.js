app.service("AppointmentService", function ($http) {

    this.getAppointmentStatus = function (appointmentID) {
        var response = $http({
            method: "get",
            url: "/Appointment/GetAppointmentStatus",
            params: { appointmentID: appointmentID }
        });
        return response;
    }

    // ===== User Appointment =====
    this.getDoctors = function () {
        return $http.get("/Appointment/GetDoctors");
    }

    this.getDoctorsAvailability = function (userID) {
        var response = $http({
            method: "get",
            url: "/Appointment/GetDoctorsAvailability",
            params: { userID: userID }
        });
        return response;
    }

    this.bookAppointment = function (appointmentData) {
        var response = $http({
            method: "post",
            url: "/Appointment/BookAppointment",
            data: appointmentData
        });
        return response;
    }

    this.getPatientAppointments = function () {
        var response = $http({
            method: "get",
            url: "/Appointment/GetPatientAppointments",
        });
        return response;
    }

    this.cancelAppointment = function (appointmentID) {
        var response = $http({
            method: "post",
            url: "/Appointment/CancelAppointment",
            data: { appointmentID: appointmentID }
        });
        return response;
    }

    // ===== Doctor Appointment =====
    this.getDoctorAppointments = function () {
        var response = $http({
            method: "get",
            url: "/Appointment/GetDoctorAppointments",
        });
        return response;
    }

    this.approveAppointment = function (appointmentID) {
        var response = $http({
            method: "post",
            url: "/Appointment/ApproveAppointment",
            data: { appointmentID: appointmentID }
        });
        return response;
    }
});