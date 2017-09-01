using System;
using System.Diagnostics;
using System.IO;

namespace KladrService.Kladr
{
    public static class _7Za
    {

        public static bool Unzip(string archive, string targetFolder,out string outLines)
        {
            if (!File.Exists(archive)) throw new Exception("Archive " + archive + " not found!");
            var zipProcess = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = @"7za.exe",
                    Arguments = @"x -o" + targetFolder + " -y " + archive
                }
            };
            if (!File.Exists(zipProcess.StartInfo.FileName)) throw new Exception("Archivator " + zipProcess.StartInfo.FileName + " not found!");

            if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder); 
            zipProcess.Start();
            var outStr =  zipProcess.StandardOutput.ReadToEnd();

            zipProcess.WaitForExit();
            outLines = outStr;
            var res = !outStr.ToUpper().Contains("ERROR");
            return res;
        }
    }
}
