app.service("AccountService", function ($http) {
    // ===== Account Management =====
    this.getRoles = function () {
        return $http.get("/Account/GetRoles").then(function (response) {
            if (response.data.success) {
                return {
                    data: response.data.data,
                    message: response.data.message
                };
            }
            else {
                return Promise.reject({
                    message: response.data.message,
                    data: response.data.data
                });
            }
        });
    };

    this.getGenders = function () {
        return $http.get("/Account/GetGenders").then(function (response) {
            if (response.data.success) {
                return {
                    data: response.data.data,
                    message: response.data.message
                }
            }
            else {
                return Promise.reject({
                    message: response.data.message,
                    data: response.data.data
                });
            }
        });
    }

    this.getDaysOfWeek = function () {
        return $http.get("/Account/GetDaysOfWeek").then(function (response) {
            if (response.data.success) {
                return {
                    data: response.data.data,
                    message: response.data.message
                };
            }
            else {
                return Promise.reject({
                    message: response.data.message,
                    data: response.data.data
                });
            }
        });
    }

    this.registerUser = function (userData) {
        var response = $http({
            method: "post",
            url: "/Account/RegisterUser",
            data: userData
        })
        return response;
    }

    this.login = function (loginData) {
        var response = $http({
            method: "post",
            url: "/Account/AuthenticateUser",
            data: loginData
        });
        return response;
    }
});