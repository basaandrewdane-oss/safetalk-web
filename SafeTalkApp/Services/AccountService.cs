using MySqlX.XDevAPI.Common;
using SafeTalkApp.DTOs.Account;
using SafeTalkApp.DTOs.Profile;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
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
                    birthDate = signUp.birthDate,
                    genderID = signUp.genderID,
                    phoneNumber = signUp.phoneNumber,
                    licenseNumber = signUp.roleID == 2 ? signUp.licenseNumber : null,
                    specialization = signUp.roleID == 2 ? signUp.specialization : null,
                    slotDuration = signUp.roleID == 2 ? signUp.slotDuration : null,
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
                            dateCreated = DateTime.Now,
                            dateUpdated = DateTime.Now
                        };
                        _db.user_availability_tbl.Add(userAvailability);
                    }
                    _db.SaveChanges();
                }

                try
                {
                    //var verificationLink = $"https://localhost:44338/Account/VerifyEmail?token={HttpUtility.UrlEncode(token)}";
                    //_emailService.SendVerificationEmail(signUp.email, verificationLink);
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
                return ApiResponse<bool>.Fail("An unexpected error occurred. Please try again.");
            }
        }

        public ApiResponse<UserDTO> AuthenticateUser(LoginDTO login)
        {
            try
            {
                var user = _db.user_tbl.FirstOrDefault(u => u.email == login.email);

                if (user == null)
                {
                    return ApiResponse<UserDTO>.Fail("Invalid email.");
                }

                if (!user.isEmailVerified)
                {
                    return ApiResponse<UserDTO>.Fail("Please verify your email before logging in.");
                }

                if (user.isVerified == false)
                {
                    return ApiResponse<UserDTO>.Fail("Your account is pending verification. Please wait for an administrator to verify your account.");
                }

                if (!BCrypt.Net.BCrypt.Verify(login.password, user.password))
                {
                    return ApiResponse<UserDTO>.Fail("Wrong password.");
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
                return ApiResponse<UserDTO>.Fail("An unexpected error occurred. Please try again.");
            }
        }

        public ClaimsIdentity GenerateUserIdentity(UserDTO user)
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

        public ApiResponse<VerifyEmailResultDTO> VerifyEmail(string token)
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

        public ApiResponse<ResendVerficationResultDTO> ResendVerificationEmail(string email)
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

            var verificationLink = $"https://localhost/Account/VerifyEmail?token={newToken}";
            _emailService.SendVerificationEmail(user.email, verificationLink);

            return ApiResponse<ResendVerficationResultDTO>.Ok(new ResendVerficationResultDTO
            {
                Email = user.email,
                Expiry = user.emailVerificationExpiry
            },
            "A new verification email has been sent. Please check your inbox.");
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
            catch
            {
                return ApiResponse<IEnumerable<object>>.Fail("Error retrieving roles");
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
            catch
            {
                return ApiResponse<IEnumerable<object>>.Fail("Error retrieving genders");
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
            catch
            {
                return ApiResponse<IEnumerable<object>>.Fail("Error retrieving days of the week");
            }
        }
    }
}