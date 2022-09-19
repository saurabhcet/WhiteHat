using WhiteHat.API.Model;

namespace WhiteHat.API.Email
{
    public interface IEmailSender
    {
        bool SendMail(Minute mom);
        bool SendTestMail(string subject);
    }
}