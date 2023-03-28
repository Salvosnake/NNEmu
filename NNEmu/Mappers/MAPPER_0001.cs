using static NNEmu.Hardware.CARTRIDGE;

namespace NNEmu.Mappers
{
    public class MAPPER_001 : MAPPER
    {
        public byte NPRGBanks;
        public byte NCHRBanks;
        public byte NCHRBankSelect4Lo = 0;
        public byte NCHRBankSelect4Hi = 0;
        public byte NCHRBankSelect8 = 0;
        public byte NPRGBankselect16Lo = 0;
        public byte NPRGBankselect16Hi = 0;
        public byte NPRGBankselect32 = 0;
        public byte NLoadRegister = 0;
        public byte NLoadRegisterCount = 0;
        public byte NControlRegister = 0;


        MIRROR MirrorMode = MIRROR.HORIZONTAL;

        public byte[] VRAMStatic;

        public MAPPER_001(byte NPRGBanks, byte NCHRBanks) 
        {
            this.NPRGBanks = NPRGBanks;
            this.NCHRBanks = NCHRBanks;
            VRAMStatic = new byte[32 * 1024];
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

            if (addr >= 0x8000)
            {
                if ((NControlRegister & 0b01000) > 0)
                {
                    // 16K Mode
                    if (addr >= 0x8000 && addr <= 0xBFFF)
                    {
                        mapped_addr = (uint)(NPRGBankselect16Lo * 0x4000 + (addr & 0x3FFF));
                        return true;
                    }

                    if (addr >= 0xC000 && addr <= 0xFFFF)
                    {
                        mapped_addr = (uint)(NPRGBankselect16Hi * 0x4000 + (addr & 0x3FFF));
                        return true;
                    }
                }
                else
                {
                    mapped_addr = (uint)(NPRGBankselect32 * 0x8000 + (addr & 0x7FFF));
                    return true;
                }
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

            if (addr >= 0x8000)
            {
                if ((data & 0x80) > 0)
                {
                    NLoadRegister = 0;
                    NLoadRegisterCount = 0;
                    NControlRegister = (byte)(NControlRegister | 0x0C);
                }
                else
                {
                    NLoadRegister >>= 1;
                    NLoadRegister |= (byte)((data & 0x01) << 4);
                    NLoadRegisterCount++;

                    if (NLoadRegisterCount == 5)
                    {
                        byte nTargetRegister = (byte)((addr >> 13) & 0x03);

                        if (nTargetRegister == 0)
                        {
                            NControlRegister = (byte)(NLoadRegister & 0x1F);

                            switch (NControlRegister & 0x03)
                            {
                                case 0: MirrorMode = MIRROR.ONESCREEN_LO; break;
                                case 1: MirrorMode = MIRROR.ONESCREEN_HI; break;
                                case 2: MirrorMode = MIRROR.VERTICAL; break;
                                case 3: MirrorMode = MIRROR.HORIZONTAL; break;
                            }
                        }
                        else if (nTargetRegister == 1)
                        {
                            if ((NControlRegister & 0b10000) > 0)
                            {
                                NCHRBankSelect4Lo = (byte)(NLoadRegister & 0x1F);
                            }
                            else
                            {
                                NCHRBankSelect8 = (byte)(NLoadRegister & 0x1E);
                            }
                        }
                        else if (nTargetRegister == 2)
                        {
                            if ((NControlRegister & 0b10000) > 0)
                            {
                                NCHRBankSelect4Hi = (byte)(NLoadRegister & 0x1F);
                            }
                        }
                        else if (nTargetRegister == 3)
                        {
                            byte nPRGMode = (byte)((NControlRegister >> 2) & 0x03);

                            if (nPRGMode == 0 || nPRGMode == 1)
                            {
                                NPRGBankselect32 = (byte)((NLoadRegister & 0x0E) >> 1);
                            }
                            else if (nPRGMode == 2)
                            {
                                NPRGBankselect16Lo = 0;
                                NPRGBankselect16Hi = (byte)(NLoadRegister & 0x0F);
                            }
                            else if (nPRGMode == 3)
                            {
                                NPRGBankselect16Lo = (byte)(NLoadRegister & 0x0F);
                                NPRGBankselect16Hi = (byte)(NPRGBanks - 1);
                            }
                        }

                        NLoadRegister = 0;
                        NLoadRegisterCount = 0;
                    }

                }

            }


            return false;
        }

        public bool PpuMapRead(ushort addr,out uint mapped_addr)
        {
            mapped_addr = 0;
            if (addr < 0x2000)
            {
                if (NCHRBanks == 0)
                {
                    mapped_addr = addr;
                    return true;
                }
                else
                {
                    if ((NControlRegister & 0b10000) > 0)
                    {
                        // 4K CHR Bank Mode
                        if (addr >= 0x0000 && addr <= 0x0FFF)
                        {
                            mapped_addr = (uint)(NCHRBankSelect4Lo * 0x1000 + (addr & 0x0FFF));
                            return true;
                        }

                        if (addr >= 0x1000 && addr <= 0x1FFF)
                        {
                            mapped_addr = (uint)(NCHRBankSelect4Hi * 0x1000 + (addr & 0x0FFF));
                            return true;
                        }
                    }
                    else
                    {
                        // 8K CHR Bank Mode
                        mapped_addr = (uint)(NCHRBankSelect8 * 0x2000 + (addr & 0x1FFF));
                        return true;
                    }
                }
            }

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

                return true;
            }
            else
                return false;
        }

        public void Reset()
        {
            NControlRegister = 0x1C;
            NLoadRegister = 0;
            NLoadRegisterCount = 0;

            NCHRBankSelect4Lo = 0;
            NCHRBankSelect4Hi = 0;
            NCHRBankSelect8 = 0;

            NPRGBankselect32 = 0;
            NPRGBankselect16Lo = 0;
            NPRGBankselect16Hi = (byte)(NPRGBanks - 1);
        }
        public MIRROR Mirror()
        {
            return MirrorMode;
        }

        public bool IrqState()
        {
            return false;
        }

        public void IrqClear()
        {
        }

        public void Scanline()
        {
        }
    }
}
