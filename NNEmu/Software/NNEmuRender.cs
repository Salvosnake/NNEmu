using NNEmu.Hardware;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace NNEmu.Software
{
    public class NNEmuRender : GameWindow
    {
        private readonly BUS nes;
        private volatile int[] displayBuffer;
        public NNEmuRender(BUS nes, GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            MakeCurrent();
            this.nes = nes;
            displayBuffer = nes.Gpu.GetScreen();
        }

        protected override void OnLoad() 
        {
            // Reset Nes state
            nes.Reset();

            //Init GameWindow
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.Texture2D);
            GL.ClearColor(1, 1, 1, 1);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {

            //Console.WriteLine(this.RenderFrequency);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            GL.TexImage2D(TextureTarget.Texture2D, 0,
                PixelInternalFormat.Rgb,
                256, 240, 0,
                PixelFormat.Bgra,
                PixelType.UnsignedByte,
                displayBuffer);

            
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 1); GL.Vertex2(-1, -1);
            GL.TexCoord2(1, 1); GL.Vertex2(1, -1);
            GL.TexCoord2(1, 0); GL.Vertex2(1, 1);
            GL.TexCoord2(0, 0); GL.Vertex2(-1, 1);
            GL.End();

            GL.DeleteTexture(id);
            SwapBuffers();

        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (Program.EmulationRun)
            {
                do { nes.Clock(); } while (!nes.Gpu.FrameComplete);
                nes.Gpu.FrameComplete = false;
            }
            displayBuffer = nes.Gpu.GetScreen();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            nes.Controller[0] = 0;
            switch (e.Key)
            {
                case Keys.Space:
                    Program.EmulationRun = !Program.EmulationRun;
                    break;
                case Keys.R:
                    nes.Reset();
                    break;
                case Keys.Right:
                    nes.Controller[0] |= 0x01;
                    break;
                case Keys.Left:
                    nes.Controller[0] |= 0x02;
                    break;
                case Keys.Down:
                    nes.Controller[0] |= 0x04;
                    break;
                case Keys.Up:
                    nes.Controller[0] |= 0x08;
                    break;
                case Keys.S://Start
                    nes.Controller[0] |= 0x10;
                    break;
                case Keys.A://Select
                    nes.Controller[0] |= 0x20;
                    break;
                case Keys.Z://B
                    nes.Controller[0] |= 0x40;
                    break;
                case Keys.X://A
                    nes.Controller[0] |= 0x80;
                    break;
                default:
                    break;
            }
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            nes.Controller[0] = 0;
            switch (e.Key)
            {
                case Keys.Right:
                    nes.Controller[0] |= 0x00;
                    break;
                case Keys.Left:
                    nes.Controller[0] |= 0x00;
                    break;
                case Keys.Down:
                    nes.Controller[0] |= 0x00;
                    break;
                case Keys.Up:
                    nes.Controller[0] |= 0x00;
                    break;
                case Keys.S://Start
                    nes.Controller[0] |= 0x00;
                    break;
                case Keys.A://Select
                    nes.Controller[0] |= 0x00;
                    break;
                case Keys.Z://B
                    nes.Controller[0] |= 0x00;
                    break;
                case Keys.X://A
                    nes.Controller[0] |= 0x00;
                    break;
                default:
                    break;
            }
        }
        
    }
}
