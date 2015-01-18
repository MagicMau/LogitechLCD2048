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
        private int marginLeft = 0;
        private StringFormat tileStringFormat;

        private bool hasStateChanged = true;

        // keyboard state
        private bool isUpDown = false, isRightDown = false, isDownDown = false, isLeftDown = false, isCancelDown = false;

        
        public Game()
        {
            board = new Board(4);
            engine = new GameEngine(board);

            marginLeft = (LogitechGSDK.LOGI_LCD_COLOR_WIDTH - 4 * cellSize) / 2;
            tileStringFormat = new StringFormat();
            tileStringFormat.LineAlignment = StringAlignment.Center;
            tileStringFormat.Alignment = StringAlignment.Center;
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
                    if (hasStateChanged)
                    {
                        DrawScreenBuffer();
                        UpdateLCD();
                        hasStateChanged = false;
                    }
                    Thread.Sleep(1);
                }
            };
        }

        /// <summary>
        /// Updates the game state
        /// </summary>
        private void UpdateInformation()
        {
            CheckButtons();
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
                // draw the board
                using (Font f = new Font("Arial", 16, FontStyle.Bold))
                {
                    foreach (var tile in board)
                    {
                        DrawTile(tile, g, f);
                    }
                }
            }
        }

        private void DrawTile(Tile tile, Graphics g, Font f)
        {
            int x = tile.Column * cellSize + marginLeft;
            int y = tile.Row * cellSize;
            Rectangle rect = new Rectangle(x, y, cellSize, cellSize);

            using (Brush brush = new SolidBrush(tile.Color))
            {
                g.FillRectangle(brush, rect);
                g.DrawRectangle(Pens.White, x, y, cellSize - 1, cellSize - 1);
                if (tile.Value > 0)
                {
                    g.DrawString(tile.Value.ToString(), f, Brushes.White, rect, tileStringFormat);
                }
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

        private void CheckButtons()
        {
            if (LogitechGSDK.LogiLcdIsButtonPressed(LogitechGSDK.LOGI_LCD_COLOR_BUTTON_DOWN))
            {
                isDownDown = true;
            }
            else if (isDownDown)
            {
                isDownDown = false;
                engine.Move(Direction.Down);
                hasStateChanged = true;
            }

            if (LogitechGSDK.LogiLcdIsButtonPressed(LogitechGSDK.LOGI_LCD_COLOR_BUTTON_LEFT))
            {
                isLeftDown = true;
            }
            else if (isLeftDown)
            {
                isLeftDown = false;
                engine.Move(Direction.Left);
                hasStateChanged = true;
            }

            if (LogitechGSDK.LogiLcdIsButtonPressed(LogitechGSDK.LOGI_LCD_COLOR_BUTTON_UP))
            {
                isUpDown = true;
            }
            else if (isUpDown)
            {
                isUpDown = false;
                engine.Move(Direction.Up);
                hasStateChanged = true;
            }

            if (LogitechGSDK.LogiLcdIsButtonPressed(LogitechGSDK.LOGI_LCD_COLOR_BUTTON_RIGHT))
            {
                isRightDown = true;
            }
            else if (isRightDown)
            {
                isRightDown = false;
                engine.Move(Direction.Right);
                hasStateChanged = true;
            }

            if (LogitechGSDK.LogiLcdIsButtonPressed(LogitechGSDK.LOGI_LCD_COLOR_BUTTON_CANCEL))
            {
                isCancelDown = true;
            }
            else if (isCancelDown)
            {
                isCancelDown = false;
                engine.Reset();
                hasStateChanged = true;
            }
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