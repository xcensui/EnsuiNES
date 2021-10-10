using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EnsuiNES.Console;
using System;
using System.Diagnostics;
using System.Threading;

namespace EnsuiNES
{
    public class Game1 : Game
    {
        private NES emulator;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont consoleFont;
        private SpriteFont consoleFontSmall;

        private RenderTarget2D debugger;
        private RenderTarget2D screen;
        private RenderTarget2D patternTable1;
        private RenderTarget2D patternTable2;

        private Texture2D screendata;
        private Texture2D patternTableData1;
        private Texture2D patternTableData2;

        private Thread emulationThread;

        private Console.Cartridge cartridge;

        private bool emulationRunning;
        private double residualTime;
        private double lastElapsedTime;

        private double emulationElapsedTime = 0.0f;

        private Stopwatch timer = new Stopwatch();

        private int framesComplete = 0;

        private ushort ramBank;

        private byte selectedPalette = 0x00;

        public Game1()
        {
            emulationRunning = false;
            residualTime = 0.0f;

            ramBank = 0x0002;

            emulator = new NES();
            cartridge = new Cartridge(@"D:\nestest.nes");
            emulator.insertCartridge(cartridge);

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1200;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();

            consoleFont = Content.Load<SpriteFont>("ConsoleFont");
            consoleFontSmall = Content.Load<SpriteFont>("ConsoleFontSmall");
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            screendata = new Texture2D(GraphicsDevice, 256, 240);
            patternTableData1 = new Texture2D(GraphicsDevice, Constants.patternTableSize, Constants.patternTableSize);
            patternTableData2 = new Texture2D(GraphicsDevice, Constants.patternTableSize, Constants.patternTableSize);

            emulationThread = new Thread(threadWork);
            emulationThread.IsBackground = true;

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            Helper.Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (emulationRunning)
            {
                
            }
            else
            {
                if (Helper.Keyboard.IsPressed(Keys.C, true))
                {
                    Debug.WriteLine("Step");

                    do
                    {
                        emulator.clock();
                    }
                    while (!emulator.cpu.complete());

                    do
                    {
                        emulator.clock();
                    }
                    while (!emulator.cpu.complete());
                }

                if (Helper.Keyboard.IsPressed(Keys.F, true))
                {
                    Debug.WriteLine("Frame");

                    do
                    {
                        emulator.clock();
                    }
                    while (!emulator.ppu.frameComplete);

                    do
                    {
                        emulator.clock();
                    }
                    while (!emulator.cpu.complete());

                    emulator.ppu.frameComplete = false;
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

                if (Helper.Keyboard.IsPressed(Keys.S, true))
                {
                    Debug.WriteLine("Switch Bank");

                    ramBank += 0x0100;
                }

                if (Helper.Keyboard.IsPressed(Keys.P, true))
                {
                    Debug.WriteLine("Switch Palette");

                    ++selectedPalette;
                    selectedPalette &= 0x07;
                }

            }

            if (Helper.Keyboard.IsPressed(Keys.R, true))
            {
                Debug.WriteLine("Reset");
                emulator.reset();
            }

            if (Helper.Keyboard.IsPressed(Keys.Space, true))
            {
                if (!emulationRunning)
                {
                    emulationElapsedTime = 0.0f;
                    residualTime = 0.0f;
                    timer.Start();

                    if (!emulationThread.IsAlive)
                    {
                        emulationThread = new Thread(threadWork);
                        emulationThread.IsBackground = true;
                    }

                    emulationThread.Start();
                }
                else
                {
                    timer.Stop();
                }

                emulationRunning = !emulationRunning;
            }

            // TODO: Add your update logic here

            Color[] screenBuffer = emulator.getScreenData();
            Color[] paletteBuffer1 = emulator.getPatternData(1, selectedPalette);
            Color[] paletteBuffer2 = emulator.getPatternData(2, selectedPalette);

            if (screenBuffer != null)
            {
                screendata.SetData(screenBuffer);
            }

            if (paletteBuffer1 != null)
            {
                patternTableData1.SetData(paletteBuffer1);
            }

            if (paletteBuffer2 != null)
            {
                patternTableData2.SetData(paletteBuffer2);
            }


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            debugger = new RenderTarget2D(_graphics.GraphicsDevice, (_graphics.PreferredBackBufferWidth - 20), (_graphics.PreferredBackBufferHeight - 20));
            screen = new RenderTarget2D(_graphics.GraphicsDevice, Constants.nesScreenWidth * 3, Constants.nesScreenHeight * 3);
            patternTable1 = new RenderTarget2D(_graphics.GraphicsDevice, Constants.patternTableSize * 2, Constants.patternTableSize * 2);
            patternTable2 = new RenderTarget2D(_graphics.GraphicsDevice, Constants.patternTableSize * 2, Constants.patternTableSize * 2);

            DrawDebugger();
            DrawScreen();
            DrawPatternTables();

            //Rectangle bounds = new Rectangle(10, 10, Constants.nesScreenWidth, Constants.nesScreenWidth);

            GraphicsDevice.SetRenderTarget(null);

            GraphicsDevice.Clear(Color.MidnightBlue);

            _spriteBatch.Begin();

            _spriteBatch.Draw(debugger, new Vector2(10, 10), Color.White);
            _spriteBatch.Draw(screen, new Vector2(20, 20), Color.White);
            _spriteBatch.Draw(patternTable1, new Vector2(630, 810), Color.White);
            _spriteBatch.Draw(patternTable2, new Vector2(906, 810), Color.White);
            _spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);

            debugger.Dispose();
            screen.Dispose();
            patternTable1.Dispose();
            patternTable2.Dispose();
        }

        protected void DrawPatternTables()
        {
            Rectangle bounds = new Rectangle(0, 0, Constants.patternTableSize * 2, Constants.patternTableSize * 2);

            GraphicsDevice.SetRenderTarget(patternTable1);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(patternTableData1, bounds, Color.White);
            _spriteBatch.End();

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(patternTableData2, bounds, Color.White);
            _spriteBatch.End();
        }

        protected void DrawDebugger()
        {
            GraphicsDevice.SetRenderTarget(debugger);

            GraphicsDevice.Clear(Color.Blue);

            _spriteBatch.Begin();

            DrawRam(10, 750, ramBank, 16, 16);

            DrawCpu(800, 10);

            DrawCode(800, 140, 32);

            //_spriteBatch.DrawString(consoleFontSmall, "SPACE: Toggle Running  C: Clock  F: Frame  R: Reset Console  I: Interrupt  N: NMI  S: Switch RAM Page", new Vector2(10, (debugger.Height - 20)), Color.White);

            _spriteBatch.End();
        }

        protected void DrawScreen()
        {
            Rectangle bounds = new Rectangle(0, 0, Constants.nesScreenWidth * 3, Constants.nesScreenHeight * 3);

            GraphicsDevice.SetRenderTarget(screen);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(screendata, bounds, Color.White);
            _spriteBatch.End();
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

                _spriteBatch.DrawString(consoleFontSmall, line, new Vector2(numberRamX, numberRamY), Color.White);

                numberRamY += 15;
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

            _spriteBatch.DrawString(consoleFont, string.Concat("PC: $", emulator.cpu.pCounter.ToString("X4")), new Vector2(xPosition, (yPosition + 20)), Color.White);
            _spriteBatch.DrawString(consoleFont, string.Concat("A: $", emulator.cpu.acc.ToString("X2"), " [", emulator.cpu.acc, "]"), new Vector2(xPosition, (yPosition + 40)), Color.White);
            _spriteBatch.DrawString(consoleFont, string.Concat("X: $", emulator.cpu.xReg.ToString("X2"), " [", emulator.cpu.xReg, "]"), new Vector2(xPosition, (yPosition + 60)), Color.White);
            _spriteBatch.DrawString(consoleFont, string.Concat("Y: $", emulator.cpu.yReg.ToString("X2"), " [", emulator.cpu.yReg, "]"), new Vector2(xPosition, (yPosition + 80)), Color.White);
            _spriteBatch.DrawString(consoleFont, string.Concat("Stack: $", emulator.cpu.stack.ToString("X4")), new Vector2(xPosition, (yPosition + 100)), Color.White);
        }

        protected void DrawCode(int xPosition, int yPosition, int numberOfLines)
        {
            string currentString = "";
            try
            {
                currentString = emulator.disassembly[emulator.cpu.pCounter];
            }
            catch (Exception e)
            {
            }

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

        protected void threadWork()
        {
            while (emulationRunning)
            {
                emulationElapsedTime = timer.ElapsedMilliseconds - lastElapsedTime;
                lastElapsedTime = timer.ElapsedMilliseconds;

                if (residualTime > 0.0f)
                {
                    residualTime -= emulationElapsedTime;
                }
                else
                {
                    residualTime += ((1.0f / 60.0f) * 1000) - emulationElapsedTime;

                    do
                    {
                        emulator.clock();
                    }
                    while (!emulator.ppu.frameComplete);

                    emulator.ppu.frameComplete = false;
                    framesComplete++;
                }
            }
        }
    }
}
