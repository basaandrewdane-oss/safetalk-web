app.controller("AdminController", ["$scope", "$timeout", "AdminService", "$sce", function ($scope, $timeout, AdminService, $sce) {
    $scope.pendingDoctors = [];
    $scope.faqs = [];
    $scope.newFaq = {};
    $scope.editFaq = null;
    $scope.termsContent = "";

    $scope.loadFaqs = function () {
        if ($.fn.DataTable.isDataTable('#faqsTable')) {
            $('#faqsTable').DataTable().destroy();
        }
        var getFaqs = AdminService.getFaqs();
        getFaqs.then(function (result) {
            $scope.faqs = result.data;
            $timeout(function () {
                $('#faqsTable').DataTable({
                    responsive: true,
                    language: {
                        paginate: {
                            next: 'Next ',
                            previous: 'Previous'
                        }
                    },
                    drawCallback: function () {
                        $('#faqsTable_length select').formSelect();
                    }
                });
            })
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
        AdminService.addFaq($scope.newFaq).then(function (result) {
            if (result.success) {
                Swal.fire("Success", "FAQ added successfully.", "success");
                $scope.newFaq = {};
                $scope.loadFaqs();
            } else {
                Swal.fire("Error", result.message, "error");
            }
        }, function (error) {
            console.error("Error adding FAQ", error);
            Swal.fire("Error", "Unable to add FAQ. Please try again.", "error");
        });
    }

    $scope.startEditFaq = function (faq) {
        $scope.editFaq = angular.copy(faq);
        $timeout(function () {
            var elems = document.getElementById('editModal');
            var instance = M.Modal.init(elems);
            instance.open();
            M.updateTextFields();
        });
    }

    $scope.saveEdit = function () {
        AdminService.updateFaq($scope.editFaq).then(function (result) {
            if (result.success) {
                Swal.fire("Success", "FAQ updated successfully.", "success");
                $scope.editFaq = null;
                $scope.loadFaqs();
            } else {
                Swal.fire("Error", result.message, "error");
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
                AdminService.deleteFaq(faqID).then(function (result) {
                    if (result.success) {
                        Swal.fire("Deleted!", "FAQ has been deleted.", "success");
                        $scope.loadFaqs();
                    } else {
                        Swal.fire("Error", result.message, "error");
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
        getPendingDoctors.then(function (result) {
            $scope.pendingDoctors = result.data;
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
                AdminService.verifyDoctor(userID).then(function (result) {
                    if (result.success) {
                        Swal.fire("Success", "Doctor verified successfully.", "success");
                        $timeout(function () {
                            $scope.getPendingDoctors(); // Refresh the list
                        });
                    } else {
                        Swal.fire("Error", result.message, "error");
                    }
                }, function (error) {
                    console.error("Verification error", error);
                    Swal.fire("Error", "Unable to verify doctor. Please try again.", "error");
                });
            }
        });
    }

    $scope.loadFaqs();

    $scope.getPayments = function () {
        if ($.fn.DataTable.isDataTable('#paymentsTable')) {
            $('#paymentsTable').DataTable().destroy();
        }
        var getPayments = AdminService.getPayments();
        getPayments.then(function (result) {
            $scope.payments = result.data;
            $timeout(function () {
                $('#paymentsTable').DataTable({
                    responsive: true,
                    language: {
                        paginate: {
                            next: 'Next ',
                            previous: 'Previous'
                        }
                    },
                    drawCallback: function () {
                        $('#paymentsTable_length select').formSelect();
                    }
                });
            })
        }, function (error) {
            console.error("Error loading payments", error);
            Swal.fire("Error", "Unable to load payments.", "error");
        });
    }

    $scope.getTerms = function () {
        var getTerms = AdminService.getTerms();
        getTerms.then(function (result) {
            if (result.success) {
                $scope.termsContent = result.data;

                $timeout(function () {
                    if (window.termsEditorInstance) {
                        window.termsEditorInstance.destroy()
                            .then(() => initEditor());
                    } else {
                        initEditor();
                    }

                    function initEditor() {
                        ClassicEditor
                            .create(document.querySelector('#termsEditor'), {
                                toolbar: [
                                    'heading', '|',
                                    'bold', 'italic', 'link', 'bulletedList', 'numberedList', '|',
                                    'undo', 'redo'
                                ]
                            })
                            .then(editor => {
                                window.termsEditorInstance = editor;
                                editor.setData($scope.termsContent);
                            })
                            .catch(error => {
                                console.error('Error initializing CKEditor 5:', error);
                            });
                    }
                }, 300)
            }
        })
    }

    $scope.updateTerms = function () {
        Swal.fire({
            title: "Update Terms",
            text: "Are you sure you want to update the terms?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, update",
            cancelButtonText: "No, cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                var updatedContent = window.termsEditorInstance.getData();

                var saveTerms = AdminService.updateTerms(updatedContent);
                saveTerms.then(function (result) {
                    if (result.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Saved!',
                            text: 'Terms and Conditions updated successfully.'
                        });
                    } else {
                        Swal.fire('Error', 'Failed to update terms.', 'error');
                    }
                });
            }
        });
    }

    $scope.trustedHtml = function () {
        return $sce.trustAsHtml($scope.termsContent);
    };

    $scope.getUsers = function () {
        if ($.fn.DataTable.isDataTable('#usersTable')) {
            $('#usersTable').DataTable().destroy();
        }
        AdminService.getUsers().then(function (result) {
            if (result.success) {
                $scope.users = result.data;
                $timeout(function () {
                    $('#usersTable').DataTable({
                        responsive: true,
                        language: {
                            paginate: {
                                next: 'Next ',
                                previous: 'Previous'
                            }
                        },
                        drawCallback: function () {
                            $('#usersTable_length select').formSelect();
                        }
                    });
                })
            }
        }).catch(function (error) {
            console.error("Error loading users", error);
            Swal.fire("Error", "Unable to load users.", "error");
        });
    }

    // Verify user
    $scope.verifyUser = function (user) {
        Swal.fire({
            title: "Verify user?",
            text: "Do you want to verify this user?",
            icon: "question",
            showCancelButton: true,
            confirmButtonText: "Yes, verify"
        }).then((result) => {
            if (result.isConfirmed) {
                AdminService.verifyUser(user.userID).then(function (response) {
                    if (response.success) {
                        Swal.fire("Verified!", "User has been verified.", "success");
                        user.isVerified = true;
                    } else {
                        Swal.fire("Error", response.message, "error");
                    }
                });
            }
        });
    };

    // Delete user
    $scope.deleteUser = function (user) {
        Swal.fire({
            title: "Delete user?",
            text: "This cannot be undone.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, delete"
        }).then((result) => {
            if (result.isConfirmed) {
                AdminService.deleteUser(user.userID).then(function (response) {
                    if (response.success) {
                        Swal.fire("Deleted!", "User has been removed.", "success");
                        $scope.users = $scope.users.filter(u => u.userID !== user.userID);
                    } else {
                        Swal.fire("Error", response.message, "error");
                    }
                });
            }
        });
    };
}
]);