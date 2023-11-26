using IdentityService.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Repository
{
    public interface IEmailSender
    {
        public Task<IActionResult> SendAsync(SendMailRequest mails);
    }
}
