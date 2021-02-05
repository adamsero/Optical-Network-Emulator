using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace ManagementCenter {
    class Server {

        private TcpListener server;
        private readonly int centerPort = 8888;
        private readonly string ip = "localhost";
        private readonly IPAddress centerIP;
        private ConfigLoader ConfigLoader = new ConfigLoader();
        private readonly LinkedList<HostConnection> hostConnections = new LinkedList<HostConnection>();
        private readonly LinkedList<RouterConnection> routerConnections = new LinkedList<RouterConnection>();

        private int numOfHosts = 0;
        private int numOfRouters = 0;
        private object nodeConnectionWaiter = new object();

        public Server(string config) {
            centerIP = Dns.GetHostEntry(ip).AddressList[0];
            server = new TcpListener(centerIP, centerPort);

            
            try {
                ConfigLoader.LoadConfig(config);

                GUIWindow.PrintLog("Management center is waiting for connections...");

            } catch (Exception ex) {
                Console.WriteLine(ex);
                GUIWindow.PrintLog(ex.ToString());

            } finally {
                server.Start();
            }

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
                                hostConnections.AddLast(new HostConnection(h, client, Convert.ToInt32(cut[3]), this));
                            }
                            writer.WriteLine("REGISTRATION:OK");
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
                                routerConnections.AddLast(new RouterConnection(r, client, Convert.ToInt32(cut[3])));
                            }
                            writer.WriteLine("REGISTRATION:OK");
                            writer.Flush();

                            GUIWindow.PrintLog("Router #" + cut[3] + "|" + cut[2] + "| has been registered");

                            //LinkedList<string[]> rows = new LinkedList<string[]>();
                            //foreach (var tuple in ConfigLoader.routerMPLSTables) {
                            //    if (tuple.Item1 == Convert.ToInt32(cut[3])) {
                            //        var vals = tuple.Item2;
                            //        string[] row = { vals.Item1.ToString(), vals.Item2.ToString(), vals.Item3.ToString(), vals.Item4,
                            //                         vals.Item5.ToString(), vals.Item6.ToString(), vals.Item7.ToString(), vals.Rest.Item1.ToString() };
                            //        rows.AddLast(row);
                            //    }
                            //}
                            //byte[] bytes = SerializeObject(rows);
                            //stream.Write(bytes, 0, bytes.Length);
                            //GUIWindow.PrintLog("Sending MPLS table to Router #" + cut[3]);

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

        public void HandleClientConfigRequest(string request) {

        }

        public LinkedList<HostConnection> GetHostConnections() {
            return hostConnections;
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
    }

}

