app.directive('validateAge', function () {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, ngModelCtrl) {
            ngModelCtrl.$validators.age = function (modelValue, viewValue) {
                const value = modelValue || viewValue;
                if (!value) return false;

                const birthDate = new Date(value);
                const today = new Date();
                const age = today.getFullYear() - birthDate.getFullYear();
                const m = today.getMonth() - birthDate.getMonth();

                if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
                    return age - 1 >= 18;
                }

                return age >= 18;
            };
        }
    };
});