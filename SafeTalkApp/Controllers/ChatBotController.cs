using Newtonsoft.Json;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
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
        private readonly ChatBotService _chatBotService;

        public ChatBotController(ChatBotService chatBotService)
        {
            _chatBotService = chatBotService;
        }

        [HttpPost]
        public async Task<JsonResult> GetResponse(string message)
        {
            var response = await _chatBotService.GetResponseAsync(message);
            return Json(response);
        }
    }
}