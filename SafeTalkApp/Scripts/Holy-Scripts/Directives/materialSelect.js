app.directive('materialSelect', ['$timeout', function ($timeout) {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {
            function initSelect() {
                $timeout(function () {
                    M.FormSelect.init(element[0]);

                    var input = element.parent()[0].querySelector('input.select-dropdown');
                    if (!input) return;

                    function updateClasses() {
                        var form = attrs.formName ? scope[attrs.formName] : null;
                        if (ngModel.$invalid && (ngModel.$touched || (form && form.$submitted))) {
                            input.classList.add('ng-invalid', 'ng-touched');
                        } else if (ngModel.$touched || (form && form.$submitted)) {
                            input.classList.remove('ng-invalid');
                            input.classList.add('ng-touched');
                        } else {
                            input.classList.remove('ng-invalid', 'ng-touched');
                        }
                    }

                    input.addEventListener('blur', function () {
                        scope.$apply(function () {
                            ngModel.$setTouched();
                            updateClasses();
                        });
                    });

                    scope.$watchGroup([
                        () => ngModel.$invalid,
                        () => ngModel.$touched,
                        () => scope[attrs.formName] ? scope[attrs.formName].$submitted : false
                    ], updateClasses);

                    scope.$watch(() => ngModel.$modelValue, updateClasses);
                    updateClasses();
                });
            }

            if (attrs.watchCollection) {
                scope.$watch(attrs.watchCollection, function (newVal) {
                    if (newVal && newVal.length) {
                        initSelect();
                    }
                });
            } else {
                initSelect();
            }
        }
    };
}]);
