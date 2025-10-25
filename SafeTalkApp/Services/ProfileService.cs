using SafeTalkApp.DTOs.Profile;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ISafeTalkAppContext _db;

        public ProfileService(ISafeTalkAppContext db)
        {
            _db = db;
        }

        public ApiResponse<UserDTO> GetProfile(string userId)
        {
            try
            {
                var user = (from u in _db.user_tbl
                            join ur in _db.user_role_tbl on u.userID equals ur.userID
                            join r in _db.role_tbl on ur.roleID equals r.roleID
                            where u.userID.ToString() == userId
                            select new UserDTO
                            {
                                userID = u.userID,
                                email = u.email,
                                firstName = u.firstName,
                                lastName = u.lastName,
                                roleID = ur.roleID,
                                role = r.roleName,
                                specialization = u.specialization,
                                contactNumber = u.phoneNumber,
                                profilePictureUrl = u.profilePictureUrl
                            }).FirstOrDefault();

                if (user == null)
                {
                    return ApiResponse<UserDTO>.Fail("User not found.");
                }

                return ApiResponse<UserDTO>.Ok(user);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDTO>.Fail(ex.Message);
            }
        }

        public ApiResponse<bool> UpdateProfile(ProfileUpdateDTO dto, string userId)
        {
            try
            {
                var user = _db.user_tbl.FirstOrDefault(u => u.userID.ToString() == userId);
                if (user == null)
                {
                    return ApiResponse<bool>.Fail("User not found.");
                }

                user.firstName = dto.firstName;
                user.lastName = dto.lastName;
                user.specialization = dto.specialization;
                user.phoneNumber = dto.contactNumber;

                // Handle file upload
                if (dto.file != null && dto.file.ContentLength > 0)
                {
                    string folderPath = HttpContext.Current.Server.MapPath("~/Uploads/ProfilePictures/");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    string fileName = $"{userId}{Path.GetExtension(dto.file.FileName)}";
                    string fullPath = Path.Combine(folderPath, fileName);
                    dto.file.SaveAs(fullPath);

                    user.profilePictureUrl = $"/Uploads/ProfilePictures/{fileName}";
                }

                _db.SaveChanges();
                return ApiResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail(ex.Message);
            }
        }

        public UserDTO GetUserById(string userId)
        {
            return (from u in _db.user_tbl
                    join ur in _db.user_role_tbl on u.userID equals ur.userID
                    join r in _db.role_tbl on ur.roleID equals r.roleID
                    where u.userID.ToString() == userId
                    select new UserDTO
                    {
                        userID = u.userID,
                        firstName = u.firstName,
                        lastName = u.lastName,
                        email = u.email,
                        role = r.roleName,
                        profilePictureUrl = u.profilePictureUrl
                    }).FirstOrDefault();
        }
    }
}