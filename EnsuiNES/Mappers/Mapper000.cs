using System;
using System.Collections.Generic;
using System.Text;

namespace EnsuiNES.Mappers
{
    class Mapper000 : Mapper
    {
        public Mapper000(byte programBanks, byte characterBanks) : base(programBanks, characterBanks)
        {
            
        }

        public override bool cpuMapRead(ushort address, ref uint mappedAddress)
        {
            if (address >= Console.Constants.cartridgeStart && address <= Console.Constants.cartridgeEnd)
            {
                mappedAddress = (uint)(address & ((prgBanks > 1) ? 0x7FFF : 0x3FFF));
                return true;
            }

            return false;
        }

        public override bool cpuMapWrite(ushort address, ref uint mappedAddress)
        {
            if (address >= Console.Constants.cartridgeStart && address <= Console.Constants.cartridgeEnd)
            {
                mappedAddress = (uint)(address & ((prgBanks > 1) ? 0x7FFF : 0x3FFF));
                return true;
            }

            return false;
        }

        public override bool ppuMapRead(ushort address, ref uint mappedAddress)
        {
            if (address >= Console.Constants.cartridgeStart && address <= Console.Constants.ppuCartridgeEnd)
            {
                mappedAddress = (uint)address;
                return true;
            }

            return false;
        }

        public override bool ppuMapWrite(ushort address, ref uint mappedAddress)
        {
            if (address >= Console.Constants.cartridgeStart && address <= Console.Constants.ppuCartridgeEnd)
            {
                
            }

            return false;
        }
    }
}
