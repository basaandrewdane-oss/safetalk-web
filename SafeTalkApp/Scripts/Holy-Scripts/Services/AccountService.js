app.service("AccountService", ['$http', 'ApiHelper', function ($http, ApiHelper) {
    // ===== Account Management =====
    this.getRoles = function () {
        return ApiHelper.handleApiResponse($http.get("/Account/GetRoles"));
    };

    this.getGenders = function () {
        return ApiHelper.handleApiResponse($http.get("/Account/GetGenders"));
    }

    this.getDaysOfWeek = function () {
        return ApiHelper.handleApiResponse($http.get("/Account/GetDaysOfWeek"));
    }

    this.registerUser = function (userData) {
        return ApiHelper.handleApiResponse($http.post("/Account/RegisterUser", userData));
    }

    this.checkEmailExists = function (email) {
        return ApiHelper.handleApiResponse($http.get("/Account/EmailExists?email=" + encodeURIComponent(email)));
    }

    this.login = function (loginData) {
        return ApiHelper.handleApiResponse($http.post("/Account/AuthenticateUser", loginData));
    }

    this.verifyEmail = function (token) {
        return ApiHelper.handleApiResponse(
            $http.post("/Account/VerifyEmailToken?token=" + encodeURIComponent(token))
        );
    }

    this.resendVerificationEmail = function (email) {
        return ApiHelper.handleApiResponse(
            $http.post("/Account/ResendVerificationEmail", { email: email })
        );
    };

    this.forgotPassword = function (email) {
        return ApiHelper.handleApiResponse(
            $http.post("/Account/ForgotPassword", { email: email })
        )
    }

    this.resetPassword = function (resetData) {
        return ApiHelper.handleApiResponse(
            $http.post("/Account/ResetUserPassword", resetData)
        )
    }
}]);