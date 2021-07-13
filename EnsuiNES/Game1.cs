using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EnsuiNES.Console;
using System;
using System.Diagnostics;

namespace EnsuiNES
{
    public class Game1 : Game
    {
        private NES emulator;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont consoleFont;

        private RenderTarget2D debugger;

        public Game1()
        {
            emulator = new NES();
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1360;
            _graphics.PreferredBackBufferHeight = 960;
            _graphics.ApplyChanges();


        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            consoleFont = Content.Load<SpriteFont>("ConsoleFont");

            Helper.Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Helper.Keyboard.IsPressed(Keys.Space, true))
            {
                Debug.WriteLine("Clock");

                do
                {
                    emulator.cpu.clock();
                }
                while (!emulator.cpu.complete());
                
            }

            if (Helper.Keyboard.IsPressed(Keys.R, true))
            {
                Debug.WriteLine("Reset");
                emulator.cpu.reset();
            }

            if (Helper.Keyboard.IsPressed(Keys.I, true))
            {
                Debug.WriteLine("Interrupt");
                emulator.cpu.IRQ();
            }

            if (Helper.Keyboard.IsPressed(Keys.N, true))
            {
                Debug.WriteLine("NMI");
                emulator.cpu.NMI();
            }            

            // TODO: Add your update logic here


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            debugger = new RenderTarget2D(_graphics.GraphicsDevice, (_graphics.PreferredBackBufferWidth - 20), (_graphics.PreferredBackBufferHeight - 20));

            GraphicsDevice.SetRenderTarget(debugger);

            GraphicsDevice.Clear(Color.Blue);

            _spriteBatch.Begin();

            DrawRam(10, 10, 0x0000, 16, 16);
            DrawRam(10, 360, 0x8000, 16, 16);

            DrawCpu(780, 10);

            DrawCode(780, 200, 32);

            _spriteBatch.DrawString(consoleFont, "SPACE: Clock CPU    R: Reset Console    I: Interrupt    N: NMI", new Vector2(10, (debugger.Height - 20)), Color.White);

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            GraphicsDevice.Clear(Color.MidnightBlue);

            _spriteBatch.Begin();

            _spriteBatch.Draw(debugger, new Vector2(10, 10), Color.White);

            _spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        protected void DrawRam(int xPosition, int yPosition, ushort address, int numberOfRows, int numberOfColumns)
        {
            int numberRamX = xPosition;
            int numberRamY = yPosition;
            string line;

            for (int row = 0; row < numberOfRows; row++)
            {
                line = string.Concat("$", address.ToString("X4"), ":");

                for (int column = 0; column < numberOfColumns; column++)
                {
                    line = string.Concat(line, " ", emulator.cpuRead(address, true).ToString("X2"));
                    address++;
                }

                _spriteBatch.DrawString(consoleFont, line, new Vector2(numberRamX, numberRamY), Color.White);

                numberRamY += 20;
            }
        }

        protected void DrawCpu(int xPosition, int yPosition)
        {
            _spriteBatch.DrawString(consoleFont, "STATUS:", new Vector2(xPosition, yPosition), Color.White);

            _spriteBatch.DrawString(consoleFont, "N", new Vector2(xPosition + 100, yPosition), (emulator.cpu.getFlag(Constants.flags.N) == 0) ? Color.Red : Color.LightGreen);
            _spriteBatch.DrawString(consoleFont, "V", new Vector2(xPosition + 130, yPosition), (emulator.cpu.getFlag(Constants.flags.V) == 0) ? Color.Red : Color.LightGreen);
            _spriteBatch.DrawString(consoleFont, "-", new Vector2(xPosition + 160, yPosition), (emulator.cpu.getFlag(Constants.flags.U) == 0) ? Color.Gray : Color.Gray);
            _spriteBatch.DrawString(consoleFont, "B", new Vector2(xPosition + 190, yPosition), (emulator.cpu.getFlag(Constants.flags.B) == 0) ? Color.Red : Color.LightGreen);
            _spriteBatch.DrawString(consoleFont, "D", new Vector2(xPosition + 220, yPosition), (emulator.cpu.getFlag(Constants.flags.D) == 0) ? Color.Red : Color.LightGreen);
            _spriteBatch.DrawString(consoleFont, "I", new Vector2(xPosition + 250, yPosition), (emulator.cpu.getFlag(Constants.flags.I) == 0) ? Color.Red : Color.LightGreen);
            _spriteBatch.DrawString(consoleFont, "Z", new Vector2(xPosition + 280, yPosition), (emulator.cpu.getFlag(Constants.flags.Z) == 0) ? Color.Red : Color.LightGreen);
            _spriteBatch.DrawString(consoleFont, "C", new Vector2(xPosition + 310, yPosition), (emulator.cpu.getFlag(Constants.flags.C) == 0) ? Color.Red : Color.LightGreen);

            _spriteBatch.DrawString(consoleFont, string.Concat("[", Convert.ToString(emulator.cpu.statusRegister, 2).PadLeft(8, '0') , "]"), new Vector2(xPosition + 340, yPosition), Color.White);

            _spriteBatch.DrawString(consoleFont, string.Concat("PC: $", emulator.cpu.pCounter.ToString("X4")), new Vector2(xPosition, (yPosition + 20)), Color.White);
            _spriteBatch.DrawString(consoleFont, string.Concat("A: $", emulator.cpu.acc.ToString("X2"), " [", emulator.cpu.acc, "]"), new Vector2(xPosition, (yPosition + 40)), Color.White);
            _spriteBatch.DrawString(consoleFont, string.Concat("X: $", emulator.cpu.xReg.ToString("X2"), " [", emulator.cpu.xReg, "]"), new Vector2(xPosition, (yPosition + 60)), Color.White);
            _spriteBatch.DrawString(consoleFont, string.Concat("Y: $", emulator.cpu.yReg.ToString("X2"), " [", emulator.cpu.yReg, "]"), new Vector2(xPosition, (yPosition + 80)), Color.White);
            _spriteBatch.DrawString(consoleFont, string.Concat("Stack: $", emulator.cpu.stack.ToString("X4")), new Vector2(xPosition, (yPosition + 100)), Color.White);
        }

        protected void DrawCode(int xPosition, int yPosition, int numberOfLines)
        {
            string currentString = emulator.disassembly[emulator.cpu.pCounter];
            int indexOfKey = emulator.disassembly.IndexOfKey(emulator.cpu.pCounter);
            int numberLineY = (numberOfLines >> 1) * 20 + yPosition;

            if (indexOfKey != (emulator.disassembly.Keys.Count - 1))
            {
                _spriteBatch.DrawString(consoleFont, currentString, new Vector2(xPosition, numberLineY), Color.Cyan);

                while (numberLineY < (numberOfLines * 20) + yPosition)
                {
                    numberLineY += 20;

                    if (++indexOfKey < emulator.disassembly.Keys.Count)
                    {
                        _spriteBatch.DrawString(consoleFont, emulator.disassembly.Values[indexOfKey], new Vector2(xPosition, numberLineY), Color.White);
                    }
                    else
                    {
                        break;
                    }
                }

                numberLineY = (numberOfLines >> 1) * 20 + yPosition;
                indexOfKey = emulator.disassembly.IndexOfKey(emulator.cpu.pCounter);

                while (numberLineY > yPosition)
                {
                    numberLineY -= 20;

                    if (--indexOfKey != emulator.disassembly.Keys.Count - 1)
                    {
                        if (indexOfKey < 0)
                        {
                            break;
                        }

                        _spriteBatch.DrawString(consoleFont, emulator.disassembly.Values[indexOfKey], new Vector2(xPosition, numberLineY), Color.White);
                    }
                }
            }
        }
    }
}
