using System;
using System.Diagnostics;

namespace KladrService.Kladr
{
    class MyProcess : Process
    {
        private static Process _myProcess;

        public static Process CreateProc(string aFileName, string aArgument = "",bool autoStart = true,  bool aWait = false) // по умолчанию без ожидания
        {
            _myProcess = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    FileName = aFileName,
                    Arguments = aArgument,
                    CreateNoWindow = true
                }
            };

            if (autoStart)
            {
              Log.Instance.WriteLine(aFileName + " "+aArgument);
              _myProcess.Start();
            }

            if (aWait)
                _myProcess.WaitForExit();
            return _myProcess;
        }

        ///// Execute the command Asynchronously.
        //public void ExecuteCommandAsync(string command)
        //{
        //    try
        //    {
        //        //Asynchronously start the Thread to process the Execute command request.
        //        Thread objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync));
                
        //        //Make the thread as background thread.
        //        objThread.IsBackground = true;
        //        //Set the Priority of the thread.
        //        objThread.Priority = ThreadPriority.AboveNormal;
        //        //Start the thread.
        //        objThread.Start(command);
        //    }
        //    catch (ThreadStartException objException)
        //    {
        //        Logger.Log("ThreadStartException " + objException.Message);
        //    }
        //    catch (ThreadAbortException objException)
        //    {
        //        Logger.Log("ThreadAbortException " + objException.Message);
        //    }
        //    catch (Exception objException)
        //    {
        //        Logger.Log(objException.Message);
        //    }
        //}




        /// Executes a shell command synchronously.
        public static void ExecuteCommandSync(string command)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                var procStartInfo = new ProcessStartInfo("cmd", "/c " + command)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                // Do not create the black window.
                // Now we create a process, assign its ProcessStartInfo and start it
                var proc = new Process {StartInfo = procStartInfo};
                Log.Instance.WriteLine(command);
                proc.Start();
                // Get the output into a string
//                var fromEncodind = Encoding.GetEncoding(866); // из какой кодировки
//                var bytes = fromEncodind.GetBytes(proc.StandardOutput.ReadToEnd());
//                var toEncoding = Encoding.GetEncoding(1251);//в какую кодировку
//                var result = toEncoding.GetString(bytes);
                var result = proc.StandardOutput.ReadToEnd();
                
                // Display the command output.
                Log.Instance.WriteLine(result);
            }
            catch (Exception objException)
            {
               Log.Instance.WriteLine(objException.Message);
            }
        }
    }
}
