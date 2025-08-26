using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult PendingDoctors()
        {
            return View();
        }

        public JsonResult GetPendingDoctors()
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var pendingDoctors = (from user in db.user_tbl
                                          join userRole in db.user_role_tbl on user.userID equals userRole.userID
                                          join role in db.role_tbl on userRole.roleID equals role.roleID
                                          where role.roleName == "Doctor" && !user.isVerified // Assuming roleID 2 is for doctors
                                          select new
                                          {
                                              user.userID,
                                              fullName = user.firstName + " " + user.lastName,
                                              user.birthDate,
                                              user.licenseNumber,
                                              user.specialization,
                                          }).ToList();
                    return Json(pendingDoctors, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return Json(new { success = false, message = "Error retrieving pending doctors: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult VerifyDoctor(int userID)
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var user = db.user_tbl.Find(userID);
                    if (user != null)
                    {
                        user.isVerified = true; // Approve the doctor
                        user.dateUpdated = DateTime.Now;
                        db.SaveChanges();
                        return Json(new { success = true, message = "Doctor verified successfully." }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { success = false, message = "Doctor not found." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return Json(new { success = false, message = "Error approving doctor: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}