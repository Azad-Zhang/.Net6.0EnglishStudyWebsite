using Identity.Repository;
using IdentityService.Domain;
using Microsoft.Extensions.Logging;
using Zack.EventBus;

namespace IdentityService.WebAPI.Events
{
    [EventName("IdentityService.User.PasswordReset")]
    public class ResetPasswordEventHandler : JsonIntegrationEventHandler<ResetPasswordEvent>
    {
        private readonly ILogger<ResetPasswordEventHandler> logger;
        //private readonly ISmsSender smsSender;
        private readonly IEmailSender emailSender;

        public ResetPasswordEventHandler(ILogger<ResetPasswordEventHandler> logger, IEmailSender emailSender)
        {
            this.logger = logger;
            this.emailSender = emailSender;
        }

        public override Task HandleJson(string eventName, ResetPasswordEvent? eventData)
        {
            string body = $"尊敬的用户：{eventData.UserName}，您的密码重置为：{eventData.Password}";


            
            var emailOptions = new SendMailRequest() { recipientArry = new string[]{ eventData.Email }, mailTitle = "学英语网站重置密码" , mailBody = body };

            //发送密码给被用户的手机
            return emailSender.SendAsync(emailOptions);
            
            
        }
    }
}