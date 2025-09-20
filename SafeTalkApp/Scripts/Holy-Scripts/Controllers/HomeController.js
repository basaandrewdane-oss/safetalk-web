app.controller("HomeController", function ($scope, HomeService) {
    $scope.doctors = {}

    $scope.getDoctors = function () {
        var getDoctors = HomeService.getDoctors();
        getDoctors.then(function (result) {
            $scope.doctors = result.data;
        })
    }
});