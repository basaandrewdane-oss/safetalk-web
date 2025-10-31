using Microsoft.AspNet.SignalR;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Hubs;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Unity;

namespace SafeTalkApp.Services
{
    public class TranscriptionService : ITranscriptionService
    {
        private readonly ISafeTalkAppContext _db;
        private readonly IEmailService _emailService;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public TranscriptionService(ISafeTalkAppContext db, HttpClient httpClient, [Dependency("AssemblyAIKey")] string apiKey, IEmailService emailService)
        {
            _db = db;
            _httpClient = httpClient;
            _apiKey = apiKey;
            _emailService = emailService;
        }

        private async Task<ApiResponse<string>> TranscribeWithAssemblyAI(string filePath)
        {
            try
            {
                // 1️⃣ Upload the audio file to AssemblyAI
                using (var fileStream = File.OpenRead(filePath))
                {
                    var uploadRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.assemblyai.com/v2/upload")
                    {
                        Content = new StreamContent(fileStream)
                    };
                    uploadRequest.Headers.Add("authorization", _apiKey);

                    var uploadResponse = await _httpClient.SendAsync(uploadRequest);
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

                    var transcriptReq = new HttpRequestMessage(HttpMethod.Post, "https://api.assemblyai.com/v2/transcript")
                    {
                        Content = transcriptContent
                    };
                    transcriptReq.Headers.Add("authorization", _apiKey);

                    var transcriptResponse = await _httpClient.SendAsync(transcriptReq);
                    transcriptResponse.EnsureSuccessStatusCode();

                    var transcriptJson = await transcriptResponse.Content.ReadAsStringAsync();
                    dynamic transcriptData = Newtonsoft.Json.JsonConvert.DeserializeObject(transcriptJson);
                    string transcriptId = transcriptData.id;

                    // 3️⃣ Poll until done
                    string status = transcriptData.status;
                    while (status != "completed" && status != "error")
                    {
                        await Task.Delay(3000); // wait 3 sec

                        var pollReq = new HttpRequestMessage(HttpMethod.Get, $"https://api.assemblyai.com/v2/transcript/{transcriptId}");
                        pollReq.Headers.Add("authorization", _apiKey);

                        var pollResponse = await _httpClient.SendAsync(pollReq);
                        pollResponse.EnsureSuccessStatusCode();

                        var pollJson = await pollResponse.Content.ReadAsStringAsync();
                        dynamic pollData = Newtonsoft.Json.JsonConvert.DeserializeObject(pollJson);
                        status = pollData.status;

                        if (status == "completed")
                            return ApiResponse<string>.Ok((string)pollData.text);

                        if (status == "error")
                            return ApiResponse<string>.Fail("Transcription failed: " + pollData.error);
                    }

                    return ApiResponse<string>.Fail("Unexpected transcription status");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail("Error during transcription", ex.Message);
            }
        }

        public async Task<ApiResponse<string>> ProcessAndSaveTranscription(HttpPostedFileBase file, int appointmentID)
        {
            try
            {
                if (file == null || file.ContentLength == 0)
                {
                    return ApiResponse<string>.Fail("No file uploaded");
                }

                var tempPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Uploads/Recordings"), file.FileName);
                Directory.CreateDirectory(Path.GetDirectoryName(tempPath));
                file.SaveAs(tempPath);

                try
                {
                    var audioHash = ComputeFileHash(tempPath);

                    var transcriptResponse = await TranscribeWithAssemblyAI(tempPath);
                    if (!transcriptResponse.success)
                    {
                        return transcriptResponse;
                    }

                    var transcript = transcriptResponse.data;

                    var transcriptHash = ComputeStringHash(transcript);

                    // 2️⃣ Save transcript as a .txt file
                    var transcriptDir = HttpContext.Current.Server.MapPath("~/Uploads/Transcripts");
                    Directory.CreateDirectory(transcriptDir);

                    var transcriptFileName = $"appointment_{appointmentID}_{DateTime.Now:yyyyMMddHHmmss}.txt";
                    var transcriptFilePath = Path.Combine(transcriptDir, transcriptFileName);
                    System.Diagnostics.Debug.WriteLine("Transcript text: " + transcript);
                    File.WriteAllText(transcriptFilePath, transcript);

                    var appointment = _db.appointments_tbl.FirstOrDefault(a => a.appointmentID == appointmentID);
                    if (appointment != null)
                    {
                        appointment.transcriptFilePath = "/Uploads/Transcripts/" + transcriptFileName; // store relative path
                        appointment.audioFileHash = audioHash;
                        appointment.transcriptHash = transcriptHash;
                    }
                    _db.SaveChanges();

                    //try
                    //{
                    //    var patient = _db.user_tbl.FirstOrDefault(u => u.userID == appointment.patientID);
                    //    var doctor = _db.user_tbl.FirstOrDefault(u => u.userID == appointment.doctorID);
                    //    if (patient != null && doctor != null)
                    //    {
                    //        _emailService.SendTranscriptionReadyToPatient(patient, doctor, appointment, transcriptFileName);
                    //        _emailService.SendTranscriptionReadyToDoctor(doctor, patient, appointment, transcriptFileName);
                    //    }
                    //}
                    //catch (Exception emailEx)
                    //{
                    //    System.Diagnostics.Debug.WriteLine("Error sending transcription emails: " + emailEx.Message);
                    //}

                    // Optional: Broadcast immediately to clients in the appointment
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                    hubContext.Clients.Group($"appointment_{appointmentID}").transcriptReady("/Uploads/Transcripts/" + transcriptFileName);

                    return ApiResponse<string>.Ok(transcript, "Transcription completed.");
                }
                catch (Exception ex)
                {
                    return ApiResponse<string>.Fail("Error processing transcription", ex.Message);
                }
                finally
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail(ex.Message);
            }
        }

        public async Task<ApiResponse<byte[]>> DownloadTranscriptFile(int appointmentID)
        {
            try
            {
                var appointment = _db.appointments_tbl.FirstOrDefault(a => a.appointmentID == appointmentID);

                if (appointment == null || string.IsNullOrEmpty(appointment.transcriptFilePath))
                {
                    return ApiResponse<byte[]>.Fail("Transcript not found.");
                }

                var fullPath = HttpContext.Current.Server.MapPath(appointment.transcriptFilePath);
                if (!File.Exists(fullPath))
                {
                    return ApiResponse<byte[]>.Fail("Transcript file missing on server.");
                }

                // 🔑 Verify integrity
                var transcriptContent = File.ReadAllText(fullPath);
                var recomputedHash = ComputeStringHash(transcriptContent);

                if (!string.Equals(recomputedHash, appointment.transcriptHash, StringComparison.OrdinalIgnoreCase))
                {
                    return ApiResponse<byte[]>.Fail("Transcript integrity check failed.");
                }

                // ✅ Return file as byte[]
                var fileBytes = await Task.Run(() => File.ReadAllBytes(fullPath));
                return ApiResponse<byte[]>.Ok(fileBytes, "Transcript download ready.");
            }
            catch (Exception ex)
            {
                return ApiResponse<byte[]>.Fail("Error during transcript download" + ex.Message);
            }
        }

        public static string ComputeFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                return BitConverter.ToString(sha256.ComputeHash(stream))
                    .Replace("-", "")
                    .ToLowerInvariant();
            }
        }

        public static string ComputeStringHash(string text)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                return BitConverter.ToString(sha256.ComputeHash(bytes))
                    .Replace("-", "")
                    .ToLowerInvariant();
            }
        }
    }
}