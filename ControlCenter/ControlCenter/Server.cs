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
        private readonly int port = 15000;
        private readonly string ip = "localhost";
        private readonly IPAddress ipAddress;

        public static List<NCC> nccList;
        private static readonly LinkedList<HostConnection> hostConnections = new LinkedList<HostConnection>();
        private static readonly LinkedList<RouterConnection> routerConnections = new LinkedList<RouterConnection>();

        private int numOfHosts = 0;
        private int numOfRouters = 0;

        public Server(List<NCC> ncc) {
            ipAddress = Dns.GetHostEntry(ip).AddressList[0];
            server = new TcpListener(ipAddress, port);
            nccList = ncc; 
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
                                hostConnections.AddLast(new HostConnection(h, client, Convert.ToInt32(cut[3]), this, h.GetAsID(), GetNCCByAsID(h.GetAsID())));
                            }
                            writer.WriteLine("REGISTRATION:OK");
                            writer.Flush();
                            GUIWindow.PrintLog("Host #" + cut[3] + "|" + cut[2] + "| has been registered", h.GetAsID());
                            break;
                        }
                    }
                }
                else if (cut[0].Equals("REGISTRATION") && cut[1].Equals("ROUTER")) {
                    numOfRouters++;

                    foreach (Router r in ConfigLoader.GetRouters()) {
                        if (r.getIP() == cut[2]) {
                            lock (routerConnections) {
                                routerConnections.AddLast(new RouterConnection(r, client, Convert.ToInt32(cut[3]), r.GetAsID(), this));
                            }
                            writer.WriteLine("REGISTRATION:OK");
                            writer.Flush();

                            GUIWindow.PrintLog("Router #" + cut[3] + "|" + cut[2] + "| has been registered", r.GetAsID());

                            break;
                        }
                    }
                }

            } catch (IOException ex) {
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

        public static List<NCC> GetNCCList() {
            return nccList;
        }

        public static NCC GetNCCByAsID(int asID) {
            foreach(NCC n in nccList){
                if (asID == n.getAsID()) {
                    return n;
                }
            }
            return null;
        }

        //TODO: temporary
        public static void SendRTs() {
            Dictionary<int, int> ports = new Dictionary<int, int>();
            ports.Add(1, 200);
            ports.Add(2, 300);
            ports.Add(3, 200);
            ports.Add(4, 200);
            ports.Add(5, 100);
            ports.Add(6, 300);
            ports.Add(7, 100);
            ports.Add(8, 200);
            ports.Add(9, 200);

            foreach(RouterConnection routerConnection in routerConnections) {
                ports.TryGetValue(routerConnection.GetID(), out int port);
                Dictionary<int, int> routingTable = new Dictionary<int, int>();
                routingTable.Add(1, port);
                routerConnection.SendRoutingTable(routingTable);
            }

            ports.Clear();
            /*
            new Thread(() => {

                Thread.Sleep(3000);
                ports.Add(1, -1);
                ports.Add(3, -1);
                ports.Add(5, -1);

                foreach (RouterConnection routerConnection in routerConnections) {
                    bool succeeded = ports.TryGetValue(routerConnection.GetID(), out int port);
                    Dictionary<int, int> routingTable = new Dictionary<int, int>();
                    routingTable.Add(1, port);
                    if(port != 0)
                        routerConnection.SendRoutingTable(routingTable);
                }
            }).Start();
            */
        }
    }
}
