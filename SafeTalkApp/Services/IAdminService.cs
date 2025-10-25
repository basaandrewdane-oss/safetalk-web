using SafeTalkApp.DTOs.Admin;
using SafeTalkApp.DTOs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public interface IAdminService
    {
        ApiResponse<IEnumerable<object>> GetFaqs();
        ApiResponse<FAQsDTO> AddFaq(FAQsDTO faqDto);
        ApiResponse<FAQsDTO> UpdateFaq(FAQsDTO faqDto);
        ApiResponse<bool> DeleteFaq(int faqID);
        ApiResponse<IEnumerable<PromptsDTO>> GetPrompts();
        ApiResponse<IEnumerable<DoctorDTO>> GetPendingDoctors();
        ApiResponse<bool> VerifyDoctor(int userID);
        ApiResponse<IEnumerable<PaymentDTO>> GetPayments();
        ApiResponse<string> GetTerms();
        ApiResponse<bool> UpdateTerms(string content);
    }
}