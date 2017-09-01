using System;
using System.IO;

namespace KladrService
{
    abstract partial class Log
    {
        class FileLog : Log
        {
            private static string logName = "Logs\\Log_{0:yyyy.MM.dd}.txt";
            static readonly object locker = new object();
            public FileLog(int maxSizeFile)
            {
                if (Directory.Exists("Logs") == false) Directory.CreateDirectory("Logs");
            }
            public override void Dispose()
            {

            }


            public override void WriteException(Exception ex)
            {
                try
                {
                    lock (locker)
                    {
                        using (FileStream stream = GetStream())
                        using (StreamWriter log = new StreamWriter(stream) { AutoFlush = true })
                        {
                            for (Exception e = ex; e != null; e = e.InnerException)
                            {
                                log.WriteLine();
                                log.Write("{0:dd.MM.yyyy HH:mm:ss}> ", DateTime.Now);
                                log.WriteLine(ex);
                            }
                        }
                    }
                }
                catch { }
            }

            public override void WriteLine()
            {
                try
                {
                    lock (locker)
                    {
                        using (FileStream stream = GetStream())
                        using (StreamWriter log = new StreamWriter(stream) { AutoFlush = true })
                        {
                            log.WriteLine();
                        }
                    }
                }
                catch { }
            }

            public override void WriteLine(string format)
            {
                try
                {
                    lock (locker)
                    {
                        using (FileStream stream = GetStream())
                        using (StreamWriter log = new StreamWriter(stream) { AutoFlush = true })
                        {
                            log.Write("{0:dd.MM.yyyy HH:mm:ss}> ", DateTime.Now);
                            log.WriteLine(format);
                        }
                    }
                }
                catch { }
            }

            public override void WriteLine(string format, params object[] args)
            {
                try
                {
                    lock (locker)
                    {
                        using (FileStream stream = GetStream())
                        using (StreamWriter log = new StreamWriter(stream) { AutoFlush = true })
                        {
                            log.Write("{0:dd.MM.yyyy HH:mm:ss}> ", DateTime.Now);
                            log.WriteLine(format, args);
                        }
                    }
                }
                catch { }
            }

            public override void WriteError(string format)
            {
                try
                {
                    lock (locker)
                    {
                        using (FileStream stream = GetStream())
                        using (StreamWriter log = new StreamWriter(stream) { AutoFlush = true })
                        {
                            log.Write("{0:dd.MM.yyyy HH:mm:ss}> (ERROR) ", DateTime.Now);
                            log.WriteLine(format);
                        }
                    }
                }
                catch { }
            }

            public override void WriteError(string format, params object[] args)
            {
                try
                {
                    lock (locker)
                    {
                        using (FileStream stream = GetStream())
                        using (StreamWriter log = new StreamWriter(stream) { AutoFlush = true })
                        {
                            log.Write("{0:dd.MM.yyyy HH:mm:ss}> (ERROR) ", DateTime.Now);
                            log.WriteLine(format, args);
                        }
                    }
                }
                catch { }
            }

            private FileStream GetStream()
            {
                if (Directory.Exists("Logs") == false) Directory.CreateDirectory("Logs");
                string fileName = string.Format(logName, DateTime.Now);
                FileStream stream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
                return stream;
            }
        }
    }
}
