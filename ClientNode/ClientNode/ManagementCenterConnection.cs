using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using FrameLib;

namespace ClientNode {
    class ManagementCenterConnection {
        //do wczytania z config
        private readonly int centerPort = 8888;
        private static NetworkStream stream;
        private static StreamReader reader;
        private static StreamWriter writer;
        //private static ushort ClientPort = 50000;
        //private static string ClientIP = "160.0.0.50";

        public ManagementCenterConnection() {

            try {
                TcpClient tcpClient = new TcpClient("localhost", centerPort);
                stream = tcpClient.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                GUIWindow.PrintLog("Connection with Management Center has been established");

                GUIWindow.PrintLog("Sent registration request to Management Center");
                SendRegistrationRequest();
                RecieveRegistrationInfo();

                lock (Program.waiterCloud) {
                    Monitor.Pulse(Program.waiterCloud);
                }

                //new Thread(ReceiveDataFromManagementCenter).Start();

            } catch (SocketException) {
                GUIWindow.PrintLog("Connection with Management Center has NOT been established");
            }
        }

        private void SendRegistrationRequest() {
            writer.WriteLine("REGISTRATION:HOST:" + ConfigLoader.ip + ":" + ConfigLoader.nodeID);
            writer.Flush();
        }

        private void RecieveRegistrationInfo() {
            //for (int i = 0; i < 2; i++) {
            try {
                string msg = reader.ReadLine();
                string[] parameters = msg.Split(':');

                if (parameters[0].Equals("REGISTRATION") && parameters[1].Equals("OK")) {
                    GUIWindow.PrintLog("Managment Center accepted registration request");
                }
                else {
                    GUIWindow.PrintLog("Managment Center denied registration request");
                }
            } catch (IOException e) {
                GUIWindow.PrintLog(e.Message);
            }
            //}
        }

        public static void SendLabelRequest(string line) {
            writer.WriteLine(line);
            writer.Flush();
        }

        //private void ReceiveDataFromManagementCenter() {
        //    while (true) {
        //        string msg = reader.ReadLine();
        //        string[] keyWords = msg.Split(' ');
        //        switch (keyWords[0]) {
        //            case "SEND_LABEL":
                        
        //                string label = keyWords[1];
        //                if(label.Equals("-1")) {
        //                    GUIWindow.PrintLog("There is no available connection to target Host");
        //                    break;
        //                }

        //                GUIWindow.PrintLog("Received MPLS label from Management Center");
        //                GUIWindow.SendMessage(label);
        //                break;
        //        }
        //    }
        //}

    }
}

