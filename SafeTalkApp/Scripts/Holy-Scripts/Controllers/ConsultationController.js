app.controller("ConsultationController", function ($scope, $timeout, ConsultationService) {

    $scope.goToConsultation = function (appointmentID) {
        window.location.href = '/Consultation/ChatRoom?appointmentID=' + appointmentID;
    };

    $scope.downloadTranscript = function (appointmentID) {
        ConsultationService.downloadTranscript(appointmentID)
            .then(function (response) {
                // Create a blob from the response
                var blob = new Blob([response.data], { type: "text/plain" });

                // Create a download link
                var downloadUrl = URL.createObjectURL(blob);
                var a = document.createElement("a");
                a.href = downloadUrl;
                a.download = "appointment_" + appointmentID + "_transcript.txt";
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(downloadUrl);
            })
            .catch(function () {
                alert("Error downloading transcript.");
            });
    };

    // ====== User Appointment =====
    $scope.getPatientConsultations = function () {
        if ($.fn.DataTable.isDataTable('#verifiedAppointmentsTable')) {
            $('#verifiedAppointmentsTable').DataTable().destroy();
        }
        var getPatientConsultations = ConsultationService.getPatientConsultations();
        getPatientConsultations.then(function (result) {
            $scope.verifiedAppointments = [];
            $scope.verifiedAppointments = result.data;
            $timeout(function () {
                $('#verifiedAppointmentsTable').DataTable({
                    responsive: true,
                    drawCallback: function () {
                        $('#verifiedAppointmentsTable_length select').formSelect();
                    }
                })
            })
        }).catch(function (error) {
            console.error("Error loading verified appointments", error);
            Swal.fire("Error", "Unable to load verified appointments.", "error");
        });
    }

    $scope.viewReferral = function (referralID) {
        ConsultationService.getReferralDetails(referralID)
            .then(function (result) {
                if (result.success) {
                    $scope.selectedReferral = result.data
                    var modalElem = document.getElementById('viewReferralModal');
                    var modalInstance = M.Modal.getInstance(modalElem);
                    if (!modalInstance) {
                        modalInstance = M.Modal.init(modalElem);
                    }
                    modalInstance.open();
                }
            })
    }

    // ====== Doctor Consultation =====
    $scope.getDoctorConsultations = function () {
        if ($.fn.DataTable.isDataTable('#verifiedAppointmentsTable')) {
            $('#verifiedAppointmentsTable').DataTable().destroy();
        }
        var getDoctorConsultations = ConsultationService.getDoctorConsultations();
        getDoctorConsultations.then(function (result) {
            $scope.verifiedAppointments = [];
            $scope.verifiedAppointments = result.data;
            $timeout(function () {
                $('#verifiedAppointmentsTable').DataTable({
                    responsive: true,
                    drawCallback: function () {
                        $('#verifiedAppointmentsTable_length select').formSelect();
                    }
                })
            })
        }).catch(function (error) {
            console.error("Error loading verified appointments", error);
            Swal.fire("Error", "Unable to load verified appointments.", "error");
        });
    }

    $scope.openReferralModal = function (consult) {
        $scope.selectedConsult = consult;
        $scope.referral = {}; // clear previous data

        var modalElem = document.getElementById('referralModal');

        // Initialize modal only once
        var modalInstance = M.Modal.getInstance(modalElem);
        if (!modalInstance) {
            modalInstance = M.Modal.init(modalElem, {
                onOpenEnd: function () {
                    // Reinitialize selects once modal content is visible
                    var selectElems = modalElem.querySelectorAll('select');
                    selectElems.forEach(e => {
                        var inst = M.FormSelect.getInstance(e);
                        if (inst) inst.destroy(); // remove old one if exists
                    });
                    M.FormSelect.init(selectElems);
                }
            });
        }

        modalInstance.open();
    };

    $scope.submitReferral = function (consult) {
        const referralData = {
            appointmentID: consult.appointmentID,
            doctorID: consult.doctorID,
            patientID: consult.patientID,
            reason: $scope.referral.reason,
            notes: $scope.referral.notes,
            urgencyLevel: parseInt($scope.referral.urgencyLevel),
            sentTo: $scope.referral.sentTo,
            status: 1 // default Pending
        };

        var createReferral = ConsultationService.createReferral(referralData);
        createReferral.then(function (result) {
            if (result.success) {
                M.toast({ html: 'Referral submitted successfully', classes: 'teal lighten-2' });
                $scope.referral = {}; // clear form
                var modal = M.Modal.getInstance(document.getElementById('referralModal'));
                modal.close();
            }
        })
            .catch(() => {
                M.toast({ html: 'Failed to submit referral', classes: 'red lighten-2' });
            });
    };
});