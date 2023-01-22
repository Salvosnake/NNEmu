namespace NNEmu.Hardware.Mappers
{
    public class MAPPER_003 : MAPPER
    {

        public byte NPRGBanks;
        public byte NCHRBanks;
        public byte NCHRBankSelect = 0;

        public MAPPER_003(byte NPRGBanks, byte NCHRBanks) 
        {
            this.NPRGBanks = NPRGBanks;
            this.NCHRBanks = NCHRBanks;
        } 

        public bool CpuMapRead(ushort addr, out uint mapped_addr, out byte data)
        {
            mapped_addr = 0;
            data = 0;
            if (addr >= 0x8000 && addr <= 0xFFFF)
            {
                if (NPRGBanks == 1) // 16K ROM 
                    mapped_addr = (uint)(addr & 0x3FFF);
                if (NPRGBanks == 2) // 32K ROM
                    mapped_addr = (uint)(addr & 0x7FFF);
                return true;
            }
            else
                return false;
        }

        public bool CpuMapWrite(ushort addr, out uint mapped_addr, out byte data)
        {
            data = 0;
            mapped_addr = 0;
            if (addr >= 0x8000 && addr <= 0xFFFF)
            {
                NCHRBankSelect = (byte)(data & 0x03);
                mapped_addr = addr;
            }

            return false;
        }

        public bool PpuMapRead(ushort addr,out uint mapped_addr)
        {
            mapped_addr = 0;
            if (addr < 0x2000)
            {
                mapped_addr = (uint)(NCHRBankSelect * 0x2000 + addr);
                return true;
            }
            else
                return false;
        }

        public bool PpuMapWrite(ushort addr, out uint mapped_addr)
        {
            mapped_addr = 0;
            return false;
        }

        public void Reset()
        {
            NCHRBankSelect = 0;
        }

    }
}
