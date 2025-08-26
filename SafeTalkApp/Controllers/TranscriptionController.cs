using Microsoft.AspNet.SignalR;
using SafeTalkApp.Hubs;
using SafeTalkApp.Models;
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
        // GET: Transcription
        public ActionResult Index()
        {
            return View();
        }

        private async Task<string> TranscribeWithAssemblyAI(string filePath)
        {
            var apiKey = System.Configuration.ConfigurationManager.AppSettings["AssemblyAIKey"];

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("authorization", apiKey);

                // 1️⃣ Upload the audio file to AssemblyAI
                using (var fileStream = System.IO.File.OpenRead(filePath))
                {
                    var uploadResponse = await httpClient.PostAsync(
                        "https://api.assemblyai.com/v2/upload",
                        new StreamContent(fileStream)
                    );
                    uploadResponse.EnsureSuccessStatusCode();
                    var uploadJson = await uploadResponse.Content.ReadAsStringAsync();
                    dynamic uploadData = Newtonsoft.Json.JsonConvert.DeserializeObject(uploadJson);
                    string uploadUrl = uploadData.upload_url;

                    // 2️⃣ Request transcription
                    var transcriptRequest = new
                    {
                        audio_url = uploadUrl,
                        language_code = "en_us"
                    };

                    var transcriptContent = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(transcriptRequest),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    var transcriptResponse = await httpClient.PostAsync("https://api.assemblyai.com/v2/transcript", transcriptContent);
                    transcriptResponse.EnsureSuccessStatusCode();
                    var transcriptJson = await transcriptResponse.Content.ReadAsStringAsync();
                    dynamic transcriptData = Newtonsoft.Json.JsonConvert.DeserializeObject(transcriptJson);
                    string transcriptId = transcriptData.id;

                    // 3️⃣ Poll until done
                    string status = transcriptData.status;
                    while (status != "completed" && status != "error")
                    {
                        await Task.Delay(3000); // wait 3 sec
                        var pollResponse = await httpClient.GetAsync($"https://api.assemblyai.com/v2/transcript/{transcriptId}");
                        pollResponse.EnsureSuccessStatusCode();
                        var pollJson = await pollResponse.Content.ReadAsStringAsync();
                        dynamic pollData = Newtonsoft.Json.JsonConvert.DeserializeObject(pollJson);
                        status = pollData.status;
                        if (status == "completed")
                            return pollData.text;
                        if (status == "error")
                            throw new Exception("Transcription failed: " + pollData.error);
                    }

                    throw new Exception("Unexpected transcription status.");
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> UploadAudio(HttpPostedFileBase file, int appointmentId)
        {
            if (file == null || file.ContentLength == 0)
                return Json(new { success = false, message = "No file uploaded" });

            var tempPath = Path.Combine(Server.MapPath("~/Uploads/Recordings"), file.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(tempPath));
            file.SaveAs(tempPath);

            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var transcript = await TranscribeWithAssemblyAI(tempPath);

                    // 2️⃣ Save transcript as a .txt file
                    var transcriptDir = Server.MapPath("~/Uploads/Transcripts");
                    Directory.CreateDirectory(transcriptDir);

                    var transcriptFileName = $"appointment_{appointmentId}_{DateTime.Now:yyyyMMddHHmmss}.txt";
                    var transcriptFilePath = Path.Combine(transcriptDir, transcriptFileName);
                    System.IO.File.WriteAllText(transcriptFilePath, transcript);

                    var appointment = db.appointments_tbl.FirstOrDefault(a => a.appointmentID == appointmentId);
                    if (appointment != null)
                    {
                        appointment.transcriptFilePath = "/Uploads/Transcripts/" + transcriptFileName; // store relative path
                    }
                    db.SaveChanges();

                    // Optional: Broadcast immediately to clients in the appointment
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                    hubContext.Clients.Group($"appointment_{appointmentId}").transcriptReady("/Uploads/Transcripts/" + transcriptFileName);

                    return Json(new { success = true, transcript });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            finally
            {
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
        }

    }
}