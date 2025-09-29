app.service("ChatBotService", function ($http, ApiHelper) {

    this.sendMessage = function (userMessage) {
        return ApiHelper.handleApiResponse(
            $http.post("/ChatBot/GetResponse", { message: userMessage })
        );
    };

    this.getPrompts = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Admin/GetPrompts")
        )
    }
});