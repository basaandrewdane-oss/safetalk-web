app.controller('ProfileController', function ($scope, $timeout, ProfileService) {
    $scope.user = {};
    $scope.selectedFile = null;

    $scope.loadProfile = function () {
        ProfileService.getProfile().then(function (response) {
            if (response.data.success) {
                $scope.user = response.data.data;
            } else {
                Swal.fire("Error", response.data.message, "error");
            }

            $timeout(function () {
                if (M && M.updateTextFields) {
                    M.updateTextFields(); // built-in Materialize helper
                }
            }, 0);
        });
    };

    $scope.onFileSelect = function (files) {
        if (files && files.length > 0) {
            $scope.selectedFile = files[0];
            const reader = new FileReader();
            reader.onload = function (e) {
                $scope.$apply(() => {
                    $scope.user.profilePictureUrl = e.target.result;
                });
            };
            reader.readAsDataURL(files[0]);
        }
    };

    $scope.updateProfile = function () {
        var formData = new FormData();
        formData.append("firstName", $scope.user.firstName);
        formData.append("lastName", $scope.user.lastName);
        formData.append("specialization", $scope.user.specialization);
        formData.append("contactNumber", $scope.user.contactNumber);
        formData.append("file", $scope.profilePicture); // file input

        var updateProfile = ProfileService.updateProfile
        updateProfile(formData).then(function (response) {
            if (response.data.success) {
                Swal.fire("Success", "Profile updated successfully", "success");
            }
        }, function (error) {
            Swal.fire("Error", "Failed to update profile", "error");
        });
    };

    $scope.$watch('profilePicture', function (newFile) {
        if (newFile) {
            $scope.previewUrl = URL.createObjectURL(newFile);
        }
    });

    $scope.loadProfile();
});
