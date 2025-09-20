using SafeTalkApp.DTOs.Account;
using SafeTalkApp.DTOs.Shared;
using System.Collections;
using System.Collections.Generic;

namespace SafeTalkApp.Services
{
    public interface IAccountService
    {
        ApiResponse<object> RegisterUser(SignUpDTO signUp);
        ApiResponse<LoginDTO> AuthenticateUser(LoginDTO login);
        ApiResponse<bool> VerifyEmail(string token);

        ApiResponse<IEnumerable<object>> GetRoles();
        ApiResponse<IEnumerable<object>> GetGenders();
        ApiResponse<IEnumerable<object>> GetDaysOfWeek();
    }
}