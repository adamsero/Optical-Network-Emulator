using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace ControlCenter {
    class InterCcCommunicationServer {

        private readonly TcpListener server;
        private readonly int port = 12500;
        private readonly string ip = "localhost";
        private readonly IPAddress ipAddress;
        private readonly NCC ncc;

        public InterCcCommunicationServer(NCC ncc) {
            this.ncc = ncc;
            ipAddress = Dns.GetHostEntry(ip).AddressList[0];
            server = new TcpListener(ipAddress, port);
            server.Start();

            new Thread(StartListening).Start();
        }

        private void StartListening() {

            while (true) {
                TcpClient client = server.AcceptTcpClient();
                if (client == null)
                    continue;

                new Thread(() => HandleClient(client)).Start();
            }
        }

        private void HandleClient(TcpClient client) {
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);

            string message = reader.ReadLine();
            string[] pieces = message.Split(';');
            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach (string piece in pieces) {
                string[] keyAndValue = piece.Split(':');
                data.Add(keyAndValue[0], keyAndValue[1]);
            }

            if(data["name"].Equals("Registration") && data["type"].Equals("peer")) {
                Program.peerConnection = new PeerConnection(client, false, ncc);

                //GUIWindow.PrintLog("[TEST] Peer registered");
            } else if(data["name"].Equals("Registration") && data["type"].Equals("child")) {
                Program.parentConnection = new ParentConnection(client, ncc);
                //GUIWindow.PrintLog("[TEST] Child registered");
            }
        }
    }
}
