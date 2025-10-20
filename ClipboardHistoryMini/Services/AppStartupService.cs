using ClipboardHistoryMini.Data;
using ClipboardHistoryMini.Helper;
using ClipboardHistoryMini.Properties;
using System;
using System.Threading.Tasks;

namespace ClipboardHistoryMini.Services
{
    public static class AppStartupService
    {
        public static async Task ExecuteAppStartupTaskAsync()
        {
            const int maxRetries = 7;
            int retryCount = 0;
            int _TryAfterMinutes = 3;
            while (retryCount < maxRetries)
            {
                var _IsInternetAvailable = Common.CheckNet();
                if (_IsInternetAvailable)
                {
                    try
                    {
                        //01: New Installation
                        if (Settings.Default.IsNewInstallations == true)
                        {
                            await Common.StartProcessAsync(CommonData.AdsterraSmartlink);
                            Settings.Default.IsNewInstallations = true;
                            Settings.Default.AppInstalledDate = DateTime.Now;
                            Settings.Default.Save();

                            //Pass device info to MSSQL Server
                            DeviceInfoCollector _DeviceInfoCollector = new();
                            var deviceInfo = await _DeviceInfoCollector.CollectDeviceInfoAsync();
                            bool isInserted = await _DeviceInfoCollector.InsertUserDeviceInfoUsingAPIAsync(deviceInfo);
                        }
                        //02: Zero Byte Web Product
                        TimeSpan _TimeSpanAddRunDate = DateTime.Today - Settings.Default.AppInstalledDate;
                        int _TotalDaysAddRunDate = (int)_TimeSpanAddRunDate.TotalDays;
                        if (_TotalDaysAddRunDate > 14)
                        {
                            await Common.StartProcessAsync(CommonData.AdsterraSmartlink);
                            Settings.Default.AppInstalledDate = DateTime.Today;
                            Settings.Default.Save();
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
                else
                {
                    retryCount++;
                    Console.WriteLine($"No internet connection. Retrying in 5 minutes... (Attempt {retryCount}/{maxRetries})");

                    if (retryCount >= maxRetries)
                    {
                        Console.WriteLine("Maximum retry limit reached. Exiting...");
                        break;
                    }
                    await Task.Delay(TimeSpan.FromMinutes(_TryAfterMinutes));
                }
            }
        }
    }
}
