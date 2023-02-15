using NNEmu.Hardware;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace NNEmu.Software
{
    public class NNEmuRender : GameWindow
    {
        private volatile BUS? Nes;
        private volatile int[] DisplayBuffer;
        private volatile int[] DefaultScreenBuffer;
        public NNEmuRender(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            MakeCurrent();
            DisplayBuffer = new int[Program.GameWidth * Program.GameHeight];
            DefaultScreenBuffer = new int[Program.GameWidth * Program.GameHeight];
            ClearBufferDisplay();
            LoadDefaultScreen();
        }

        private void Window_FileDrop(FileDropEventArgs fileDrop)
        {
            string[] files = fileDrop.FileNames;
            string file = files[0];
            if(file.EndsWith(".nes")) {
                CARTRIDGE cart = new CARTRIDGE(file);
                Nes = new BUS(cart,Program.GameWidth, Program.GameHeight);
                //Reset stato
                Nes.Reset();
                //Init controller
                Nes.Controller[0] = 0;
                //Start emulazione
                Program.EmulationRun = true;
            }
        }

        protected override void OnLoad() 
        {
            //File drop init
            FileDrop += Window_FileDrop;

            //Init GameWindow
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.Texture2D);
            GL.ClearColor(1, 1, 1, 1);
        }
        //Render immagine
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            GL.TexImage2D(TextureTarget.Texture2D, 0,
                PixelInternalFormat.Rgb,
                Program.GameWidth, Program.GameHeight, 0,
                PixelFormat.Bgra,
                PixelType.UnsignedByte,
                DisplayBuffer);

            
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
        //Prendo i dati per l'immagine
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (Program.EmulationRun && Nes != null)
            {
                do { Nes.Clock(); } while (!Nes.Gpu.FrameComplete);
                Nes.Gpu.FrameComplete = false;
                DisplayBuffer = Nes.Gpu.GetScreen();
            }
            else//Default screen
                DisplayBuffer = DefaultScreenBuffer;
            
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (Program.EmulationRun && Nes != null)
            {
                switch (e.Key)
                {
                    case Keys.R:
                        Nes.Reset();
                        break;
                    case Keys.Right:
                        Nes.Controller[0] |= 0x01;
                        break;
                    case Keys.Left:
                        Nes.Controller[0] |= 0x02;
                        break;
                    case Keys.Down:
                        Nes.Controller[0] |= 0x04;
                        break;
                    case Keys.Up:
                        Nes.Controller[0] |= 0x08;
                        break;
                    case Keys.S://Start
                        Nes.Controller[0] |= 0x10;
                        break;
                    case Keys.A://Select
                        Nes.Controller[0] |= 0x20;
                        break;
                    case Keys.Z://B
                        Nes.Controller[0] |= 0x40;
                        break;
                    case Keys.X://A
                        Nes.Controller[0] |= 0x80;
                        break;
                    default:
                        break;
                }
            }
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            //Rimuovo i valori dei tasti rilasciati
            if (Program.EmulationRun && Nes != null)
            {
                switch (e.Key)
                {
                    case Keys.Right:
                        Nes.Controller[0] &= unchecked((byte)~0x01);
                        break;
                    case Keys.Left:
                        Nes.Controller[0] &= unchecked((byte)~0x02);
                        break;
                    case Keys.Down:
                        Nes.Controller[0] &= unchecked((byte)~0x04);
                        break;
                    case Keys.Up:
                        Nes.Controller[0] &= unchecked((byte)~0x08);
                        break;
                    case Keys.S://Start
                        Nes.Controller[0] &= unchecked((byte)~0x10);
                        break;
                    case Keys.A://Select
                        Nes.Controller[0] &= unchecked((byte)~0x20);
                        break;
                    case Keys.Z://B
                        Nes.Controller[0] &= unchecked((byte)~0x40);
                        break;
                    case Keys.X://A
                        Nes.Controller[0] &= unchecked((byte)~0x80);
                        break;
                    default:
                        break;
                }
            }
        }

        //Resetta il buffer dello schermo
        private void ClearBufferDisplay()
        {
            for (int i = 0; i < DisplayBuffer.Length; i++)
                DisplayBuffer[i] = 0;
        }
        //Carica la schermata di default
        private void LoadDefaultScreen()
        {
            Bitmap bitmap = new Bitmap(@"Utils\NNEmu.dat");
            DefaultScreenBuffer =  GetRGB(bitmap,0, 0, Program.GameWidth, Program.GameHeight, 0, Program.GameWidth);
        }
        //Trasforma un immagine in un array di int
        public int[] GetRGB(Bitmap image, int startX, int startY, int w, int h, int offset, int scansize)
        {
            int[] rgbArray = new int[w*h];
            const int PixelWidth = 3;
            const System.Drawing.Imaging.PixelFormat PixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;

            if (image == null) throw new ArgumentNullException("image");
            if (rgbArray == null) throw new ArgumentNullException("rgbArray");
            if (startX < 0 || startX + w > image.Width) throw new ArgumentOutOfRangeException("startX");
            if (startY < 0 || startY + h > image.Height) throw new ArgumentOutOfRangeException("startY");
            if (w < 0 || w > scansize || w > image.Width) throw new ArgumentOutOfRangeException("w");
            if (h < 0 || (rgbArray.Length < offset + h * scansize) || h > image.Height) throw new ArgumentOutOfRangeException("h");

            BitmapData data = image.LockBits(new Rectangle(startX, startY, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat);
            try
            {
                byte[] pixelData = new byte[data.Stride];
                for (int scanline = 0; scanline < data.Height; scanline++)
                {
                    Marshal.Copy(data.Scan0 + (scanline * data.Stride), pixelData, 0, data.Stride);
                    for (int pixeloffset = 0; pixeloffset < data.Width; pixeloffset++)
                    {
                        // PixelFormat.Format32bppRgb means the data is stored
                        // in memory as BGR. We want RGB, so we must do some 
                        // bit-shuffling.
                        rgbArray[offset + (scanline * scansize) + pixeloffset] =
                            (pixelData[pixeloffset * PixelWidth + 2] << 16) +   // R 
                            (pixelData[pixeloffset * PixelWidth + 1] << 8) +    // G
                            pixelData[pixeloffset * PixelWidth];                // B
                    }
                }
            }
            finally
            {
                image.UnlockBits(data);
            }

            return rgbArray;
        }

    }
}
