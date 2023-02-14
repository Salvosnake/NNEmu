using NNEmu.Software;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

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
            
            //0 a entrambi consente di adattarsi al hardware
            settings.RenderFrequency = 0;
            settings.UpdateFrequency = 0;

            NativeWindowSettings nativeWindow = new NativeWindowSettings();
            nativeWindow.API = ContextAPI.OpenGL;
            //Setto le dimensioni della finestra, che sono diverse da quelle di gioco
            //viene aumentata in automatico la dimensine dei pixel e si auto adatta
            nativeWindow.Size = new Vector2i(512, 480);
            nativeWindow.Title = "NNEmu";
            nativeWindow.Profile = ContextProfile.Compatability;
            nativeWindow.WindowBorder = WindowBorder.Fixed;

            NNEmuRender nnemu = new NNEmuRender(settings, nativeWindow);
            nnemu.VSync = VSyncMode.On;
            nnemu.Run();
        }
    }
}