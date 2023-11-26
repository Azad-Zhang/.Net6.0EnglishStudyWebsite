using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSP.MailKit
{
    public class MailRequest
    {
        //收件地址
        public string ToEmail { get; set; }
        //邮件标题
        public string Subject { get; set; }
        //邮件内容,支持html
        public string Body { get; set; }
        //要发送的附件
        public List<IFormFile> Attachments { get; set; }
    }
}
