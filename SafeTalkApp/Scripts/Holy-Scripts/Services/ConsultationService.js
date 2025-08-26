app.service("ConsultationService", function ($http) {

    this.getChatMessages = function (appointmentID) {
        var response = $http({
            method: "get",
            url: "/Consultation/GetChatMessages",
            params: { appointmentID: appointmentID }
        });
        return response;
    }

    // ===== User Consultation =====
    this.getPatientConsultations = function () {
        var response = $http({
            method: "get",
            url: "/Consultation/GetPatientConsultations",
        });
        return response;
    }

    // ===== Doctor Consultation =====
    this.getDoctorConsultations = function () {
        var response = $http({
            method: "get",
            url: "/Consultation/GetDoctorConsultations",
        });
        return response;
    }
});