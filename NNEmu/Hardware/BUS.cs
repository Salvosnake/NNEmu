﻿namespace NNEmu.Hardware
{
    public class BUS
    {
        public CPU Cpu;
        public GPU Gpu;
        public CARTRIDGE Cart;
        public byte[] Controller = new byte[2];

        private byte[] CpuRam = new byte[64 * 1024];
        private uint SystemClockCounter = 0;
        private byte[] ControllerStatus = new byte[2];
        private byte dmaPage = 0x00;
        private byte dmAddr = 0x00;
        private byte dmaData = 0x00;
        private byte tmpAdrM;
        private bool dmaFlag = true;
        private bool dmaTransfer = false;

        public BUS(CARTRIDGE cart) 
        {
            //Azzero la ram
            for (uint i=0; i< CpuRam.Length; i++)
                CpuRam[i] = 0x00;

            Cpu = new CPU(this);
            Cart = cart;
            Gpu = new GPU(cart);

        }

        public void CpuWrite(ushort addr, byte data)
        {
            if (Cart.CpuWrite(addr, data))
            {
            }
            else if (addr >= 0x0000 && addr <= 0x1FFF)
            {
                CpuRam[addr & 0x07FF] = data;
            }
            else if (addr >= 0x2000 && addr <= 0x3FFF)
            {
                Gpu.CpuWrite((ushort)(addr & 0x0007), data);
            }
            else if (addr == 0x4014)
            {
                dmaPage = data;
                dmAddr = 0x00;
                dmaTransfer = true;
            }
            else if (addr >= 0x4016 && addr <= 0x4017)
            {
                ControllerStatus[addr & 0x0001] = Controller[addr & 0x0001];
            }
        }
        
        public byte CpuRead(ushort addr, bool bReadOnly)
        {
            byte data;

            if (Cart.CpuRead(addr, out data))
            {
            }
            else if (addr >= 0x0000 && addr <= 0x1FFF)
            {
                data = CpuRam[addr];
            }
            else if (addr >= 0x2000 && addr <= 0x3FFF)
            {
                data = Gpu.CpuRead((ushort)(addr & 0x0007), bReadOnly);
            }
            else if (addr >= 0x4016 && addr <= 0x4017)
            {
                data = (byte)(((ControllerStatus[addr & 0x0001] & 0x80) > 0) ? 1 : 0);
                ControllerStatus[addr & 0x0001] <<= 1;
            }
            return data;
        }

        public void Reset()
        {
            Cpu.Reset();
            Cart.Reset();
            Gpu.Reset();
            SystemClockCounter = 0;
            dmaPage = 0;
            dmAddr = 0;
            dmaData = 0;
            dmaFlag = true;
            dmaTransfer = false;
        }

        public void Clock()
        {
            Gpu.Clock();
            //Il clock della CPU e' 3 volte più lento di quella della GPU.
            if (SystemClockCounter % 3 == 0)
            {
                if (dmaTransfer)
                {
                    if (dmaFlag)
                    {
                        if (SystemClockCounter % 2 == 1)
                        {
                            dmaFlag = false;
                        }
                    }
                    else
                    {
                        if (SystemClockCounter % 2 == 0)
                        {
                            dmaData = CpuRead((ushort)(dmaPage << 8 | dmAddr), false);
                        }
                        else
                        {
                            tmpAdrM = (byte)(dmAddr % 4);

                            if (tmpAdrM == 0)
                                Gpu.MemoryOAM[dmAddr / 4].Y = dmaData;
                            else if(tmpAdrM == 1)
                                Gpu.MemoryOAM[dmAddr / 4].Id = dmaData;
                            else if (tmpAdrM == 2)
                                Gpu.MemoryOAM[dmAddr / 4].Attribute = dmaData;
                            else if (tmpAdrM == 3)
                                Gpu.MemoryOAM[dmAddr / 4].X = dmaData;

                            dmAddr++;
                            if (dmAddr == 0x00)
                            {
                                dmaTransfer = false;
                                dmaFlag = true;
                            }
                        }
                    }
                }
                else
                {
                    Cpu.Clock();
                }
            }

            if (Gpu.Nmi)
            {
                Gpu.Nmi = false;
                Cpu.Nmi();
            }

            SystemClockCounter++;
        }

    }
}
