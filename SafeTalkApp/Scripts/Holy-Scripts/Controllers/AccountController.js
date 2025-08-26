app.controller("AccountController", function ($scope, $timeout, AccountService) {

    $scope.getRoles = function () {
        var getRoles = AccountService.getRoles();
        getRoles.then(function (response) {
            const roles = response.data.filter(r => r.roleName === "User" || r.roleName === "Doctor");

            const inputOptions = {};
            roles.forEach(r => {
                inputOptions[r.roleID] = r.roleName;
            });

            Swal.fire({
                title: "Choose Role",
                input: "select",
                inputOptions: inputOptions,
                inputPlaceholder: "Select a role",
                showCancelButton: true,

                inputValidator: (value) => {
                    return new Promise((resolve) => {
                        if (value) {
                            resolve(); // valid selection
                        } else {
                            resolve("Please select a role.");
                        }
                    });
                }
            }).then(result => {
                if (result.isConfirmed) {
                    var chosenRole = inputOptions[result.value];
                    Swal.fire({
                        title: `You selected: ${chosenRole}`,
                        text: "Redirecting...",
                        icon: "success",
                        timer: 2000,           // Show this for 2 seconds
                        showConfirmButton: false
                    });

                    sessionStorage.setItem("selectedRole", result.value);

                    setTimeout(() => {
                        if (chosenRole === "User" || chosenRole === "Doctor") {
                            window.location.href = "/Account/Signup/" + chosenRole.toLowerCase();
                        }
                    }, 2000); // Redirect after 2 seconds
                }
            });
        }).catch(function (error) {
            console.error("Could not load roles", error);
            Swal.fire("Error", "Unable to load role list.", "error");
        });
    }

    $scope.getGenders = function () {
        var getGenders = AccountService.getGenders();
        getGenders.then(function (response) {
            $scope.genders = response.data;
        }, function (error) {
            console.error("Error loading genders", error);
        });
    }

    $scope.getDaysOfWeek = function () {
        AccountService.getDaysOfWeek().then(function (response) {
            $scope.daysOfWeek = response.data;
            $scope.availabilityTimes = {};
            // Prepopulate times
            $scope.daysOfWeek.forEach(d => {
                $scope.availabilityTimes[d.dayID] = { start: null, end: null };
            });
            $scope.getDayName = function (dayID) {
                const day = $scope.daysOfWeek.find(d => d.dayID === dayID);
                return day ? day.day : 'Unknown';
            };
        });
    };

    $scope.signUp = function () {
        if (($scope.userForm && $scope.userForm.$invalid) ||
            ($scope.doctorForm && $scope.doctorForm.$invalid)) {
            Swal.fire("Error", "Please check required fields", "error");
            return;
        }
        Swal.fire({
            title: "Terms and Conditions",
            input: "checkbox",
            inputValue: 1,
            inputPlaceholder: `I agree with the <a href="https://example.com/terms" target="_blank" style="color: #3085d6;">terms and conditions</a>`,
            confirmButtonText: `Continue&nbsp;<i class="fa fa-arrow-right"></i>`,
            inputValidator: (result) => {
                return !result && "You need to agree with T&C";
            }
        }).then(({ value: accept }) => {
            if (accept) {
                $scope.selectedRole = parseInt(sessionStorage.getItem("selectedRole"));

                if ($scope.selectedRole == 2) {
                    var availability = $scope.selectedDays
                        .map(day => ({
                            dayID: day,
                            availabilityStart: $scope.availabilityTimes[day].start,
                            availabilityEnd: $scope.availabilityTimes[day].end,
                            fee: $scope.availabilityTimes[day].fee
                        }))
                        .filter(a => a.availabilityStart && a.availabilityEnd);
                }

                var userData = {
                    roleID: $scope.selectedRole,
                    firstName: $scope.firstName,
                    middleName: $scope.middleName,
                    lastName: $scope.lastName,
                    birthDate: $scope.birthDate,
                    genderID: $scope.selectedGender,
                    phoneNumber: $scope.phoneNumber,
                    licenseNumber: $scope.licenseNumber,
                    specialization: $scope.specialization,
                    availability: availability,
                    email: $scope.email,
                    password: $scope.password
                }

                var createAccount = AccountService.createAccount(userData);
                createAccount.then(function (response) {
                    if (response.data.success) {
                        Swal.fire({
                            title: `Account created successfully!`,
                            icon: "success",
                            showConfirmButton: false,
                        });

                        setTimeout(() => {
                            window.location.href = "/Account/Login"; // Redirect to login page
                        }, 1000); // Redirect after 1 second

                        sessionStorage.removeItem("selectedRole"); // Clear the session storage

                    }
                    else {
                        Swal.fire("Error", response.data.message, "error");
                    }

                }, function (error) {
                    Swal.fire("Something went wrong.");
                });
            }
        });
    }

    $scope.currentStep = 1;

    $scope.nextStep = function () {
        if ($scope.currentStep < 3) $scope.currentStep++;
    };

    $scope.prevStep = function () {
        if ($scope.currentStep > 1) $scope.currentStep--;
    };

    $scope.login = function () {
        if ($scope.loginForm.$invalid) {
            Swal.fire("Error", "Please check required fields", "error");
            return;
        }

        var loginData = {
            email: $scope.email,
            password: $scope.password
        };

        var login = AccountService.login(loginData);
        login.then(function (response) {
            if (response.data.success) {
                const role = response.data.role;
                const verified = response.data.verified;

                if (role === "Doctor" && !verified) {
                    Swal.fire({
                        title: "Awaiting Verification",
                        text: "Your account is pending admin approval.",
                        icon: "info",
                        showConfirmButton: false
                    })
                }
                else {
                    Swal.fire({
                        title: "Login Successful",
                        text: "Redirecting to your dashboard...",
                        icon: "success",
                        timer: 2000,
                        showConfirmButton: false
                    }).then(() => {
                        setTimeout(() => {
                            window.location.href = "/Dashboard/Index";
                        }, 0);
                    });
                }
            } else {
                Swal.fire("Login Failed", response.data.message, "error");
            }
        }, function (error) {
            console.error("Login error", error);
            Swal.fire("Error", "Unable to login. Please try again.", "error");
        });
    }

    $scope.logout = function () {
        Swal.fire({
            title: "Are you sure?",
            text: "You will be logged out of your account.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, log me out",
            cancelButtonText: "No, keep me logged in"
        }).then((result) => {
            if (result.isConfirmed) {
                AccountService.logout().then(function () {
                    Swal.fire("Logged Out", "You have been successfully logged out.", "success");
                    setTimeout(() => {
                        window.location.href = "/Account/Logout";
                    }, 1000);
                }, function (error) {
                    console.error("Logout error", error);
                    Swal.fire("Error", "Unable to log out. Please try again.", "error");
                });
            }
        });
    }
});