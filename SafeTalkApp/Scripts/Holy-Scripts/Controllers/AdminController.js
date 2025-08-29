app.controller("AdminController", function ($scope, $timeout, AdminService) {
    $scope.pendingDoctors = [];
    $scope.faqs = [];
    $scope.newFaq = {};
    $scope.editFaq = null;

    $scope.loadFaqs = function () {
        AdminService.getFaqs().then(function (response) {
            $scope.faqs = response.data;
        }, function (error) {
            console.error("Error loading FAQs", error);
            Swal.fire("Error", "Unable to load FAQs.", "error");
        });
    };

    $scope.addFaq = function () {
        if (!$scope.newFaq.question || !$scope.newFaq.answer) {
            Swal.fire("Error", "Both question and answer are required.", "error");
            return;
        }
        AdminService.addFaq($scope.newFaq).then(function (response) {
            if (response.data.success) {
                Swal.fire("Success", "FAQ added successfully.", "success");
                $scope.newFaq = {};
                $scope.loadFaqs();
            } else {
                Swal.fire("Error", response.data.message, "error");
            }
        }, function (error) {
            console.error("Error adding FAQ", error);
            Swal.fire("Error", "Unable to add FAQ. Please try again.", "error");
        });
    }

    $scope.startEditFaq = function (faq) {
        $scope.editFaq = angular.copy(faq);
        $('#editModal').modal('open');
        setTimeout(function () { M.updateTextFields(); }, 100);
    }

    $scope.saveEdit = function () {
        AdminService.updateFaq($scope.editFaq).then(function (response) {
            if (response.data.success) {
                Swal.fire("Success", "FAQ updated successfully.", "success");
                $scope.editFaq = null;
                $scope.loadFaqs();
            } else {
                Swal.fire("Error", response.data.message, "error");
            }
        });
    }

    $scope.deleteFaq = function (faqID) {
        Swal.fire({
            title: "Delete FAQ",
            text: "Are you sure you want to delete this FAQ?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, delete it",
            cancelButtonText: "No, cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                AdminService.deleteFaq(faqID).then(function (response) {
                    if (response.data.success) {
                        Swal.fire("Deleted!", "FAQ has been deleted.", "success");
                        $scope.loadFaqs();
                    } else {
                        Swal.fire("Error", response.data.message, "error");
                    }
                }, function (error) {
                    console.error("Error deleting FAQ", error);
                    Swal.fire("Error", "Unable to delete FAQ. Please try again.", "error");
                });
            }
        });
    }

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

    $(document).ready(function () {
        $('.modal').modal();
    });

    $scope.loadFaqs();
});