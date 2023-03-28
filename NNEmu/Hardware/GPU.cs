using Newtonsoft.Json;

namespace NNEmu.Hardware
{
    //GPU 2C02
    public class GPU
    {
        public class GPUStatus
        {
            //I primi 5 bit non servono
            public byte Unused;
            public byte SpriteOverflow;
            public byte SpriteZeroHit;
            public byte VerticalBlank;
            public byte Reg
            {
                get
                {
                    byte ret = (byte)((VerticalBlank << 7) |
                        (SpriteZeroHit << 6) |
                        (SpriteOverflow << 5) |
                        (Unused));
                    return ret;
                }
                set
                {
                    //Unused = (byte)(value & 0x1F); 
                    Unused = 0;
                    SpriteOverflow = (byte)((value & 0x20) >> 5);
                    SpriteZeroHit = (byte)((value & 0x40) >> 6);
                    VerticalBlank = (byte)((value & 0x80) >> 7);
                }
            }

        }
        public class GPUMask
        {
            //Gli ultimi 3 bit non servono.
            public bool Grayscale;
            public bool RenderBackgroundLeft;
            public bool RenderSpritesLeft;
            public bool RenderBackground;
            public bool RenderSprites;
            public byte Reg
            {
                set
                {
                    Grayscale = (value & 0x1) > 0;
                    RenderBackgroundLeft = (value & 0x2) > 0;
                    RenderSpritesLeft = (value & 0x4) > 0;
                    RenderBackground = (value & 0x8) > 0;
                    RenderSprites = (value & 0x10) > 0;
                }
            }


        }

        public class GPUControl
        {
            public byte NametableX;
            public byte NametableY;
            public byte IncrementMode;
            public byte PatternSprite;
            public byte PatternBackground;
            public byte SpriteSize;
            public bool SlaveMode; // unused
            public bool EnableNmi;
            public byte Reg
            {
                set
                {
                    NametableX = (byte)(value & 0x1);
                    NametableY = (byte)((value & 0x2) >> 1);
                    IncrementMode = (byte)((value & 0x4) >> 2);
                    PatternSprite = (byte)((value & 0x8) >> 3);
                    PatternBackground = (byte)((value & 0x10) >> 4);
                    SpriteSize = (byte)((value & 0x20) >> 5);
                    SlaveMode = (value & 0x40) > 0;
                    EnableNmi = (value & 0x80) > 0;
                }
            }

        }

        public class LOOPYReg
        {
            //L'ultimo bit non serve
            public ushort CoarseX;
            public ushort CoarseY;
            public ushort NametableX;
            public ushort NametableY;
            public ushort FineY;
            public ushort Unused;
            public ushort Reg
            {
                get
                {
                    ushort _CoarseX = CoarseX;
                    ushort _CoarseY = (ushort)(CoarseY << 5);
                    ushort _NametableX = (ushort)(NametableX << 10);
                    ushort _NametableY = (ushort)(NametableY << 11);
                    ushort _FineY = (ushort)(FineY << 12);
                    ushort _Unused = (ushort)(Unused << 15);
                    ushort ret = (ushort)(_CoarseX | _CoarseY | _NametableX | _NametableY | _FineY | _Unused);
                    return ret;
                }
                set
                {
                    CoarseX = (ushort)(value & 0x1F);
                    CoarseY = (ushort)((value & 0x3E0) >> 5);
                    NametableX = (ushort)((value & 0x400) >> 10);
                    NametableY = (ushort)((value & 0x800) >> 11);
                    FineY = (ushort)((value & 0x7000) >> 12);
                    Unused = (ushort)((value & 0x8000) >> 15);
                }
            }
        }

        public  class GpuAttributeEntry
        {
            public byte Y;          // Y position of sprite
            public byte Id;         // ID of tile from pattern memory
            public byte Attribute;  // Flags define how sprite should be rendered
            public byte X;          // X position of sprite

            public GpuAttributeEntry()
            {
            }

            public GpuAttributeEntry(byte val) 
            { 
                Y = Id = Attribute = X = val;
            }
            public GpuAttributeEntry(GpuAttributeEntry obj)
            {
                Y = obj.Y;
                Id = obj.Id;
                Attribute = obj.Attribute;
                X  = obj.X;
            }
        }

        //Memoria addizionale della GPU chiamata OAM
        public GpuAttributeEntry[] MemoryOAM = new GpuAttributeEntry[64];

        //PixelScreenData
        public int GameHeight;
        public int GameWidth;

        public byte[][] TblName;
        public byte[][] TblPattern;
        public byte[] TblPalette = new byte[32];
        public int[] SprScreen;
        public short Scanline = 0;
        public short Cycle = 0;
        public byte tmpAdrdM;
        public byte FineX = 0;
        public byte AddressLatch = 0;
        public byte GpuDataBuffer = 0;
        public byte BgNextTileId = 0;
        public byte BgNextTileAttrib = 0;
        public byte BgNextTileLsb = 0;
        public byte BgNextTileMsb = 0;
        public ushort BgShifterPatternLo = 0;
        public ushort BgShifterPatternHi = 0;
        public ushort BgShifterAttribLo = 0;
        public ushort BgShifterAttribHi = 0;
        public byte MemoryOAMAddr = 0;
        public bool OddFrame = false;
        public GpuAttributeEntry[] SpriteScanline = new GpuAttributeEntry[8];
        public byte SpriteCount;
        public byte[] SpriteShifterPatternLo = new byte[8];
        public byte[] SpriteShifterPatternHi = new byte[8];
        public bool BSpriteZeroHitPossible = false;
        public bool BSpriteZeroBeingRendered = false;
        public GPUStatus GpuStatus;
        public GPUMask GpuMask;
        public GPUControl GpuControl;
        public LOOPYReg VramAddr;
        public LOOPYReg TramAddr;

        [JsonIgnore]
        public CARTRIDGE Cartridge;

        public volatile bool FrameComplete;
        public volatile bool Nmi;

        public readonly int[] PaletteData = {
            0x7C7C7C, 0x0000FC, 0x0000BC, 0x4428BC, 0x940084, 0xA80020, 0xA81000, 0x881400,
            0x503000, 0x007800, 0x006800, 0x005800, 0x004058, 0x000000, 0x000000, 0x000000,
            0xBCBCBC, 0x0078F8, 0x0058F8, 0x6844FC, 0xD800CC, 0xE40058, 0xF83800, 0xE45C10,
            0xAC7C00, 0x00B800, 0x00A800, 0x00A844, 0x008888, 0x000000, 0x000000, 0x000000,
            0xF8F8F8, 0x3CBCFC, 0x6888FC, 0x9878F8, 0xF878F8, 0xF85898, 0xF87858, 0xFCA044,
            0xF8B800, 0xB8F818, 0x58D854, 0x58F898, 0x00E8D8, 0x787878, 0x000000, 0x000000,
            0xFCFCFC, 0xA4E4FC, 0xB8B8F8, 0xD8B8F8, 0xF8B8F8, 0xF8A4C0, 0xF0D0B0, 0xFCE0A8,
            0xF8D878, 0xD8F878, 0xB8F8B8, 0xB8F8D8, 0x00FCFC, 0xF8D8F8, 0x000000, 0x000000
        };

        public GPU(CARTRIDGE cart, int gameHeight, int gameWidth)
        {
            Cartridge = cart;
            Nmi = false;
            SprScreen = new int[gameHeight * gameWidth];

            for (int i = 0; i < MemoryOAM.Length; i++)
                MemoryOAM[i] = new GpuAttributeEntry();

            for (int i = 0; i < SpriteScanline.Length; i++)
                SpriteScanline[i] = new GpuAttributeEntry();

            TblName = new byte[2][];
            TblPattern = new byte[2][];

            for (int i = 0; i < 2; i++)
            {
                TblName[i] = new byte[1024];
                TblPattern[i] = new byte[4096];
            }

            GpuStatus = new GPUStatus();
            GpuMask = new GPUMask();
            GpuControl = new GPUControl();
            VramAddr = new LOOPYReg();
            TramAddr = new LOOPYReg();
            GameHeight = gameHeight;
            GameWidth = gameWidth;

        }

        public int[] GetScreen()
        {
            return SprScreen;
        }
        
        public int GetColourFromPaletteRam(byte palette, byte pixel)
        {
            return PaletteData[(byte)(PpuRead((ushort)(0x3F00 + (palette << 2) + pixel)) & 0x3F)];
        }

        public byte CpuRead(ushort addr, bool? rdonly = false)
        {
            byte data = 0;

            if (rdonly.HasValue && rdonly.Value)
            {

                switch (addr)
                {
                    case 0x0000: // Control
                        break;
                    case 0x0001: // Mask
                        break;
                    case 0x0002: // Status
                        break;
                    case 0x0003: // MemoryOAM Address
                        break;
                    case 0x0004: // MemoryOAM Data
                        break;
                    case 0x0005: // Scroll
                        break;
                    case 0x0006: // PPU Address
                        break;
                    case 0x0007: // PPU Data
                        break;
                }
            }
            else
            {

                switch (addr)
                {
                    // Control - Not readable
                    case 0x0000: break;

                    // Mask - Not readable
                    case 0x0001: break;

                    // Status
                    case 0x0002:

                        data = (byte)((GpuStatus.Reg & 0xE0) | (GpuDataBuffer & 0x1F));

                        // Clear the vertical blanking flag
                        GpuStatus.VerticalBlank = 0;

                        // Reset Loopy's Address latch flag
                        AddressLatch = 0;

                        break;

                    // MemoryOAM Address
                    case 0x0003: break;

                    // MemoryOAM Data
                    case 0x0004:

                        tmpAdrdM = (byte)(MemoryOAMAddr % 4);

                        if (tmpAdrdM == 0)
                            data = MemoryOAM[MemoryOAMAddr / 4].Y;
                        else if (tmpAdrdM == 1)
                            data = MemoryOAM[MemoryOAMAddr / 4].Id ;
                        else if (tmpAdrdM == 2)
                            data = MemoryOAM[MemoryOAMAddr / 4].Attribute;
                        else if (tmpAdrdM == 3)
                            data = MemoryOAM[MemoryOAMAddr / 4].X;
                        break;

                    // Scroll - Not Readable
                    case 0x0005: break;

                    // PPU Address - Not Readable
                    case 0x0006: break;

                    // PPU Data
                    case 0x0007:

                        data = GpuDataBuffer;
                        GpuDataBuffer = PpuRead(VramAddr.Reg);

                        if (VramAddr.Reg >= 0x3F00)
                            data = GpuDataBuffer;

                        VramAddr.Reg += GpuControl.IncrementMode > 0 ? (ushort)32 : (ushort)1;
                        break;
                }
            }

            return data;
        }

        public void CpuWrite(ushort addr, byte data)
        {
            GpuStatus.Reg = (byte)(data & 0xFF);
            switch (addr)
            {
                case 0x0000: // Control
                    GpuControl.Reg = data;
                    TramAddr.NametableX = GpuControl.NametableX;
                    TramAddr.NametableY = GpuControl.NametableY;
                    break;
                case 0x0001: // Mask
                    GpuMask.Reg = data;
                    break;
                case 0x0002: // Status
                    break;
                case 0x0003:
                    // MemoryOAM Address
                    MemoryOAMAddr = data;
                    break;
                case 0x0004: // MemoryOAM Data
                    tmpAdrdM = (byte)(MemoryOAMAddr % 4);
                    if (tmpAdrdM == 0)
                        MemoryOAM[MemoryOAMAddr / 4].Y = data;
                    else if (tmpAdrdM == 1)
                        MemoryOAM[MemoryOAMAddr / 4].Id = data;
                    else if (tmpAdrdM == 2)
                        MemoryOAM[MemoryOAMAddr / 4].Attribute = data;
                    else if (tmpAdrdM == 3)
                        MemoryOAM[MemoryOAMAddr / 4].X = data;
                    break;
                case 0x0005: // Scroll
                    if (AddressLatch == 0)
                    {
                        FineX = (byte)(data & 0x07);
                        TramAddr.CoarseX = (ushort)(data >> 3);
                        AddressLatch = 1;
                    }
                    else
                    {
                        TramAddr.FineY = (ushort)(data & 0x07);
                        TramAddr.CoarseY = (ushort)(data >> 3);
                        AddressLatch = 0;
                    }
                    break;
                case 0x0006: // PPU Address
                    if (AddressLatch == 0)
                    {
                        TramAddr.Reg = (ushort)(((data & 0x3F) << 8) | (TramAddr.Reg & 0x00FF));
                        AddressLatch = 1;
                    }
                    else
                    {
                        TramAddr.Reg = (ushort)((TramAddr.Reg & 0xFF00) | data);
                        VramAddr.Reg = TramAddr.Reg;
                        AddressLatch = 0;
                    }
                    break;
                case 0x0007: // PPU Data
                    PpuWrite(VramAddr.Reg, data);

                    VramAddr.Reg += GpuControl.IncrementMode > 0 ? (ushort)32 : (ushort)1;
                    break;
            }


        }

        public byte PpuRead(ushort addr, bool? rdonly = false)
        {
            byte data = 0;
            addr &= 0x3FFF;

            if (Cartridge.PpuRead(addr, out data))
            {
            }
            else if (addr >= 0x0000 && addr <= 0x1FFF)
            {
                data = TblPattern[(addr & 0x1000) >> 12][addr & 0x0FFF];
            }
            else if (addr >= 0x2000 && addr <= 0x3EFF)
            {
                addr &= 0x0FFF;

                if (Cartridge.Mirror == CARTRIDGE.MIRROR.VERTICAL)
                {
                    // Vertical
                    if (addr >= 0x0000 && addr <= 0x03FF)
                        data = TblName[0][addr & 0x03FF];
                    if (addr >= 0x0400 && addr <= 0x07FF)
                        data = TblName[1][addr & 0x03FF];
                    if (addr >= 0x0800 && addr <= 0x0BFF)
                        data = TblName[0][addr & 0x03FF];
                    if (addr >= 0x0C00 && addr <= 0x0FFF)
                        data = TblName[1][addr & 0x03FF];
                }
                else if (Cartridge.Mirror == CARTRIDGE.MIRROR.HORIZONTAL)
                {
                    // Horizontal
                    if (addr >= 0x0000 && addr <= 0x03FF)
                        data = TblName[0][addr & 0x03FF];
                    if (addr >= 0x0400 && addr <= 0x07FF)
                        data = TblName[0][addr & 0x03FF];
                    if (addr >= 0x0800 && addr <= 0x0BFF)
                        data = TblName[1][addr & 0x03FF];
                    if (addr >= 0x0C00 && addr <= 0x0FFF)
                        data = TblName[1][addr & 0x03FF];
                }
            }
            else if (addr >= 0x3F00 && addr <= 0x3FFF)
            {
                addr &= 0x001F;
                if (addr == 0x0010) addr = 0x0000;
                if (addr == 0x0014) addr = 0x0004;
                if (addr == 0x0018) addr = 0x0008;
                if (addr == 0x001C) addr = 0x000C;
                data = (byte)(TblPalette[addr] & (GpuMask.Grayscale ? 0x30 : 0x3F));
            }

            return data;
        }

        public void PpuWrite(ushort addr, byte data)
        {

            addr &= 0x3FFF;

            if (Cartridge.PpuWrite(addr, data))
            {
            }
            else if (addr >= 0x0000 && addr <= 0x1FFF)
            {
                TblPattern[(addr & 0x1000) >> 12][addr & 0x0FFF] = data;
            }
            else if (addr >= 0x2000 && addr <= 0x3EFF)
            {
                addr &= 0x0FFF;
                if (Cartridge.Mirror == CARTRIDGE.MIRROR.VERTICAL)
                {
                    if (addr >= 0x0000 && addr <= 0x03FF)
                        TblName[0][addr & 0x03FF] = data;
                    if (addr >= 0x0400 && addr <= 0x07FF)
                        TblName[1][addr & 0x03FF] = data;
                    if (addr >= 0x0800 && addr <= 0x0BFF)
                        TblName[0][addr & 0x03FF] = data;
                    if (addr >= 0x0C00 && addr <= 0x0FFF)
                        TblName[1][addr & 0x03FF] = data;
                }
                else if (Cartridge.Mirror == CARTRIDGE.MIRROR.HORIZONTAL)
                {
                    if (addr >= 0x0000 && addr <= 0x03FF)
                        TblName[0][addr & 0x03FF] = data;
                    if (addr >= 0x0400 && addr <= 0x07FF)
                        TblName[0][addr & 0x03FF] = data;
                    if (addr >= 0x0800 && addr <= 0x0BFF)
                        TblName[1][addr & 0x03FF] = data;
                    if (addr >= 0x0C00 && addr <= 0x0FFF)
                        TblName[1][addr & 0x03FF] = data;
                }
            }
            else if (addr >= 0x3F00 && addr <= 0x3FFF)
            {
                addr &= 0x001F;
                if (addr == 0x0010) addr = 0x0000;
                if (addr == 0x0014) addr = 0x0004;
                if (addr == 0x0018) addr = 0x0008;
                if (addr == 0x001C) addr = 0x000C;
                TblPalette[addr] = data;
            }

        }

        public void Reset()
        {
            FineX = 0;
            AddressLatch = 0;
            GpuDataBuffer = 0;
            Scanline = 0;
            Cycle = 0;
            BgNextTileId = 0;
            BgNextTileAttrib = 0;
            BgNextTileLsb = 0;
            BgNextTileMsb = 0;
            BgShifterPatternLo = 0;
            BgShifterPatternHi = 0;
            BgShifterAttribLo = 0;
            BgShifterAttribHi = 0;
            GpuStatus.Reg = 0;
            GpuMask.Reg = 0;
            GpuControl.Reg = 0;
            VramAddr.Reg = 0;
            TramAddr.Reg = 0;
            OddFrame = false;
        }

        public void Clock()
        {
            if (Scanline >= -1 && Scanline < 240)
            {
                if (Scanline == 0 && Cycle == 0 && OddFrame && (GpuMask.RenderBackground || GpuMask.RenderSprites))
                {
                    Cycle = 1;
                }

                if (Scanline == -1 && Cycle == 1)
                {
                    GpuStatus.VerticalBlank = 0;

                    GpuStatus.SpriteOverflow = 0;

                    GpuStatus.SpriteZeroHit = 0;

                    for (int i = 0; i < 8; i++)
                    {
                        SpriteShifterPatternLo[i] = 0;
                        SpriteShifterPatternHi[i] = 0;
                    }

                }


                if ((Cycle >= 2 && Cycle < 258) || (Cycle >= 321 && Cycle < 338))
                {
                    UpdateShifters();

                    switch ((Cycle - 1) % 8)
                    {
                        case 0:

                            LoadBackgroundShifters();

                            BgNextTileId = PpuRead((ushort)(0x2000 | (VramAddr.Reg & 0x0FFF)));
                            
                            break;
                        case 2:
                            BgNextTileAttrib = PpuRead((ushort)(0x23C0 | (VramAddr.NametableY << 11)
                                                                 | (VramAddr.NametableX << 10)
                                                                 | ((VramAddr.CoarseY >> 2) << 3)
                                                                 | (VramAddr.CoarseX >> 2)));
                            
                            if ((VramAddr.CoarseY & 0x02) != 0)
                                BgNextTileAttrib >>= 4;
                            if ((VramAddr.CoarseX & 0x02) != 0)
                                BgNextTileAttrib >>= 2;

                            BgNextTileAttrib &= 0x03;
                            break;

                        case 4:
                            BgNextTileLsb = PpuRead((ushort)((GpuControl.PatternBackground << 12)
                                                       + (BgNextTileId << 4)
                                                       + VramAddr.FineY + 0));
                            break;
                        case 6:
                            BgNextTileMsb = PpuRead((ushort)((GpuControl.PatternBackground << 12)
                                                       + (BgNextTileId << 4)
                                                       + VramAddr.FineY + 8));
                            break;
                        case 7:
                            IncrementScrollX();
                            break;
                    }
                }

                if (Cycle == 256)
                {
                    IncrementScrollY();
                }

                if (Cycle == 257)
                {
                    LoadBackgroundShifters();
                    TransferAddressX();
                }

                if (Cycle == 338 || Cycle == 340)
                {
                    BgNextTileId = PpuRead((ushort)(0x2000 | (VramAddr.Reg & 0x0FFF)));
                }

                if (Scanline == -1 && Cycle >= 280 && Cycle < 305)
                {
                    TransferAddressY();
                }

                if (Cycle == 257 && Scanline >= 0)
                {
                    Array.Clear(SpriteScanline,0,SpriteScanline.Length);

                    for (int i = 0; i < SpriteScanline.Length; i++)
                        SpriteScanline[i] = new GpuAttributeEntry(0xFF);

                    SpriteCount = 0;

                    for (byte i = 0; i < 8; i++)
                    {
                        SpriteShifterPatternLo[i] = 0;
                        SpriteShifterPatternHi[i] = 0;
                    }

                    byte nMemoryOAMEntry = 0;

                    BSpriteZeroHitPossible = false;

                    while (nMemoryOAMEntry < 64 && SpriteCount < 9)
                    {

                        short diff = (short)(Scanline - MemoryOAM[nMemoryOAMEntry].Y);

                        if (diff >= 0 && diff < (GpuControl.SpriteSize > 0 ? 16 : 8) && SpriteCount < 8)
                        {

                            if (SpriteCount < 8)
                            {
                                if (nMemoryOAMEntry == 0)
                                {
                                    BSpriteZeroHitPossible = true;
                                }

                                SpriteScanline[SpriteCount] = new GpuAttributeEntry(MemoryOAM[nMemoryOAMEntry]);
                                SpriteCount++;
                            }
                        }

                        nMemoryOAMEntry++;
                    }

                    GpuStatus.SpriteOverflow = (byte)((SpriteCount >= 8) ? 1 : 0);
                }

                if (Cycle == 340)
                {

                    for (byte i = 0; i < SpriteCount; i++)
                    {
                        byte sprite_pattern_bits_lo, sprite_pattern_bits_hi;
                        ushort sprite_pattern_addr_lo, sprite_pattern_addr_hi;

                        if (GpuControl.SpriteSize <= 0)
                        {
                            if (!((SpriteScanline[i].Attribute & 0x80) > 0))
                            { 
                                sprite_pattern_addr_lo =
                                  (ushort)((GpuControl.PatternSprite << 12)
                                | (SpriteScanline[i].Id << 4)
                                | (Scanline - SpriteScanline[i].Y));

                            }
                            else
                            {
                                sprite_pattern_addr_lo =
                                  (ushort)((GpuControl.PatternSprite << 12)
                                | (SpriteScanline[i].Id << 4)
                                | (7 - (Scanline - SpriteScanline[i].Y)));
                            }

                        }
                        else
                        {
                            if (!((SpriteScanline[i].Attribute & 0x80) > 0))
                            {
                                if (Scanline - SpriteScanline[i].Y < 8)
                                {
                                    sprite_pattern_addr_lo =
                                      (ushort)(((SpriteScanline[i].Id & 0x01) << 12)
                                    | ((SpriteScanline[i].Id & 0xFE) << 4)
                                    | ((Scanline - SpriteScanline[i].Y) & 0x07));
                                }
                                else
                                {
                                    sprite_pattern_addr_lo =
                                      (ushort)(((SpriteScanline[i].Id & 0x01) << 12)
                                    | (((SpriteScanline[i].Id & 0xFE) + 1) << 4)
                                    | ((Scanline - SpriteScanline[i].Y) & 0x07));
                                }
                            }
                            else
                            {
                                if (Scanline - SpriteScanline[i].Y < 8)
                                {
                                    sprite_pattern_addr_lo =
                                      (ushort)(((SpriteScanline[i].Id & 0x01) << 12)
                                    | (((SpriteScanline[i].Id & 0xFE) + 1) << 4)
                                    | (7 - (Scanline - SpriteScanline[i].Y) & 0x07));
                                }
                                else
                                {
                                    // Reading Bottom Half Tile
                                    sprite_pattern_addr_lo =
                                      (ushort)(((SpriteScanline[i].Id & 0x01) << 12)
                                    | ((SpriteScanline[i].Id & 0xFE) << 4)
                                    | (7 - (Scanline - SpriteScanline[i].Y) & 0x07));
                                }
                            }
                        }

                        sprite_pattern_addr_hi = (ushort)(sprite_pattern_addr_lo + 8);

                        sprite_pattern_bits_lo = PpuRead(sprite_pattern_addr_lo);
                        sprite_pattern_bits_hi = PpuRead(sprite_pattern_addr_hi);
                        
                        if ((SpriteScanline[i].Attribute & 0x40) > 0)
                        {
                            sprite_pattern_bits_lo = FlipByte(sprite_pattern_bits_lo);
                            sprite_pattern_bits_hi = FlipByte(sprite_pattern_bits_hi);
                        }

                        SpriteShifterPatternLo[i] = sprite_pattern_bits_lo;
                        SpriteShifterPatternHi[i] = sprite_pattern_bits_hi;
                    }
                }

            }

            if (Scanline == 240)
            {
            }

            if (Scanline >= 241 && Scanline < 261)
            {
                if (Scanline == 241 && Cycle == 1)
                {
                    GpuStatus.VerticalBlank = 1;

                    if (GpuControl.EnableNmi)
                        Nmi = true;
                }
            }

            byte bg_pixel = 0;
            byte bg_palette = 0;

            if (GpuMask.RenderBackground)
            {
                ushort bit_mux = (ushort)(0x8000 >> FineX);

                byte p0_pixel = (byte)(((BgShifterPatternLo & bit_mux) > 0) ? 1 : 0);
                byte p1_pixel = (byte)(((BgShifterPatternHi & bit_mux) > 0) ? 1 : 0);

                bg_pixel = (byte)((p1_pixel << 1) | p0_pixel);

                byte bg_pal0 = (byte)(((BgShifterAttribLo & bit_mux) > 0) ? 1 : 0);
                byte bg_pal1 = (byte)(((BgShifterAttribHi & bit_mux) > 0) ? 1 : 0);
                bg_palette = (byte)((bg_pal1 << 1) | bg_pal0);
            }

            byte fg_pixel = 0;
            byte fg_palette = 0;
            byte fg_priority = 0;

            if (GpuMask.RenderSprites)
            {
                if (GpuMask.RenderSpritesLeft || (Cycle >= 9))
                {
                    BSpriteZeroBeingRendered = false;

                    for (byte i = 0; i < SpriteCount; i++)
                    {
                        if (SpriteScanline[i].X == 0)
                        {

                            byte fg_pixel_lo = (byte)((SpriteShifterPatternLo[i] & 0x80) > 0 ? 1 : 0);
                            byte fg_pixel_hi = (byte)((SpriteShifterPatternHi[i] & 0x80) > 0 ? 1 : 0);
                            fg_pixel = (byte)((fg_pixel_hi << 1) | fg_pixel_lo);

                            fg_palette = (byte)((SpriteScanline[i].Attribute & 0x03) + 0x04);
                            fg_priority = (byte)((SpriteScanline[i].Attribute & 0x20) == 0 ? 1 : 0);


                            if (fg_pixel != 0)
                            {
                                if (i == 0)
                                {
                                    BSpriteZeroBeingRendered = true;
                                }

                                break;
                            }
                        }
                    }
                }
            }

            byte pixel = 0;
            byte palette = 0;

            if (bg_pixel == 0 && fg_pixel == 0)
            {
                pixel = 0;
                palette = 0;
            }
            else if (bg_pixel == 0 && fg_pixel > 0)
            {
                pixel = fg_pixel;
                palette = fg_palette;
            }
            else if (bg_pixel > 0 && fg_pixel == 0)
            {
                pixel = bg_pixel;
                palette = bg_palette;
            }
            else if (bg_pixel > 0 && fg_pixel > 0)
            {
                if (fg_priority != 0)
                {
                    pixel = fg_pixel;
                    palette = fg_palette;
                }
                else
                {
                    pixel = bg_pixel;
                    palette = bg_palette;
                }

                if (BSpriteZeroHitPossible && BSpriteZeroBeingRendered)
                {
                    if (GpuMask.RenderBackground && GpuMask.RenderSprites)
                    {
                        //Controlla
                        if ((GpuMask.RenderBackgroundLeft || GpuMask.RenderSpritesLeft) ? false : true)
                        {
                            if (Cycle >= 9 && Cycle < 258)
                            {
                                GpuStatus.SpriteZeroHit = 1;
                            }
                        }
                        else
                        {
                            if (Cycle >= 1 && Cycle < 258)
                            {
                                GpuStatus.SpriteZeroHit = 1;
                            }
                        }
                    }
                }
            }

            //Set pixel of image
            SetPixel(Cycle - 1, Scanline, GetColourFromPaletteRam(palette, pixel));

            Cycle++;

            if (GpuMask.RenderBackground || GpuMask.RenderSprites)
            {
                if (Cycle == 260 && Scanline < 240)
                {
                    Cartridge.GetMapper().Scanline();
                }
            }
            if (Cycle >= 341)
            {
                Cycle = 0;
                Scanline++;
                if (Scanline >= 261)
                {
                    Scanline = -1;
                    FrameComplete = true;
                    OddFrame = !OddFrame;
                }
            }
        }

        #region public method

        public void IncrementScrollX()
        {

            if (GpuMask.RenderBackground || GpuMask.RenderSprites)
            {

                if (VramAddr.CoarseX == 31)
                {
                    VramAddr.CoarseX = 0;
                    VramAddr.NametableX = (ushort)(VramAddr.NametableX == 0 ? 1 : 0);
                }
                else
                {
                    VramAddr.CoarseX++;
                }
            }

        }

        public void IncrementScrollY()
        {
            if (GpuMask.RenderBackground || GpuMask.RenderSprites)
            {
                if (VramAddr.FineY < 7)
                {
                    VramAddr.FineY++;
                }
                else
                {
                    VramAddr.FineY = 0;
                    if (VramAddr.CoarseY == 29)
                    {
                        VramAddr.CoarseY = 0;
                        VramAddr.NametableY = (ushort)(VramAddr.NametableY == 0 ? 1 : 0);
                    }
                    else if (VramAddr.CoarseY == 31)
                    {
                        VramAddr.CoarseY = 0;
                    }
                    else
                    {
                        VramAddr.CoarseY++;
                    }
                }
            }
        }

        public void TransferAddressX()
        {
            if (GpuMask.RenderBackground || GpuMask.RenderSprites)
            {
                VramAddr.NametableX = TramAddr.NametableX;
                VramAddr.CoarseX = TramAddr.CoarseX;
            }
        }

        public void TransferAddressY()
        {
            if (GpuMask.RenderBackground || GpuMask.RenderSprites)
            {
                VramAddr.FineY = TramAddr.FineY;
                VramAddr.NametableY = TramAddr.NametableY;
                VramAddr.CoarseY = TramAddr.CoarseY;
            }
        }

        public void LoadBackgroundShifters()
        {
            BgShifterPatternLo = (ushort)((BgShifterPatternLo & 0xFF00) | BgNextTileLsb);
            BgShifterPatternHi = (ushort)((BgShifterPatternHi & 0xFF00) | BgNextTileMsb);

            BgShifterAttribLo = (ushort)((BgShifterAttribLo & 0xFF00) | (((BgNextTileAttrib & 0b01) != 0) ? 0xFF : 0x00));
            BgShifterAttribHi = (ushort)((BgShifterAttribHi & 0xFF00) | (((BgNextTileAttrib & 0b10) != 0) ? 0xFF : 0x00));
        }

        public void UpdateShifters()
        {
            if (GpuMask.RenderBackground)
            {
                BgShifterPatternLo <<= 1;
                BgShifterPatternHi <<= 1;

                BgShifterAttribLo <<= 1;
                BgShifterAttribHi <<= 1;

            }

            if (GpuMask.RenderSprites && Cycle >= 1 && Cycle < 258)
            {
                for (int i = 0; i < SpriteCount; i++)
                {
                    if (SpriteScanline[i].X > 0)
                    {
                        SpriteScanline[i].X--;
                    }
                    else
                    {
                        SpriteShifterPatternLo[i] <<= 1;
                        SpriteShifterPatternHi[i] <<= 1;
                    }
                }
            }

        }

        public byte FlipByte(byte b)
        {
            b = (byte)((b & 0xF0) >> 4 | (b & 0x0F) << 4);
            b = (byte)((b & 0xCC) >> 2 | (b & 0x33) << 2);
            b = (byte)((b & 0xAA) >> 1 | (b & 0x55) << 1);
            return b;
        }

        public void SetPixel(int x, int y, int p)
        {
            if (x >= 0 && x < GameWidth && y >= 0 && y < GameHeight)
                SprScreen[y * GameWidth + x] = p;
        }

        #endregion
    }
}
