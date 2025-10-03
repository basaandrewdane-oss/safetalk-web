using SafeTalkApp.DTOs.Resources;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    public class ResourcesController : Controller
    {
        private readonly IResourceService _resourceService;

        public ResourcesController(IResourceService resourceService)
        {
            _resourceService = resourceService;
        }

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

            return View("~/Views/Resources/Public/Index.cshtml");
        }

        public JsonResult GetResources()
        {
            var response = _resourceService.GetResources();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        // Add resource
        public JsonResult AddResource(ResourcesDTO model)
        {
            var response = _resourceService.AddResource(model);
            return Json(response);
        }

        // Edit resource
        public JsonResult EditResource(ResourcesDTO model)
        {
            var response = _resourceService.EditResource(model);
            return Json(response);
        }

        // Delete resource
        public JsonResult DeleteResource(int id)
        {
            var response = _resourceService.DeleteResource(id);
            return Json(response);
        }
    }
}