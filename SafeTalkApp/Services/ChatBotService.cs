using Newtonsoft.Json;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Unity;

namespace SafeTalkApp.Services
{
    public class ChatBotService : IChatBotService
    {
        private readonly ISafeTalkAppContext _db;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ChatBotService(ISafeTalkAppContext db, HttpClient httpClient, [Dependency("CohereApiKey")] string apiKey)
        {
            _db = db;
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<ApiResponse<string>> GetResponseAsync(string message)
        {
            try
            {
                // Normalize input
                var userMsg = message.ToLower().Trim();
                // ✅ 1. Hardcoded FAQ (App-specific answers)
                var faqs = _db.faqs_tbl.ToList();
                foreach (var faq in faqs)
                {
                    if (!string.IsNullOrEmpty(faq.keywords))
                    {
                        var keywords = faq.keywords.ToLower().Split(',');
                        if (keywords.All(k => userMsg.Contains(k.Trim())))
                        {
                            return ApiResponse<string>.Ok(faq.answer);
                        }
                    }
                }
                // ✅ 2. If not an FAQ → forward to AI for Sexual Education
                string reply = await AskCohereAI(message);
                return ApiResponse<string>.Ok(reply);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail("Error getting chatbot response: " + ex.Message);
            }
        }

        private async Task<string> AskCohereAI(string message)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var body = new
            {
                model = "command-r-plus-08-2024",
                message,
                preamble = "You are a friendly chatbot providing clear, safe, and respectful sexual education answers."
            };

            var jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.cohere.ai/v1/chat", content);
            var result = await response.Content.ReadAsStringAsync();

            dynamic json = JsonConvert.DeserializeObject(result);
            return json?.text ?? "Sorry, I didn’t quite get that. Could you rephrase?";
        }
    }
}