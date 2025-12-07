using SafeTalkApp.DTOs.Profile;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Interfaces;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ISafeTalkAppContext _db;
        private readonly IFileStorageService _fileStorageService;

        public ProfileService(ISafeTalkAppContext db, IFileStorageService fileStorageService)
        {
            _db = db;
            _fileStorageService = fileStorageService;
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

                // ✅ Build a safe public URL for the profile picture
                if (!string.IsNullOrEmpty(user.profilePictureUrl))
                {
                    // Create a controller endpoint URL
                    user.profilePictureUrl = $"/Profile/GetProfilePicture?fileName={Uri.EscapeDataString(user.profilePictureUrl)}";
                }
                else
                {
                    // Default image path if user has no profile picture
                    user.profilePictureUrl = "/Uploads/ProfilePictures/default-avatar.png";
                }
                System.Diagnostics.Debug.WriteLine($"[GetProfile] Returning profile picture URL: {user.profilePictureUrl}");


                return ApiResponse<UserDTO>.Ok(user);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDTO>.Fail(ex.Message);
            }
        }

        public ApiResponse<bool> UpdateProfile(ProfileUpdateDTO dto, string userId)
        {
            if (dto == null || string.IsNullOrEmpty(userId))
                return ApiResponse<bool>.Fail("Invalid input.");

            var user = _db.user_tbl.FirstOrDefault(u => u.userID.ToString() == userId);
            if (user == null)
                return ApiResponse<bool>.Fail("User not found.");

            // Update basic user properties
            user.firstName = dto.firstName;
            user.lastName = dto.lastName;
            user.specialization = dto.specialization;
            user.phoneNumber = dto.contactNumber;

            // Handle profile picture upload
            if (dto.file != null && dto.file.ContentLength > 0)
            {
                var result = _fileStorageService.SaveProfilePicture(dto.file);
                if (!result.Success)
                    return ApiResponse<bool>.Fail(result.ErrorMessage);

                // Store only the file name in DB
                user.profilePictureUrl = result.FileName;
            }

            try
            {
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

        private bool IsValidImage(Stream stream)
        {
            try
            {
                using (var img = System.Drawing.Image.FromStream(stream, false, true))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}