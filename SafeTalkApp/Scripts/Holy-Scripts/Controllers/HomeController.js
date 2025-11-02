app.controller("HomeController", ["$scope", "$sce", "HomeService", function ($scope, $sce, HomeService) {
    $scope.doctors = {}

    $scope.getDoctors = function () {
        var getDoctors = HomeService.getDoctors();
        getDoctors.then(function (result) {
            $scope.doctors = result.data;
        })
    }

    $scope.submitFeedback = function () {
        var data = {
            email: $scope.email,
            feedback: $scope.feedback
        }
        Swal.fire({
            title: 'Are you sure you want to submit feedback?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Yes, submit it!',
            cancelButtonText: 'No, cancel!'
        }).then((result) => {
            if (result.isConfirmed) {
                var submitFeedback = HomeService.submitFeedback(data);
                submitFeedback.then(function (result) {
                    if (result.success) {
                        Swal.fire(
                            'Submitted!',
                            'Your feedback has been submitted.',
                            'success'
                        )
                        $scope.email = "";
                        $scope.feedback = "";
                        $scope.feedbackForm.$setPristine();
                    } else {
                        Swal.fire(
                            'Error!',
                            'There was an error submitting your feedback. Please try again later.',
                            'error'
                        )
                    }
                })
            }
        })
    }

    $scope.getTerms = function () {
        HomeService.getTerms().then(function (result) {
            $scope.terms = result.data.content;
        })
    }

    $scope.trustedTerms = function () {
        return $sce.trustAsHtml($scope.terms);
    }
}]);