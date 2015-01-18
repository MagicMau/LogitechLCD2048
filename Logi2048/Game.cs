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
        // the screenbuffer. allocated only once, saves time
        private byte[] screenbuffer = new byte[LogitechGSDK.LOGI_LCD_COLOR_WIDTH * LogitechGSDK.LOGI_LCD_COLOR_HEIGHT * 4];

        // and our screen (as bitmap)
        private Bitmap screen = new Bitmap(LogitechGSDK.LOGI_LCD_COLOR_WIDTH, LogitechGSDK.LOGI_LCD_COLOR_HEIGHT);

        // game state
        private Board board;
        private GameEngine engine;
        private int cellSize = LogitechGSDK.LOGI_LCD_COLOR_HEIGHT / 4;

        private int y = 20; private int dir = 1;
        
        public Game()
        {
            board = new Board(4);
            engine = new GameEngine(board);
        }

        internal void Start()
        {
            // our loop which does
            // 1. update information
            // 2. draw bitmap in memory
            // 3. draw bitmap on lcd

            // Render loop taken from http://blogs.msdn.com/b/tmiller/archive/2005/05/05/415008.aspx
            System.Windows.Forms.Application.Idle += (s, e) =>
            {
                while (isAppIdle)
                {
                    UpdateInformation();
                    DrawScreenBuffer();
                    UpdateLCD();
                }
            };
        }

        /// <summary>
        /// Updates the game state
        /// </summary>
        private void UpdateInformation()
        {
            if (dir > 0 && y > 200)
                dir = -1;
            if (dir < 0 && y < 20)
                dir = 1;

            y += dir;
        }

        /// <summary>
        /// Draws the game state to the screen buffer
        /// </summary>
        private void DrawScreenBuffer()
        {
            using (Graphics g = Graphics.FromImage(screen))
            {
                // clear the screen
                g.FillRectangle(Brushes.Black, 0, 0, screen.Width, screen.Height);
                // draw something
                g.DrawString("Boe", new Font("Comic Sans MS", 12), Brushes.White, 20, y);
            }
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

        /// <summary>
        /// Returns true if the app is still idle (there are no windows messages waiting)
        /// </summary>
        /// <returns></returns>
        private bool isAppIdle
        {
            get
            {
                NativeMethods.Message msg;
                return !NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
            }
        }
    }
}