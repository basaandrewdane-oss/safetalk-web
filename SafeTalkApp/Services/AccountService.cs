using MySqlX.XDevAPI.Common;
using SafeTalkApp.DTOs.Account;
using SafeTalkApp.DTOs.Profile;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.UI.WebControls;

namespace SafeTalkApp.Services
{
    public class AccountService : IAccountService
    {
        private readonly ISafeTalkAppContext _db;
        private readonly IEmailService _emailService;

        public AccountService(ISafeTalkAppContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public ApiResponse<object> RegisterUser(SignUpDTO signUp)
        {
            try
            {
                var token = Guid.NewGuid().ToString();

                var createUser = new UserTblModel()
                {
                    firstName = signUp.firstName,
                    middleName = signUp.middleName,
                    lastName = signUp.lastName,
                    birthDate = DateTime.ParseExact(
                        signUp.birthDate,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture),
                    genderID = signUp.genderID,
                    phoneNumber = signUp.phoneNumber,
                    licenseNumber = signUp.roleID == 2 ? signUp.licenseNumber : null,
                    specialization = signUp.roleID == 2 ? signUp.specialization : null,
                    email = signUp.email,
                    password = BCrypt.Net.BCrypt.HashPassword(signUp.password),
                    isVerified = signUp.roleID == 2 ? false : true, // Doctors are not verified by default
                    emailVerificationToken = token,
                    emailVerificationExpiry = DateTime.Now.AddHours(24),
                    isEmailVerified = false,
                    dateCreated = DateTime.Now,
                    dateUpdated = DateTime.Now,
                };
                _db.user_tbl.Add(createUser);
                _db.SaveChanges();

                var userRole = new UserRoleTblModel()
                {
                    userID = createUser.userID,
                    roleID = signUp.roleID,
                    dateCreated = DateTime.Now,
                    dateUpdated = DateTime.Now,
                };
                _db.user_role_tbl.Add(userRole);
                _db.SaveChanges();

                if (signUp.roleID == 2 && signUp.availability != null)
                {
                    foreach (var slot in signUp.availability)
                    {
                        var userAvailability = new UserAvailabilityTblModel()
                        {
                            userID = createUser.userID,
                            dayID = slot.dayID,
                            availabilityStart = TimeSpan.Parse(slot.availabilityStart),
                            availabilityEnd = TimeSpan.Parse(slot.availabilityEnd),
                            fee = slot.fee,
                            slotDuration = slot.slotDuration,
                            dateCreated = DateTime.Now,
                            dateUpdated = DateTime.Now
                        };
                        _db.user_availability_tbl.Add(userAvailability);
                    }
                    _db.SaveChanges();
                }

                try
                {
                    var verificationLink = $"https://safe-talk.online/Account/VerifyEmail?token={HttpUtility.UrlEncode(token)}";
                    _emailService.SendVerificationEmail(signUp.email, verificationLink);
                }
                catch (Exception emailEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Email Sending Error: {emailEx.Message}\n{emailEx.StackTrace}");
                }
                return ApiResponse<object>.Ok("Registration successful! Please verify your email.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: " + ex.Message);
                return ApiResponse<object>.Fail($"An unexpected error occurred. Please try again. Error: {ex.Message}");
            }
        }

        public ApiResponse<bool> EmailExists(string email)
        {
            try
            {
                var exists = _db.user_tbl.Any(u => u.email == email);
                return ApiResponse<bool>.Ok(exists);
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: " + ex.Message);
                return ApiResponse<bool>.Fail($"An unexpected error occurred. Please try again. Error: {ex.Message}");
            }
        }

        public ApiResponse<UserDTO> AuthenticateUser(LoginDTO login)
        {
            try
            {
                var user = _db.user_tbl.FirstOrDefault(u => u.email == login.email);

                if (user == null)
                {
                    return ApiResponse<UserDTO>.Fail("Invalid email or password.");
                }

                // Check if user is locked
                if (user.isLocked)
                {
                    if (user.lockoutEnd != null && user.lockoutEnd > DateTime.Now)
                    {
                        var minutesLeft = user.lockoutEnd.HasValue ? (user.lockoutEnd.Value - DateTime.Now).Minutes : 0;
                        return ApiResponse<UserDTO>.Fail($"Your account is locked. Try again in {minutesLeft} minute(s).");
                    }
                    else
                    {
                        // Unlock after cooldown
                        user.isLocked = false;
                        user.failedLoginAttempts = 0;
                        user.lockoutEnd = null;
                        _db.SaveChanges();
                    }
                }

                // Check password
                if (!BCrypt.Net.BCrypt.Verify(login.password, user.password))
                {
                    user.failedLoginAttempts += 1;
                    user.lastFailedLoginAttempt = DateTime.Now;

                    // Lock user after 5 failed attempts
                    if (user.failedLoginAttempts >= 5)
                    {
                        user.isLocked = true;
                        user.lockoutEnd = DateTime.Now.AddMinutes(15);
                        _db.SaveChanges();
                        return ApiResponse<UserDTO>.Fail("Too many failed attempts. Your account is locked for 15 minutes.");
                    }

                    _db.SaveChanges();
                    return ApiResponse<UserDTO>.Fail($"Invalid email or password. Attempts left: {5 - user.failedLoginAttempts}");
                }

                // ✅ Successful login
                user.failedLoginAttempts = 0;
                user.isLocked = false;
                user.lockoutEnd = null;
                _db.SaveChanges();

                if (!user.isEmailVerified)
                {
                    return ApiResponse<UserDTO>.Fail("Please verify your email before logging in. Check your email inbox or spam section.");
                }

                if (user.isVerified == false)
                {
                    return ApiResponse<UserDTO>.Fail("Your account is pending verification. Please wait for an administrator to verify your account.");
                }

                // Get role name
                var roleName = (from ur in _db.user_role_tbl
                                join r in _db.role_tbl on ur.roleID equals r.roleID
                                where ur.userID == user.userID
                                select r.roleName).FirstOrDefault();

                // Create DTO
                var userDto = new UserDTO
                {
                    userID = user.userID,
                    email = user.email,
                    firstName = user.firstName,
                    lastName = user.lastName,
                    role = roleName,
                    profilePictureUrl = user.profilePictureUrl
                };

                return ApiResponse<UserDTO>.Ok(userDto);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login Error: {ex.Message}\n{ex.StackTrace}");
                Console.WriteLine("EXCEPTION: " + ex.Message);
                return ApiResponse<UserDTO>.Fail($"An unexpected error occurred. Please try again. {ex.Message}");
            }
        }

        public ClaimsIdentity GenerateUserIdentity(UserDTO user)
        {
            try
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.userID.ToString()),
                new Claim(ClaimTypes.Name, user.email),
                new Claim(ClaimTypes.GivenName, user.firstName + " " + user.lastName),
                new Claim(ClaimTypes.Role, user.role ?? "User"),
                new Claim("ProfilePictureUrl", user.profilePictureUrl ?? "/Uploads/ProfilePictures/default-avatar.png")
            };

                return new ClaimsIdentity(claims, "ApplicationCookie");
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: " + ex.Message);
                return null;
            }
        }

        public ApiResponse<VerifyEmailResultDTO> VerifyEmail(string token)
        {
            try
            {
                Console.WriteLine("Incoming token: " + token);
                var user = _db.user_tbl.FirstOrDefault(u => u.emailVerificationToken == token);

                if (user == null)
                {
                    return ApiResponse<VerifyEmailResultDTO>.Fail("Invalid token.");
                }

                if (user.emailVerificationExpiry < DateTime.Now)
                {
                    return ApiResponse<VerifyEmailResultDTO>.Fail(
                        "Token has expired. Please request a new verification email.",
                        new VerifyEmailResultDTO { IsVerified = false, IsExpired = true, Email = user.email }
                    );
                }

                user.isEmailVerified = true;
                user.emailVerificationToken = null;
                _db.SaveChanges();

                return ApiResponse<VerifyEmailResultDTO>.Ok(
                    new VerifyEmailResultDTO { IsVerified = true, IsExpired = false, Email = user.email },
                    "Email verified successfully."
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<VerifyEmailResultDTO>.Fail($"An unexpected error occurred. Please try again. {ex.Message}");
            }
        }

        public ApiResponse<ResendVerficationResultDTO> ResendVerificationEmail(string email)
        {
            try
            {
                var user = _db.user_tbl.FirstOrDefault(u => u.email == email);

                if (user == null)
                {
                    return ApiResponse<ResendVerficationResultDTO>.Fail("User not found.");
                }

                if (user.isEmailVerified)
                {
                    return ApiResponse<ResendVerficationResultDTO>.Fail("Email is already verified.");
                }

                // Generate a new token + expiry
                var newToken = Guid.NewGuid().ToString();
                user.emailVerificationToken = newToken;
                user.emailVerificationExpiry = DateTime.Now.AddHours(24);
                user.dateUpdated = DateTime.Now;

                _db.SaveChanges();

                var verificationLink = $"https://safe-talk.online/Account/VerifyEmail?token={newToken}";
                _emailService.SendVerificationEmail(user.email, verificationLink);

                return ApiResponse<ResendVerficationResultDTO>.Ok(new ResendVerficationResultDTO
                {
                    Email = user.email,
                    Expiry = user.emailVerificationExpiry
                },
                "A new verification email has been sent. Please check your inbox.");
            }
            catch (Exception ex)
            {
                return ApiResponse<ResendVerficationResultDTO>.Fail($"Error resending verification email. Please try again later. {ex.Message}");
            }
        }

        public ApiResponse<IEnumerable<object>> GetRoles()
        {
            try
            {
                var roles = _db.role_tbl
                    .Select(r => new { r.roleID, r.roleName })
                    .ToList();

                return ApiResponse<IEnumerable<object>>.Ok(roles);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<object>>.Fail("Error retrieving roles " + ex.Message);
            }
        }

        public ApiResponse<IEnumerable<object>> GetGenders()
        {
            try
            {
                var genders = _db.gender_tbl
                    .Select(g => new { g.genderID, g.gender })
                    .ToList();

                return ApiResponse<IEnumerable<object>>.Ok(genders);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<object>>.Fail($"Error retrieving genders: {ex.Message}");
            }
        }

        public ApiResponse<IEnumerable<object>> GetDaysOfWeek()
        {
            try
            {
                var days = _db.days_of_week_tbl
                    .Select(d => new { d.dayID, d.day })
                    .ToList();

                return ApiResponse<IEnumerable<object>>.Ok(days);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<object>>.Fail($"Error retrieving days of the week: {ex.Message}");
            }
        }

        public ApiResponse<bool> ForgotPassword(string email)
        {
            var user = _db.user_tbl.FirstOrDefault(u => u.email == email);
            if (user == null)
            {
                return ApiResponse<bool>.Fail("User not found.");
            }

            try
            {
                var resetToken = Guid.NewGuid().ToString();
                user.emailVerificationToken = resetToken;
                user.emailVerificationExpiry = DateTime.Now.AddHours(1);
                _db.SaveChanges();
                var resetLink = $"https://safe-talk.online/Account/ResetPassword?token={HttpUtility.UrlEncode(resetToken)}";
                _emailService.SendPasswordResetEmail(user.email, resetLink);
                return ApiResponse<bool>.Ok(true, "Password reset email sent. Please check your inbox.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Error sending password reset email: {ex.Message}");
            }
        }

        public ApiResponse<bool> ResetPassword(ResetPasswordDTO resetData)
        {
            var user = _db.user_tbl.FirstOrDefault(u => u.emailVerificationToken == resetData.token);
            if (user == null)
            {
                return ApiResponse<bool>.Fail("Invalid token.");
            }
            if (user.emailVerificationExpiry < DateTime.Now)
            {
                return ApiResponse<bool>.Fail("Token has expired. Please request a new password reset email.");
            }
            try
            {
                user.password = BCrypt.Net.BCrypt.HashPassword(resetData.newPassword);
                user.emailVerificationToken = null;
                user.emailVerificationExpiry = null;
                _db.SaveChanges();
                return ApiResponse<bool>.Ok(true, "Password has been reset successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Error resetting password: {ex.Message}");
            }
        }
    }
}