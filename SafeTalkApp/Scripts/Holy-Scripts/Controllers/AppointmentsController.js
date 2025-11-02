app.controller("AppointmentsController", ["$scope", "$timeout", "AppointmentService", function ($scope, $timeout, AppointmentService) {
    $scope.appointmentFilter = "active";
    var instance;
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

    $scope.$watch('selectedDoctor', function (newVal, oldVal) {
        // Reset dependent data
        $scope.selectedDate = null;
        $scope.selectedSlot = null;
        $scope.selectedAvailability = null;
        $scope.fee = null;

        $scope.clearDatepicker();

        if (newVal && newVal.userID) {
            $scope.getDoctorsAvailability(newVal.userID);
        } else {
            $scope.doctorAvailability = [];
        }

        $timeout(function () {
            if (window.M && M.updateTextFields) {
                M.updateTextFields();
            }
        });
    });

    $scope.getDoctorsAvailability = function (userID) {
        var getDoctorsAvailability = AppointmentService.getDoctorsAvailability(userID);
        getDoctorsAvailability.then(function (result) {
            $scope.doctorAvailability = result.data;
            if ($scope.doctorAvailability.length > 0) {
                $scope.selectedSlotIndex = null; // no default selected yet
            }
            $timeout(function () {
                var tabs = document.querySelectorAll('.tabs');
                M.Tabs.init(tabs);
            }, 0);
        }).catch(function (error) {
            console.error("Error fetching availability", error);
        });
    }

    // Watch doctors loaded
    $scope.$watch('doctorAvailability', function (avail) {
        if (avail && avail.length > 0) {
            $scope.initDatepicker();
        }
    });

    // Initialize datepicker once doctors’ availability is loaded
    $scope.initDatepicker = function () {
        $timeout(function () {
            const elems = document.querySelectorAll('.datepicker');
            // Collect valid dayIDs (0=Sun, 1=Mon, ...)
            const validDays = $scope.doctorAvailability.map(a => a.dayID);
            M.Datepicker.init(elems, {
                format: 'yyyy-mm-dd',
                minDate: new Date(),
                autoClose: true,
                // Disable days not in doctor's availability
                disableDayFn: function (date) {
                    return !validDays.includes(date.getDay());
                },
                onSelect: function (date) {
                    $scope.$apply(function () {
                        $scope.selectedDate = date;
                        $scope.updateAvailabilityByDate(date);
                    });
                }
            });
        }, 200);
    };

    $scope.clearDatepicker = function () {
        const elems = document.querySelectorAll('.datepicker');
        elems.forEach(e => e.value = '');
    };

    // When date changes, show slots for that day
    $scope.updateAvailabilityByDate = function (date) {
        const dayID = date.getDay(); // 0 = Sunday, 1 = Monday, ...
        $scope.selectedAvailability = $scope.doctorAvailability.find(a => a.dayID === dayID) || null;
        $scope.selectedSlot = null;
        $scope.selectedDayID = null;
        $scope.fee = null;
    };

    $scope.$watch('selectedAvailability.slots', function (newSlots) {
        $timeout(function () {
            if (window.M && M.FormSelect) {
                M.FormSelect.init(document.querySelectorAll('select'));
            }
            if (window.M && M.updateTextFields) {
                M.updateTextFields();
            }
        }, 100);
    });

    $scope.updateSelectedSlot = function (slot) {
        if (slot) {
            $scope.selectedSlot = slot;
            $scope.selectedDayID = $scope.selectedAvailability.dayID;
            $scope.fee = $scope.selectedAvailability.fee;
        } else {
            $scope.selectedSlot = null;
            $scope.fee = null;
        }
    };

    $scope.confirmBooking = function () {
        $('#reviewModal').modal('close'); // close modal
        $scope.bookAppointment();         // call existing submit
    };

    $scope.bookAppointment = function () {
        if ($scope.bookingForm && $scope.bookingForm.$invalid) {
            Swal.fire("Error", "Please check required fields", "error");
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

    $scope.checkSlotAndReview = function () {
        if (!$scope.selectedDoctor || !$scope.selectedSlot || !$scope.selectedDate) {
            M.toast({ html: 'Please select doctor, date, and time slot first.', classes: 'red' });
            return;
        }

        const payload = {
            doctorID: $scope.selectedDoctor.userID,
            date: $scope.selectedDate,
            startTime: $scope.selectedSlot.start,
            endTime: $scope.selectedSlot.end
        };

        // Call the backend check endpoint
        AppointmentService.checkSlotAvailability(payload).then(function (result) {
            if (result.success) {
                // Slot available → open review modal
                const modal = M.Modal.getInstance(document.getElementById('reviewModal'));
                modal.open();
            } else {
                // Slot booked → show message
                M.toast({ html: result.message || 'This slot is already booked.', classes: 'red' });
            }
        }, function () {
            M.toast({ html: 'Error checking slot availability.', classes: 'red' });
        });
    };

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
                    return appt.status !== 4 && appt.status !== 5 && appt.status !== 6 && appt.status !== 7;
                case "includeRejected":
                    return appt.status !== 5 && appt.status !== 6 && appt.status !== 7;
                case "includeCanceled":
                    return appt.status !== 4 && appt.status !== 6 && appt.status !== 7;
                case "includeMissed":
                    return appt.status !== 4 && appt.status !== 5 && appt.status !== 6;
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
        $scope.selectedAppointmentID = appointmentID;
        $scope.rejectReason = "";
        instance.open();
    }

    // Cancel the rejection
    $scope.cancelReject = function () {
        $scope.rejectReason = "";
        $scope.selectedAppointmentID = null;
        instance.close();
    };

    // Confirm and send to backend
    $scope.confirmReject = function () {
        if (!$scope.rejectReason.trim()) {
            M.toast({ html: 'Please provide a reason before rejecting.', classes: 'red' });
            return;
        }

        var data = {
            appointmentID: $scope.selectedAppointmentID,
            rejectReason: $scope.rejectReason
        }

        AppointmentService.rejectAppointment(data).then(function (result) {
            if (result.success) {
                M.toast({ html: 'Appointment rejected.', classes: 'green' });
                $scope.getDoctorAppointments(); // refresh list
            } else {
                M.toast({ html: 'Failed to reject appointment.', classes: 'red' });
            }
        }).catch(function () {
            M.toast({ html: 'Error processing request.', classes: 'red' });
        }).finally(function () {
            instance.close();
        });
    };

    // Init
    angular.element(document).ready(function () {
        var modal = document.getElementById('rejectModal');
        instance = M.Modal.init(modal);
    });
}
]);