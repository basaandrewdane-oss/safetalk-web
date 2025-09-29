using SafeTalkApp.DTOs.Account;
using SafeTalkApp.DTOs.Shared;
using System.Collections;
using System.Collections.Generic;

namespace SafeTalkApp.Services
{
    public interface IAccountService
    {
        ApiResponse<object> RegisterUser(SignUpDTO signUp);
        ApiResponse<bool> EmailExists(string email);
        ApiResponse<LoginDTO> AuthenticateUser(LoginDTO login);
        ApiResponse<VerifyEmailResultDTO> VerifyEmail(string token);
        ApiResponse<ResendVerficationResultDTO> ResendVerificationEmail(string email);
        ApiResponse<IEnumerable<object>> GetRoles();
        ApiResponse<IEnumerable<object>> GetGenders();
        ApiResponse<IEnumerable<object>> GetDaysOfWeek();
    }
}