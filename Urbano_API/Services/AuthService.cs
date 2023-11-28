using System.Net.Mail;
using Urbano_API.Interfaces;

namespace Urbano_API.Services;

public class AuthService: IAuthService
{
    public bool isValidUserName(string userName)
    {
        var valid = true;

        try
        {
            var emailAddress = new MailAddress(userName);
        }
        catch
        {
            valid = false;
        }

        return valid;
    }
}

