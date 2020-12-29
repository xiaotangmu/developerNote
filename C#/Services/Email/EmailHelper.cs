using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;

namespace WebApi.Services.Email
{
    public class EmailHelper
    {
        /// <summary>
        /// exchange服务对象
        /// </summary>
        private static ExchangeService _exchangeService = new ExchangeService(ExchangeVersion.Exchange2010_SP1);

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static void Send(Email email, string userId, string pwd, string domain)
        {
            try
            {
                _exchangeService.Credentials = new NetworkCredential(userId, pwd, domain);
                _exchangeService.Url = new Uri(WebConfig.ExchangeServerUrl);
                //发送人
                Mailbox mail = new Mailbox(email.Mail_from);
                //邮件内容
                EmailMessage message = new EmailMessage(_exchangeService);
                string[] strTos = email.Mail_to.Split(';');
                //接收人
                foreach (string item in strTos)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        message.ToRecipients.Add(item);
                    }
                }
                //抄送人
                foreach (string item in email.Mail_cc.Split(';'))
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        message.CcRecipients.Add(item);
                    }

                }
                //邮件标题
                message.Subject = email.Subject;
                //邮件内容
                message.Body = new MessageBody(email.body);
                //发送并且保存
                message.SendAndSaveCopy();

            }
            catch (Exception ex)
            {
                throw new Exception("发送邮件出错，" + ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }
}
