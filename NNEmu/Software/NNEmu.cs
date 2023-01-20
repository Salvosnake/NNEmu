using PixelEngine;
using static NNEmu.Hardware.INSTRUCTION;
using NNEmu.Hardware;

namespace NNEmu.Software
{
    public class NNEmu : Game
    {

        private readonly BUS nes;
        private float fResidualTime = 0.0f;
        public NNEmu(BUS nes)
        {
            this.nes = nes;
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


            if (GetKey(Key.Space).Pressed)
                Program.EmulationRun = !Program.EmulationRun;

            if (GetKey(Key.R).Pressed)
                nes.Reset();


            // Draw rendered output ========================================================
            DrawSprite(new PixelEngine.Point(0, 0), nes.Gpu.GetScreen());
        }

    }
}
