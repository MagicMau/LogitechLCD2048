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
            if (!LogitechGSDK.LogiLcdInit("2048", LogitechGSDK.LOGI_LCD_TYPE_COLOR))
            {
                MessageBox.Show("Logitech keyboard not found. Quitting...");
            }

            var game = new Game();
            game.Start();
            // start the message loop
            Application.ApplicationExit += Application_ApplicationExit;
            Application.Run();
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            // Detach ourselves as requested by 
            // http://msdn.microsoft.com/en-us/library/system.windows.forms.application.applicationexit(v=vs.110).aspx
            Application.ApplicationExit -= Application_ApplicationExit;

            // Shut down the LCD
            LogitechGSDK.LogiLcdShutdown();
        }
    }
}
