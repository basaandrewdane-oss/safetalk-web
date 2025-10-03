app.service("ConsultationService", function ($http, ApiHelper) {

    this.getChatMessages = function (appointmentID) {
        return ApiHelper.handleApiResponse(
            $http.get("/Consultation/GetChatMessages", { params: { appointmentID: appointmentID } })
        )
    }

    this.downloadTranscript = function (appointmentID) {
        return $http.get("/Transcription/DownloadTranscript", {
            params: { appointmentID: appointmentID },
            responseType: "arraybuffer" // binary data
        });
    };

    // ===== User Consultation =====
    this.getPatientConsultations = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Consultation/GetPatientConsultations")
        )
    }

    // ===== Doctor Consultation =====
    this.getDoctorConsultations = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Consultation/GetDoctorConsultations")
        )
    }
});