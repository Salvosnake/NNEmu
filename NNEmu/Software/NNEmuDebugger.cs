using PixelEngine;
using static NNEmu.Hardware.INSTRUCTION;
using NNEmu.Hardware;

namespace NNEmu.Software
{
    public class NNEmuDebugger : Game
    {

        private readonly BUS nes;
        private Dictionary<ushort, string> mapAsm;
        private LinkedList<ushort> mapsAddr;
        private byte NSelectedPalette = 0x00;
        private float fResidualTime = 0.0f;
        public NNEmuDebugger(BUS nes)
        {
            this.nes = nes;

            // Extract dissassembly
            mapAsm = nes.Cpu.Disassemble(0x0000, 0xFFFF, out mapsAddr);
        }

        private string HexConvert(uint n, byte d)
        {
            char[] s = new char[d];
            for (int i = d - 1; i >= 0; i--, n >>= 4)
                s[i] = "0123456789ABCDEF"[(int)(n & 0xF)];


            return new string(s);
        }


        public void DrawRam(int x, int y, ushort nAddr, int nRows, int nColumns)
        {
            int nRamX = x, nRamY = y;
            for (int row = 0; row < nRows; row++)
            {
                string sOffset = "$" + HexConvert(nAddr, 4) + ":";
                for (int col = 0; col < nColumns; col++)
                {
                    sOffset += " " + HexConvert(nes.CpuRead(nAddr, true), 2);
                    nAddr += 1;
                }
                DrawString(nRamX, nRamY, sOffset, Pixel.FromRgb(0xFBA07A));
                nRamY += 10;
            }
        }

        public void DrawCpu(int x, int y)
        {
            DrawString(x, y, "Status:", Pixel.FromRgb(0xFFFFFF));
            DrawString(x + 64, y, "N", (nes.Cpu.Status & (byte)FLAGS6502.N) != 0 ? Pixel.FromRgb(0xFFFFFF) : Pixel.FromRgb(0xFFA07A));
            DrawString(x + 80, y, "V", (nes.Cpu.Status & (byte)FLAGS6502.V) != 0 ? Pixel.FromRgb(0xFFFFFF) : Pixel.FromRgb(0xFFA07A));
            DrawString(x + 96, y, "U", (nes.Cpu.Status & (byte)FLAGS6502.U) != 0 ? Pixel.FromRgb(0xFFFFFF) : Pixel.FromRgb(0xFFA07A));
            DrawString(x + 112, y, "B", (nes.Cpu.Status & (byte)FLAGS6502.B) != 0 ? Pixel.FromRgb(0xFFFFFF) : Pixel.FromRgb(0xFFA07A));
            DrawString(x + 128, y, "D", (nes.Cpu.Status & (byte)FLAGS6502.D) != 0 ? Pixel.FromRgb(0xFFFFFF) : Pixel.FromRgb(0xFFA07A));
            DrawString(x + 144, y, "I", (nes.Cpu.Status & (byte)FLAGS6502.I) != 0 ? Pixel.FromRgb(0xFFFFFF) : Pixel.FromRgb(0xFFA07A));
            DrawString(x + 160, y, "Z", (nes.Cpu.Status & (byte)FLAGS6502.Z) != 0 ? Pixel.FromRgb(0xFFFFFF) : Pixel.FromRgb(0xFFA07A));
            DrawString(x + 178, y, "C", (nes.Cpu.Status & (byte)FLAGS6502.C) != 0 ? Pixel.FromRgb(0xFFFFFF) : Pixel.FromRgb(0xFFA07A));
            DrawString(x, y + 10, "PC: $" + HexConvert(nes.Cpu.Pc, 4), Pixel.FromRgb(0xFFA07A));
            DrawString(x, y + 20, "A: $" + HexConvert(nes.Cpu.A, 2) + "  [" + nes.Cpu.A + "]", Pixel.FromRgb(0xFFA07A));
            DrawString(x, y + 30, "X: $" + HexConvert(nes.Cpu.X, 2) + "  [" + nes.Cpu.X + "]", Pixel.FromRgb(0xFFA07A));
            DrawString(x, y + 40, "Y: $" + HexConvert(nes.Cpu.Y, 2) + "  [" + nes.Cpu.Y + "]", Pixel.FromRgb(0xFFA07A));
            DrawString(x, y + 50, "Stack P: $" + HexConvert(nes.Cpu.Stkp, 4), Pixel.FromRgb(0xFFA07A));
        }



        public void DrawCode(int x, int y, int nLines)
        {
            string it_a = string.Empty;
            LinkedListNode<ushort>? valueOpr = mapsAddr.Find(nes.Cpu.Pc);
            if (valueOpr != null)
                it_a = mapAsm[valueOpr.Value];

            int nLineY = (nLines >> 1) * 10 + y;
            if (it_a != null)
            {
                DrawString(x, nLineY, it_a, Pixel.FromRgb(0xFFFFFF));
                while (nLineY < nLines * 10 + y)
                {
                    nLineY += 10;
                    valueOpr = valueOpr?.Next;
                    if (valueOpr != null)
                        it_a = mapAsm[valueOpr.Value];

                    if (it_a != null)
                        DrawString(x, nLineY, it_a, Pixel.FromRgb(0xFFA07A));
                }
            }

            valueOpr = mapsAddr.Find(nes.Cpu.Pc);
            if (valueOpr != null)
                it_a = mapAsm[valueOpr.Value];

            nLineY = (nLines >> 1) * 10 + y;
            if (it_a != null)
            {
                while (nLineY > y)
                {
                    nLineY -= 10;

                    valueOpr = valueOpr?.Previous;
                    if (valueOpr != null)
                        it_a = mapAsm[valueOpr.Value];

                    if (it_a != null)
                        DrawString(x, nLineY, it_a, Pixel.FromRgb(0xFFA07A));
                }
            }

        }

        public override void OnCreate()
        {
            // Reset
            nes.Reset();
        }

        public override void OnUpdate(float fElapsedTime)
        {
            Clear(Pixel.FromRgb(0x000000));


            nes.Controller[0] = 0x00;
            nes.Controller[0] |= (byte)(GetKey(Key.X).Down ? 0x80 : 0x00); //A
            nes.Controller[0] |= (byte)(GetKey(Key.Z).Down ? 0x40 : 0x00); //B
            nes.Controller[0] |= (byte)(GetKey(Key.A).Pressed ? 0x20 : 0x00); //SELECT
            nes.Controller[0] |= (byte)(GetKey(Key.S).Pressed ? 0x10 : 0x00); //START
            nes.Controller[0] |= (byte)(GetKey(Key.Up).Down ? 0x08 : 0x00);
            nes.Controller[0] |= (byte)(GetKey(Key.Down).Down ? 0x04 : 0x00);
            nes.Controller[0] |= (byte)(GetKey(Key.Left).Down ? 0x02 : 0x00);
            nes.Controller[0] |= (byte)(GetKey(Key.Right).Down ? 0x01 : 0x00);

            if (Program.EmulationRun)
            {
                if (fResidualTime > 0.0f)
                    fResidualTime -= fElapsedTime;
                else
                {
                    fResidualTime += 1.0f / 60.0f - fElapsedTime;
                    do { nes.Clock(); } while (!nes.Gpu.FrameComplete);
                    nes.Gpu.FrameComplete = false;
                }
            }
            else
            {
                // Emulate code step-by-step
                if (GetKey(Key.C).Pressed)
                {
                    // Clock enough times to execute a whole CPU instruction
                    do { nes.Clock(); } while (!nes.Cpu.Complete());
                    // CPU clock runs slower than system clock, so it may be
                    // complete for additional system clock cycles. Drain
                    // those out
                    do { nes.Clock(); } while (nes.Cpu.Complete());
                }

                // Emulate one whole frame
                if (GetKey(Key.F).Pressed)
                {
                    // Clock enough times to draw a single frame
                    do { nes.Clock(); } while (!nes.Gpu.FrameComplete);
                    // Use residual clock cycles to complete current instruction
                    do { nes.Clock(); } while (!nes.Cpu.Complete());
                    // Reset frame completion flag
                    nes.Gpu.FrameComplete = false;
                }
            }

            if (GetKey(Key.Space).Pressed)
                Program.EmulationRun = !Program.EmulationRun;

            if (GetKey(Key.R).Pressed)
                nes.Reset();

            if (GetKey(Key.P).Pressed)
            {
                NSelectedPalette++;
                NSelectedPalette &= 0x07;
            }

            DrawCpu(516, 2);
            DrawCode(516, 72, 26);
            /*
            // Draw Palettes & Pattern Tables ==============================================
            const int nSwatchSize = 6;
            for (int p = 0; p < 8; p++) // For each palette
                for (int s = 0; s < 4; s++) // For each index
                    FillRect(new Point(516 + p * (nSwatchSize * 5) + s * nSwatchSize, 340),
                        nSwatchSize, nSwatchSize, nes.Gpu.GetColourFromPaletteRam((byte)p, (byte)s));
            */
            // Draw selection reticule around selected palette
            //DrawRect(new Point(516 + NSelectedPalette * (nSwatchSize * 5) - 1, 339), (nSwatchSize * 4), nSwatchSize, Pixel.FromRgb(0xFFA07A));

            // Generate Pattern Tables
            //DrawSprite(new Point(516, 348), nes.Gpu.GetPatternTable(0, NSelectedPalette));
            //DrawSprite(new Point(648, 348), nes.Gpu.GetPatternTable(1, NSelectedPalette));

            DrawRam(0, 300, 0x00, 18, 18);

            // Draw rendered output ========================================================
            DrawSprite(new Point(0, 0), nes.Gpu.GetScreen());

        }

    }
}
