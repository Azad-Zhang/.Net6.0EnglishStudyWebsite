using Identity.Repository;
using IdentityService.Domain;
using Zack.EventBus;

namespace IdentityService.WebAPI.Events
{
    [EventName("IdentityService.User.Created")]
    public class UserCreatedEventHandler : JsonIntegrationEventHandler<UserCreatedEvent>
    {
        //private readonly ISmsSender smsSender;
        private readonly IEmailSender emailSender;

        public UserCreatedEventHandler(IEmailSender emailSender)
        {
            this.emailSender = emailSender;
        }

        public override Task HandleJson(string eventName, UserCreatedEvent? eventData)
        {
            string body = $"感谢您的到来！！！\n您的用户名是：{eventData.UserName},密码：{eventData.Password}";
            var mailOptions = new SendMailRequest() { };
            //发送初始密码给被创建用户的手机
            return emailSender.SendAsync(mailOptions);
        }
    }
}