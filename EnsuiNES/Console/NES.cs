using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Text;

namespace EnsuiNES.Console
{
    class NES
    {
        public _6502 cpu;
        public PPU ppu;
        public Cartridge cartridge;
        public RAM memory;
        public SortedList<ushort, string> disassembly;

        private uint systemClockCounter = 0;

        public NES()
        {
            cpu = new _6502();
            ppu = new PPU();
            memory = new RAM(2048);

            cpu.Connect(this);
        }

        public Color[] getScreenData()
        {
            return ppu.getScreenData();
        }

        public Color[] getPatternData(int patternTable, byte palette)
        {
            return ppu.getPatternTable(patternTable, palette);
        }

        public void cpuWrite(ushort address, byte data)
        {
            if (cartridge.cpuWrite(address, data))
            {

            }
            else if ((address >= Constants.memoryStart && address <= Constants.memoryEnd) == true)
            {
                memory.write((byte)(address & 0x07FF), data);
            }
            else if ((address >= Constants.ppuStart && address <= Constants.ppuEnd) == true)
            {
                ppu.cpuWrite((ushort)(address & 0x0007), data);
            }                
            else
            {
                Debug.WriteLine("Something is wrong");
            }
        }

        public byte cpuRead(ushort address, bool readOnly = false)
        {
            byte data = 0x00;

            if (cartridge.cpuRead(address, ref data))
            {

            }
            else if ((address >= Constants.memoryStart && address <= Constants.memoryEnd) == true) 
                data = memory.read((byte)(address & 0x07FF), readOnly);

            else if ((address >= Constants.ppuStart && address <= Constants.ppuEnd) == true)
                data = ppu.cpuRead((ushort)(address & 0x0007), readOnly);

            return data;
        }

        public void insertCartridge(Cartridge cart)
        {
            cartridge = cart;
            ppu.ConnectCartridge(cartridge);

            disassembly = cpu.disassembled(0x0000, 0xFFFF);

            reset();
        }

        public void reset()
        {
            memory.reset();
            cpu.reset();
            ppu.reset();
            systemClockCounter = 0;
        }

        public void clock()
        {
            ppu.clock();

            if (systemClockCounter % 3 == 0)
            {
                cpu.clock();
            }

            systemClockCounter++;
        }
    }
}
