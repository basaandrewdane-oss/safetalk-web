app.controller("ConsultationController", function ($scope, $timeout, ConsultationService) {

    $scope.goToConsultation = function (appointmentID) {
        window.location.href = '/Consultation/ChatRoom?appointmentID=' + appointmentID;
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
});