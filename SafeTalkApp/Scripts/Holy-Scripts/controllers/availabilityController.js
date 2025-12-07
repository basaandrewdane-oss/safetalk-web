app.controller('AvailabilityController', ["$scope", "AccountService", "AvailabilityService", function ($scope, AccountService, AvailabilityService) {
    $scope.availabilities = [];
    $scope.daysOfWeek = [];

    // Load existing availability
    $scope.loadAvailability = function () {
        AvailabilityService.getAvailability()
            .then(function (res) {
                if (res.success) {
                    $scope.availabilities = res.data.map(function (a) {
                        // Convert "HH:mm" string to Date object
                        if (a.availabilityStart) {
                            const [h, m] = a.availabilityStart.split(':');
                            a.availabilityStart = new Date(1970, 0, 1, h, m);
                        }
                        if (a.availabilityEnd) {
                            const [h, m] = a.availabilityEnd.split(':');
                            a.availabilityEnd = new Date(1970, 0, 1, h, m);
                        }
                        // 🕒 Auto-fill hours and minutes based on slotDuration (in minutes)
                        if (a.slotDuration) {
                            a.slotHours = Math.floor(a.slotDuration / 60);
                            a.slotMinutes = a.slotDuration % 60;
                        } else {
                            a.slotHours = 0;
                            a.slotMinutes = 0;
                        }
                        return a;
                    });
                } else {
                    Swal.fire('Error', 'Failed to load availabilities.', 'error');
                }
            });
    };

    // Load days of the week
    $scope.getDaysOfWeek = function () {
        AccountService.getDaysOfWeek().then(function (result) {
            $scope.daysOfWeek = result.data;
        });
    };

    // Add a new availability row
    $scope.addAvailability = function () {
        $scope.availabilities.push({
            dayID: '',
            startTime: '',
            endTime: '',
            slotDuration: 30
        });
    };

    // Remove a row
    $scope.removeAvailability = function (index) {
        $scope.availabilities.splice(index, 1);
    };

    // Save all availabilities
    $scope.saveAvailability = function () {
        if (!$scope.availabilities || $scope.availabilities.length === 0) {
            Swal.fire('Error', 'Please add at least one availability before saving.', 'error');
            return;
        }
        // ensure times are strings in "HH:mm" format
        $scope.availabilities.forEach(function (a) {
            if (a.availabilityStart instanceof Date) {
                a.availabilityStart = a.availabilityStart.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false });
            }
            if (a.availabilityEnd instanceof Date) {
                a.availabilityEnd = a.availabilityEnd.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false });
            }

            // Convert hours & minutes into total slot duration (in minutes)
            const hours = parseInt(a.slotHours || 0);
            const minutes = parseInt(a.slotMinutes || 0);
            a.slotDuration = (hours * 60) + minutes;
        });
        Swal.fire({
            title: 'Confirm Save',
            text: 'Are you sure you want to save these availabilities?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, save it!',
            cancelButtonText: 'No, cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                AvailabilityService.saveAvailability($scope.availabilities)
                    .then(function (res) {
                        if (res.success) {
                            Swal.fire('Success', 'Availabilities saved successfully.', 'success');
                            $scope.loadAvailability();
                        }
                    });
            }
        });
    };

    // Initialize
    $scope.loadAvailability();
    $scope.getDaysOfWeek();
}
]);
