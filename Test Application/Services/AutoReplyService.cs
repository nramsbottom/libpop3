using System;
using System.Collections.Generic;
using System.Text;

namespace Test_Application.Services
{
    public class AutoReplyService : IEmailService
    {

        #region IEmailService Members

        void IEmailService.Process(Iskai.Net.Mail.Pop3Message msg)
        {

            if (msg.From.Address.ToLower() == "neil@xxxxxxxxxxxxxxx.com")
            {
                System.Net.Mail.MailMessage outMsg = new System.Net.Mail.MailMessage();
                System.Net.Mail.SmtpClient svr = new System.Net.Mail.SmtpClient("mail.xxxxxxxxxxxxxxx.com");

                outMsg.To.Add(msg.From);
                outMsg.From = msg.To;
                outMsg.Subject = "Re: " + msg.Subject;
                outMsg.Body = "Thanks for your message!";

                svr.Send(outMsg);

            }

        }

        #endregion
    }
}
