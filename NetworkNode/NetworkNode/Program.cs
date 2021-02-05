using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;

namespace NetworkNode {
    static class Program {

        public static object waiterConfig = new object();
        public static object waiterManagement = new object();
        public static object waiterCloud = new object();
        public static CloudConnection cloudConnection;

        public static List<Tuple<int, int>> routingTable = new List<Tuple<int, int>>();

        [STAThread]
        static void Main(string[] args) {
            try {

            
            args = Environment.GetCommandLineArgs();

            new Thread(() => {
                //Thread.Sleep(2000);
                lock (waiterConfig) {
                    Monitor.Wait(Program.waiterConfig);
                    //String config = String.Concat(File.ReadAllLines("./../../../../sharedResources/tsst_config.xml"));
                    //ConfigLoader.LoadConfig(config, "1");

                    String config = String.Concat(File.ReadAllLines(args[1]));
                    ConfigLoader.LoadConfig(config, args[2]);
                    lock (Program.waiterManagement) {
                        Monitor.Pulse(Program.waiterManagement);
                    }
                }
                
            }).Start(); 


            new Thread(() => {
                //Thread.Sleep(2000);
                lock (Program.waiterManagement) {
                    Monitor.Wait(Program.waiterManagement);
                }
                new ManagementCenterConnection();
                new ControlCenterConnection();
            }).Start();

            new Thread (() => {
                lock (Program.waiterCloud) {
                    Monitor.Wait(Program.waiterCloud); 
                }
                cloudConnection = new CloudConnection();
            }).Start(); 

            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GUIWindow());
            }catch(Exception e) {
                GUIWindow.PrintLog(e.StackTrace);
            }
        }
    }
}
