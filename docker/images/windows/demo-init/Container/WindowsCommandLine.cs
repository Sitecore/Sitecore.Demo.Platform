using System;
using System.Diagnostics;

namespace Sitecore.Demo.Init.Container
{
    public class WindowsCommandLine
    {
        private readonly string workingDirectory;
        private string standardOutput;
        private string standardError;

        public WindowsCommandLine(string workingDirectory)
        {
            this.workingDirectory = workingDirectory;
        }

        // The code below is written this way to avoid deadlocks, see https://stackoverflow.com/questions/4291912/process-start-how-to-get-the-output
        public string Run(string command)
        {
            var cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.WorkingDirectory = workingDirectory;
            cmd.ErrorDataReceived += ErrorOutputHandler;

            cmd.Start();
            cmd.BeginErrorReadLine();
            cmd.StandardInput.WriteLine(command);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            standardOutput = cmd.StandardOutput.ReadToEnd();
            cmd.WaitForExit();

            standardOutput += cmd.StandardOutput.ReadToEnd();
            return standardOutput + Environment.NewLine + standardError;
        }

        void ErrorOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            standardError += outLine.Data;
            Console.WriteLine(outLine.Data);
        }
    }
}
