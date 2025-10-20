using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ClipboardHistoryMini.Helper
{
    public static class Common
    {
        public static string GetUniqueID(string Prefix)
        {
            Random _Random = new Random();
            var result = Prefix + DateTime.Now.ToString("yyyyMMddHHmmss") + _Random.Next(1, 1000);
            return result;
        }

        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        public static bool CheckNet()
        {
            int desc;
            return InternetGetConnectedState(out desc, 0);
        }        
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static string GetIPv4Address(NetworkInterface networkInterface)
        {
            foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
            {
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.Address.ToString() +
                        "\n  IPv4Mask:      " + ip.IPv4Mask.ToString() +
                        "\n  Address Preferred Lifetime:      " + ip.AddressPreferredLifetime.ToString();
                }
            }
            return "Not Found";
        }

        public static string ConnectionDetailsDE(NetworkInterface _NetworkInterface)
        {
            if (_NetworkInterface == null)
                return "No active NIC card have found in your device.";
            else if (CheckNet())
            {
                string processText = "  __________________________________" +
                "\n  Network Interface Name:      " + _NetworkInterface.Name +
                "\n  Operational Status:      " + _NetworkInterface.OperationalStatus +
                "\n  Local IP:      " + GetLocalIPAddress() +
                "\n  IPv4 Address:      " + GetIPv4Address(_NetworkInterface) +
                "\n  ID:      " + _NetworkInterface.Id +
                "\n  Description:      " + _NetworkInterface.Description;
                return processText;
            }
            return "Please check your inetrnet connection.";
        }
        

        public static Process PriorProcess()
        {
            Process curr = Process.GetCurrentProcess();
            Process[] procs = Process.GetProcessesByName(curr.ProcessName);
            foreach (Process p in procs)
            {
                if ((p.Id != curr.Id) &&
                    (p.MainModule.FileName == curr.MainModule.FileName))
                    return p;
            }
            return null;
        }
        public static string GetDownloadDirPath()
        {
            string dirPath = string.Empty;
            RegistryKey rKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\Main");
            if (rKey != null)
                dirPath = (string)rKey.GetValue("Default Download Directory");
            if (string.IsNullOrEmpty(dirPath))
                dirPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\downloads";
            return dirPath;
        }
        public static void WriteIntoFile(string strWriteFileLoc, string WriteValue)
        {
            using (TextWriter _TextWriter = new StreamWriter(strWriteFileLoc))
            {
                _TextWriter.WriteLine(WriteValue);
            }
        }

        public static string ApplicationDataDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        public static string AppRootDirectory()
        {
            string _BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.GetFullPath(Path.Combine(_BaseDirectory, @"..\"));
        }

        public static void OpenProcessStart(string _FileName)
        {
            var _ProcessStartInfo = new ProcessStartInfo
            {
                FileName = _FileName,
                UseShellExecute = true
            };
            Process.Start(_ProcessStartInfo);
        }
        public static int GetOSThemeValue()
        {
            /*
            0 : dark theme
            1 : light theme
            -1 : AppsUseLightTheme could not be found
            */
            int res = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", -1);
            return res;
        }
        public static void CreateDirIfMissing(string DirPath)
        {
            bool _DirExists = Directory.Exists(DirPath);
            if (!_DirExists)
                Directory.CreateDirectory(DirPath);
        }
        public static async Task StartProcessAsync(string _FileName)
        {
            var _ProcessStartInfo = new ProcessStartInfo
            {
                FileName = _FileName,
                UseShellExecute = true
            };

            await Task.Run(() => Process.Start(_ProcessStartInfo));
        }
    }
}
