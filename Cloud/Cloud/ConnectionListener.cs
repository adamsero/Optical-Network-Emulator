using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Cloud {
    class ConnectionListener {
        private readonly Dictionary<string, NodeConnection> nodeConnections = new Dictionary<string, NodeConnection>();
        private TcpListener server;

        //Do wczytania z pliku
        private readonly int cloudPort = 9999;
        private readonly string ip = "localhost";
        private readonly IPAddress cloudIP;

        public ConnectionListener() {
            cloudIP = Dns.GetHostEntry(ip).AddressList[0];
            server = new TcpListener(cloudIP, cloudPort);

            try {
                server.Start();
                GUIWindow.PrintLog("Server launched, waiting for clients...");
            }
            catch(Exception ex) {
                GUIWindow.PrintLog(ex.ToString());
            }

            StartListening();
        }

        public void DisconnectNode(string ip) {
            nodeConnections.Remove(ip);
        }

        private void StartListening() {
            while (true) {
                TcpClient client = server.AcceptTcpClient(); 
                if (client == null)
                    continue;
                new NodeConnection(client, this); 
            }       
        }
        
        public void RedirectFrame(string destAddress, ushort destPort, byte[] frame) {
            if (!nodeConnections.TryGetValue(destAddress, out NodeConnection connection)) {
                GUIWindow.PrintLog("The Node to which the message is attempted to be redirected is not connected!");
                return;
            }

            //GUIWindow.PrintLog("Redirecting message to " + destAddress + ":" + destPort);
            connection.SendMessage(frame);
        }

        public void RegisterNode(string ip, NodeConnection connection) {
            nodeConnections.Add(ip, connection);
            GUIWindow.PrintLog("Node " + ip + " has connected to the server");
        }
    }
}
