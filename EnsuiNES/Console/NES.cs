using System;
using System.Collections.Generic;
using System.Text;

namespace EnsuiNES.Console
{
    class NES
    {
        public _6502 cpu;
        public RAM memory;

        public NES()
        {
            cpu = new _6502();
            memory = new RAM();

            cpu.Connect(this);
            memory.reset();
        }

        public void write(ushort address, byte data)
        {
            if (address >= Constants.memoryStart && address <= Constants.memoryEnd)
                memory.write(address, data);
        }

        public byte read(ushort address, bool readOnly = false)
        {
            if (address >= Constants.memoryStart && address <= Constants.memoryEnd) 
                return memory.read(address, readOnly);

            return 0x00;
        }
    }
}
