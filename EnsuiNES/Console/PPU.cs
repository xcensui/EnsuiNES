using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace EnsuiNES.Console
{
    class PPU
    {
        private Cartridge cartridge;

        public ushort cycle;
        public short scanline;

        public bool frameComplete;

        private byte mask;
        private byte control; 
        private byte status;

        private byte addressLatch;
        private byte dataBuffer;
        private ushort ppuAddress;

        private RAM vram1;
        private RAM vram2;
        private RAM palette;
        private RAM pattern1;
        private RAM pattern2;

        Color[] screenPalette;
        Color[] screenData;
        Color[] nameTable1;
        Color[] nameTable2;
        Color[] patternTable1;
        Color[] patternTable2;

        public PPU()
        {
            frameComplete = false;
            scanline = 0;
            cycle = 0;

            vram1 = new RAM(1024);
            vram2 = new RAM(1024);
            palette = new RAM(32);
            pattern1 = new RAM(4096);
            pattern2 = new RAM(4096);

            screenPalette = new Color[0x40];
            screenData = new Color[Constants.nesScreenWidth * Constants.nesScreenHeight];
            nameTable1 = new Color[Constants.nesScreenWidth * Constants.nesScreenHeight];
            nameTable2 = new Color[Constants.nesScreenWidth * Constants.nesScreenHeight];
            patternTable1 = new Color[Constants.patternTableSize * Constants.patternTableSize];
            patternTable2 = new Color[Constants.patternTableSize * Constants.patternTableSize];

            addressLatch = 0x00;
            dataBuffer = 0x00;
            ppuAddress = 0x0000;

            mask = 0x00;
            control = 0x00;
            status = 0x00;

            loadPaletteData();
        }

        public void reset()
        {
            Array.Fill(screenData, new Color(0, 0, 0));
        }
        public Color[] getScreenData()
        {
            return screenData;
        }

        public Color[] getNameTable(int nameTable)
        {
            if (nameTable == 1)
            {
                return nameTable1;
            }

            return nameTable2;
        }

        public Color getColorFromPalette(byte palette, byte pixel)
        {
            byte colourLocation = ppuRead((byte)(0x3F00 + (palette << 2) + pixel));

            return screenPalette[colourLocation & 0x3F];
        }

        public Color[] getPatternTable(int patternTable, byte palette)
        {
            for (ushort tileY = 0; tileY < 16; tileY++)
            {
                for (ushort tileX = 0; tileX < 16; tileX++)
                {
                    ushort tileOffset = (ushort)((tileY * 256) + tileX * 16);

                    for (ushort row = 0; row < 8; row++)
                    {
                        byte tileLSB = ppuRead((ushort)((patternTable - 1) * 0x1000 + tileOffset + row));
                        byte tileMSB = ppuRead((ushort)((patternTable - 1) * 0x1000 + tileOffset + row + 0x0008));

                        for (ushort col = 0; col < 8; col++)
                        {
                            byte pixel = (byte)((byte)(tileLSB & 0x01) + (byte)(tileMSB & 0x01));
                            tileLSB >>= 1;
                            tileMSB >>= 1;

                            int tableX = tileX * 8 + (7 - col);
                            int tableY = tileY * 8 + row;
                            int tablePosition = tableX + (tableY * Constants.patternTableSize);
                            Color colour = getColorFromPalette(palette, pixel);

                            if (patternTable == 1)
                            {
                                patternTable1[tablePosition] = colour;
                            }
                            else
                            {
                                patternTable2[tablePosition] = colour;
                            }
                        }
                    }
                }
            }
            
            if (patternTable == 1)
            {
                return patternTable1;
            }

            return patternTable2;
        }

        protected void setPixel(int xPosition, int yPosition, Color colour)
        {
            if (xPosition < 0)
            {
                xPosition = 0;
            }
            else if (xPosition >= Constants.nesScreenWidth) {
                xPosition = Constants.nesScreenWidth - 1;
            }

            if (yPosition < 0)
            {
                yPosition = 0;
            }
            else if (yPosition >= Constants.nesScreenHeight)
            {
                yPosition = Constants.nesScreenHeight - 1;
            }


            int position = xPosition + (yPosition * Constants.nesScreenWidth);

            screenData[position] = colour;
        }

        public void clock()
        {
            Random rand = new Random();

            setPixel(cycle - 1, scanline, screenPalette[(rand.Next() % 2 == 0) ? 0x3F : 0x30]);
            cycle++;

            if (cycle >= Constants.endOfLine)
            {
                cycle = 0;
                scanline++;

                if (scanline >= Constants.endOfRows)
                {
                    scanline = -1;
                    frameComplete = true;
                }
            }
        }

        public void ConnectCartridge(Cartridge cart)
        {
            cartridge = cart;
        }

        public byte cpuRead(ushort address, bool readOnly = false)
        {
            byte data = 0x00;

            switch (address)
            {
                case (ushort)Constants.ppuAddress.Control:
                    break;
                case (ushort)Constants.ppuAddress.Mask:
                    break;
                case (ushort)Constants.ppuAddress.Status:
                    setStatusValue(Constants.ppuStatus.spriteVBlank, true);

                    data = (byte)((status & 0xE0) | (dataBuffer & 0x1F));

                    setStatusValue(Constants.ppuStatus.spriteVBlank, false);
                    addressLatch = 0;       
                    break;
                case (ushort)Constants.ppuAddress.OAMAddress:
                    break;
                case (ushort)Constants.ppuAddress.OAMData:
                    break;
                case (ushort)Constants.ppuAddress.Scroll:
                    break;
                case (ushort)Constants.ppuAddress.PPUAddress:
                    break;
                case (ushort)Constants.ppuAddress.PPUData:
                    data = dataBuffer;
                    dataBuffer = ppuRead(ppuAddress);

                    if (ppuAddress > Constants.paletteMemoryStart)
                    {
                        data = dataBuffer;
                    }

                    ppuAddress++;
                    break;
            }

            return data;
        }

        protected byte getValueForStatus(byte data, Constants.ppuStatus flag)
        {
            return ((byte)((byte)(data & (byte)flag) > 0 ? 1 : 0));
        }

        protected void setStatusValue(Constants.ppuStatus flag, bool value)
        {
            if (value)
            {
                status |= (byte)flag;
            }
            else
            {
                status &= (byte)~flag;
            }
        }

        protected byte getStatusValue(Constants.ppuStatus flag)
        {
            return ((byte)((byte)(status & (byte)flag) > 0 ? 1 : 0));
        }

        public void cpuWrite(ushort address, byte data)
        {
            switch (address)
            {
                case (ushort)Constants.ppuAddress.Control:
                    control = data;
                    break;
                case (ushort)Constants.ppuAddress.Mask:
                    mask = data;
                    break;
                case (ushort)Constants.ppuAddress.Status:
                    
                    break;
                case (ushort)Constants.ppuAddress.OAMAddress:
                    break;
                case (ushort)Constants.ppuAddress.OAMData:
                    break;
                case (ushort)Constants.ppuAddress.Scroll:
                    break;
                case (ushort)Constants.ppuAddress.PPUAddress:
                    if (addressLatch == 0)
                    {
                        ppuAddress = (ushort)((ppuAddress & 0x00FF) | (data << 8));
                        //ppuAddress = (ushort)((ppuAddress & 0x3F) | (ppuAddress & 0x00FF));
                        addressLatch = 1;
                    }
                    else
                    {
                        ppuAddress = (ushort)((ppuAddress & 0xFF00) | data);
                        //ppuAddress = (ushort)((ppuAddress & 0xFF00) | data);
                        addressLatch = 0;
                    }
                    break;
                case (ushort)Constants.ppuAddress.PPUData:
                    ppuWrite(ppuAddress, data);
                    ppuAddress++;
                    break;
                default:
                    Debug.WriteLine("Something has gone wrong");
                    break;
            }
        }

        public byte ppuRead(ushort address, bool readOnly = false)
        {
            byte data = 0x00;
            address &= 0x3FFF;

            if (cartridge.ppuRead(address, ref data))
            {

            }
            else if (address >= Constants.patternMemoryStart && address <= Constants.patternMemoryEnd)
            {
                ushort pattern = (ushort)((address & 0x1000) >> 12);

                if (pattern == 0)
                {
                    data = pattern1.read((ushort)(address & 0x0FFF));
                }
                else
                {
                    data = pattern2.read((ushort)(address & 0x0FFF));
                }
            }
            else if (address >= Constants.nameTableStart && address <= Constants.nameTableEnd)
            {

            }
            else if (address >= Constants.paletteMemoryStart && address <= Constants.paletteMemoryEnd)
            {
                address &= 0x001F;

                if (address == 0x0010) address = 0x0000;
                if (address == 0x0010) address = 0x0004;
                if (address == 0x0010) address = 0x0008;
                if (address == 0x0010) address = 0x000C;

                data = palette.read(address);
            }

            return data;
        }

        public void ppuWrite(ushort address, byte data)
        {
            address &= 0x3FFF;

            if (cartridge.ppuWrite(address, data))
            {

            }
            else if ((address >= Constants.patternMemoryStart && address <= Constants.patternMemoryEnd) == true)
            {
                ushort pattern = (ushort)((address & 0x1000) >> 12);

                if (pattern == 0)
                {
                    pattern1.write((ushort)(address & 0x0FFF), data);
                }
                else
                {
                    pattern2.write((ushort)(address & 0x0FFF), data);
                }
            }
            else if ((address >= Constants.nameTableStart && address <= Constants.nameTableEnd) == true)
            {

            }
            else if ((address >= Constants.paletteMemoryStart && address <= Constants.paletteMemoryEnd) == true)
            {
                address &= 0x001F;

                if (address == 0x0010) address = 0x0000;
                if (address == 0x0010) address = 0x0004;
                if (address == 0x0010) address = 0x0008;
                if (address == 0x0010) address = 0x000C;

                palette.write(address, data);
            }
        }

        protected void loadPaletteData()
        {
            screenPalette[0x00] = new Color(84, 84, 84);
            screenPalette[0x01] = new Color(0, 30, 116);
            screenPalette[0x02] = new Color(8, 16, 144);
            screenPalette[0x03] = new Color(48, 0, 136);
            screenPalette[0x04] = new Color(68, 0, 100);
            screenPalette[0x05] = new Color(92, 0, 48);
            screenPalette[0x06] = new Color(84, 4, 0);
            screenPalette[0x07] = new Color(60, 24, 0);
            screenPalette[0x08] = new Color(32, 42, 0);
            screenPalette[0x09] = new Color(8, 58, 0);
            screenPalette[0x0A] = new Color(0, 64, 0);
            screenPalette[0x0B] = new Color(0, 60, 0);
            screenPalette[0x0C] = new Color(0, 50, 60);
            screenPalette[0x0D] = new Color(0, 0, 0);
            screenPalette[0x0E] = new Color(0, 0, 0);
            screenPalette[0x0F] = new Color(0, 0, 0);

            screenPalette[0x10] = new Color(152, 150, 152);
            screenPalette[0x11] = new Color(8, 76, 196);
            screenPalette[0x12] = new Color(48, 50, 236);
            screenPalette[0x13] = new Color(92, 30, 228);
            screenPalette[0x14] = new Color(136, 20, 176);
            screenPalette[0x15] = new Color(160, 20, 100);
            screenPalette[0x16] = new Color(152, 34, 32);
            screenPalette[0x17] = new Color(120, 60, 0);
            screenPalette[0x18] = new Color(84, 90, 0);
            screenPalette[0x19] = new Color(40, 114, 0);
            screenPalette[0x1A] = new Color(8, 126, 84);
            screenPalette[0x1B] = new Color(0, 118, 40);
            screenPalette[0x1C] = new Color(0, 102, 120);
            screenPalette[0x1D] = new Color(0, 0, 0);
            screenPalette[0x1E] = new Color(0, 0, 0);
            screenPalette[0x1F] = new Color(0, 0, 0);

            screenPalette[0x20] = new Color(236, 238, 236);
            screenPalette[0x21] = new Color(76, 154, 236);
            screenPalette[0x22] = new Color(120, 124, 236);
            screenPalette[0x23] = new Color(176, 98, 236);
            screenPalette[0x24] = new Color(228, 84, 236);
            screenPalette[0x25] = new Color(236, 88, 180);
            screenPalette[0x26] = new Color(236, 106, 100);
            screenPalette[0x27] = new Color(212, 136, 32);
            screenPalette[0x28] = new Color(160, 170, 0);
            screenPalette[0x29] = new Color(116, 196, 0);
            screenPalette[0x2A] = new Color(76, 208, 32);
            screenPalette[0x2B] = new Color(56, 204, 108);
            screenPalette[0x2C] = new Color(56, 180, 204);
            screenPalette[0x2D] = new Color(60, 60, 60);
            screenPalette[0x2E] = new Color(0, 0, 0);
            screenPalette[0x2F] = new Color(0, 0, 0);

            screenPalette[0x30] = new Color(236, 238, 236);
            screenPalette[0x31] = new Color(168, 204, 236);
            screenPalette[0x32] = new Color(188, 188, 236);
            screenPalette[0x33] = new Color(212, 178, 236);
            screenPalette[0x34] = new Color(236, 174, 236);
            screenPalette[0x35] = new Color(236, 174, 212);
            screenPalette[0x36] = new Color(236, 180, 176);
            screenPalette[0x37] = new Color(228, 196, 144);
            screenPalette[0x38] = new Color(204, 210, 120);
            screenPalette[0x39] = new Color(180, 222, 120);
            screenPalette[0x3A] = new Color(168, 226, 144);
            screenPalette[0x3B] = new Color(152, 226, 180);
            screenPalette[0x3C] = new Color(160, 214, 228);
            screenPalette[0x3D] = new Color(160, 162, 160);
            screenPalette[0x3E] = new Color(0, 0, 0);
            screenPalette[0x3F] = new Color(0, 0, 0);
        }
    }
}
