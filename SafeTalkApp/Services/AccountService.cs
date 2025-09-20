using Microsoft.Owin.Security;
using SafeTalkApp.DTOs.Account;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
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
                    email = signUp.email,
                    password = BCrypt.Net.BCrypt.HashPassword(signUp.password),
                    isVerified = signUp.roleID == 2 ? false : true, // Doctors are not verified by default
                    emailVerificationToken = token,
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
                //_emailService.SendVerificationEmail(createUser.email, token);
                return ApiResponse<object>.Ok("Registration successful! Please verify your email.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: " + ex.Message);
                return ApiResponse<object>.Fail($"An unexpected error occurred. Please try again. Error: {ex.Message}");
            }
        }

        public ApiResponse<LoginDTO> AuthenticateUser(LoginDTO login)
        {
            try
            {
                var user = _db.user_tbl.FirstOrDefault(u => u.email == login.email);

                if (user == null)
                {
                    return ApiResponse<LoginDTO>.Fail("Invalid email or password.");
                }

                if (!user.isEmailVerified)
                {
                    return ApiResponse<LoginDTO>.Fail("Please verify your email before logging in.");
                }

                if (user.isVerified == false)
                {
                    return ApiResponse<LoginDTO>.Fail("Your account is pending verification. Please wait for an administrator to verify your account.");
                }

                if (user != null && BCrypt.Net.BCrypt.Verify(login.password, user.password))
                {
                    return ApiResponse<LoginDTO>.Ok(new LoginDTO
                    {
                        success = true,
                        message = "Login successful.",
                        userID = user.userID,
                        firstName = user.firstName,
                        lastName = user.lastName,
                        email = user.email,
                        role = _db.user_role_tbl.Where(ur => ur.userID == user.userID).Join(_db.role_tbl, ur => ur.roleID, r => r.roleID, (ur, r) => r.roleName).FirstOrDefault(),
                    });
                }
                else
                {
                    // Invalid credentials
                    return ApiResponse<LoginDTO>.Fail("Invalid email or password.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login Error: {ex.Message}\n{ex.StackTrace}");
                Console.WriteLine("EXCEPTION: " + ex.Message);
                return ApiResponse<LoginDTO>.Fail("An unexpected error occurred. Please try again.");
            }
        }

        public ApiResponse<bool> VerifyEmail(string token)
        {
            var user = _db.user_tbl.FirstOrDefault(u => u.emailVerificationToken == token);
            if (user == null)
            {
                return ApiResponse<bool>.Fail("Invalid or expired token.");
            }

            user.isEmailVerified = true;
            user.emailVerificationToken = null; // Optional: clear the token
            _db.SaveChanges();

            return ApiResponse<bool>.Ok(true, "Email verified successfully.");
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