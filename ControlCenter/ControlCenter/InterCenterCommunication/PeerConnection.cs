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
        }

        private void Register() {
            SendMessage("name:Registration;type:peer");
        }

        public void SendMessage(string message) {
            writer.WriteLine(message);
            writer.Flush();
        }

        private void RecieveMessages() {

            while (true) {
                string message = reader.ReadLine();
                string[] pieces = message.Split(';');
                Dictionary<string, string> data = new Dictionary<string, string>();
                foreach (string piece in pieces) {
                    string[] keyAndValue = piece.Split(':');
                    data.Add(keyAndValue[0], keyAndValue[1]);
                }

                switch (data["component"]) {
                    case "NCC":
                        ncc.HandleRequest(data, null);
                        break;
                }
            }
        }
    }
}
