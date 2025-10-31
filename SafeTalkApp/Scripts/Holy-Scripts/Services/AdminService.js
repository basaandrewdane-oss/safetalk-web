app.service("AdminService", function ($http, ApiHelper) {

    this.getFaqs = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Admin/GetFaqs")
        )
    }

    this.addFaq = function (faq) {
        return ApiHelper.handleApiResponse(
            $http.post("/Admin/AddFaq", faq)
        )
    }

    this.updateFaq = function (faq) {
        return ApiHelper.handleApiResponse(
            $http.post("/Admin/UpdateFaq", faq)
        )
    }

    this.deleteFaq = function (faqID) {
        return ApiHelper.handleApiResponse(
            $http.post("/Admin/DeleteFaq", { faqID: faqID })
        )
    }

    this.getPendingDoctors = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Admin/GetPendingDoctors")
        )
    }

    this.verifyDoctor = function (userID) {
        return ApiHelper.handleApiResponse(
            $http.post("/Admin/VerifyDoctor", { userID: userID })
        )
    }

    this.getPayments = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Admin/GetPayments")
        )
    }

    this.getTerms = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Admin/GetTerms")
        )
    }

    this.updateTerms = function (content) {
        return ApiHelper.handleApiResponse(
            $http.post("/Admin/UpdateTerms", { content: content })
        )
    }

    this.getUsers = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Admin/GetUsers")
        )
    }

    this.verifyUser = function (userID) {
        return ApiHelper.handleApiResponse(
            $http.post("/Admin/VerifyUser", { userID: userID })
        );
    };

    this.deleteUser = function (userID) {
        return ApiHelper.handleApiResponse(
            $http.post("/Admin/DeleteUser", { userID: userID })
        );
    };
});