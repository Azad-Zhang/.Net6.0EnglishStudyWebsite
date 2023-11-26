namespace ZSP.MailKit
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
    }
}