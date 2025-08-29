app.controller("ChatBotController", function($scope, ChatBotService) {
    $scope.chatOpen = false;
    $scope.messages = [];
    $scope.message = "";
    $scope.typing = false;

    $scope.toggleChat = function() {
        $scope.chatOpen = !$scope.chatOpen;
    };

    $scope.sendMessage = function() {
        if (!$scope.message) return;

        console.log(JSON.stringify($scope.message));
        $scope.messages.push({ from: 'user', text: cleanText($scope.message) });
        let userMessage = $scope.message;
        $scope.message = "";
        $scope.typing = true;
        // Call backend API
        ChatBotService.sendMessage(userMessage)
            .then(function (response) {
                console.log(JSON.stringify(response.data));
                $scope.typing = false;
                $scope.messages.push({ from: 'bot', text: cleanText(response.data) });
            }, function() {
                $scope.messages.push({ from: 'bot', text: "⚠️ Error contacting AI." });
            });
    };

    function cleanText(text) {
        return text.replace(/^\s+|\s+$/g, "").replace(/^\n+/, "");
    }
});
