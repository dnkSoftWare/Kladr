using System;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace KladrService.Kladr
{
    static class Mail
    {
        private static SmtpClient _client;

        private static SmtpClient Smtp()
        {
            // Command line argument must the the SMTP host.
            _client = new SmtpClient
            {
                Port = Settings.MyXmlData.Port,
                Host = Settings.MyXmlData.Host,
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials =
                    new System.Net.NetworkCredential(Settings.MyXmlData.MailBox, Settings.MyXmlData.Password)
            };
            return _client;
        }

        private static void Send(string aFrom, string aTo, string aSubject, string aBody)
        {
                try
                {
                    var mm = new MailMessage(aFrom, aTo, aSubject, aBody)
                    {
                        BodyEncoding = Encoding.UTF8,
                        DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure
                    };

                    Smtp().Send(mm);
                }
        
                catch (Exception ex)
                {
                        Log.Instance.WriteLine(ex.Message);
                }

                finally
                {
                    _client?.Dispose();
                }

        }

        public static void SendMail(string aSubject, string aBody)
        {
            Task.Factory.StartNew(() => Send(Settings.MyXmlData.MailBox, Settings.MyXmlData.MailRecipients, 
                 KladrService.AppName + " v." + KladrService.AppVersion + " : " + aSubject, aBody) );
        }

        public static void SendMail(string aSubject, string aBody, string onlyFor)
        {
            Task.Factory.StartNew(() => Send(Settings.MyXmlData.MailBox, onlyFor,
                KladrService.AppName + " v." + KladrService.AppVersion + " : " + aSubject, aBody));
        }

    }
}
