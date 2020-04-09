using System;
using System.Collections.Generic;
using System.Drawing;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace percentage
{
    class TrayIcon
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool DestroyIcon(IntPtr handle);

        private const string iconFont = "Segoe UI";
        private const int iconFontSize = 14;

        private string batteryPercentage;
        private NotifyIcon notifyIcon;
        private int refreshInterval = 1000;

        public TrayIcon()
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();

            notifyIcon = new NotifyIcon();

            // initialize contextMenu
            contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem });


            // initialize menuItem
            menuItem.Text = "Exit";
            menuItem.Click += new System.EventHandler(menuItem_Click);

            notifyIcon.ContextMenu = contextMenu;

            batteryPercentage = "?";

            notifyIcon.Visible = true;

            Timer timer = new Timer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = refreshInterval;
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            float batteryPercentageInt = powerStatus.BatteryLifePercent * 100;
            batteryPercentage = batteryPercentageInt.ToString();
            
            BatteryChargeStatus chargeStatus = SystemInformation.PowerStatus.BatteryChargeStatus;
            bool charging = chargeStatus.HasFlag(BatteryChargeStatus.Charging);

            Color fontColor = Color.White;
            if (charging)
            {
                fontColor = Color.Lime;
            }
            else if (batteryPercentageInt <= 40)
            {
                fontColor = Color.Red;

            }

            using (Bitmap bitmap = new Bitmap(DrawText(batteryPercentage, new Font(iconFont, iconFontSize), fontColor, Color.Transparent)))
            {
                System.IntPtr intPtr = bitmap.GetHicon();
                try
                {
                    using (Icon icon = Icon.FromHandle(intPtr))
                    {
                        notifyIcon.Icon = icon;
                        notifyIcon.Text = batteryPercentage + "%";

                        if (!charging)
                        {
                            int seconds = SystemInformation.PowerStatus.BatteryLifeRemaining;
                            int mins = seconds / 60;
                            int hours = mins / 60;
                            mins = mins % 60;
                            notifyIcon.Text += " " + hours + ":" + mins + " remaining";
                        }
                        else
                        {
                            notifyIcon.Text = "Charging";
                        }
                    }
                }
                finally
                {
                    DestroyIcon(intPtr);
                }
            }
        }

        private void menuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

        private Image DrawText(String text, Font font, Color textColor, Color backColor)
        {
            var textSize = GetImageSize(text, font);
            Image image = new Bitmap((int) textSize.Width, (int) textSize.Height);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                // paint the background
                graphics.Clear(backColor);

                using (Brush textBrush = new SolidBrush(textColor))
                {
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    graphics.DrawString(text, font, textBrush, 0, 0);
                    graphics.Save();
                }
            }

            return image;
        }

        private static SizeF GetImageSize(string text, Font font)
        {
            using (Image image = new Bitmap(1, 1))
            using (Graphics graphics = Graphics.FromImage(image))
                return graphics.MeasureString(text, font);
        }
    }
}
