namespace NNEmu.Mappers
{
    public class MAPPER_066 : MAPPER
    {

        public byte NPRGBanks;
        public byte NCHRBanks;
        public byte NCHRBankSelect = 0;
        public byte NPRGBankSelect = 0;

        public MAPPER_066(byte NPRGBanks, byte NCHRBanks) 
        {
            this.NPRGBanks = NPRGBanks;
            this.NCHRBanks = NCHRBanks;
        } 

        public bool CpuMapRead(ushort addr, out uint mapped_addr, ref byte data)
        {
            mapped_addr = 0;
            if (addr >= 0x8000 && addr <= 0xFFFF)
            {
                mapped_addr = (uint)(NPRGBankSelect * 0x8000 + (addr & 0x7FFF));
                return true;
            }
            else
                return false;
        }

        public bool CpuMapWrite(ushort addr, out uint mapped_addr, ref byte data)
        {
            mapped_addr = 0;
            if (addr >= 0x8000 && addr <= 0xFFFF)
            {
                NCHRBankSelect = (byte)(data & 0x03);
                NPRGBankSelect = (byte)((data & 0x30) >> 4);
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
            NPRGBankSelect = 0;
        }

        public void Scanline()
        {
        }
    }
}
