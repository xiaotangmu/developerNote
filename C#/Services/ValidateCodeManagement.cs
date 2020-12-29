using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System;
using System.Text;
using System.Threading.Tasks;
using Cache;
using Common;
using Localization;

namespace WebApi.Services
{
    /// <summary>
    /// 验证码管理器
    /// </summary>
    public class ValidateCodeManagement
    {
        /// <summary>
        /// 邮箱验证
        /// </summary>
        /// <param name="username">用户名/登录名</param>
        /// <param name="targetMail">验证邮箱</param>
        /// <param name="config">系统配置项</param>
        /// <returns></returns>
        public async static Task EmailValidateAsync(string username, string targetMail, IConfiguration config)
        {
            if (string.IsNullOrEmpty(targetMail))
            {
                throw new Exception(await Localizer.GetValueAsync("MailIsNull"));
            }
            //验证邮箱格式有效性
            if (!Library.Utilities.RegExp.IsEmail(targetMail))
            {
                throw new Exception(await Localizer.GetValueAsync("ErrorEmail"));
            }
            await SendEmail(username, targetMail, config);
        }

        private async static Task SendEmail(string username, string targetMail, IConfiguration config)
        {
            string corporation = await Localizer.GetValueAsync("Corporation");
            string subject = await Localizer.GetValueAsync("SendEmail_Subject");
            string body = string.Format(await Localizer.GetValueAsync("SendEmail_Body"), username, AutoGenerateValidateCode(targetMail, config));

            using (SmtpClient smtpClient = new SmtpClient())
            {
                try
                {
                    smtpClient.Timeout = 10 * 1000;   //设置超时时间
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    string host = config.GetSection("SmtpEmail:Host").Value;
                    int port = int.Parse(config.GetSection("SmtpEmail:Port").Value);
                    string address = config.GetSection("SmtpEmail:UserName").Value;
                    string password = config.GetSection("SmtpEmail:Password").Value;
                    bool isSSL = bool.Parse(config.GetSection("SmtpEmail:IsSSL").Value);
                    string displayName = config.GetSection("SmtpEmail:DisplayName").Value;
                    smtpClient.Connect(host, port, isSSL);//连接到远程smtp服务器
                    smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
                    smtpClient.Authenticate(address, password);
                    MimeMessage message = new MimeMessage();
                    message.From.Add(new MailboxAddress(Encoding.Default, displayName, address));
                    message.Cc.Add(new MailboxAddress(Encoding.Default, displayName, address));
                    message.Subject = subject;
                    TextPart text = new TextPart(TextFormat.Html);
                    text.SetText(Encoding.Default, body);
                    message.Body = text;
                    message.To.Add(new MailboxAddress(targetMail));
                    await smtpClient.SendAsync(message);//发送邮件
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private static string AutoGenerateValidateCode(string targetMail, IConfiguration _config)
        {
            string validationCode = RandomHelper.GenerateRandomNumber(6);
            //缓存验证码，设置有效期
            int.TryParse(_config.GetSection("SmtpEmail:ValidationCodeExpiredTime").Value, out int expireTime);
            CacheHelper.SetCache("validation_" + targetMail, validationCode, new TimeSpan(0, 0, expireTime));//邮箱验证
            //手机验证
            return validationCode;
        }
    }
}
