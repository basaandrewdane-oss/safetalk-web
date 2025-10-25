app.directive('endAfterStart', function () {
    return {
        require: 'ngModel',
        scope: {
            startTime: '=endAfterStart'
        },
        link: function (scope, element, attrs, ngModel) {
            ngModel.$validators.endAfterStart = function (end) {
                if (!end || !scope.startTime) return true;
                return end > scope.startTime; // must be later
            };

            scope.$watch('startTime', function () {
                ngModel.$validate(); // revalidate when start changes
            });
        }
    };
});