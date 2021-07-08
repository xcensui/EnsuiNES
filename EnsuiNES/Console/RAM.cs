using System;
using System.Collections.Generic;
using System.Text;

namespace EnsuiNES.Console
{
    class RAM
    {
        private byte[] mem;

        public RAM() {
            mem = new byte[64 * 1024];
        }

        public void reset()
        {
            for (int index = 0; index < mem.Length; index++)
            {
                mem[index] = 0x00;
            }
        }

        public void write(ushort address, byte data)
        {
            mem[address] = data;
        }

        public byte read(ushort address, bool readOnly = false)
        {
            return mem[address];
        }
    }
}
