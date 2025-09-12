app.service("ChatBotService", function ($http) {

    this.sendMessage = function (userMessage) {
        return $http.post("/ChatBot/GetResponse", { message: userMessage });
    };

    this.getPrompts = function () {
        return $http.get("/Admin/GetPrompts");
    }
});