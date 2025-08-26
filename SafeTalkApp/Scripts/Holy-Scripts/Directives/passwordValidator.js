app.directive('passwordValidator', function () {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, ngModelCtrl) {
            const pattern = /^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}\[\]:;<>,.?~\\/-]).{8,}$/;

            ngModelCtrl.$validators.pattern = function (modelValue, viewValue) {
                const value = modelValue || viewValue;
                return pattern.test(value);
            };
        }
    };
});