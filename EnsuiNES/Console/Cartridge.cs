using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace EnsuiNES.Console
{
    class Cartridge 
    {
        private RAM prgMemory;
        private RAM chrMemory;

        private byte mapperID = 0;
        private byte prgBanks = 0;
        private byte chrBanks = 0;

        private Constants.cartHeader header;

        private Mappers.Mapper mapper;

        public Cartridge(string fileName)
        {
            BinaryReader fileStream = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read));
            //FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            
            int headerSize = Marshal.SizeOf(typeof(Constants.cartHeader));

            byte[] data = new byte[headerSize];

            //fileStream.Read(data, 0, headerSize);

            header.name = new char[4];
            header.name[0] = fileStream.ReadChar();
            header.name[1] = fileStream.ReadChar();
            header.name[2] = fileStream.ReadChar();
            header.name[3] = fileStream.ReadChar();

            header.prgRomChunks = fileStream.ReadByte();
            header.chrRomChunks = fileStream.ReadByte();
            header.mapper1 = fileStream.ReadByte();
            header.mapper2 = fileStream.ReadByte();
            header.prgRamSize = fileStream.ReadByte();
            header.tvSystem1 = fileStream.ReadByte();
            header.tvSystem2 = fileStream.ReadByte();

            header.unused = new char[5];
            header.unused[0] = fileStream.ReadChar();
            header.unused[1] = fileStream.ReadChar();
            header.unused[2] = fileStream.ReadChar();
            header.unused[3] = fileStream.ReadChar();
            header.unused[4] = fileStream.ReadChar();

            //data = fileStream.ReadBytes(headerSize);

            //GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            //header = (Constants.cartHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Constants.cartHeader));

            if ((byte)(header.mapper1 & 0x04) != 0)
            {
                fileStream.BaseStream.Seek(512, SeekOrigin.Current);
            }

            mapperID = (byte)(((header.mapper2 >> 4) << 4) | (header.mapper1 >> 4));

            byte fileType = 1;

            if (fileType == 0)
            {

            }

            if (fileType == 1)
            {
                prgBanks = header.prgRomChunks;
                uint prgSize = (uint)(prgBanks * Constants.programBankSize);

                chrBanks = header.chrRomChunks;
                uint chrSize = (uint)(chrBanks * Constants.characterBankSize);

                data = new byte[prgSize];

                prgMemory = new RAM(prgSize);

                fileStream.Read(data, 0, (int)prgSize);

                for (ushort index = 0; index < prgSize; index++)
                {
                    prgMemory.write(index, data[index]);
                }

                data = new byte[chrSize];

                chrMemory = new RAM(chrSize);

                fileStream.Read(data, 0, (int)chrSize);

                for (ushort index = 0; index < chrSize; index++)
                {
                    chrMemory.write(index, data[index]);
                }
            }

            if (fileType == 2)
            {

            }

            setMapper();

            fileStream.Close();
            fileStream.Dispose();
        }

        public bool cpuRead(ushort address, ref byte data)
        {
            uint mappedAddress = 0;

            if (mapper.cpuMapRead(address, ref mappedAddress))
            {
                data = prgMemory.read((ushort)mappedAddress);
                return true;
            }

            return false;
        }

        public bool cpuWrite(ushort address, byte data)
        {
            uint mappedAddress = 0;

            if (mapper.cpuMapWrite(address, ref mappedAddress))
            {
                prgMemory.write((ushort)mappedAddress, data);
                return true;
            }

            return false;
        }

        public bool ppuRead(ushort address, ref byte data)
        {
            uint mappedAddress = 0;

            if (mapper.ppuMapRead(address, ref mappedAddress))
            {
                data = chrMemory.read((ushort)mappedAddress);
                return true;
            }

            return false;
        }

        public bool ppuWrite(ushort address, byte data)
        {
            uint mappedAddress = 0;

            if (mapper.ppuMapWrite(address, ref mappedAddress))
            {
                chrMemory.write((ushort)mappedAddress, data);
                return true;
            }

            return false;
        }

        protected void setMapper()
        {
            switch (mapperID)
            {
                case 0:
                    mapper = new Mappers.Mapper000(prgBanks, chrBanks);
                    break;
                default:
                    Debug.WriteLine(String.Concat("This mapper (", mapperID.ToString(), ") is not implemented!"));
                    break;
            }
        }
    }
}
