app.service('ProfileService', ['$http', function ($http) {
    this.getProfile = function () {
        return $http.get('/Profile/GetProfile');
    };

    this.updateProfile = function (formData) {
        return $http.post('/Profile/UpdateProfile', formData, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        });
    };
}]);
