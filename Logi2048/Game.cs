using Logitech;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Logi2048
{
    internal class Game
    {
        // this is the signaler that we can use to announce that the app may exit
        private ManualResetEvent close;

        // are we still running?
        private bool isRunning = true;

        // the screenbuffer. allocated only once, saves time
        private byte[] screenbuffer = new byte[LogitechGSDK.LOGI_LCD_COLOR_WIDTH * LogitechGSDK.LOGI_LCD_COLOR_HEIGHT * 4];

        // and our screen (as bitmap)
        private Bitmap screen = new Bitmap(LogitechGSDK.LOGI_LCD_COLOR_WIDTH, LogitechGSDK.LOGI_LCD_COLOR_HEIGHT);

        public Game(ManualResetEvent close)
        {
            this.close = close;
        }

        internal void Start()
        {
            // our loop which does
            // 1. update information
            // 2. draw bitmap in memory
            // 3. draw bitmap on lcd
            Application.Idle += (s, e) =>
            {
                UpdateInformation();
                DrawScreenBuffer();
                UpdateLCD();
            };
        }

        private void UpdateInformation()
        {

        }

        private void DrawScreenBuffer()
        {
            // load a pretty picture
            var img = Image.FromFile("Millau.jpg");
            // resize the image to the width and height of the Logitech screen

            var bitmap = new Bitmap(img, LogitechGSDK.LOGI_LCD_COLOR_WIDTH, LogitechGSDK.LOGI_LCD_COLOR_HEIGHT);
            // Draw it on screen!
            //Draw(bitmap);
        }

        /// <summary>
        /// Uses the LockBits() method on bitmap to quickly copy all pixels to a byte array.
        /// This method takes no time at all, whereas a for-loop costs about 30-40 ms on my machine for this low resolution
        /// Give it a bitmap and it will render it to screen
        /// </summary>
        private void UpdateLCD()
        {
            // lock the bits
            var data = screen.LockBits(Rectangle.FromLTRB(0, 0, screen.Width, screen.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
            // copy the bits
            Marshal.Copy(data.Scan0, screenbuffer, 0, screenbuffer.Length);
            // don't forget to free our bits!
            screen.UnlockBits(data);
            // Draw the background
            LogitechGSDK.LogiLcdColorSetBackground(screenbuffer);
            // And signal an update
            LogitechGSDK.LogiLcdUpdate();
        }
    }
}