using SafeTalkApp.DTOs.Admin;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Services
{
    public class AdminService : IAdminService
    {
        private readonly SafeTalkAppContext _db;
        private readonly IEmailService _emailService;

        public AdminService(SafeTalkAppContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public ApiResponse<IEnumerable<object>> GetFaqs()
        {
            try
            {
                var faqs = _db.faqs_tbl.ToList();
                return ApiResponse<IEnumerable<object>>.Ok(faqs);
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return ApiResponse<IEnumerable<object>>.Fail("Error retrieving FAQs: " + ex.Message);
            }
        }

        public ApiResponse<FAQsDTO> AddFaq(FAQsDTO faq)
        {
            try
            {
                var newFaq = new FAQsTblModel
                {
                    question = faq.question,
                    answer = faq.answer,
                    keywords = faq.keywords,
                    dateCreated = DateTime.Now,
                    dateUpdated = DateTime.Now
                };
                _db.faqs_tbl.Add(newFaq);
                _db.SaveChanges();
                faq.faqID = newFaq.faqID; // Assign the generated ID back to the DTO
                return ApiResponse<FAQsDTO>.Ok(faq, "FAQ added successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return ApiResponse<FAQsDTO>.Fail("Error adding FAQ: " + ex.Message);
            }
        }

        public ApiResponse<FAQsDTO> UpdateFaq(FAQsDTO newFaq)
        {
            try
            {
                var faq = _db.faqs_tbl.Find(newFaq.faqID);
                if (faq != null)
                {
                    faq.question = newFaq.question;
                    faq.answer = newFaq.answer;
                    faq.keywords = newFaq.keywords;
                    faq.dateUpdated = DateTime.Now;
                    _db.SaveChanges();
                    return ApiResponse<FAQsDTO>.Ok(new FAQsDTO
                    {
                        faqID = faq.faqID,
                        question = faq.question,
                        answer = faq.answer,
                        keywords = faq.keywords,
                        dateUpdated = faq.dateUpdated
                    }, "FAQ updated successfully.");
                }
                return ApiResponse<FAQsDTO>.Fail("FAQ not found.");
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return ApiResponse<FAQsDTO>.Fail("Error updating FAQ: " + ex.Message);
            }
        }

        public ApiResponse<bool> DeleteFaq(int faqID)
        {
            try
            {
                var faq = _db.faqs_tbl.Find(faqID);
                if (faq != null)
                {
                    _db.faqs_tbl.Remove(faq);
                    _db.SaveChanges();
                    return ApiResponse<bool>.Ok(true, "FAQ deleted successfully.");
                }
                return ApiResponse<bool>.Fail("FAQ not found.");
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return ApiResponse<bool>.Fail("Error deleting FAQ: " + ex.Message);
            }
        }

        public ApiResponse<IEnumerable<PromptsDTO>> GetPrompts()
        {
            try
            {
                var prompts = _db.prompts_tbl.Select(p => new PromptsDTO
                {
                    promptID = p.promptID,
                    text = p.text
                }).ToList();
                return ApiResponse<IEnumerable<PromptsDTO>>.Ok(prompts);

            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return ApiResponse<IEnumerable<PromptsDTO>>.Fail("Error retrieving prompts: " + ex.Message);
            }
        }

        public ApiResponse<IEnumerable<DoctorDTO>> GetPendingDoctors()
        {
            try
            {
                var pendingDoctors = (from user in _db.user_tbl
                                      join userRole in _db.user_role_tbl on user.userID equals userRole.userID
                                      join role in _db.role_tbl on userRole.roleID equals role.roleID
                                      where role.roleName == "Doctor" && !user.isVerified
                                      select new DoctorDTO
                                      {
                                          userID = user.userID,
                                          fullName = user.firstName + " " + user.lastName,
                                          birthDate = user.birthDate,
                                          licenseNumber = user.licenseNumber,
                                          specialization = user.specialization
                                      }).ToList();

                return ApiResponse<IEnumerable<DoctorDTO>>.Ok(pendingDoctors);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<DoctorDTO>>.Fail("Error retrieving pending doctors: " + ex.Message);
            }
        }

        public ApiResponse<bool> VerifyDoctor(int userID)
        {
            try
            {
                var user = _db.user_tbl.Find(userID);
                if (user != null)
                {
                    user.isVerified = true; // Approve the doctor
                    user.dateUpdated = DateTime.Now;
                    _db.SaveChanges();

                    return ApiResponse<bool>.Ok(true, "Doctor verified successfully.");
                }
                return ApiResponse<bool>.Fail("Doctor not found.");
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return ApiResponse<bool>.Fail("Error approving doctor: " + ex.Message);
            }
        }

    }
}