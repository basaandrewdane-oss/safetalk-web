using SafeTalkApp.DTOs.Account;
using SafeTalkApp.DTOs.Profile;
using SafeTalkApp.DTOs.Shared;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;

namespace SafeTalkApp.Services
{
    public interface IAccountService
    {
        ApiResponse<object> RegisterUser(SignUpDTO signUp);
        ApiResponse<bool> EmailExists(string email);
        ApiResponse<UserDTO> AuthenticateUser(LoginDTO login);
        ClaimsIdentity GenerateUserIdentity(UserDTO user);
        ApiResponse<VerifyEmailResultDTO> VerifyEmail(string token);
        ApiResponse<ResendVerficationResultDTO> ResendVerificationEmail(string email);
        ApiResponse<IEnumerable<object>> GetRoles();
        ApiResponse<IEnumerable<object>> GetGenders();
        ApiResponse<IEnumerable<object>> GetDaysOfWeek();
        ApiResponse<bool> ForgotPassword(string email);
        ApiResponse<bool> ResetPassword(ResetPasswordDTO resetData);
    }
}