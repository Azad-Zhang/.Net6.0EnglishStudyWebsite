using Identity.Repository;
using IdentityService.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Services
{
    public class MockEmailSender : IEmailSender
    {
        private readonly ILogger<MockEmailSender> logger;
        public MockEmailSender(ILogger<MockEmailSender> logger)
        {
            this.logger = logger;
        }

        public async Task<IActionResult> SendAsync(SendMailRequest mails)
        {
            //logger.LogInformation("Send Email to {0},title:{1}, body:{2}", toEmail, subject, body);
            //return Task.CompletedTask;
            return new JsonResult(new
            {
                message = "发送成功",
                code = 200,
                success = true
            });
        }
    }
}
