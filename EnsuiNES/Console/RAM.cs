using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace EnsuiNES.Console
{
    class RAM
    {
        private byte[] mem;

        public RAM() {
            mem = new byte[2048];
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

        public void setResetVector(byte hiByte, byte loByte)
        {
            write(0xFFFC, hiByte);
            write(0xFFFD, loByte);
        }

        public void loadTestData()
        {
            ushort address = 0x8000;

            byte[] program = new byte[] {
                0xA2, 0x0A, 0x8E, 0x00, 0x00, 0xA2, 0x03, 0x8E,
                0x01, 0x00, 0xAC, 0x00, 0x00, 0xA9, 0x00, 0x18,
                0x6D, 0x01, 0x00, 0x88, 0xD0, 0xFA, 0x8D, 0x02,
                0x00, 0xEA, 0xEA, 0xEA
            };

            foreach (byte instruction in program)
            {
                write(address, instruction);
                address++;
            }
        }
    }
}
