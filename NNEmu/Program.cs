using NNEmu.Hardware;
using NNEmu.Software;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace NNEmu
{
    public class Program
    {
        public static volatile CARTRIDGE? cart;
        public static volatile BUS? nes;
        public static volatile bool EmulationRun = true;
        private static bool Debug = false;
        private static uint GameWidth = 256;
        private static uint GameHeight = 240;
        public static void Main(string[] args)
        {

            if (args.Length > 0)
                cart = new CARTRIDGE(args[0]);
            else
            {
                Console.WriteLine("Specificare il percorso del file .nes!");
                Console.ReadKey();
                Environment.Exit(1);
            }

            if (args.Length > 1 && args[1] != null && args[1].ToUpper().Equals("DEBUG"))
                Debug = true;
            
            nes = new BUS(cart,GameHeight,GameWidth);

            if (!Debug)
            {
                GameWindowSettings settings = new GameWindowSettings();
                settings.RenderFrequency = 60;
                settings.UpdateFrequency = 60;
                NativeWindowSettings nativeWindow = new NativeWindowSettings();
                nativeWindow.API = ContextAPI.OpenGL;
                nativeWindow.Size = new Vector2i(512, 480);
                nativeWindow.Title = "NNEmu";
                nativeWindow.Profile = ContextProfile.Compatability;

                NNEmuRender nnemu = new NNEmuRender(nes, settings, nativeWindow);
                nnemu.VSync = VSyncMode.On;
                nnemu.Run();
            }
            

        }
    }
}