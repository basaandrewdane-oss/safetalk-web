app.service("PaymentService", function ($http) {
    // ===== User Payment =====


    this.submitPayment = function (formData) {
        var response = $http({
            method: "post",
            url: "/Payment/SubmitPayment",
            data: formData,
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        });
        return response;
    }

    this.createPayPalOrder = function (appointmentID) {
        var response = $http({
            method: "post",
            url: "/Payment/CreatePaypalOrder",
            data: { appointmentID: appointmentID }
        });
        return response;
    }

    // ===== Doctor Payment =====


    this.verifyPayment = function (appointmentID) {
        var response = $http({
            method: "post",
            url: "/Payment/VerifyPayment",
            data: { appointmentID: appointmentID }
        });
        return response;
    }
});