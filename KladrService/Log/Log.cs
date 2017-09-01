using System;
using System.Collections.Generic;
using KladrService.Kladr;

namespace KladrService
{
    abstract partial class Log : IDisposable
    {
        private static readonly Log instance = CreateLogInstance();

        private static Log CreateLogInstance()
        {
            List<Log> array = new List<Log>();
            array.Add(new FileLog(1048576));
            if (System.Diagnostics.Process.GetCurrentProcess().SessionId != 0)
                array.Add(new ConsoleLog());
            array.Add(new MailLog(Settings.MyXmlData.ExeptionRecipient));
            return new Mixed(array.ToArray());
        }
        public static Log Instance { get { return instance; } }

        protected Log()
        {
        }

        public abstract void WriteLine();
        public abstract void WriteLine(string format);
        public abstract void WriteLine(string format, params object[] args);

        public abstract void WriteError(string format);
        public abstract void WriteError(string format, params object[] args);
        public abstract void WriteException(Exception ex);
        public abstract void Dispose();
    }
}
