using SafeTalkApp.DTOs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SafeTalkApp.Services
{
    public interface ITranscriptionService
    {
        Task<ApiResponse<string>> ProcessAndSaveTranscription(HttpPostedFileBase file, int appointmentID);
        Task<ApiResponse<byte[]>> DownloadTranscriptFile(int appointmentID);
    }
}