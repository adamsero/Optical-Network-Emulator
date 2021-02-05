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
    class ChildConnection {

        private readonly TcpClient client;
        private readonly NetworkStream stream;
        private readonly StreamReader reader;
        private readonly StreamWriter writer;
        private readonly NCC ncc;

        public ChildConnection(TcpClient client, NCC ncc) {
            this.client = client;
            this.ncc = ncc;
            stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            Register();

            new Thread(RecieveMessages).Start();
        }

        private void Register() {
            SendMessage("name:Registration;type:child");
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
                }
            }
        }

    }
}
