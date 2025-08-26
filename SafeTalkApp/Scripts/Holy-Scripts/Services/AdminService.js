app.service("AdminService", function ($http) {

    this.getPendingDoctors = function () {
        return $http.get("/Admin/GetPendingDoctors");
    }

    this.verifyDoctor = function (userID) {
        var response = $http({
            method: "post",
            url: "/Admin/VerifyDoctor",
            data: { userID: userID }
        });
        return response;
    }

});