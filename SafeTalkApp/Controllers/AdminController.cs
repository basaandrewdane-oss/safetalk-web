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
        public ActionResult FAQs()
        {
            return View();
        }

        public JsonResult GetFaqs()
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var faqs = db.faqs_tbl.ToList();
                    return Json(faqs, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return Json(new { success = false, message = "Error retrieving FAQs: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult AddFaq(string question, string answer, string keywords)
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var newFaq = new FAQsTblModel
                    {
                        question = question,
                        answer = answer,
                        keywords = keywords,
                        dateCreated = DateTime.Now,
                        dateUpdated = DateTime.Now
                    };
                    db.faqs_tbl.Add(newFaq);
                    db.SaveChanges();
                    return Json(new { success = true, message = "FAQ added successfully." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return Json(new { success = false, message = "Error adding FAQ: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UpdateFaq(int faqID, string question, string answer, string keywords)
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var faq = db.faqs_tbl.Find(faqID);
                    if (faq != null)
                    {
                        faq.question = question;
                        faq.answer = answer;
                        faq.keywords = keywords;
                        faq.dateUpdated = DateTime.Now;
                        db.SaveChanges();
                        return Json(new { success = true, message = "FAQ updated successfully." }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { success = false, message = "FAQ not found." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return Json(new { success = false, message = "Error updating FAQ: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DeleteFaq(int faqID)
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var faq = db.faqs_tbl.Find(faqID);
                    if (faq != null)
                    {
                        db.faqs_tbl.Remove(faq);
                        db.SaveChanges();
                        return Json(new { success = true, message = "FAQ deleted successfully." }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { success = false, message = "FAQ not found." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return Json(new { success = false, message = "Error deleting FAQ: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

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