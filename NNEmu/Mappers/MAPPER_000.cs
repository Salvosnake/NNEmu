namespace NNEmu.Hardware.Mappers
{
    public class MAPPER_000 : MAPPER
    {

        public byte NPRGBanks;
        public byte NCHRBanks;

        public MAPPER_000(byte NPRGBanks, byte NCHRBanks) 
        {
            this.NPRGBanks = NPRGBanks;
            this.NCHRBanks = NCHRBanks;
        } 

        public bool CpuMapRead(ushort addr, out uint mapped_addr, ref byte data)
        {
            mapped_addr = 0;	
            if (addr >= 0x8000 && addr <= 0xFFFF)
            {
                mapped_addr = (uint)(addr & (NPRGBanks > 1 ? 0x7FFF : 0x3FFF));
                return true;
            }

            return false;
        }

        public bool CpuMapWrite(ushort addr, out uint mapped_addr, ref byte data)
        {
            if (addr >= 0x8000 && addr <= 0xFFFF)
            {
                mapped_addr = (uint)(addr & (NPRGBanks > 1 ? 0x7FFF : 0x3FFF));
                return true;
            }
            else
                mapped_addr = 0;

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
            if (addr >= 0x0000 && addr <= 0x1FFF)
            {
                mapped_addr = addr;
                return true;
            }
            else
                mapped_addr = 0;

            return false;
        }

        public bool PpuMapWrite(ushort addr, out uint mapped_addr)
        {
            mapped_addr = 0;
            if (addr >= 0x0000 && addr <= 0x1FFF)
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

        }

        public void Scanline()
        {
        }
    }
}
