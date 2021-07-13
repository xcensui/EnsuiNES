using System;
using System.Collections.Generic;
using System.Text;

namespace EnsuiNES.Console
{
    class Constants
    {
        public delegate byte method();

        public const ushort memoryStart = 0x0000;
        public const ushort memoryEnd = 0x1FFF;

        public enum flags
        {
            C = (1 << 0), //Carry
            Z = (1 << 1), //Zero
            I = (1 << 2), //Disable Interupts
            D = (1 << 3), //Decimal Mode (Unused)
            B = (1 << 4), //Break
            U = (1 << 5), //Unused
            V = (1 << 6), //Overflow
            N = (1 << 7)  //Negative
        }

        public struct instruction
        {
            public string name;
            public byte cycles;
            public method operation;
            public method addressMode;
        }
    }
}
