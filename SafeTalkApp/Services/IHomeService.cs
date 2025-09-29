using SafeTalkApp.DTOs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public interface IHomeService
    {
        ApiResponse<IEnumerable<DoctorDTO>> GetVerifiedDoctors();
    }
}