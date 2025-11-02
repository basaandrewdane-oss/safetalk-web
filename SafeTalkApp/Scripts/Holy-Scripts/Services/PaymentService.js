app.service("PaymentService", ['$http', 'ApiHelper', function ($http, ApiHelper) {
    // ===== User Payment =====
    this.submitPayment = function (formData) {
        return ApiHelper.handleApiResponse(
            $http.post(
                "/Payment/SubmitPayment",
                formData,
                {
                    transformRequest: angular.identity,
                    headers: { 'Content-Type': undefined }
                },
            )
        )
    }

    this.createPayPalOrder = function (appointmentID) {
        return ApiHelper.handleApiResponse(
            $http.post("/Payment/CreatePayPalOrder", { appointmentID: appointmentID })
        );
    }

    // ===== Doctor Payment =====
    this.verifyPayment = function (appointmentID) {
        return ApiHelper.handleApiResponse(
            $http.post("/Payment/VerifyPayment", { appointmentID: appointmentID })
        );
    }

    this.rejectPayment = function(appointmentID) {
        return ApiHelper.handleApiResponse(
            $http.post("/Payment/RejectPayment", { appointmentID: appointmentID })
        );
    }
}]);