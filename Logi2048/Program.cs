using Logitech;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Logi2048
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Mutex that will signal us when it's time to quit.
            ManualResetEvent close = new ManualResetEvent(false);
            
            // quit when Windows session ends, aka user logs out
            SystemEvents.SessionEnded += (s, e) => close.Set();

            if (!LogitechGSDK.LogiLcdInit("2048", LogitechGSDK.LOGI_LCD_TYPE_COLOR))
            {
                MessageBox.Show("Logitech keyboard not found. Quitting...");
                close.Set();
            }

            var game = new Game(close);
            game.Start();
            
            close.WaitOne();
            LogitechGSDK.LogiLcdShutdown();
        }
    }
}
