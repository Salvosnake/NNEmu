namespace NNEmu.Mappers
{
    public class MAPPER_002 : MAPPER
    {
        public byte NPRGBanks;
        public byte NCHRBanks;
        public byte NPRGBankselectLo = 0;
        public byte NPRGBankselectHi = 0;

        public MAPPER_002(byte NPRGBanks, byte NCHRBanks) 
        {
            this.NPRGBanks = NPRGBanks;
            this.NCHRBanks = NCHRBanks;
        } 

        public bool CpuMapRead(ushort addr, out uint mapped_addr, ref byte data)
        {
            mapped_addr = 0; 
            if (addr >= 0x8000 && addr <= 0xBFFF)
            {
                mapped_addr = (uint)(NPRGBankselectLo * 0x4000 + (addr & 0x3FFF));
                return true;
            }

            if (addr >= 0xC000 && addr <= 0xFFFF)
            {
                mapped_addr = (uint)(NPRGBankselectHi * 0x4000 + (addr & 0x3FFF));
                return true;
            }

            return false;
        }

        public bool CpuMapWrite(ushort addr, out uint mapped_addr, ref byte data)
        {
            mapped_addr = 0;
            if (addr >= 0x8000 && addr <= 0xFFFF)
            {
                NPRGBankselectLo = (byte)(data & 0x0F);
            }

            return false;
        }

        public void IrqClear()
        {
        }

        public bool IrqState()
        {
            return false;
        }

        public bool PpuMapRead(ushort addr,out uint mapped_addr)
        {
            mapped_addr = 0;
            if (addr < 0x2000)
            {
                mapped_addr = addr;
                return true;
            }
            else
                return false;
        }

        public bool PpuMapWrite(ushort addr, out uint mapped_addr)
        {
            mapped_addr = 0;
            if (addr < 0x2000)
            {
                if (NCHRBanks == 0)
                {
                    mapped_addr = addr;
                    return true;
                }
            }
            return false;
        }
        public void Reset()
        {
            NPRGBankselectLo = 0;
            NPRGBankselectHi = (byte)(NPRGBanks - 1);
        }

        public void Scanline()
        {
        }
    }
}
