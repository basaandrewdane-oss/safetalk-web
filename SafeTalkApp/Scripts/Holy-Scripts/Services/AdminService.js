app.service("AdminService", function ($http) {

    this.getFaqs = function () {
        return $http.get("/Admin/GetFaqs");
    }

    this.addFaq = function (faq) {
        return $http.post("/Admin/AddFaq", faq);
    }

    this.updateFaq = function (faq) {
        return $http.post("/Admin/UpdateFaq", faq);
    }

    this.deleteFaq = function (faqID){
        return $http.post("/Admin/DeleteFaq", { faqID: faqID });
    }

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