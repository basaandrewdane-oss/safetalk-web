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

    this.getReferralDetails = function (referralID) {
        return ApiHelper.handleApiResponse(
            $http.get("/Consultation/GetReferralDetails", { params: { referralID: referralID } })
        )
    }

    // ===== Doctor Consultation =====
    this.getDoctorConsultations = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Consultation/GetDoctorConsultations")
        )
    }

    this.createReferral = function (referralData) {
        return ApiHelper.handleApiResponse(
            $http.post("/Consultation/CreateReferral", referralData)
        );
    }
});