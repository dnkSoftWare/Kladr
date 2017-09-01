using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using KladrService.Kladr;


namespace KladrService
{
    abstract partial class Log
    {
        class MailLog : Log
        {
            private readonly string _recipients;
            public MailLog(string recipients)
            {
                _recipients = recipients;
            }

            public override void Dispose()
            {
               
            }

            public override void WriteError(string format)
            {
                Mail.SendMail("Ошибка", format, _recipients);
            }

            public override void WriteError(string format, params object[] args)
            {
                
            }

            public override void WriteException(Exception ex)
            {
                var msg = "";
                for (Exception e = ex; e != null; e = e.InnerException)
                {
                    msg = msg + ex.Message + "\r\n";
                }
                Mail.SendMail("Исключение", msg, _recipients);
            }

            public override void WriteLine()
            {
               
            }

            public override void WriteLine(string format)
            {
                
            }

            public override void WriteLine(string format, params object[] args)
            {
                
            }
        }
    }
}

