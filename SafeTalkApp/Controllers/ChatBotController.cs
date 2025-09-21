using Newtonsoft.Json;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    public class ChatBotController : Controller
    {
        private static readonly HttpClient httpClient = new HttpClient();

        [HttpPost]
        public async Task<JsonResult> GetResponse(string message)
        {
            // Normalize input
            var userMsg = message.ToLower().Trim();

            // ✅ 1. Hardcoded FAQ (App-specific answers)
            using (var db = new SafeTalkAppContext())
            {
                var faqs = db.faqs_tbl.ToList();

                foreach (var faq in faqs)
                {
                    if (!string.IsNullOrEmpty(faq.keywords))
                    {
                        var keywords = faq.keywords.ToLower().Split(',');
                        if (keywords.All(k => userMsg.Contains(k.Trim())))
                        {
                            return Json(faq.answer);
                        }
                    }
                }
            }

            // ✅ 2. If not an FAQ → forward to AI for Sexual Education
            string reply = await AskCohereAI(message);

            return Json(reply);
        }

        private async Task<string> AskCohereAI(string message)
        {
            string apiKey = "beKf3Mp5MbFNxRcmtkW1MrOhaESQqtGCl5nxKmMe"; // from https://dashboard.cohere.com/
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var body = new
            {
                model = "command-r-plus-08-2024",
                message = message,
                preamble = "You are a friendly chatbot providing clear, safe, and respectful sexual education answers."
            };

            // Serialize request
            var jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

            // Send POST
            var response = await httpClient.PostAsync("https://api.cohere.ai/v1/chat", content);
            var result = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine(result);

            dynamic json = JsonConvert.DeserializeObject(result);
            string reply = json.text ?? "Sorry, I didn’t quite get that. Could you rephrase?";

            return reply;
        }

    }
}