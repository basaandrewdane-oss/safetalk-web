using SafeTalkApp.DTOs.Account;
using SafeTalkApp.DTOs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public interface IAvailabilityService
    {
        ApiResponse<IEnumerable<AvailabilityDTO>> GetAvailability(int userID);
        ApiResponse<bool> SaveAvailability(int userID, List<AvailabilityDTO> availabilities);
    }
}