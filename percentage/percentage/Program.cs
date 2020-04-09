using System;
using System.Windows.Forms;

namespace percentage
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            TrayIcon trayIcon = new TrayIcon();

            Application.Run();
        }
    }
}
