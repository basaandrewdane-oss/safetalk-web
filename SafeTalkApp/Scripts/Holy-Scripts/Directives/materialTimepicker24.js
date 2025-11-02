app.directive('materialTimepicker24', ['$timeout', function ($timeout) {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {

            function convertTo24Hour(time12h) {
                if (!time12h) return '';
                var parts = time12h.trim().split(' ');
                if (parts.length !== 2) return time12h;

                var time = parts[0];
                var modifier = parts[1].toUpperCase();
                var timeParts = time.split(':');

                if (timeParts.length < 2) return time12h;

                var hours = parseInt(timeParts[0], 10);
                var minutes = timeParts[1];

                if (modifier === 'PM' && hours < 12) hours += 12;
                if (modifier === 'AM' && hours === 12) hours = 0;

                return hours.toString().padStart(2, '0') + ':' + minutes + ":00";
            }

            function convertTo12Hour(time24h) {
                if (!time24h) return '';
                var [hoursStr, minutes] = time24h.split(':');
                var hours = parseInt(hoursStr, 10);
                var modifier = 'AM';

                if (hours >= 12) {
                    modifier = 'PM';
                    if (hours > 12) hours -= 12;
                }
                if (hours === 0) hours = 12;

                return hours.toString().padStart(2, '0') + ':' + minutes + ' ' + modifier;
            }

            // Initialize Materialize
            $timeout(function () {
                M.Timepicker.init(element[0], {
                    twelveHour: true,
                    onCloseEnd: function () {
                        scope.$apply(function () {
                            var pickedTime = element[0].value; // "08:30 PM"
                            var time24 = convertTo24Hour(pickedTime); // "20:30"
                            ngModel.$setViewValue(time24);
                        });
                    }
                });
            });

            // Set input display whenever model updates
            ngModel.$render = function () {
                var modelVal = ngModel.$viewValue;
                element[0].value = convertTo12Hour(modelVal); // Update input display
            };
        }
    };
}]);
