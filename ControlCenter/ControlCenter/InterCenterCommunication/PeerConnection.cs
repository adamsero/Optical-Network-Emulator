using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ControlCenter {
    class PeerConnection {

        private readonly TcpClient client;
        private readonly NetworkStream stream;
        private readonly StreamReader reader;
        private readonly StreamWriter writer;
        private readonly NCC ncc;

        public PeerConnection(TcpClient client, bool register, NCC ncc) {
            this.client = client;
            this.ncc = ncc;
            stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);

            if(register) {
                Register();
            }

            new Thread(RecieveMessages).Start();
            //try {
            //    new Thread(() => {
            //        Thread.Sleep(100);
            //        Program.rc.SendNetworkTopology();
            //    }).Start();
            //} catch(Exception e) {
            //    GUIWindow.PrintLog(e.Message);
            //    GUIWindow.PrintLog(e.StackTrace);
            //}
        }

        private void Register() {
            SendMessage("name:Registration;type:peer");
        }

        public void SendMessage(string message) {
            writer.WriteLine(message);
            writer.Flush();
        }

        private void RecieveMessages() {

            try {
                while (true) {
                    string message = reader.ReadLine();
                    Dictionary<string, string> data = Util.DecodeRequest(message);

                    switch (data["component"]) {
                        case "NCC":
                            ncc.HandleRequest(data, null);
                            break;

                        case "RC":
                            Program.rc.HandleRequest(data);
                            break;

                        case "LRM":
                            Program.lrm.HandleRequest(data);
                            break;

                        case "CC":
                            Program.cc.HandleRequest(data);
                            break;
                    }
                }
            } catch(Exception e) {
                GUIWindow.PrintLog(e.Message);
                GUIWindow.PrintLog(e.StackTrace);
            }
        }
    }
}
