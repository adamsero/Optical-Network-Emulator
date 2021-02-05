using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ClientNode {
    static class Program {

        public static object waiterConfig = new object();
        public static object waiterManagement = new object();
        public static object waiterCloud = new object();

        public static String ipAdress;
        public static int port;
        public static CloudConnection cloudConnection;
        public static CPCC cpcc;

        [STAThread]
        static void Main(string[] args) {

            args = Environment.GetCommandLineArgs();

            new Thread(() => { 

                Thread.Sleep(1000);
                try {
                    lock (Program.waiterConfig) {
                        //Monitor.Wait(Program.waiterConfig);
                        //String config = String.Concat(File.ReadAllLines("./../../../../sharedResources/tsst_config.xml"));
                        //ConfigLoader.LoadConfig(config, "1");
                        String config = String.Concat(File.ReadAllLines(args[1]));
                        ConfigLoader.LoadConfig(config, args[2]); 
                        lock (Program.waiterManagement) {
                            Monitor.Pulse(Program.waiterManagement);
                        }
                    }
                } catch(Exception e) {
                    GUIWindow.PrintLog(e.StackTrace);
                }

            }).Start();

            new Thread(() => {

                lock (Program.waiterManagement) {
                    Monitor.Wait(Program.waiterManagement);
                }
                new ManagementCenterConnection(); 
            }).Start();


            new Thread(() =>{

                lock (Program.waiterCloud){
                    Monitor.Wait(Program.waiterCloud);
                }
                cloudConnection = new CloudConnection();
            }).Start();

            new Thread(() => {
                Thread.Sleep(2500);
                /*lock (Program.waiterCloud) {
                    Monitor.Wait(Program.waiterCloud);
                }*/
                cpcc = new CPCC();
            }).Start();


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GUIWindow());
        }

        public static void SetConfig(String ip, int port) {
            Program.ipAdress = ip;
            Program.port = port;
        }
    }
}
