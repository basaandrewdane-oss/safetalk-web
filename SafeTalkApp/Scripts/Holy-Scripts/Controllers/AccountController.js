app.controller("AccountController", function ($scope, AccountService) {
    $scope.emailExists = false;

    window.addEventListener('pageshow', function (event) {
        if (event.persisted) {
            Swal.close(); // Close any open SweetAlert modals on back/forward navigation
        }
    });

    $scope.getRoles = function () {
        var getRoles = AccountService.getRoles();
        getRoles.then(function (result) {
            const filteredRoles = result.data.filter(r => r.roleName === "User" || r.roleName === "Doctor");
            var inputOptions = {};
            filteredRoles.forEach(r => {
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
                    var chosenRoleId = result.value;
                    var chosenRole = inputOptions[result.value];
                    Swal.fire({
                        title: `You selected: ${chosenRole}`,
                        text: "You will be redirected to the signup page.",
                        icon: "info",
                        showCancelButton: true,
                        confirmButtonText: "Proceed",
                        cancelButtonText: "Cancel"
                    }).then((result) => {
                        if (result.isConfirmed) {
                            sessionStorage.setItem("selectedRole", chosenRoleId);

                            // ✅ SweetAlert2 loading modal
                            Swal.fire({
                                title: "Redirecting...",
                                allowOutsideClick: false,
                                didOpen: () => {
                                    Swal.showLoading();
                                }
                            });
                            setTimeout(() => {
                                if (chosenRole === "User" || chosenRole === "Doctor") {
                                    window.location.href = "/Account/Signup/" + chosenRole.toLowerCase();
                                }
                            }, 200); // Redirect after 2 seconds
                        }
                    })
                }
            });
        }).catch(function (error) {
            console.error("Could not load roles", error.message);
            Swal.fire("Error", error.message || "Unable to load role list.", "error");
        });
    }

    $scope.getGenders = function () {
        var getGenders = AccountService.getGenders();
        getGenders.then(function (result) {
            $scope.genders = result.data;
        }, function (error) {
            console.error("Error loading genders", error.message);
        });
    }

    $scope.getDaysOfWeek = function () {
        AccountService.getDaysOfWeek().then(function (result) {
            $scope.daysOfWeek = result.data;
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
            inputPlaceholder: `I agree with the <a href="https://example.com/terms" target="_blank" style="color: #3085d6;">terms and conditions</a>`,
            confirmButtonText: `Continue&nbsp;<i class="fa fa-arrow-right"></i>`,
            inputValidator: (result) => {
                if (!result) {
                    return "You need to agree with T&C";
                }
            }
        }).then(({ value: accept }) => {
            if (accept) {
                // show loading
                Swal.fire({
                    title: "Creating account...",
                    allowOutsideClick: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });
                $scope.selectedRole = parseInt(sessionStorage.getItem("selectedRole"));

                if ($scope.selectedRole == 2) {
                    var availability = $scope.selectedDays
                        .map(day => {
                            const startDate = new Date($scope.availabilityTimes[day].start);
                            const endDate = new Date($scope.availabilityTimes[day].end);

                            const pad = n => n.toString().padStart(2, '0');

                            return {
                                dayID: day,
                                availabilityStart: `${pad(startDate.getHours())}:${pad(startDate.getMinutes())}`,
                                availabilityEnd: `${pad(endDate.getHours())}:${pad(endDate.getMinutes())}`,
                                fee: $scope.availabilityTimes[day].fee
                            };
                        })
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

                var createAccount = AccountService.registerUser(userData);
                createAccount.then(function (result) {
                    if (result.success) {
                        Swal.fire({
                            title: `Account created successfully!`,
                            icon: "success",
                            allowOutsideClick: false,
                            showCancelButton: false,
                            confirmButtonText: "Proceed to Login"
                        }).then((result) => {
                            if (result.isConfirmed) {
                                setTimeout(() => {
                                    window.location.href = "/Account/Login"; // Redirect to login page
                                }, 200); // Redirect after 1 second
                                sessionStorage.removeItem("selectedRole"); // Clear the session storage
                            }
                        });
                    }
                    else {
                        Swal.fire("Error", result.message, "error");
                    }
                }, function (error) {
                    Swal.fire("Something went wrong.", error, "error");
                });
            }
        });
    }

    $scope.checkEmailExists = function (email) {
        var checkEmail = AccountService.checkEmailExists(email);
        checkEmail.then(function (result) {
            if (result.success) {
                $scope.emailExists = result.data;
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
        login.then(function (result) {
            if (result.success) {
                Swal.fire({
                    title: "Login Successful",
                    text: "Redirecting to your dashboard...",
                    icon: "success",
                    showConfirmButton: false,
                    didOpen: () => {
                        Swal.showLoading();
                        setTimeout(() => {
                            window.location.href = "/Dashboard/Index";
                        }, 1500);
                    }
                })
            } else {
                Swal.fire("Login Failed", result.message, "warning");
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
                Swal.fire({
                    title: "Logging Out",
                    text: "Please wait...",
                    allowOutsideClick: false,
                    didOpen: () => {
                        Swal.showLoading();
                        setTimeout(() => {
                            window.location.href = "/Account/Logout";
                        }, 1500);
                    }
                });
            }
        });
    }

    $scope.cancelLogIn = function () {
        Swal.fire({
            title: "Are you sure?",
            text: "Your login process will be cancelled.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, cancel it.",
            cancelButtonText: "No, keep going.",
        }).then((result) => {
            if (result.isConfirmed) {
                window.location.href = "/Home/Index";
            }
        })
    }

    $scope.cancelSignUp = function () {
        Swal.fire({
            title: "Are you sure?",
            text: "Your signup process will be cancelled.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, cancel it.",
            cancelButtonText: "No, keep going.",
        }).then((result) => {
            if (result.isConfirmed) {
                sessionStorage.removeItem("selectedRole");
                window.location.href = "/Account/Login";
            }
        })
    }

    $scope.verifyEmail = function (token) {
        $scope.isLoading = true;
        var verifyEmail = AccountService.verifyEmail(token);
        verifyEmail.then(function (result) {
            $scope.isLoading = false;
            if (result.success) {
                Swal.fire({
                    title: "✅ Email Verified Successfully!",
                    text: "You can now login to your account.",
                    icon: "success"
                }).then(() => {
                    window.location.href = "/Account/Login";
                });
            } if (!result.success) {
                let msg = result.message || "Verification failed.";
                if (result.data.IsExpired) {
                    Swal.fire({
                        title: "❌ Verification Failed",
                        text: msg,
                        icon: "error",
                        showCancelButton: true,
                        confirmButtonText: "Resend Verification Email"
                    }).then((res) => {
                        if (res.isConfirmed) {
                            var resend = AccountService.resendVerificationEmail(result.data.Email);
                            resend.then(function (res) {
                                if (res.success) {
                                    Swal.fire("Success", res.message, "success");
                                } else {
                                    Swal.fire("Error", res.message, "error");
                                }
                            }).catch(function () {
                                Swal.fire("Error", "Something went wrong while resending verification email.", "error");
                            });
                        }
                    });
                } else {
                    Swal.fire({
                        title: "❌ Verification Failed",
                        text: msg,
                        icon: "error"
                    });
                }
            }
        }).catch(function (error) {
            $scope.isLoading = false;
            console.log(error);
            Swal.fire("Error", "Something went wrong while verifying.", "error");
        });
    };
});