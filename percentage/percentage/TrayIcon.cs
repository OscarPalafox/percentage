using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Topshelf.Runtime.Windows;
using System.IO;

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
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = refreshInterval;
            timer.Start();
        }

        private void timerTick(object sender, EventArgs e)
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            float batteryPercentageFloat = powerStatus.BatteryLifePercent * 100;
            //float batteryPercentageFloat = 100;
            batteryPercentage = batteryPercentageFloat.ToString();
            
            BatteryChargeStatus chargeStatus = SystemInformation.PowerStatus.BatteryChargeStatus;
            bool fullyCharged = (powerStatus.BatteryLifePercent == 1.0);
            bool charging = chargeStatus.HasFlag(BatteryChargeStatus.Charging);
            bool noBattery = chargeStatus.HasFlag(BatteryChargeStatus.NoSystemBattery);
            bool critical = chargeStatus.HasFlag(BatteryChargeStatus.Critical);

            PowerLineStatus powerLineStatus = SystemInformation.PowerStatus.PowerLineStatus;
            bool pluggedIn = (powerLineStatus == PowerLineStatus.Online);


            Color fontColor = Color.White;
            if (!noBattery)
            {
                if (charging || (pluggedIn && fullyCharged))
                {
                    fontColor = Color.Lime;
                }
                else if (batteryPercentageFloat <= 40)
                {
                    if (critical)
                    {
                        fontColor = Color.Red;
                    }
                    else
                    {
                        fontColor = Color.Orange;
                    }

                }
            }

            using (Bitmap bitmap = new Bitmap(DrawText(batteryPercentage, new Font(iconFont, iconFontSize), fontColor, Color.Transparent)))
            {
                System.IntPtr intPtr = bitmap.GetHicon();
                try
                {
                    using (Icon icon = Icon.FromHandle(intPtr))
                    {
                        string description = "";
                        
                        if (noBattery)
                        {
                            description = "No Battery Detected";
                        }
                        else if (pluggedIn)
                        {
                            if (charging)
                            {
                                description = string.Format("Charging ({0}%)", batteryPercentage);
                            }
                            else if (fullyCharged)
                            {
                                description = string.Format("Fully Charged ({0}%)", batteryPercentage);
                            }
                            else
                            {
                                description = string.Format("Not Charging ({0}%)", batteryPercentage);
                            }
                        }
                        else
                        {
                            int totalRemaining = SystemInformation.PowerStatus.BatteryLifeRemaining;
                            
                            if (totalRemaining > 0)
                            {
                                TimeSpan timeSpan = TimeSpan.FromSeconds(totalRemaining);
                                description = string.Format("{0} hr {1:D2} min remaining", timeSpan.Hours, timeSpan.Minutes);
                            }
                            else
                            {
                                description = string.Format("{0}% remaining", batteryPercentage);
                            }
                        }

                        notifyIcon.Icon = icon;
                        notifyIcon.Text = description;
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
