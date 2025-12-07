using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public interface IHomeService
    {
        ApiResponse<IEnumerable<DoctorDTO>> GetVerifiedDoctors();
        ApiResponse<bool> SubmitFeedback(FeedbackDTO feedback);
        ApiResponse<TermsDTO> GetTerms();
    }
}