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
    class ParentConnection {

        private readonly TcpClient client;
        private readonly NetworkStream stream;
        private readonly StreamReader reader;
        private readonly StreamWriter writer;
        private readonly NCC ncc;

        public ParentConnection(TcpClient client, NCC ncc) {
            this.client = client;
            this.ncc = ncc;
            stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);

            new Thread(RecieveMessages).Start();
        }

        public void SendMessage(string message) {
            writer.WriteLine(message);
            writer.Flush();
        }

        private void RecieveMessages() {

            try {
                while (true) {
                    string message = reader.ReadLine();
                    string[] pieces = message.Split(';');
                    Dictionary<string, string> data = new Dictionary<string, string>();
                    foreach (string piece in pieces) {
                        string[] keyAndValue = piece.Split(':');
                        data.Add(keyAndValue[0], keyAndValue[1]);
                    }

                    switch (data["component"]) {
                        case "RC":
                            Program.rc.HandleRequest(data);
                            break;

                        case "CC":
                            Program.cc.HandleRequest(data);
                            break;
                    }

                }
            } catch (Exception e) {
                GUIWindow.PrintLog(e.Message);
                GUIWindow.PrintLog(e.StackTrace);
            }
        }
    }
}
