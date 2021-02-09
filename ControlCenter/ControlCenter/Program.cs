using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using ControlCenter.Components;

namespace ControlCenter {
    static class Program {

        private static Server server;
        public static bool ConnectionAvaliable = true;
        public static int lastPathIndex = 1;

        public static InterCcCommunicationServer interCCServer;
        public static PeerConnection peerConnection;
        public static ChildConnection childConnection;
        public static ParentConnection parentConnection;

        public static CC cc = new CC();
        public static RC rc = new RC();
        public static LRM lrm = new LRM();
        public static NCC ncc;

        [STAThread]
        static void Main(String[] args) {
            try {
                args = Environment.GetCommandLineArgs();

                new Thread(() => {

                    Thread.Sleep(1000);
                    try {

                        String config = String.Concat(File.ReadAllLines(args[1]));
                        ConfigLoader.loadConfig(config, args[2]);

                        //String config = String.Concat(File.ReadAllLines("./../../../../sharedResources/tsst_config.xml")); 
                        //ConfigLoader.loadConfig(config, "1");


                        //String config = String.Concat(File.ReadAllLines("./../../../../sharedResources/tsst_config.xml"));
                        //ConfigLoader.loadConfig(config);
                        //String config = String.Concat(File.ReadAllLines(args[1]));
                        //ConfigLoader.LoadConfig(config, args[2]);
                        ncc = new NCC();
                        server = new Server(ncc);

                        if(ConfigLoader.ccID == 2) {
                            interCCServer = new InterCcCommunicationServer(ncc);
                        } else if(ConfigLoader.ccID == 1) {
                            peerConnection = new PeerConnection(new TcpClient("localhost", 12500), true, ncc);
                        } else {
                            childConnection = new ChildConnection(new TcpClient("localhost", 12500), ncc);
                        }
                        GUIWindow.UpdateChannelTable();
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
