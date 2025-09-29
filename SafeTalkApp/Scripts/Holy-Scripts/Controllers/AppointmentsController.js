app.controller("AppointmentsController", function ($scope, $timeout, AppointmentService) {
    $scope.appointmentFilter = "active";
    // ===== User Appointments =====
    $scope.getDoctors = function () {
        var getDoctors = AppointmentService.getDoctors();
        getDoctors.then(function (result) {
            $scope.doctors = result.data;
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
        getDoctorsAvailability.then(function (result) {
            $scope.doctorAvailability = result.data;
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
            $scope.selectedDate = null;

            // Clear the input text
            var dateInput = document.getElementById('appointmentDate');
            if (dateInput) {
                dateInput.value = '';
            }
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

    $scope.confirmBooking = function () {
        $('#reviewModal').modal('close'); // close modal
        $scope.bookAppointment();         // call existing submit
    };

    $scope.bookAppointment = function () {
        if (!$scope.selectedSlot || !$scope.selectedDate) {
            Swal.fire("Error", "Please select a date and time slot.", "error");
            return;
        }
        var appointmentData = {
            doctorID: $scope.selectedDoctor.userID,
            date: $scope.selectedDate,
            startTime: $scope.selectedSlot.start,
            endTime: $scope.selectedSlot.end,
            fee: $scope.fee,
            chiefComplaint: $scope.appointment.chiefComplaint
        };
        AppointmentService.bookAppointment(appointmentData).then(function (result) {
            if (result.success) {
                Swal.fire({
                    icon: "success",
                    title: "Appointment booked successfully.",
                    confirmButtonText: "Proceed to Appointments"
                }).then(() => {
                    window.location.href = "/Appointment/Appointments";
                });
            } else {
                Swal.fire("Error", result.message, "error");
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
        getPatientAppointments.then(function (result) {
            $scope.appointments = result.data;
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
                AppointmentService.cancelAppointment(appointmentID).then(function (result) {
                    if (result.success) {
                        Swal.fire("Success", "Appointment cancelled successfully.", "success");
                        $timeout(function () {
                            $scope.getPatientAppointments(); // Refresh the list
                        });
                    } else {
                        Swal.fire("Error", result.message, "error");
                    }
                }, function (error) {
                    console.error("Cancellation error", error);
                    Swal.fire("Error", "Unable to cancel appointment. Please try again.", "error");
                });
            }
        });
    }

    $scope.rebookAppointment = function () {
        window.location.href = "/Appointment/Book";
    }

    $scope.filteredAppointments = function () {
        if (!$scope.appointments) return [];

        return $scope.appointments.filter(function (appt) {
            switch ($scope.appointmentFilter) {
                case "all":
                    return appt.status !== 6; // all except completed
                case "active":
                    return appt.status !== 4 && appt.status !== 5 && appt.status !== 6;
                case "includeRejected":
                    return appt.status !== 5 && appt.status !== 6;
                case "includeCanceled":
                    return appt.status !== 4 && appt.status !== 6;
                default:
                    return true;
            }
        });
    };

    // ===== Doctor Appointments =====
    $scope.getDoctorAppointments = function () {
        if ($.fn.DataTable.isDataTable('#appointmentsTable')) {
            $('#appointmentsTable').DataTable().destroy();
        }
        var getDoctorAppointments = AppointmentService.getDoctorAppointments();
        getDoctorAppointments.then(function (result) {
            $scope.appointments = result.data;
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
                AppointmentService.approveAppointment(appointmentID).then(function (result) {
                    if (result.success) {
                        Swal.fire("Success", "Appointment approved successfully.", "success");
                        $timeout(function () {
                            $scope.getDoctorAppointments(); // Refresh the list
                        });
                    } else {
                        Swal.fire("Error", result.message, "error");
                    }
                }, function (error) {
                    console.error("Approval error", error);
                    Swal.fire("Error", "Unable to approve appointment. Please try again.", "error");
                });
            }
        });
    }

    $scope.rejectAppointment = function (appointmentID) {
        Swal.fire({
            title: "Reject Appointment",
            text: "Are you sure you want to reject this appointment?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, reject",
            cancelButtonText: "No, cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                AppointmentService.rejectAppointment(appointmentID).then(function (result) {
                    if (result.success) {
                        Swal.fire("Success", "Appointment rejected successfully.", "success");
                        $timeout(function () {
                            $scope.getDoctorAppointments(); // Refresh the list
                        });
                    } else {
                        Swal.fire("Error", result.message, "error");
                    }
                }, function (error) {
                    console.error("Rejection error", error);
                    Swal.fire("Error", "Unable to reject appointment. Please try again.", "error");
                });
            }
        });
    }
});