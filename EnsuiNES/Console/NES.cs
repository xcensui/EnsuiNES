using System;
using System.Collections.Generic;
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

        public NES()
        {
            cpu = new _6502();
            ppu = new PPU();
            memory = new RAM();

            memory.reset();
            memory.setResetVector(0x00, 0x80);
            memory.loadTestData();

            cpu.Connect(this);

            disassembly = cpu.disassembled(0x0000, 0xFFFF);
        }

        public void cpuWrite(ushort address, byte data)
        {
            if (address >= Constants.memoryStart && address <= Constants.memoryEnd)
                memory.write((byte)(address & 0x07FF), data);
        }

        public byte cpuRead(ushort address, bool readOnly = false)
        {
            byte data = 0x00;

            if (address >= Constants.memoryStart && address <= Constants.memoryEnd) 
                data = memory.read((byte)(address & 0x07FF), readOnly);

            return data;
        }
    }
}
