using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;


namespace KeepHangfireAliveService
{

    public class MailRequest
    {
        public string EmailFrom = "SystemAlerts@redington.market";
        public string EmailTo = "rahul.sharma09@nagarro.com";
        public bool IsAddCC = false;
        public string EmailCC = "";
        public bool IsAddBCC = false;
        public string BCCEmail = "";
        public string Host = "smtp.office365.com";
        public string userId = "SystemAlerts@redington.market";
        public string Password = "!SysAR@DMT";
        public int Port = 587;

    }

  
    public class SendMail
    {
      

        public bool SendEMail(string body, string subject)
        {
            var email = new MailRequest();
            try
            {
                var smtpClient = new SmtpClient(email.Host)
                {
                    Port = Convert.ToInt32(email.Port),
                    Credentials = new NetworkCredential(email.userId, email.Password),
                    EnableSsl = true,
                };
                bool isAddCC = Convert.ToBoolean(email.IsAddBCC);
                System.Net.Mail.MailMessage MM = new System.Net.Mail.MailMessage();
                MM.From = new MailAddress(email.EmailFrom);
                var mailArr = email.EmailTo.Split(',');
                foreach (var _toEmail in mailArr)
                {
                    MM.To.Add(new MailAddress(_toEmail));
                }
                MM.Subject = body;
                MM.Body = body;
                MM.IsBodyHtml = true;
                MM.Priority = System.Net.Mail.MailPriority.High;
                if (isAddCC)
                {
                    MM.CC.Add(new MailAddress(email.EmailCC));
                }

                var fromEmail = email.EmailFrom;
                smtpClient.Send(MM);
                return true;

            }
            catch (SmtpFailedRecipientException ex)
            {
                return false;
                //throw new Exception("SMTP error in sending email");
            }
            catch (Exception ex)
            {
                return false;
                //throw new Exception("Error in Sending Email");
            }
        }
    }
}
