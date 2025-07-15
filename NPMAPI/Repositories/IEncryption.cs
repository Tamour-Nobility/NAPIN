using NPMAPI.Models;

namespace NPMAPI.Repositories
{
    public interface IEncryption
    {
        string HashPassword(string password);
        bool VerifyHashedPassword(string hashedPassword, string password);
        LoggedInUserbyCodeViewModel VerifyUser(string username, string password);
        LoggedInUserbyCodeViewModel VerifyUserResend(long userid);
        LoggedInUserViewModel VerifyUserCode(string code, long userid);
    }
}
