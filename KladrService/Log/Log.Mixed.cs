using System;
using KladrService.Kladr;

namespace KladrService
{
    abstract partial class Log
    {
        class Mixed : Log
        {
            private readonly Log[] logs;
            public Mixed(params Log[] logs)
            {
                this.logs = logs;
            }
            public override void Dispose()
            {
                foreach (var item in logs)
                    item.Dispose();
            }

            public override void WriteError(string format)
            {
                foreach (var item in logs)
                    item.WriteError(format);
                
            }

            public override void WriteError(string format, params object[] args)
            {
                foreach (var item in logs)
                    item.WriteError(format, args);
            }

            public override void WriteException(Exception ex)
            {
                foreach (var item in logs)
                    item.WriteException(ex);
            }

            public override void WriteLine()
            {
                foreach (var item in logs)
                    item.WriteLine();
            }

            public override void WriteLine(string format)
            {
                foreach (var item in logs)
                    item.WriteLine(format);
            }

            public override void WriteLine(string format, params object[] args)
            {
                foreach (var item in logs)
                    item.WriteLine(format, args);
            }
        }
    }
}
