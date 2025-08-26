using SafeTalkApp.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    public class ResourcesController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            if (User.IsInRole("Doctor"))
            {
                return View("~/Views/Resources/Doctor/Index.cshtml");
            }
            else if (User.IsInRole("User") || User.IsInRole("Patient"))
            {
                return View("~/Views/Resources/User/Index.cshtml");
            }
            else if (User.IsInRole("Admin"))
            {
                return View("~/Views/Resources/Admin/Index.cshtml");
            }

            return RedirectToAction("Index", "Home");
        }

        public JsonResult GetResources()
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {

                    var resources = db.resource_tbl.ToList();
                    return Json(resources, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error getting resources", ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Add resource
        public JsonResult AddResource(ResourceTblModel model)
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {

                    model.dateCreated = DateTime.Now;
                    model.dateUpdated = DateTime.Now;
                    db.resource_tbl.Add(model);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding resource", ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Edit resource
        public JsonResult EditResource(ResourceTblModel model)
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var existing = db.resource_tbl.Find(model.resourceID);
                    if (existing != null)
                    {
                        existing.title = model.title;
                        existing.content = model.content;
                        existing.category = model.category;
                        existing.type = model.type;
                        existing.url = model.url;
                        existing.publishedDate = model.publishedDate;
                        existing.dateUpdated = DateTime.Now;

                        db.SaveChanges();
                    }
                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error editing resource", ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Delete resource
        public JsonResult DeleteResource(int id)
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var existing = db.resource_tbl.Find(id);
                    if (existing != null)
                    {
                        db.resource_tbl.Remove(existing);
                        db.SaveChanges();
                    }
                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting resource.", ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}