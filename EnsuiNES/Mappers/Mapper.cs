using System;
using System.Collections.Generic;
using System.Text;

namespace EnsuiNES.Mappers
{
    class Mapper
    {
        protected byte prgBanks = 0;
        protected byte chrBanks = 0;

        public Mapper(byte programBanks, byte characterBanks)
        {
            prgBanks = programBanks;
            chrBanks = characterBanks;
        }

        public virtual bool cpuMapRead(ushort address, ref uint mappedAddress)
        {
            return false;
        }

        public virtual bool cpuMapWrite(ushort address, ref uint mappedAddress)
        {
            return false;
        }

        public virtual bool ppuMapRead(ushort address, ref uint mappedAddress)
        {
            return false;
        }

        public virtual bool ppuMapWrite(ushort address, ref uint mappedAddress)
        {
            return false;
        }
    }
}
