$scope.messages = [];
$scope.user = 'User' + Math.floor(Math.random() * 1000);
$scope.message = '';

var chat = $.connection.chatHub;

chat.client.broadcastMessage = function (name, message) {
    $scope.$apply(function () {
        $scope.messages.push({ name: name, message: message });
    });
};

$.connection.hub.start().done(function () {
    $scope.sendMessage = function () {
        if ($scope.message.trim() !== '') {
            chat.server.send($scope.user, $scope.message);
            $scope.message = '';
        }
    };
});