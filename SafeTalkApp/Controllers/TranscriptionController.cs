using Microsoft.AspNet.SignalR;
using SafeTalkApp.Hubs;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    public class TranscriptionController : Controller
    {
        private readonly ITranscriptionService _transcriptionService;

        public TranscriptionController(ITranscriptionService transcriptionService)
        {
            _transcriptionService = transcriptionService;
        }
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> UploadAudio(HttpPostedFileBase file, int appointmentId)
        {
            var response = await _transcriptionService.ProcessAndSaveTranscription(file, appointmentId);
            return Json(response);
        }

    }
}