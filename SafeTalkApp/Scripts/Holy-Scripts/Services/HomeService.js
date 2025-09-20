app.service("HomeService", function ($http) {

    this.getDoctors = function () {
        return $http.get("/Home/GetDoctors").then(function (response) {
            if (response.data.success) {
                return {
                    data: response.data.data,
                    message: response.message
                };
            }
            else {
                return Promise.reject({
                    message: response.data.message,
                    data: response.data.data
                })
            }
        })
    }
})