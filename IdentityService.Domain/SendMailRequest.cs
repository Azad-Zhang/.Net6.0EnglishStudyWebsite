using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Domain;

public class SendMailRequest
{
    /// <summary>
    /// 发送人
    /// </summary>
    public string fromPerson { get; set; } = "1120148291@qq.com";

    /// <summary>
    /// 收件人地址(多人)
    /// </summary>
    public string[] recipientArry { get; set; } = { "3420855431@qq.com" };

    /// <summary>
    /// 抄送地址(多人)
    /// </summary>
    //public string[] mailCcArray { get; set; } = { "3420855431@qq.com" };

    /// <summary>
    /// 标题
    /// </summary>
    public string mailTitle { get; set; } = "测试邮件发送";

    /// <summary>
    /// 正文
    /// </summary>
    public string mailBody { get; set; } = "正文内容";

    /// <summary>
    /// 客户端授权码(可存在配置文件中)
    /// </summary>
    public string code { get; set; } = "ilernymfckyzhfaa";

    /// <summary>
    /// SMTP邮件服务器
    /// </summary>
    public string? host { get; set; }

    /// <summary>
    /// 正文是否是html格式
    /// </summary>
    public bool isbodyHtml { get; set; } = true;
    /// <summary>
    /// 接收文件
    /// </summary>
    //public List<IFormFile>? files { get; set; }
}
