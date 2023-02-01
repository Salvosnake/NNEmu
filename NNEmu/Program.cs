using NNEmu.Hardware;
using NNEmu.Software;

namespace NNEmu
{
    public class Program
    {
        public static volatile CARTRIDGE? cart;
        public static volatile BUS? nes;
        public static volatile bool EmulationRun = true;
        private static bool Debug = false;
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
            
            nes = new BUS(cart);
            if (Debug)
            {
                new Thread(delegate ()
                {
                    NNEmuDebugger debugger = new NNEmuDebugger(nes);
                    debugger.Construct(780, 480, 2, 2);
                    debugger.Start();
                })
                { }.Start();
            }
            else
            {
                new Thread(delegate ()
                {
                    Software.NNEmu nnemu = new Software.NNEmu(nes);
                    nnemu.Construct(256, 240, 2, 2);
                    nnemu.Start();
                })
                { }.Start();
            }
        }
    }
}