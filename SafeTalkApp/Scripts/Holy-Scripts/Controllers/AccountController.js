app.controller("AccountController", ["$scope", "AccountService", "AdminService", function ($scope, AccountService, AdminService) {
    $scope.emailExists = false;
    $scope.forgot = {};
    $scope.token = "";
    $scope.roles = [];
    $scope.roleName = null; // used in ng-show
    $scope.selectedRoleId = null;
    let forgotModal;

    window.addEventListener('pageshow', function (event) {
        if (event.persisted) {
            Swal.close(); // Close any open SweetAlert modals on back/forward navigation
        }
    });

    $scope.getRoles = function () {
        AccountService.getRoles().then(function (result) {
            $scope.roles = result.data.filter(r => r.roleName === "User" || r.roleName === "Doctor");
        }).catch(function (error) {
            console.error("Could not load roles", error.message);
            Swal.fire("Error", error.message || "Unable to load role list.", "error");
        });
    };

    $scope.onRoleChange = function () {
        const selectedRole = $scope.roles.find(r => r.roleID === $scope.selectedRoleId);
        if (selectedRole) {
            $scope.roleName = selectedRole.roleName; // triggers ng-show form
        }
    };

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
        if ($scope.signupForm && $scope.signupForm.$invalid) {
            Swal.fire("Error", "Please check required fields", "error");
            return;
        }
        // Fetch latest Terms & Privacy Policy
        var getTerms = AdminService.getTerms();
        getTerms.then(function (result) {
            if (result.success) {
                const termsHtml = result.data;

                Swal.fire({
                    title: "Terms and Conditions",
                    html: `
                    <div style="text-align:left; max-height:300px; overflow-y:auto; border:1px solid #ddd; padding:10px;">
                        ${termsHtml}
                    </div>
                    <br>
                    <label style="display:flex; align-items:center; gap:5px;">
                        <input type="checkbox" id="agreeTerms" />
                        <span>I have read and agree to the Terms & Privacy Policy</span>
                    </label>
                `,
                    showCancelButton: true,
                    confirmButtonText: "Continue",
                    preConfirm: () => {
                        const checked = document.getElementById('agreeTerms').checked;
                        if (!checked) {
                            Swal.showValidationMessage("You need to agree before continuing.");
                            return false;
                        }
                    }
                }).then(result => {
                    if (result.isConfirmed) {
                        $scope.createAccount(); // call your existing create account logic
                    }
                });
            } else {
                Swal.fire("Error", "Could not load terms and conditions.", "error");
            }
        });
    }

    $scope.createAccount = function () {
        Swal.fire({
            title: "Creating account...",
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
        const selectedRole = $scope.selectedRoleId

        if (selectedRole == 2) {
            var availability = $scope.selectedDays
                .map(day => {
                    const startDate = new Date($scope.availabilityTimes[day].start);
                    const endDate = new Date($scope.availabilityTimes[day].end);
                    const pad = n => n.toString().padStart(2, '0');
                    const hours = parseInt($scope.slotHours || 0);
                    const minutes = parseInt($scope.slotMinutes || 0);
                    const totalMinutes = (hours * 60) + minutes;

                    return {
                        dayID: day,
                        availabilityStart: `${pad(startDate.getHours())}:${pad(startDate.getMinutes())}`,
                        availabilityEnd: `${pad(endDate.getHours())}:${pad(endDate.getMinutes())}`,
                        fee: $scope.fee,
                        slotDuration: totalMinutes
                    };
                })
                .filter(a => a.availabilityStart && a.availabilityEnd && a.slotDuration > 0);
        }

        var formatDate = date => {
            const d = new Date(date);
            const pad = n => n.toString().padStart(2, '0');
            return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
        };

        var userData = {
            roleID: selectedRole,
            firstName: $scope.firstName,
            middleName: $scope.middleName,
            lastName: $scope.lastName,
            birthDate: formatDate($scope.birthDate),
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

    $scope.checkEmailExists = function (email) {
        var checkEmail = AccountService.checkEmailExists(email);
        checkEmail.then(function (result) {
            if (result.success) {
                $scope.emailExists = result.data;
                $scope.signupForm.email.$setValidity('emailExists', !$scope.emailExists);
            }
        });
    }

    $scope.login = function () {
        if ($scope.loginForm.$invalid) {
            Swal.fire("Error", "Please check required fields", "error");
            return;
        }

        var loginData = {
            email: $scope.email,
            password: $scope.password
        };

        // 👉 Show loading immediately
        Swal.fire({
            title: "Logging in...",
            text: "Please wait while we verify your credentials.",
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

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
                $scope.resetForm();
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

    $scope.openForgotPassword = function () {
        forgotModal.open()
    };

    $scope.submitForgotPassword = function () {
        if (!$scope.forgot.email) {
            Swal.fire("Warning", "Please enter your email address.", "warning");
            return;
        }

        AccountService.forgotPassword($scope.forgot.email).then(function (result) {
            if (result.success) {
                Swal.fire("Success", result.message, "success");
                forgotModal.close()
            } else {
                Swal.fire("Error", result.message, "error");
            }
        });
    };

    $scope.init = function () {
        // Extract token from query string
        const params = new URLSearchParams(window.location.search);
        $scope.token = params.get("token");
    };

    $scope.resetPassword = function () {
        if (!$scope.password.newPassword || !$scope.password.confirmPassword) {
            Swal.fire("Warning", "Please fill in both password fields.", "warning");
            return;
        }
        if ($scope.password.newPassword !== $scope.password.confirmPassword) {
            Swal.fire("Error", "Passwords do not match.", "error");
            return;
        }

        resetData = {
            token: $scope.token,
            newPassword: $scope.password.newPassword
        }

        AccountService.resetPassword(resetData).then(function (response) {
            if (response.success) {
                Swal.fire("Success", response.message, "success").then(() => {
                    window.location.href = "/Account/Login";
                });
            } else {
                Swal.fire("Error", response.message, "error");
            }
        });
    };

    $scope.resetForm = function () {
        if ($scope.signupForm) {
            $scope.signupForm.$setPristine();
            $scope.signupForm.$setUntouched();
            $scope.firstName = '';
            $scope.middleName = '';
            $scope.lastName = '';
            $scope.birthDate = '';
            $scope.selectedGender = '';
            $scope.phoneNumber = '';
            $scope.email = '';
            $scope.password = '';
            $scope.confirmPassword = '';

            // Clear doctor-only fields
            $scope.licenseNumber = '';
            $scope.specialization = '';
            $scope.selectedDays = [];
            $scope.availabilityTimes = {};
            $scope.$apply();
        }
        setTimeout(function () {
            M.updateTextFields(); // refresh label positions
        }, 100);
    }

    angular.element(document).ready(function () {
        var elem = document.getElementById('forgotPasswordModal');
        forgotModal = M.Modal.init(elem, {
            dismissible: true,
            onOpenEnd: function () {
            }
        });
    });
}
]);