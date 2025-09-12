using SafeTalkApp.DTOs.Account;

namespace SafeTalkApp.Services
{
    public interface IAccountService
    {
        SignUpResult RegisterUser(SignUpDTO signUp);
        LoginDTO AuthenticateUser(LoginDTO login);
        bool VerifyEmail(string token);
    }
}