using SafeTalkApp.DTOs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SafeTalkApp.Services
{
    public interface IChatBotService
    {
        Task<ApiResponse<string>> GetResponseAsync(string message);
    }
}