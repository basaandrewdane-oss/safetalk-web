app.controller("AppointmentsController", function ($scope, $timeout, AppointmentService) {
    // ===== User Appointments =====
    $scope.getDoctors = function () {
        var getDoctors = AppointmentService.getDoctors();
        getDoctors.then(function (response) {
            $scope.doctors = response.data;
        }).catch(function (error) {
            console.error("Error loading doctors", error);
            Swal.fire("Error", "Unable to load doctors.", "error");
        });
    }

    $scope.$watch('selectedDoctor', function (newVal) {
        if (newVal && newVal.userID) {
            $scope.getDoctorsAvailability(newVal.userID);
            $timeout(function () {
                if (window.M && M.updateTextFields) {
                    M.updateTextFields(); // Materialize 1.x
                }
            });
        } else {
            $scope.doctorAvailability = [];
            $timeout(function () {
                if (window.M && M.updateTextFields) {
                    M.updateTextFields();
                }
            });
        }
    });

    $scope.getDoctorsAvailability = function (userID) {
        var getDoctorsAvailability = AppointmentService.getDoctorsAvailability(userID);
        getDoctorsAvailability.then(function (response) {
            $scope.doctorAvailability = response.data;
            if ($scope.doctorAvailability.length > 0) {
                $scope.selectedSlotIndex = null; // no default selected yet
            }
        }).catch(function (error) {
            console.error("Error fetching availability", error);
        });
    }

    $scope.selectSlot = function (slot, dayID, fee) {

        $scope.selectedSlot = slot;
        $scope.selectedDayID = dayID;
        $scope.fee = fee;
    };

    $scope.$watch('selectedDayID', function (dayID) {
        if (typeof dayID === 'number') {
            // Delay to allow DOM to render the input
            setTimeout(function () {
                var elems = document.querySelectorAll('.datepicker');
                var dayOfWeek = dayID; // 0 = Sunday, 1 = Monday, etc.

                M.Datepicker.init(elems, {
                    format: 'yyyy-mm-dd',
                    minDate: new Date(),
                    autoClose: true,
                    disableDayFn: function (date) {
                        return date.getDay() !== dayOfWeek;
                    },
                    onSelect: function (date) {
                        // Update Angular model manually since Materialize doesn't trigger digest
                        $scope.$apply(function () {
                            $scope.selectedDate = date;
                        });
                    }
                });
            }, 100);
        }
    });

    $scope.bookAppointment = function () {
        if (!$scope.selectedSlot || !$scope.selectedDate) {
            Swal.fire("Error", "Please select a date and time slot.", "error");
            return;
        }
        var appointmentData = {
            doctorID: $scope.selectedDoctor.userID,
            date: $scope.selectedDate.toISOString().split('T')[0],
            startTime: $scope.selectedSlot.Start,
            endTime: $scope.selectedSlot.End,
            fee: $scope.fee
        };
        AppointmentService.bookAppointment(appointmentData).then(function (response) {
            if (response.data.success) {
                Swal.fire("Success", "Appointment booked successfully.", "success");
                // Optionally, redirect or clear the form
                setTimeout(() => {
                    window.location.href = "/Appointment/Appointments"; // Redirect to appointment history
                }, 1000); // Redirect after 1 second)
            } else {
                Swal.fire("Error", response.data.message, "error");
            }
        }).catch(function (error) {
            console.error("Booking error", error);
            Swal.fire("Error", "Unable to book appointment. Please try again.", "error");
        });
    }

    $scope.getPatientAppointments = function () {
        if ($.fn.DataTable.isDataTable('#appointmentsTable')) {
            $('#appointmentsTable').DataTable().destroy();
        }
        var getPatientAppointments = AppointmentService.getPatientAppointments();
        getPatientAppointments.then(function (response) {
            $scope.appointments = response.data;
            $timeout(function () {
                $('#appointmentsTable').DataTable({
                    responsive: true,
                    drawCallback: function () {
                        $('#appointmentsTable_length select').formSelect();
                    }
                })
            })
        }).catch(function (error) {
            console.error("Error loading appointments", error);
            Swal.fire("Error", "Unable to load appointments.", "error");
        });
    }

    $scope.cancelAppointment = function (appointmentID) {
        Swal.fire({
            title: "Cancel Appointment",
            text: "Are you sure you want to cancel this appointment?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, cancel",
            cancelButtonText: "No, keep it"
        }).then((result) => {
            if (result.isConfirmed) {
                AppointmentService.cancelAppointment(appointmentID).then(function (response) {
                    if (response.data.success) {
                        Swal.fire("Success", "Appointment cancelled successfully.", "success");
                        $timeout(function () {
                            $scope.getPatientAppointments(); // Refresh the list
                        });
                    } else {
                        Swal.fire("Error", response.data.message, "error");
                    }
                }, function (error) {
                    console.error("Cancellation error", error);
                    Swal.fire("Error", "Unable to cancel appointment. Please try again.", "error");
                });
            }
        });
    }

    $scope.filteredAppointments = function () {
        $scope.showCanceled = false;
        if (!$scope.appointments) return [];
        return $scope.appointments.filter(function (appt) {
            return $scope.showCanceled || appt.status !== 5;
        });
    };

    // ===== Doctor Appointments =====
    $scope.getDoctorAppointments = function () {
        if ($.fn.DataTable.isDataTable('#appointmentsTable')) {
            $('#appointmentsTable').DataTable().destroy();
        }
        var getDoctorAppointments = AppointmentService.getDoctorAppointments();
        getDoctorAppointments.then(function (response) {
            $scope.appointments = response.data;
            $timeout(function () {
                $('#appointmentsTable').DataTable({
                    responsive: true,
                    drawCallback: function () {
                        $('#appointmentsTable_length select').formSelect();
                    }
                })
            })
        }).catch(function (error) {
            console.error("Error loading appointments", error);
            Swal.fire("Error", "Unable to load appointments.", "error");
        });
    }

    $scope.approveAppointment = function (appointmentID) {
        Swal.fire({
            title: "Approve Appointment",
            text: "Are you sure you want to approve this appointment?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, approve",
            cancelButtonText: "No, cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                AppointmentService.approveAppointment(appointmentID).then(function (response) {
                    if (response.data.success) {
                        Swal.fire("Success", "Appointment approved successfully.", "success");
                        $timeout(function () {
                            $scope.getDoctorAppointments(); // Refresh the list
                        });
                    } else {
                        Swal.fire("Error", response.data.message, "error");
                    }
                }, function (error) {
                    console.error("Approval error", error);
                    Swal.fire("Error", "Unable to approve appointment. Please try again.", "error");
                });
            }
        });
    }
});