app.service("AccountService", function ($http) {
    // ===== Account Management =====
    this.getRoles = function () {
        return $http.get("/Account/GetRoles");   // returns a promise
    };

    this.getGenders = function () {
        return $http.get("/Account/GetGenders");
    }

    this.getDaysOfWeek = function () {
        return $http.get("/Account/GetDaysOfWeek");
    }

    this.createAccount = function (userData) {
        var response = $http({
            method: "post",
            url: "/Account/CreateAccount",
            data: userData
        })
        return response;
    }

    this.login = function (loginData) {
        var response = $http({
            method: "post",
            url: "/Account/LoginUser",
            data: loginData
        });
        return response;
    }

    this.logout = function () {
        return $http.get("/Account/Logout");
    }
});