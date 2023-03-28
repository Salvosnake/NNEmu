using static NNEmu.Hardware.CARTRIDGE;

namespace NNEmu.Mappers
{
    public class MAPPER_004 : MAPPER
    {
        public byte NPRGBanks;
        public byte NCHRBanks;

        public byte NTargetRegister = 0x00;
        public bool BPRGBankMode = false;
        public bool BCHRInversion = false;
        public MIRROR MirrorMode = MIRROR.HORIZONTAL;
        public uint[] PRegister = new uint [8];
        public uint[] PCHRBank = new uint[8];
        public uint[] PPRGBank = new uint[4];
        public bool BIRQActive = false;
        public bool BIRQEnable = false;
        public ushort BIRQCounter = 0;
        public ushort BIRQReload = 0;
        public byte[] VRAMStatic = new byte[32 * 1024];

        public MAPPER_004(byte NPRGBanks, byte NCHRBanks) 
        {
            this.NPRGBanks = NPRGBanks;
            this.NCHRBanks = NCHRBanks;
        } 

        public bool CpuMapRead(ushort addr, out uint mapped_addr, ref byte data)
        {
            mapped_addr = 0;
            if (addr >= 0x6000 && addr <= 0x7FFF)
            {
                mapped_addr = 0xFFFFFFFF;

                data = VRAMStatic[addr & 0x1FFF];

                return true;
            }


            if (addr >= 0x8000 && addr <= 0x9FFF)
            {
                mapped_addr = (uint)(PPRGBank[0] + (addr & 0x1FFF));
                return true;
            }

            if (addr >= 0xA000 && addr <= 0xBFFF)
            {
                mapped_addr = (uint)(PPRGBank[1] + (addr & 0x1FFF));
                return true;
            }

            if (addr >= 0xC000 && addr <= 0xDFFF)
            {
                mapped_addr = (uint)(PPRGBank[2] + (addr & 0x1FFF));
                return true;
            }

            if (addr >= 0xE000 && addr <= 0xFFFF)
            {
                mapped_addr = (uint)(PPRGBank[3] + (addr & 0x1FFF));
                return true;
            }

            return false;
        }

        public bool CpuMapWrite(ushort addr, out uint mapped_addr, ref byte data)
        {
            mapped_addr = 0;
            if (addr >= 0x6000 && addr <= 0x7FFF)
            {
                mapped_addr = 0xFFFFFFFF;

                VRAMStatic[addr & 0x1FFF] = data;

                return true;
            }

            if (addr >= 0x8000 && addr <= 0x9FFF)
            {
                if ((addr & 0x0001) == 0)
                {
                    NTargetRegister = (byte)(data & 0x07);
                    BPRGBankMode = (data & 0x40) > 0;
                    BCHRInversion = (data & 0x80) > 0;
                }
                else
                {
                    PRegister[NTargetRegister] = data;

                    if (BCHRInversion)
                    {
                        PCHRBank[0] = PRegister[2] * 0x0400;
                        PCHRBank[1] = PRegister[3] * 0x0400;
                        PCHRBank[2] = PRegister[4] * 0x0400;
                        PCHRBank[3] = PRegister[5] * 0x0400;
                        PCHRBank[4] = (PRegister[0] & 0xFE) * 0x0400;
                        PCHRBank[5] = PRegister[0] * 0x0400 + 0x0400;
                        PCHRBank[6] = (PRegister[1] & 0xFE) * 0x0400;
                        PCHRBank[7] = PRegister[1] * 0x0400 + 0x0400;
                    }
                    else
                    {
                        PCHRBank[0] = (PRegister[0] & 0xFE) * 0x0400;
                        PCHRBank[1] = PRegister[0] * 0x0400 + 0x0400;
                        PCHRBank[2] = (PRegister[1] & 0xFE) * 0x0400;
                        PCHRBank[3] = PRegister[1] * 0x0400 + 0x0400;
                        PCHRBank[4] = PRegister[2] * 0x0400;
                        PCHRBank[5] = PRegister[3] * 0x0400;
                        PCHRBank[6] = PRegister[4] * 0x0400;
                        PCHRBank[7] = PRegister[5] * 0x0400;
                    }

                    if (BPRGBankMode)
                    {
                        PPRGBank[2] = (PRegister[6] & 0x3F) * 0x2000;
                        PPRGBank[0] = (uint)((NPRGBanks * 2 - 2) * 0x2000);
                    }
                    else
                    {
                        PPRGBank[0] = (PRegister[6] & 0x3F) * 0x2000;
                        PPRGBank[2] = (uint)((NPRGBanks * 2 - 2) * 0x2000);
                    }

                    PPRGBank[1] = (PRegister[7] & 0x3F) * 0x2000;
                    PPRGBank[3] = (uint)((NPRGBanks * 2 - 1) * 0x2000);

                }

                return false;
            }

            if (addr >= 0xA000 && addr <= 0xBFFF)
            {
                if ((addr & 0x0001) == 0)
                {
                    if ((data & 0x01) > 0)
                        MirrorMode = MIRROR.HORIZONTAL;
                    else
                        MirrorMode = MIRROR.VERTICAL;
                }
                else
                {
                }
                return false;
            }

            if (addr >= 0xC000 && addr <= 0xDFFF)
            {
                if ((addr & 0x0001) == 0)
                {
                    BIRQReload = data;
                }
                else
                {
                    BIRQCounter = 0x0000;
                }
                return false;
            }

            if (addr >= 0xE000 && addr <= 0xFFFF)
            {
                if ((addr & 0x0001) == 0)
                {
                    BIRQEnable = false;
                    BIRQActive = false;
                }
                else
                {
                    BIRQEnable = true;
                }
                return false;
            }

            return false;

        }

        public bool PpuMapRead(ushort addr,out uint mapped_addr)
        {
            mapped_addr = 0;
            if (addr >= 0x0000 && addr <= 0x03FF)
            {
                mapped_addr = (uint)(PCHRBank[0] + (addr & 0x03FF));
                return true;
            }

            if (addr >= 0x0400 && addr <= 0x07FF)
            {
                mapped_addr = (uint)(PCHRBank[1] + (addr & 0x03FF));
                return true;
            }

            if (addr >= 0x0800 && addr <= 0x0BFF)
            {
                mapped_addr = (uint)(PCHRBank[2] + (addr & 0x03FF));
                return true;
            }

            if (addr >= 0x0C00 && addr <= 0x0FFF)
            {
                mapped_addr = (uint)(PCHRBank[3] + (addr & 0x03FF));
                return true;
            }

            if (addr >= 0x1000 && addr <= 0x13FF)
            {
                mapped_addr = (uint)(PCHRBank[4] + (addr & 0x03FF));
                return true;
            }

            if (addr >= 0x1400 && addr <= 0x17FF)
            {
                mapped_addr = (uint)(PCHRBank[5] + (addr & 0x03FF));
                return true;
            }

            if (addr >= 0x1800 && addr <= 0x1BFF)
            {
                mapped_addr = (uint)(PCHRBank[6] + (addr & 0x03FF));
                return true;
            }

            if (addr >= 0x1C00 && addr <= 0x1FFF)
            {
                mapped_addr = (uint)(PCHRBank[7] + (addr & 0x03FF));
                return true;
            }

            return false;
        }

        public bool PpuMapWrite(ushort addr, out uint mapped_addr)
        {
            mapped_addr = 0;
            return false;
        }

        public void Reset()
        {
            NTargetRegister = 0;
            BPRGBankMode = false;
            BCHRInversion = false;
            MirrorMode = MIRROR.HORIZONTAL;
            BIRQActive = false;
            BIRQEnable = false;
            BIRQCounter = 0;
            BIRQReload = 0;

            for (int i = 0; i < 4; i++) 
                PPRGBank[i] = 0;

            for (int i = 0; i < 8; i++) 
            { 
                PCHRBank[i] = 0; 
                PRegister[i] = 0; 
            }

            PPRGBank[0] = 0 * 0x2000;
            PPRGBank[1] = 1 * 0x2000;
            PPRGBank[2] = (uint)((NPRGBanks * 2 - 2) * 0x2000);
            PPRGBank[3] = (uint)((NPRGBanks * 2 - 1) * 0x2000);
        }

        public bool IrqState()
        {
            return BIRQActive;
        }

        public void IrqClear()
        {
            BIRQActive = false;
        }

        public void Scanline()
        {
            if (BIRQCounter == 0)
            {
                BIRQCounter = BIRQReload;
            }
            else
                BIRQCounter--;

            if (BIRQCounter == 0 && BIRQEnable)
            {
                BIRQActive = true;
            }

        }

        public MIRROR Mirror()
        {
            return MirrorMode;
        }

    }
}
