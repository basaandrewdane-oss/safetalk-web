app.controller("AdminController", function ($scope, $timeout, AdminService) {

    $scope.getPendingDoctors = function () {
        if ($.fn.DataTable.isDataTable('#pendingDoctors')) {
            $('#pendingDoctors').DataTable().destroy();
        }
        var getPendingDoctors = AdminService.getPendingDoctors();
        getPendingDoctors.then(function (response) {
            $scope.pendingDoctors = response.data;
            $timeout(function () {
                $('#pendingDoctors').DataTable({
                    responsive: true,
                    language: {
                        paginate: {
                            next: 'Next ',
                            previous: 'Previous'
                        }
                    },
                    drawCallback: function () {
                        $('#pendingDoctors_length select').formSelect();
                    }
                });
            })
        }, function (error) {
            console.error("Error loading pending doctors", error);
            Swal.fire("Error", "Unable to load pending doctors.", "error");
        });
    }

    $scope.verifyDoctor = function (userID) {
        Swal.fire({
            title: "Verify Doctor",
            text: "Are you sure you want to verify this doctor?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, verify",
            cancelButtonText: "No, cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                AdminService.verifyDoctor(userID).then(function (response) {
                    if (response.data.success) {
                        Swal.fire("Success", "Doctor verified successfully.", "success");
                        $timeout(function () {
                            $scope.getPendingDoctors(); // Refresh the list
                        });
                    } else {
                        Swal.fire("Error", response.data.message, "error");
                    }
                }, function (error) {
                    console.error("Verification error", error);
                    Swal.fire("Error", "Unable to verify doctor. Please try again.", "error");
                });
            }
        });
    }
});