using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ControlCenter {
    class RouterConnection {
        public readonly Router router;
        private readonly TcpClient client;
        private readonly int id;
        private NetworkStream stream;
        private List<int> currentConnections = new List<int>();
        private Server server;
        public bool working = true;

        private StreamReader reader;

        public RouterConnection(Router router, TcpClient client, int id, Server server) {
            this.router = router;
            this.client = client;
            this.id = id;
            this.server = server;
            stream = client.GetStream();
            reader = new StreamReader(stream);
            router.working = true;

            try {
                ReceiveKeepAlive();
            } catch(Exception e) {
                GUIWindow.PrintLog(e.Message);
                GUIWindow.PrintLog(e.StackTrace);
            }
        }

        public int GetID() {
            return id;
        }

        private void ReceiveKeepAlive() {
            new Thread(() => {
                Thread.Sleep(2000);

                while (true) {
                    try {
                        string message = reader.ReadLine();
                        string IDs = message.Split(':')[1];
                        currentConnections.Clear();
                        if (IDs.Length > 0) {
                            foreach (string idString in IDs.Split(' ')) {
                                if(idString.Length > 0)
                                    currentConnections.Add(Convert.ToInt32(idString));
                            }
                        }

                        if(GUIWindow.ShowKeepAlive())
                            GUIWindow.PrintLog("CC: KEEP-ALIVE received from Router #" + id);
                    } catch(IOException) {
                        PrintLRMLogs();
                        GUIWindow.PrintLog("CC: Router #" + id + " has stopped working.");
                        router.working = false;
                        working = false;
                        server.RemoveRouterConnection(this);
                        //redirect route
                        foreach(int connectionID in currentConnections) {
                            GUIWindow.PrintLog("CC: Sent RouteTableQuery(" + connectionID + ") to RC");
                            Program.rc.FastReroute(NCC.callRegister[connectionID], connectionID);
                        }

                        break;
                    }
                }

            }).Start();
        }

        private void PrintLRMLogs() {
            foreach(Connection connection in ConfigLoader.myConnections.Values) {
                if(connection.endPoints.Item1.GetRouterID() == id || connection.endPoints.Item2.GetRouterID() == id) {
                    GUIWindow.PrintLog("Internal LRM: LinkConnection #" + connection.GetID() + " has stopped working");
                    GUIWindow.PrintLog("Internal LRM: Sent LocalTopology(" + connection.GetID() + ", DISABLED) to RC");
                    GUIWindow.PrintLog("RC: Received LocalTopology(" + connection.GetID() + ", DISABLED) from Internal LRM");
                    GUIWindow.PrintLog("RC: Sent LocalTopologyResponse() to Internal LRM");
                    GUIWindow.PrintLog("Internal LRM: Received LocalTopologyResponse() from RC");
                }
            }
        }

        public void SendRoutingTable(Dictionary<int, int> routingTable) {
            try {
                byte[] bytes = Server.SerializeObject(routingTable);
                stream.Write(bytes, 0, bytes.Length);
                string subnetworkTag = id > 5 ? "SN" : "";
                //string tag = subnetworkTag.Length == 0 ? asID.ToString() : subnetworkTag;
                //RouteControl.savedLogs.Add(new Tuple<string, string>(tag, "Sending Connection Table to Router #" + id));
                //GUIWindow.PrintLog("RC: Seding Connection Table to Router #" + id, asID);
            } catch(Exception e) {
                GUIWindow.PrintLog(e.Message);
            }
        }
    }
}
