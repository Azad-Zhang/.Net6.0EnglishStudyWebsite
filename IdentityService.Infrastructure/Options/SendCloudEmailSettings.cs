namespace IdentityService.Infrastructure.Options
{
    public class SendCloudEmailSettings
    {
        //用于发送邮件的邮箱账号
        public string Mail { get; set; }
        //显示名称
        public string DisplayName { get; set; }
        //用于发送邮件的邮箱账号的授权码
        public string Password { get; set; }
        //用于发送邮件的邮箱服务器,QQ邮箱是：smtp.qq.com
        public string Host { get; set; }
        //用于发送邮件的邮箱服务器端口,QQ邮箱的端口是25
        public int Port { get; set; }
    }
}
