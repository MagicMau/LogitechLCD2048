using Logitech;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;

namespace Logi2048
{
    internal class Game
    {
        private ManualResetEvent close;

        // are we still running?
        private bool isRunning = true;

        // the screenbuffer. allocated only once, saves time
        private byte[] screenbuffer = new byte[LogitechGSDK.LOGI_LCD_COLOR_WIDTH * LogitechGSDK.LOGI_LCD_COLOR_HEIGHT * 4];

        // and our screen (as bitmap)
        private Bitmap screen = new Bitmap(LogitechGSDK.LOGI_LCD_COLOR_WIDTH, LogitechGSDK.LOGI_LCD_COLOR_HEIGHT);

        // game state
        private bool flipper = true;
        private Image img1, img2;
        private long frameCounter = 0;
        private int fps = 0;
        private long startTime = 0;
        
        public Game(ManualResetEvent close)
        {
            this.close = close;
            this.img1 = Image.FromFile("fields.jpg");
            this.img2 = Image.FromFile("Millau.jpg");
        }

        internal void Start()
        {
            // our loop which does
            // 1. update information
            // 2. draw bitmap in memory
            // 3. draw bitmap on lcd
            startTime = DateTime.Now.Ticks;
            var inLoop = false;
            var timer = new Timer(state =>
            {
                if (!inLoop)
                {
                    inLoop = true;
                    UpdateInformation();
                    DrawScreenBuffer();
                    UpdateLCD();
                    inLoop = false;
                }
            }, null, 0, 1);
        }

        private void UpdateInformation()
        {
            flipper = !flipper;
            double time = (DateTime.Now.Ticks - startTime) / 10000000;
            frameCounter++;
            fps = (int)(frameCounter / time);
            if (fps < 0) fps = 0;
        }

        private void DrawScreenBuffer()
        {
            // load a pretty picture
            var img = flipper ? img1 : img2;
            // resize the image to the width and height of the Logitech screen

            screen = new Bitmap(img, LogitechGSDK.LOGI_LCD_COLOR_WIDTH, LogitechGSDK.LOGI_LCD_COLOR_HEIGHT);
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
            // draw fps
            LogitechGSDK.LogiLcdColorSetTitle(string.Format("{0} fps", fps), 255, 255, 255);
            // And signal an update
            LogitechGSDK.LogiLcdUpdate();
        }
    }
}