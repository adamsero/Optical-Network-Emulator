using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ControlCenter {
    static class Program {

        private static Server server;
        public static bool ConnectionAvaliable = true;
        public static int lastPathIndex = 1;

        [STAThread]
        static void Main(String[] args) {
            try {
                args = Environment.GetCommandLineArgs();

                new Thread(() => {

                    Thread.Sleep(1000);
                    try {
                        //String config = String.Concat(File.ReadAllLines(args[1]));
                        //ConfigLoader.LoadConfig(config, args[2]);

                        String config = String.Concat(File.ReadAllLines("./../../../../sharedResources/tsst_config.xml")); 
                        ConfigLoader.loadConfig(config, "1");

                        //String config = String.Concat(File.ReadAllLines("./../../../../sharedResources/tsst_config.xml"));
                        //ConfigLoader.loadConfig(config);
                        //String config = String.Concat(File.ReadAllLines(args[1]));
                        //ConfigLoader.LoadConfig(config, args[2]);
                        NCC ncc = new NCC();
                        server = new Server(ncc);
                    }
                    catch (Exception e) {
                        GUIWindow.PrintLog(e.StackTrace);
                    }

                }).Start();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new GUIWindow());
            }
            catch (Exception ex) {
                GUIWindow.PrintLog(ex.StackTrace);
            }
        }
    }
}
