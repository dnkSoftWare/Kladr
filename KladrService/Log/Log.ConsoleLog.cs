using System;

namespace KladrService
{
    abstract partial class Log
    {
        class ConsoleLog : Log
        {
            private readonly object locker = new object();
            public override void Dispose()
            {

            }

            public override void WriteError(string format)
            {
                lock (locker)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(format);
                    Console.ResetColor();
                }
            }

            public override void WriteError(string format, params object[] args)
            {
                lock (locker)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(format, args);
                    Console.ResetColor();
                }
            }

            public override void WriteException(Exception ex)
            {
                lock (locker)
                {
                    for (Exception e = ex; e != null; e = e.InnerException)
                    {

                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.Message);
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(e);
                    }
                    Console.ResetColor();
                }
            }

            public override void WriteLine()
            {
                lock (locker)
                {
                    Console.WriteLine();
                }
            }

            public override void WriteLine(string format)
            {
                lock (locker)
                {
                    Console.WriteLine(format);
                }
            }

            public override void WriteLine(string format, params object[] args)
            {
                lock (locker)
                {
                    Console.WriteLine(format, args);
                }
            }
        }
    }
}
