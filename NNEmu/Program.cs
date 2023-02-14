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
        public static volatile bool EmulationRun = false;
        public static int GameWidth = 256;
        public static int GameHeight = 240;
        public static void Main()
        {
                GameWindowSettings settings = new GameWindowSettings();
                settings.RenderFrequency = 60;
                settings.UpdateFrequency = 60;
                NativeWindowSettings nativeWindow = new NativeWindowSettings();
                nativeWindow.API = ContextAPI.OpenGL;
                //Set window size, it can be different from game sizes
                //the window will adapt to game size increasing pixel size
                nativeWindow.Size = new Vector2i(512, 480);
                nativeWindow.Title = "NNEmu";
                nativeWindow.Profile = ContextProfile.Compatability;

                NNEmuRender nnemu = new NNEmuRender(settings, nativeWindow);
                nnemu.VSync = VSyncMode.On;
                nnemu.Run();
            

        }
    }
}