using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class RC {

        public static readonly Dictionary<string, string> cachedData = new Dictionary<string, string>();
        public static Dictionary<int, int[]> cachedChannels = new Dictionary<int, int[]>();
        public static Path currentPath;
        public static int currentConnectionID = 0;

        public void HandleRequest(Dictionary<string, string> data) {
            switch (data["name"]) {
                case "NetworkTopology":
                    switch(data["receiver"]) {
                        case "peer":
                            GUIWindow.PrintLog("RC: Received TopologyQuery() from peer RC");
                            
                            if(data["scenario"].Equals("1")) {
                                //scenariusz #1
                                GUIWindow.PrintLog("RC: Sent TopologyQuery() to child RC");
                                string message2 = "component:RC;name:NetworkTopology;receiver:child;scenario:" + data["scenario"];
                                Program.parentConnection.SendMessage(message2);
                            } else if (data["scenario"].Equals("2")) {
                                //scenariusz #2
                                string hostList = " Hosts = ";
                                foreach (Host host in ConfigLoader.myHosts)
                                    hostList += host.GetHostID() + ", ";
                                hostList = hostList.Remove(hostList.Length - 2, 2);

                                string routersList2 = "Routers = ";
                                foreach (Router router in ConfigLoader.myRouters)
                                    routersList2 += router.GetRouterID() + ", ";
                                routersList2 = routersList2.Remove(routersList2.Length - 2, 2);

                                string linksList2 = " Links = ";
                                string channelList2 = "";
                                foreach (Connection connection in ConfigLoader.myConnections.Values) {
                                    linksList2 += connection.endPoints.Item1.GetHostID() + "-" + connection.endPoints.Item2.GetHostID() + ", ";
                                    channelList2 += connection.GetID() + "=" + string.Join(",", connection.GetSlot()) + "-";
                                }                                    
                                linksList2 = linksList2.Remove(linksList2.Length - 2, 2);
                                channelList2 = channelList2.Remove(channelList2.Length - 1, 1);
                                //GUIWindow.PrintLog("Channel List: " + channelList2);

                                GUIWindow.PrintLog("RC: Sent TopologyQueryResponse(" + routersList2 + hostList + linksList2 + ") to peer RC");
                                string message = "component:RC;name:NetworkTopologyResponse;receiver:peer;routersList:" + routersList2 + ";linksList:" + linksList2 + ";hostList:" + hostList + ";scenario:" + data["scenario"] + ";channels:" + channelList2 + ";connID:" + currentConnectionID;
                                Program.peerConnection.SendMessage(message);
                            }
                            break;

                        case "child":
                            //scenariusz #1, 2 lub 3
                            GUIWindow.PrintLog("RC: Received TopologyQuery() from parent RC");
                            string routersList = "Routers = ";
                            foreach (Router router in ConfigLoader.myRouters)
                                routersList += router.GetRouterID() + ", ";
                            routersList = routersList.Remove(routersList.Length - 2, 2);

                            string linksList = " Connections = ";
                            string channelList = "";
                            foreach (Connection connection in ConfigLoader.myConnections.Values) {
                                linksList += connection.endPoints.Item1.GetHostID() + "-" + connection.endPoints.Item2.GetHostID() + ", ";
                                channelList += connection.GetID() + "=" + string.Join(",", connection.GetSlot()) + "-";
                            }
                            linksList = linksList.Remove(linksList.Length - 2, 2);
                            channelList = channelList.Remove(channelList.Length - 1, 1);
                            //GUIWindow.PrintLog("Channel List: " + channelList);

                            GUIWindow.PrintLog("RC: Sent TopologyQueryResponse(" + routersList + linksList + ") to parent RC");
                            string message1 = "component:RC;name:NetworkTopologyResponse;receiver:parent;routersList:" + routersList + ";linksList:" + linksList + ";scenario:" + data["scenario"] + ";channels:" + channelList;
                            Program.childConnection.SendMessage(message1);
                            break;
                    }

                    break;

                case "NetworkTopologyResponse":
                    switch (data["receiver"]) {
                        case "parent":
                            GUIWindow.PrintLog("RC: Received TopologyQueryResponse(" + data["routersList"] + data["linksList"] + ") from child RC");
                            if (data["scenario"].Equals("1")) {
                                //scenariusz #1
                                string hostList = " Hosts = ";
                                foreach (Host host in ConfigLoader.myHosts)
                                    hostList += host.GetHostID() + ", ";
                                hostList = hostList.Remove(hostList.Length - 2, 2);

                                string routersList = data["routersList"] + ", ";
                                foreach (Router router in ConfigLoader.myRouters)
                                    routersList += router.GetRouterID() + ", ";
                                routersList = routersList.Remove(routersList.Length - 2, 2);

                                string linksList = data["linksList"] + ", ";
                                string channelList = data["channels"] + "-";
                                foreach (Connection connection in ConfigLoader.myConnections.Values) {
                                    linksList += connection.endPoints.Item1.GetHostID() + "-" + connection.endPoints.Item2.GetHostID() + ", ";
                                    channelList += connection.GetID() + "=" + string.Join(",", connection.GetSlot()) + "-";
                                }
                                linksList = linksList.Remove(linksList.Length - 2, 2);
                                channelList = channelList.Remove(channelList.Length - 1, 1);

                                GUIWindow.PrintLog("RC: Sent TopologyQueryResponse(" + routersList + hostList + linksList + ") to peer RC");
                                string message = "component:RC;name:NetworkTopologyResponse;receiver:peer;routersList:" + routersList + ";linksList:" + linksList + ";hostList:" + hostList + ";scenario:" + data["scenario"] + ";channels:" + channelList + ";connID:" + currentConnectionID;
                                Program.peerConnection.SendMessage(message);
                            } else if(data["scenario"].Equals("2")) {
                                //scenariusz #2
                                GUIWindow.PrintLog("RC: Sent TopologyQuery() to peer RC");
                                CacheChannels(data["channels"]);
                                string message = "component:RC;name:NetworkTopology;receiver:peer;routersList:" + data["routersList"] + ";linksList:" + data["linksList"] + ";scenario:" + data["scenario"];
                                Program.peerConnection.SendMessage(message);
                            }
                            else if (data["scenario"].Equals("3")) {
                                //koniec scenariusza #3
                                CacheChannels(data["channels"]);
                                LoadMyChannels();
                                CalculateAllPaths();
                            }
                            break;

                        case "peer":
                            GUIWindow.PrintLog("RC: Received TopologyQueryResponse(" + data["routersList"] + data["hostList"] + data["linksList"] + ") from peer RC");
                            if (data["scenario"].Equals("1")) {
                                //koniec scenariusza #1
                                cachedData.Add("peerConnID", data["connID"]);
                                CacheChannels(data["channels"]);
                                LoadMyChannels();
                                CalculateAllPaths();
                            } else if (data["scenario"].Equals("2")) {
                                //koniec scenariusza #2
                                cachedData.Add("peerConnID", data["connID"]);
                                CacheChannels(data["channels"]);
                                LoadMyChannels();
                                CalculateAllPaths();
                            }
                            break;
                    }
                    break;

                case "RouteTableQuery":
                    GUIWindow.PrintLog("RC: Received RouteTableQuery(" + data["routerX"] + ", " + data["routerY"] + ", " + data["speed"] + " Gb/s, InterDomainConnection: " + data["IDC"] + ") from CC");

                    cachedData.Clear();
                    cachedData.Add("routerX", data["routerX"]);
                    cachedData.Add("routerY", data["routerY"]);
                    cachedData.Add("speed", data["speed"]);
                    cachedData.Add("IDC", data["IDC"]);

                    if (ConfigLoader.ccID == 1) {
                        if(Convert.ToBoolean(data["IDC"])) {
                            //scenariusz #1
                            string message = "component:RC;name:NetworkTopology;receiver:peer;scenario:1";
                            GUIWindow.PrintLog("RC: Sent TopologyQuery() to peer RC");
                            Program.peerConnection.SendMessage(message);
                        } else {
                            //scenariusz #4
                            LoadMyChannels();
                            CalculateAllPaths();
                        }
                    } else if(ConfigLoader.ccID == 2) {
                        if (Convert.ToBoolean(data["IDC"])) {
                            //scenariusz #2
                            string message = "component:RC;name:NetworkTopology;receiver:child;scenario:2";
                            GUIWindow.PrintLog("RC: Sent TopologyQuery() to child RC");
                            Program.parentConnection.SendMessage(message);
                        }
                        else {
                            //scenariusz #3
                            string message = "component:RC;name:NetworkTopology;receiver:child;scenario:3";
                            GUIWindow.PrintLog("RC: Sent TopologyQuery() to child RC");
                            Program.parentConnection.SendMessage(message);
                        }
                    }
                    break;

                case "SendConnectionTables":
                    GUIWindow.PrintLog("RC: Received SendConnectionTables(" + data["path"] + ", " + data["connID"] + ") from CC");
                    GUIWindow.PrintLog("RC: Sending connection tables to Routers");
                    currentConnectionID = Convert.ToInt32(data["connID"]);
                    UpdateRoutingTables(Program.cc.cachedPath, currentConnectionID, false, false);
                    UpdateRoutingTables(Program.cc.cachedPath, currentConnectionID, true, false);

                    GUIWindow.PrintLog("RC: Sent SendConnectionTablesResponse() to CC");
                    Program.cc.HandleRequest(Util.DecodeRequest("component:CC;name:SendConnectionTablesResponse"));
                    break;
            }
        }

        private void CacheChannels(string data) {
            string[] pieces = data.Split('-');
            foreach(string link in pieces) {
                string[] keyAndValue = link.Split('=');
                try {
                    cachedChannels.Add(Convert.ToInt32(keyAndValue[0]), stringToIntArray(keyAndValue[1]));
                } catch(Exception) {
                    cachedChannels[Convert.ToInt32(keyAndValue[0])] = stringToIntArray(keyAndValue[1]);
                }
            }

        }

        private int[] stringToIntArray(string data) {
            string[] channels = data.Split(',');
            int[] parsedData = new int[channels.Length];
            for(int i=0; i< channels.Length; i++) {
                parsedData[i] = Convert.ToInt32(channels[i] + "");
            }
            return parsedData;
        }

        private void LoadMyChannels() {
            foreach (Connection conn in ConfigLoader.myConnections.Values) {
                try {
                    cachedChannels.Add(conn.GetID(), conn.GetSlot());
                } catch (Exception) {
                    cachedChannels[conn.GetID()] = conn.GetSlot();
                }
            }
            try {
                cachedChannels.Add(8, ConfigLoader.connections[8].GetSlot());
            } catch(Exception) {
                cachedChannels[8] = ConfigLoader.connections[8].GetSlot();
            }
        }

        private void CalculateAllPaths() {
            Node startingNode = ConfigLoader.GetNodeByIP(cachedData["routerX"]);
            Node endingNode = ConfigLoader.GetNodeByIP(cachedData["routerY"]);
            int speed = Convert.ToInt32(cachedData["speed"]);
            Host startHost = null;
            Host endingHost = null;
            foreach(Host h in ConfigLoader.hosts) {
                if (startingNode == h.GetNeighborRouter()) startHost = h;
                if (endingNode == h.GetNeighborRouter()) endingHost = h;   
            }

            List<Path> paths = Algorithms.AllPaths(startHost, endingHost);
            if(paths == null || paths.Count == 0) {
                //nie istnieje żadna ścieżka
                GUIWindow.PrintLog("RC: Sent RouteTableQueryResponse(null) to CC");
                Program.cc.HandleRequest(Util.DecodeRequest("component:CC;name:RouteTableQueryResponse;path:null"));
                return;
            }

            Path chosenPath = null;
            foreach (Path path in paths) {
                int[] pathsID = path.getEdges();
                int n = DetermineGapNo(speed, path.GetLength());
                //GUIWindow.PrintLog("Channel count: " + n);
                int[,] LRMarray = GetLRMarray(pathsID);
                int[] indexFromTo = EvaluatePath(n, LRMarray);
                if (indexFromTo[0] != -1 && indexFromTo[1] != -1) {
                    //GUIWindow.PrintLog("Test: n: " + n + " index from: " + indexFromTo[0] + " to " + indexFromTo[1]);
                    try {
                        cachedData.Add("channelRange", indexFromTo[0] + "-" + indexFromTo[1]);
                    } catch(Exception) {
                        cachedData["channelRange"] = indexFromTo[0] + "-" + indexFromTo[1];
                    }
                    chosenPath = path;
                    break;
                }
            }
            currentPath = chosenPath;

            //GUIWindow.PrintLog("Path from Router" + startingNode.GetRouterID() + " to Router" + endingNode.GetRouterID() + " at " + speed + " Gb/s");
            if(chosenPath == null) {
                GUIWindow.PrintLog("RC: Sent RouteTableQueryResponse(null) to CC");
                Program.cc.HandleRequest(Util.DecodeRequest("component:CC;name:RouteTableQueryResponse;path:null"));
                return;
            }
            SendTablesToOwnRouters();

            int nextConnectionID = currentConnectionID + 1;
            if(Convert.ToBoolean(cachedData["IDC"])) {
                nextConnectionID = Math.Max(nextConnectionID, Convert.ToInt32(cachedData["peerConnID"]) + 1);
            }
            currentConnectionID = nextConnectionID;

            GUIWindow.PrintLog("RC: Sending connection tables to Routers");
            UpdateRoutingTables(chosenPath, currentConnectionID, false, false);
            UpdateRoutingTables(chosenPath, currentConnectionID, true, false);

            string pathString = (chosenPath == null ? "null" : chosenPath.ToString());
            string message = "component:CC;name:RouteTableQueryResponse;path:" + pathString;
            GUIWindow.PrintLog("RC: Sent RouteTableQueryResponse(" + pathString + ") to CC");
            Program.cc.HandleRequest(Util.DecodeRequest(message));
        }

        private void SendTablesToOwnRouters() {
            foreach(int routerID in currentPath.routerIDs) {
                //Router router = ConfigLoader.FindRouterByID(routerID);
                //if (router == null)
                //    continue;

                foreach(RouterConnection routerConnection in Server.GetRouterConnections()) {
                    if(routerConnection.GetID() == routerID) {

                    }
                }
            }
        }
       
        public void UpdateRoutingTables(Path path, int pathID, bool reverse, bool disconect) {

            LinkedList<Connection> edges = new LinkedList<Connection>(path.edges);

            if (reverse) {
                List<Connection> toReverse = new List<Connection>(edges);
                toReverse.Reverse();
                edges = new LinkedList<Connection>(toReverse);
            }

            Connection firstEdge = edges.First.Value;
            Router firstRouter = null;
            if (firstEdge.endPoints.Item1 is Router)
                firstRouter = (Router)firstEdge.endPoints.Item1;
            else
                firstRouter = (Router)firstEdge.endPoints.Item2;

            edges.RemoveFirst();
            foreach (Connection edge in edges) {
                try {
                    int port;
                    if (edge.endPoints.Item1 == firstRouter) {
                        port = disconect ? -1 : edge.connPorts.Item1;
                        FindRouterConnectionAndSend(firstRouter, pathID, port, !reverse);
                        firstRouter = (Router)edge.endPoints.Item2;
                    } else {
                        port = disconect ? -1 : edge.connPorts.Item2;
                        FindRouterConnectionAndSend(firstRouter, pathID, port, !reverse);
                        firstRouter = (Router)edge.endPoints.Item1;
                    }
                } catch (Exception e) {
                    //olej i oliwa

                }
            }

            //var uniqueValues = savedLogs.GroupBy(pair => pair.Value)
            //             .Select(group => group.First())
            //             .ToDictionary(pair => pair.Key, pair => pair.Value);
            //savedLogs = uniqueValues;
        }

        private void FindRouterConnectionAndSend(Router router, int id, int port, bool log) {
            foreach (RouterConnection routerConnection in Server.GetRouterConnections()) {
                if (routerConnection.router == router) {
                    Dictionary<int, int> routingTable = new Dictionary<int, int>();
                    routingTable.Add(id, port);
                    routerConnection.SendRoutingTable(routingTable);
                    if(log) {
                        GUIWindow.PrintLog("RC: Sent connection table to Router #" + router.GetRouterID());
                    }
                    break;
                }
            }
        }
  
        public int DetermineGapNo(double linkBandwidth, double pathLen) {
            double gapLen = 12.5;
            int modulation;
            if (pathLen > 400) modulation = 2;
            else if (pathLen > 300) modulation = 3;
            else if (pathLen > 200) modulation = 4;
            else if (pathLen > 100) modulation = 5;
            else modulation = 6;
            double band = (linkBandwidth * 2) / modulation;
            band *= 1.2; //tu dodajemy zapas - margines ochronny 20%, po 10% z obu stron
            int n = (int)Math.Ceiling(band / gapLen);
            //GUIWindow.PrintLog("DetermineGapNo n: " + n);
            return n;
        }

        public int[,] GetLRMarray(int[] connID) {
            int[,] LRMarray = new int[connID.Length, 90];
            int n = 0;
            try {
                foreach (int key in connID) {
                    int[] buff = cachedChannels[key];
                    
                    for (int j = 0; j < 90; j++) {
                        LRMarray[n, j] = buff[j];
                    }
                    n++;

                }
            } catch(Exception e) {
                GUIWindow.PrintLog(e.Message);
                GUIWindow.PrintLog(e.StackTrace);
            }

            return LRMarray;
        }

        public int[] EvaluatePath(int n, int[,] LRMarray) {
            int[] indexFromTo = { -1, -1 };
            int[] LRMafterEvaluation = new int[LRMarray.GetLength(1)];
            for (int i = 0; i < LRMarray.GetLength(1); i++) {
                int sum = 0;
                for (int j = 1; j < LRMarray.GetLength(0); j++) {
                    sum += LRMarray[j, i];
                }
                if (sum == 0) {
                    LRMafterEvaluation[i] = 1;
                } else {
                    LRMafterEvaluation[i] = 0;
                }
            }

            int check;
            bool pathFound = false;

            if (n == 1) {
                for (int i = 0; i < LRMafterEvaluation.Length; i++) {
                    if (LRMafterEvaluation[i] == 0) continue;
                    indexFromTo[0] = i;
                    indexFromTo[1] = i;
                    return indexFromTo;
                }
            }

            if (n > 90) {
                indexFromTo[0] = -1;
                indexFromTo[1] = -1;
                return indexFromTo;
            }

            for (int i = 0; i < LRMafterEvaluation.Length; i++) {
                if (pathFound == true || i + n > LRMafterEvaluation.Length) break;
                if (LRMafterEvaluation[i] == 0) continue;
                check = 1;
                for (int j = i + 1; j < i + n; j++) {
                    if (LRMafterEvaluation[j] == 0) break;
                    check++;
                    if (check == n) {
                        indexFromTo[0] = j + 1 - n;
                        indexFromTo[1] = j;
                        pathFound = true;
                        break;
                    }
                }
            }
            return indexFromTo;
        }

        private void UpdateConnectionGap(int[] connID, int[] indexFromTo, int LinkConnID) {
            foreach (int key in connID) {
                for (int j = indexFromTo[0]; j <= indexFromTo[1]; j++) {
                    ConfigLoader.connections[key].slot[j] = LinkConnID;
                }
            }
        }

        public void ClearConnectionGaps(int LinkConnID) {
            for (int i = 1; i <= ConfigLoader.connections.Count; i++) {
                for (int j = 0; j < 90; j++) {
                    if (ConfigLoader.connections[i].slot[j] == LinkConnID) ConfigLoader.connections[i].slot[j] = 0;
                }
            }
        }

        /*
        private bool isInternal(String startIP, String endIP) {

            bool startMatch = false;
            bool endMatch = false;
            foreach (Host h in ConfigLoader.GetHosts()) {
                if (h.getIP().Equals(startIP) && h.GetAsID() == this.getConnectonControl().GetNCC().getAsID()) {
                    startMatch = true;
                }
                if (h.getIP().Equals(endIP) && h.GetAsID() == this.getConnectonControl().GetNCC().getAsID()) {
                    endMatch = true;
                }
            }

            if (startMatch && endMatch) {
                return true;
            }
            else {
                return false;
            }

        }

        private void extractSubnetInfo(Path path, bool internalConnection) {
            if (internalConnection) {
                subnetPair = null;
            }
            else {

                LinkedList<Router> routers = new LinkedList<Router>();

                foreach (Router r in path.pathRouters) {
                    if (r.GetSubnetworkRouter()) {
                        routers.AddLast(r);
                    }
                }

                Router start = routers.First.Value;
                Router end = routers.Last.Value;

                if (start != null && end != null) {
                    subnetPair = new Tuple<Node, Node>(start, end);
                }
                else {
                    subnetPair = null;
                }
            }
        }
         
         */
    }
}
