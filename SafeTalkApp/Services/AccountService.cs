using Microsoft.Owin.Security;
using SafeTalkApp.DTOs.Account;
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
        private readonly SafeTalkAppContext _db;
        private readonly IEmailService _emailService;

        public AccountService(SafeTalkAppContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public SignUpResult RegisterUser(SignUpDTO signUp)
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
                            availabilityStart = slot.availabilityStart,
                            availabilityEnd = slot.availabilityEnd,
                            fee = slot.fee,
                            dateCreated = DateTime.Now,
                            dateUpdated = DateTime.Now
                        };
                        _db.user_availability_tbl.Add(userAvailability);
                    }
                    _db.SaveChanges();
                }
                //_emailService.SendVerificationEmail(createUser.email, token);
                return new SignUpResult { success = true, message = "Account Created Successfully" };
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: " + ex.Message);
                return new SignUpResult { success = false, message = "Error!" + ex.Message };
            }
        }

        public LoginDTO AuthenticateUser(LoginDTO login)
        {
            try
            {
                var user = _db.user_tbl.FirstOrDefault(u => u.email == login.email);

                if (user == null)
                {
                    return new LoginDTO { success = false, message = "Invalid email or password." };
                }

                if (!user.isEmailVerified)
                {
                    return new LoginDTO { success = false, message = "Please verify your email before logging in." };
                }

                if (user != null && BCrypt.Net.BCrypt.Verify(login.password, user.password))
                {

                    var roleName = (from ur in _db.user_role_tbl
                                    where ur.userID == user.userID
                                    join r in _db.role_tbl on ur.roleID equals r.roleID
                                    select r.roleName)
                                    .FirstOrDefault();

                    if (roleName == "Doctor" && user.isVerified == false)
                    {
                        return new LoginDTO { success = true, role = roleName, verified = false };
                    }

                    return new LoginDTO
                    {
                        success = true,
                        role = roleName,
                        verified = true,
                        userID = user.userID,
                        email = user.email,
                        firstName = user.firstName,
                        lastName = user.lastName
                    };
                }
                else
                {
                    // Invalid credentials
                    return new LoginDTO { success = false, message = "Invalid email or password." };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login Error: {ex.Message}\n{ex.StackTrace}");

                return new LoginDTO { success = false, message = "An unexpected error occurred. Please try again." };
            }
        }

        public bool VerifyEmail(string token)
        {
            var user = _db.user_tbl.FirstOrDefault(u => u.emailVerificationToken == token);
            if (user == null)
            {
                return false; // Or a custom error view
            }

            user.isEmailVerified = true;
            user.emailVerificationToken = null; // Optional: clear the token
            _db.SaveChanges();

            return true; // Success view
        }
    }
}