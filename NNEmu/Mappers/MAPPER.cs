namespace NNEmu.Hardware.Mappers
{
    public interface MAPPER
    {
        public bool CpuMapRead(ushort addr,out uint mapped_addr);
        public bool CpuMapWrite(ushort addr, out uint mapped_addr);

        // Trasforma gli indirizzi della ppu nella rom della cartuccia
        public bool PpuMapRead(ushort addr, out uint mapped_addr);
        public bool PpuMapWrite(ushort addr, out uint mapped_addr);
        public void Reset();

    }
}
