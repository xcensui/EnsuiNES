using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EnsuiNES.Console
{
    class Constants
    {
        public delegate byte method();

        public const int nesScreenWidth = 256;
        public const int nesScreenHeight = 240;

        public const int patternTableSize = 128;

        public const ushort memoryStart = 0x0000;
        public const ushort memoryEnd = 0x1FFF;

        public const ushort patternMemoryStart = 0x0000;
        public const ushort patternMemoryEnd = 0x1FFF;

        public const ushort nameTableStart = 0x2000;
        public const ushort nameTableEnd = 0x3EFF;

        public const ushort paletteMemoryStart = 0x3F00;
        public const ushort paletteMemoryEnd = 0x3FFF;

        public const ushort ppuStart = 0x2000;
        public const ushort ppuEnd = 0x3FFF;

        public const ushort cartridgeStart = 0x8000;
        public const ushort cartridgeEnd = 0xFFFF;

        public const ushort ppuCartridgeStart = 0x0000;
        public const ushort ppuCartridgeEnd = 0x1FFF;

        public const uint programBankSize = 16384;
        public const uint characterBankSize = 8192;

        public const int endOfLine = 341;
        public const int endOfRows = 261;

        public enum ppuAddress
        {
            Control = 0x0000,
            Mask = 0x0001,
            Status = 0x0002,
            OAMAddress = 0x0003,
            OAMData = 0x0004,
            Scroll = 0x0005,
            PPUAddress = 0x0006,
            PPUData = 0x0007,
        }

        public enum ppuStatus
        {
            U = (1 << 0),
            U2 = (1 << 1),
            U3 = (1 << 2),
            U4 = (1 << 3),
            U5 = (1 << 4),
            SpriteOverflow = (1 << 5),
            spriteZeroHit = (1 << 6),
            spriteVBlank = (1 << 7)
        }

        public enum ppuControl
        {
            NametableX = (1 << 0),
            NametableY = (1 << 1),
            IncrementMode = (1 << 2),
            PatternSprite = (1 << 3),
            PatternBackground = (1 << 4),
            SpriteSize = (1 << 5),
            SlaveMode = (1 << 6),
            EnableNMI = (1 << 7)
        }

        public enum ppuMask
        {
            GrayScale = (1 << 0),
            renderBackgroundLeft = (1 << 1),
            renderSpritesLeft = (1 << 2),
            renderBackground = (1 << 3),
            renderSprites = (1 << 4),
            enhanceRed = (1 << 5),
            enhanceGreen = (1 << 6),
            enhanceBlue = (1 << 7)
        }

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

        [StructLayout(LayoutKind.Explicit)]
        public struct cartHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            [FieldOffset(0)]
            public char[] name;
            [FieldOffset(8)]
            public byte prgRomChunks;
            [FieldOffset(9)]
            public byte chrRomChunks;
            [FieldOffset(10)]
            public byte mapper1;
            [FieldOffset(11)]
            public byte mapper2;
            [FieldOffset(12)]
            public byte prgRamSize;
            [FieldOffset(13)]
            public byte tvSystem1;
            [FieldOffset(14)]
            public byte tvSystem2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            [FieldOffset(16)]
            public char[] unused;
        }
    }
}
