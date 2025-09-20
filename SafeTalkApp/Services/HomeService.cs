using SafeTalkApp.DTOs.Home;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public class HomeService : IHomeService
    {
        private readonly SafeTalkAppContext _db;

        public HomeService(SafeTalkAppContext db)
        {
            _db = db;
        }

        public ApiResponse<IEnumerable<DoctorDTO>> GetVerifiedDoctors()
        {
            try
            {
                var doctors = (from user in _db.user_tbl
                               join userRole in _db.user_role_tbl on user.userID equals userRole.userID
                               where userRole.roleID == 2 && user.isVerified == true && user.isEmailVerified == true
                               select new DoctorDTO
                               {
                                   name = user.firstName +
                                   (user.middleName == null || user.middleName == "" ? "" : " " + user.middleName) +
                                   " " + user.lastName,
                                   specialization = user.specialization,
                                   phoneNumber = user.phoneNumber,
                                   email = user.email,
                               }).ToList();
                return ApiResponse<IEnumerable<DoctorDTO>>.Ok(doctors, "Verified doctors retrieved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: " + ex.StackTrace);
                return ApiResponse<IEnumerable<DoctorDTO>>.Fail($"An error occurred: {ex.Message}");
            }
        }
    }
}