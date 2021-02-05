using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ControlCenter {
    class Server {

        private readonly TcpListener server;
        //TODO: port wczytac z konfigu
        private readonly int port = ConfigLoader.ccListeningPort;
        private readonly string ip = "localhost";
        private readonly IPAddress ipAddress;
        private readonly NCC ncc;

        private static readonly LinkedList<HostConnection> hostConnections = new LinkedList<HostConnection>();
        private static readonly LinkedList<RouterConnection> routerConnections = new LinkedList<RouterConnection>();

        private int numOfHosts = 0;
        private int numOfRouters = 0;

        public Server(NCC ncc) {
            this.ncc = ncc;
            ipAddress = Dns.GetHostEntry(ip).AddressList[0];
            server = new TcpListener(ipAddress, port);
            //TODO: tu jakiś konfig wczytujemy

            GUIWindow.PrintLog("Control center is waiting for connections...");
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

        public void HandleClient(TcpClient client) {
            TcpClient MyClient = client;
            NetworkStream stream = MyClient.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);

            try {
                string request = reader.ReadLine();
                string[] cut = request.Split(':');

                if (cut[0].Equals("REGISTRATION") && cut[1].Equals("HOST")) {
                    numOfHosts++;

                    foreach (Host h in ConfigLoader.GetHosts()) {
                        if (h.getIP() == cut[2]) {

                            lock (hostConnections) {
                                hostConnections.AddLast(new HostConnection(h, client, Convert.ToInt32(cut[3]), this, ncc));
                            }
                            writer.WriteLine("component:NONE;name:RegistrationResponse;succeeded:true");
                            writer.Flush();
                            GUIWindow.PrintLog("Host #" + cut[3] + "|" + cut[2] + "| has been registered");
                            break;
                        }
                    }
                }
                else if (cut[0].Equals("REGISTRATION") && cut[1].Equals("ROUTER")) {
                    numOfRouters++;

                    foreach (Router r in ConfigLoader.GetRouters()) {
                        if (r.getIP() == cut[2]) {
                            lock (routerConnections) {
                                routerConnections.AddLast(new RouterConnection(r, client, Convert.ToInt32(cut[3]), this));
                            }
                            writer.WriteLine("REGISTRATION:OK");
                            writer.Flush();

                            GUIWindow.PrintLog("Router #" + cut[3] + "|" + cut[2] + "| has been registered");

                            break;
                        }
                    }
                }

            } catch (IOException) {
                GUIWindow.PrintLog("One of the network nodes has been disconected");
            }
        }

        public void RemoveHostConnection(HostConnection hostConnection) {
            GUIWindow.PrintLog("Host #" + hostConnection.GetID() + " has disconnected");
            hostConnections.Remove(hostConnection);
        }

        public void RemoveRouterConnection(RouterConnection routerConnection) {
            routerConnections.Remove(routerConnection);
        }

        public static byte[] SerializeObject(object o) {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            return stream.ToArray();
        }

        public static object DeserializeObject(byte[] bytes) {
            MemoryStream stream = new MemoryStream(bytes);
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(stream);
        }

        public static LinkedList<HostConnection> GetHostConnections() {
            return hostConnections;
        }

        public static LinkedList<RouterConnection> GetRouterConnections() {
            return routerConnections;
        }

        public static HostConnection GetHostConnectionByIP(String ip) {
            foreach (HostConnection hc in hostConnections) {
                if (hc.GetHost().getIP().Equals(ip)) {
                    return hc; 
                }
            }
            return null;
        }
    }
}
