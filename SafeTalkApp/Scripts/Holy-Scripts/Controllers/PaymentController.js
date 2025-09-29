app.controller("PaymentController", function ($scope, $timeout, $rootScope, PaymentService) {
    // ===== For user payment =====

    $rootScope.payWithPayPal = function (appointment) {
        PaymentService.createPayPalOrder(appointment.appointmentID).then(function (result) {
            if (result.success) {
                // Show loading swal before redirect
                Swal.fire({
                    title: 'Redirecting to PayPal...',
                    text: 'Please wait while we connect you to the payment gateway.',
                    allowOutsideClick: false,
                    allowEscapeKey: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });

                // Redirect to PayPal
                window.location.href = result.data;

            } else {
                Swal.fire("Error", result.message, "error");
            }
        }).catch(function (err) {
            Swal.fire("Error", "Something went wrong while creating the PayPal order.", "error");
            console.log(err);
        });
    };

    $rootScope.openPaymentModal = function (appointment) {
        $scope.selectedAppointment = appointment;
        $timeout(function () {
            const modalElem = document.getElementById('paymentModal');

            if (!modalElem.classList.contains('modal-initialized')) {
                M.Modal.init(modalElem); modalElem.classList.add('modal-initialized');
            }

            const instance = M.Modal.getInstance(modalElem); instance.open();
        }, 0);
    };

    $scope.submitPayment = function () {
        console.log("Selected File:", $scope.paymentProof);
        if (!$scope.paymentProof) {
            Swal.fire("Error", "Please upload payment proof.", "error");
            return;
        }
        Swal.fire({
            title: "Confirm Payment",
            text: "Are you sure you want to submit the payment?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, submit",
            cancelButtonText: "No, cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                var formData = new FormData();
                formData.append("appointmentID", $scope.selectedAppointment.appointmentID);
                formData.append("paymentProof", $scope.paymentProof);

                var submitPayment = PaymentService.submitPayment(formData);
                submitPayment.then(function (result) {
                    if (result.success) {
                        Swal.fire("Success", "Payment submitted successfully.", "success");
                        $('#paymentModal').modal('close');
                        $timeout(function () {
                            $scope.getPatientAppointments(); // Refresh the list
                        });
                    } else {
                        Swal.fire("Error", result.message, "error");
                    }
                }).catch(function (error) {
                    console.error("Payment submission error", error);
                    Swal.fire("Error", "Unable to submit payment. Please try again.", "error");
                });
            }
        });
    }

    //===== For doctor confirmation =====
    $scope.openImageModal = function (imageUrl, appointmentID) {
        $scope.modalImage = imageUrl;
        $scope.appointmentID = appointmentID;
        setTimeout(function () {
            const modalElem = document.getElementById('imageModal');

            if (!modalElem.classList.contains('modal-initialized')) {
                M.Modal.init(modalElem);
                modalElem.classList.add('modal-initialized');
            }

            const instance = M.Modal.getInstance(modalElem);
            instance.open();
        }, 100); // Ensure digest cycle completes
    };

    $scope.verifyPayment = function (appointmentID) {
        Swal.fire({
            title: "Verify Payment",
            text: "Are you sure you want to verify this payment?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, verify",
            cancelButtonText: "No, cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                PaymentService.verifyPayment(appointmentID).then(function (result) {
                    if (result.success) {
                        Swal.fire("Success", "Payment verified successfully.", "success");
                        $timeout(function () {
                            $scope.getPayments(); // Refresh the list
                            const modalElem = document.getElementById('imageModal');

                            if (!modalElem.classList.contains('modal-initialized')) {
                                M.Modal.init(modalElem);
                                modalElem.classList.add('modal-initialized');
                            }

                            const instance = M.Modal.getInstance(modalElem);
                            instance.close(); // Close the image modal after verification
                        });
                    } else {
                        Swal.fire("Error", result.message, "error");
                    }
                }, function (error) {
                    console.error("Verification error", error);
                    Swal.fire("Error", "Unable to verify payment. Please try again.", "error");
                });
            }
        });
    }

});