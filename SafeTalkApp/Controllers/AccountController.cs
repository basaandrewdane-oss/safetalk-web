using Microsoft.Owin.Security;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public JsonResult GetRoles()
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var roles = db.role_tbl.Select(r => new { r.roleID, r.roleName }).ToList();
                    return Json(roles, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return Json(new { success = false, message = "Error retrieving roles: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetGenders()
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var genders = db.gender_tbl.Select(g => new { g.genderID, g.gender }).ToList();
                    return Json(genders, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return Json(new { success = false, message = "Error retrieving genders: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetDaysOfWeek()
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var days = db.days_of_week_tbl.Select(d => new { d.dayID, d.day }).ToList();
                    return Json(days, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return Json(new { success = false, message = "Error retrieving days of week: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Logout()
        {
            var authManager = HttpContext.GetOwinContext().Authentication;
            authManager.SignOut("ApplicationCookie");
            return RedirectToAction("Login", "Account");
        }

        public ActionResult Waiting()
        {
            return View();
        }

        public ActionResult VerifyEmail(string token)
        {
            using (var db = new SafeTalkAppContext())
            {
                var user = db.user_tbl.FirstOrDefault(u => u.emailVerificationToken == token);
                if (user == null)
                {
                    return View("Error"); // Or a custom error view
                }

                user.isEmailVerified = true;
                user.emailVerificationToken = null; // Optional: clear the token
                db.SaveChanges();

                return View("EmailVerified"); // Success view
            }
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult EmailVerified()
        {
            return View();
        }

        public ActionResult Signup(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return RedirectToAction("SelectRole"); // optional fallback
            }

            role = role.Trim().ToLower();

            switch (role)
            {
                case "user":
                    return View("~/Views/Account/Signup/User/index.cshtml");
                case "doctor":
                    return View("~/Views/Account/Signup/Doctor/index.cshtml");
                default:
                    return View("Error"); // Or return a not found message
            }
        }

        public JsonResult CreateAccount(SignUpViewModel signUp)
        {
            try
            {
                using (var db = new SafeTalkAppContext())
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
                    db.user_tbl.AddOrUpdate(createUser);
                    db.SaveChanges();

                    var userRole = new UserRoleTblModel()
                    {
                        userID = createUser.userID,
                        roleID = signUp.roleID,
                        dateCreated = DateTime.Now,
                        dateUpdated = DateTime.Now,
                    };
                    db.user_role_tbl.AddOrUpdate(userRole);
                    db.SaveChanges();

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
                            db.user_availability_tbl.Add(userAvailability);
                        }
                        db.SaveChanges();
                    }
                    //SendVerificationEmail(signUp.email, signUp.firstName, token);
                }
                return Json(new { success = true, message = "Account created successfully." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while creating the account: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult LoginUser(LogInViewModel logIn)
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var user = db.user_tbl.FirstOrDefault(u => u.email == logIn.email);

                    if (user == null)
                    {
                        return Json(new { success = false, message = "Invalid email or password." }, JsonRequestBehavior.AllowGet);
                    }

                    if (!user.isEmailVerified)
                    {
                        return Json(new { success = false, message = "Please verify your email before logging in." }, JsonRequestBehavior.AllowGet);
                    }

                    if (user != null && BCrypt.Net.BCrypt.Verify(logIn.password, user.password))
                    {

                        var roleName = (from ur in db.user_role_tbl
                                        where ur.userID == user.userID
                                        join r in db.role_tbl on ur.roleID equals r.roleID
                                        select r.roleName)
                                        .FirstOrDefault();

                        if (roleName == "Doctor" && user.isVerified == false)
                        {
                            return Json(new { success = true, role = roleName, verified = false }, JsonRequestBehavior.AllowGet);
                        }

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.userID.ToString()),
                            new Claim(ClaimTypes.Name, user.email),
                            new Claim(ClaimTypes.GivenName, user.firstName + " " + user.lastName),
                            new Claim(ClaimTypes.Role, roleName ?? "User")
                        };

                        var identity = new ClaimsIdentity(claims, "ApplicationCookie");

                        var authManager = HttpContext.GetOwinContext().Authentication;
                        authManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);

                        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        // Invalid credentials
                        return Json(new { success = false, message = "Invalid email or password." }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login Error: {ex.Message}\n{ex.StackTrace}");

                return Json(new { success = false, message = "An unexpected error occurred. Please try again." }, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult CreateAdmin()
        //{
        //    using (var db = new SafeTalkAppContext())
        //    {
        //        var email = "admin@safetalk.com";

        //        if (db.user_tbl.Any(u => u.email == email))
        //            return Content("Admin already exists.");

        //        var adminUser = new UserTblModel
        //        {
        //            firstName = "System",
        //            lastName = "Admin",
        //            birthDate = new DateTime(1990, 1, 1),
        //            genderID = 1, // or any valid gender ID
        //            phoneNumber = "09123456789",
        //            email = email,
        //            password = BCrypt.Net.BCrypt.HashPassword("AdminPassword123!"),
        //            dateCreated = DateTime.Now,
        //            dateUpdated = DateTime.Now,
        //            isVerified = true
        //        };
        //        db.user_tbl.Add(adminUser);
        //        db.SaveChanges();

        //        var adminRole = new UserRoleTblModel
        //        {
        //            userID = adminUser.userID,
        //            roleID = 3, // Admin role ID
        //            dateCreated = DateTime.Now,
        //            dateUpdated = DateTime.Now
        //        };
        //        db.user_role_tbl.Add(adminRole);
        //        db.SaveChanges();

        //        return Content("Admin account created.");
        //    }
        //}

        private void SendVerificationEmail(string email, string name, string token)
        {
            string verificationLink = Url.Action("VerifyEmail", "Account", new { token = token }, protocol: Request.Url.Scheme);
            string body = $"Hi {name},<br/><br/>Please verify your email by clicking the link below:<br/><a href='{verificationLink}'>Verify Email</a>";

            SendEmail(email, "Verify Your Account", body);
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
            string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];

            var smtpClient = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            smtpClient.Send(mailMessage);
        }
    }
}