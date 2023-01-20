using NNEmu.Hardware.Mappers;
using System;


namespace NNEmu.Hardware
{
    public class CARTRIDGE
    {

        public enum MIRROR
        {
            HORIZONTAL,
            VERTICAL,
        }

        public MIRROR Mirror;

        private bool BImageValid = false;
        private byte NMapperID = 0;
        private byte NPRGBanks = 0;
        private byte NCHRBanks = 0;

        private byte[] VPRGMemory;
        private byte[] VCHRMemory;

        private volatile MAPPER PMapper;


        private byte[] header;


        public CARTRIDGE(string SFileName)
        {
            BImageValid = false;

            FileStream ifs;
            ifs = File.OpenRead(SFileName);

            if(ifs == null)
                throw new FileNotFoundException(SFileName);

            header = new byte[16];

            ifs.Read(header, 0, 16);

            if ((header[6] & 0x04) != 0)
                ifs.Seek(512, SeekOrigin.Current);

            NMapperID = (byte)((byte)((header[7] >> 4) << 4) | (header[6] >> 4));
            Mirror = ((header[6] & 0x01) != 0) ? MIRROR.VERTICAL : MIRROR.HORIZONTAL;


            NPRGBanks = header[4];
            VPRGMemory = new byte[NPRGBanks * 16384];

            NCHRBanks = header[5];
            VCHRMemory = new byte[NCHRBanks * 8192];

            ReadFully(ref ifs, ref VPRGMemory);
            ReadFully(ref ifs, ref VCHRMemory);

            switch (NMapperID)
            {
                case 0:
                    PMapper = new MAPPER_000(NPRGBanks, NCHRBanks);
                    break;
                default:
                    PMapper = new MAPPER_000(NPRGBanks, NCHRBanks);
                    break;
            }

            BImageValid = true;
            ifs.Close();


        }

        private void ReadFully(ref FileStream stream, ref byte[] buffer)
        {
            int offset = 0;
            int readBytes;
            do
            {
                readBytes = stream.Read(buffer, offset, buffer.Length - offset);
                offset += readBytes;
            } while (readBytes > 0 && offset < buffer.Length);

            if (offset < buffer.Length)
            {
                throw new EndOfStreamException();
            }
        }

        public bool ImageValid()
        {
            return BImageValid;
        }


        public bool CpuRead(ushort addr, out byte data)
        {
            uint mapped_addr;
            if (PMapper.CpuMapRead(addr, out mapped_addr))
            {
                data = VPRGMemory[mapped_addr];
                return true;
            }
            else
            {
                data = 0;
                return false;
            }
        }

        public bool CpuWrite(ushort addr, byte data)
        {
            uint mapped_addr;
            if (PMapper.CpuMapWrite(addr, out mapped_addr))
            {
                VPRGMemory[mapped_addr] = data;
                return true;
            }
            else
                return false;
        }

        public bool PpuRead(ushort addr, out byte data)
        {
            uint mapped_addr;
            if (PMapper.PpuMapRead(addr, out mapped_addr))
            {
                data = VCHRMemory[mapped_addr];
                return true;
            }
            else
            {
                data = 0;
                return false;
            }
        }

        public bool PpuWrite(ushort addr, byte data)
        {
            uint mapped_addr;
            if (PMapper.PpuMapRead(addr, out mapped_addr))
            {
                VCHRMemory[mapped_addr] = data;
                return true;
            }
            else
                return false;
        }

        public void Reset()
        {
            if (PMapper != null)
                PMapper.Reset();
        }

    }
}
