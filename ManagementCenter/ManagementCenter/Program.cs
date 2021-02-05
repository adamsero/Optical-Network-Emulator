using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagementCenter {
    static class Program {

        public static object waiter = new object();
        public static Server server;

        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        /// 
        [STAThread]
        static void Main(string[] args) {
            args = Environment.GetCommandLineArgs();

            new Thread(() =>
            {
                Thread.Sleep(1000);
                String config = String.Concat(File.ReadAllLines("./../sharedResources/tsst_config.xml"));
                //String config2 = String.Concat(File.ReadAllLines("./../../../../sharedResources/tsst_config_alpha_v2.xml"));
                //String config = String.Concat(File.ReadAllLines(args[1]));
                //String config2 = args.Length > 2 ? String.Concat(File.ReadAllLines(args[2])) : "";
                server = new Server(config);  
            }).Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false); 
            Application.Run(new GUIWindow());
            
        }
    }
}
