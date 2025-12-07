using SafeTalkApp.DTOs.Profile;
using SafeTalkApp.DTOs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public interface IProfileService
    {
        ApiResponse<UserDTO> GetProfile(string userId);
        ApiResponse<bool> UpdateProfile(ProfileUpdateDTO dto, string userId);
        UserDTO GetUserById(string userId);
    }
}