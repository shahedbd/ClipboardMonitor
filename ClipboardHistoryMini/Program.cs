using System;
using System.Windows.Forms;

namespace ClipboardHistoryMini
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            // Initialize the app
            var mainForm = new MainForm();
            Application.Run(mainForm);
        }
    }
}