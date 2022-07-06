using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Nova
{
    internal class Program
    {
        internal static readonly char[] chars =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        private static int count = 0;
        static int count_names = 0;
        static DataTable table = new DataTable();
        static string pass = "";
        public static string RandomString(int size)
        {
            byte[] data = new byte[4 * size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }
            return result.ToString();
        }

        private static string wifilist()
        {
            Process processWifi = new Process();
            processWifi.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processWifi.StartInfo.FileName = "netsh";
            processWifi.StartInfo.Arguments = "wlan show profile";
            processWifi.StartInfo.UseShellExecute = false;
            processWifi.StartInfo.RedirectStandardError = true;
            processWifi.StartInfo.RedirectStandardInput = true;
            processWifi.StartInfo.RedirectStandardOutput = true;
            processWifi.StartInfo.CreateNoWindow = true;
            processWifi.Start();
            string output = processWifi.StandardOutput.ReadToEnd();
            processWifi.WaitForExit();
            return output;
        }
        private static string wifipassword(string wifiname)
        {
            string argument = "wlan show profile name=\"" + wifiname + "\" key=clear";
            Process processWifi = new Process();
            processWifi.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processWifi.StartInfo.FileName = "netsh";
            processWifi.StartInfo.Arguments = argument;
            processWifi.StartInfo.UseShellExecute = false;
            processWifi.StartInfo.RedirectStandardError = true;
            processWifi.StartInfo.RedirectStandardInput = true;
            processWifi.StartInfo.RedirectStandardOutput = true;
            processWifi.StartInfo.CreateNoWindow = true;
            processWifi.Start();
            string output = processWifi.StandardOutput.ReadToEnd();
            processWifi.WaitForExit();
            return output;
        }
        private static string wifipassword_single(string wifiname)
        {
            string get_password = wifipassword(wifiname);
            using (StringReader reader = new StringReader(get_password))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Regex regex2 = new Regex(@"Contenido de la clave  * : (?<after>.*)");
                    Match match2 = regex2.Match(line);

                    if (match2.Success)
                    {
                        string current_password = match2.Groups["after"].Value;
                        return current_password;
                    }
                }
            }
            return "*Red sin Contraseña*";
        }

        private static void parse_lines(string input)
        {
            using (StringReader reader = new StringReader(input))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    count++;
                    regex_lines(line);
                }
            }
        }
        private static void regex_lines(string input2)
        {
            Regex regex1 = new Regex(@"Perfil de todos los usuarios * : (?<after>.*)");
            Match match1 = regex1.Match(input2);
            if (match1.Success)
            {
                count_names++;
                string current_name = match1.Groups["after"].Value;
                string password = wifipassword_single(current_name);
                string x = string.Format("{0}{1}{2}", count_names.ToString().PadRight(7), current_name.PadRight(20), " " + password) + "\r\n";
                pass += x;
                Clipboard.SetText(x);
            }
        }
        private static void reset_all()
        {
            pass = "";
            count = 0;
            count_names = 0;
            table.Rows.Clear();
        }
        private static void get_passwords()
        {
            string wifidata = wifilist();
            parse_lines(wifidata);
        }

       [STAThread]
        static void Main()
        {
            try { 
                var path = @"./Dumped/";
                reset_all();
                get_passwords();
                Thread.Sleep(1000);
                DateTime theDate = DateTime.Now;
                string dateString = theDate.ToString("dd-MM-yy-HH.mm.ss");
                string filename = " wifiPass-" + dateString + " [" + RandomString(10) + "]" + ".txt";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                File.WriteAllText(path + filename, pass);
            } 
            catch {
                MessageBox.Show("Dump Failed..."); 
            }
        }
    }
}
