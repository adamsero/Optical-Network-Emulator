using System;
using System.Collections.Generic;
using System.Linq;

namespace ControlCenter {
    class RouteControl {

        private ConnectionControl cc;
        private LRM lrm = null;
        private RouteControl otherRC = null;
        public static Dictionary<int, Path> LinkConnID = new Dictionary<int, Path>();
        public static Tuple<Node, Node> subnetPair;
        public static List<Tuple<string, string>> savedLogs = new List<Tuple<string, string>>();

        public int GetLastPathIndex() {
            return Program.lastPathIndex - 1;
        }

        public RouteControl(ConnectionControl cc) {
            this.cc = cc;
        }

        public void setOtherRC() {
            otherRC = this.cc.GetNCC().getOtherNCC().getConnectionControl().GetRouteControl();
        }

        public ConnectionControl getConnectonControl() {
            return cc;
        }

        public void setLRM(LRM lrm) {
            this.lrm = lrm;
        }

        public void setOtherRC(RouteControl otherRC) {
            this.otherRC = otherRC;
        }

        public void HandleRouteTableQuery(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool afterPeerCoordination, bool childLevel) {
            GUIWindow.PrintLog("RC: Received request: RouteTableQuery()", getConnectonControl().GetNCC().getAsID());
            GUIWindow.PrintLog("RC: Sending request: LocalTopology() to internal LRM", getConnectonControl().GetNCC().getAsID());
            lrm.HandleLocalTopology(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination, childLevel);
        }

        public void HandleLocalTopologyResponse(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool afterPeerCoordination, bool childLevel) {
            GUIWindow.PrintLog("RC: Received request: LocalTopologyResponse() from internal LRM", getConnectonControl().GetNCC().getAsID());

            if (!afterPeerCoordination && !childLevel) {
                GUIWindow.PrintLog("RC: Sending request: NetworkTopology() to other AS RC", getConnectonControl().GetNCC().getAsID());
                otherRC.HandleNetworkTopology(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination, childLevel);
            }
            else {
                //tak naprawde tu nie ma response -- nie pytamy sasiada -- przechodzimy dalej do zwrotki z route table query
                this.HandleNetworkTopologyResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination, childLevel);
            }
        }

        public void HandleNetworkTopology(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool afterPeerCoordination, bool childLevel) {
            GUIWindow.PrintLog("RC: Received request: NetworkTopology() from other AS RC", getConnectonControl().GetNCC().getAsID());
            GUIWindow.PrintLog("RC: Sending request: NetworkTopologyResponse() to other AS RC", getConnectonControl().GetNCC().getAsID());
            otherRC.HandleNetworkTopologyResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination, childLevel);
        }

        public void HandleNetworkTopologyResponse(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool afterPeerCoordination, bool childLevel) {

            if ((!afterPeerCoordination || (afterPeerCoordination && isInternal(startIP, endIP))) & !childLevel) {
                GUIWindow.PrintLog("RC: Received request: NetworkTopologyResponse() from other AS RC", getConnectonControl().GetNCC().getAsID());
                if (!LinkConnID.TryGetValue(Program.lastPathIndex, out Path asdf)) {

                    Host initial = null;
                    Host final = null;

                    foreach (Host h in ConfigLoader.hosts) {
                        if (h.getIP().Equals(startIP)) initial = h;
                        if (h.getIP().Equals(endIP)) final = h;
                    }

                    List<Path> paths = Algorithms.AllPaths(initial, final);

                    foreach (Path path in paths) {
                        int[] pathsID = path.getEdges();
                        int n = DetermineGapNo(linkSpeed, path.GetLength());  //skad wziac dlugosc sciezki?
                        int[,] LRMarray = GetLRMarray(pathsID);
                        int[] indexFromTo = EvaluatePath(n, LRMarray);
                        if (indexFromTo[0] == -1 && indexFromTo[1] == -1) {
                            Program.ConnectionAvaliable = false;
                        }
                        else {
                            extractSubnetInfo(path, isInternal(startIP, endIP));
                            Program.ConnectionAvaliable = true;
                            LinkConnID.Add(Program.lastPathIndex, path);
                            UpdateConnectionGap(pathsID, indexFromTo, Program.lastPathIndex);
                            GUIWindow.UpdateChannelTable();
                            UpdateRoutingTables(path, Program.lastPathIndex, false, false);
                            UpdateRoutingTables(path, Program.lastPathIndex, true, false);
                            Program.lastPathIndex++;
                            break;
                        }
                    }
                }
                else {
                    if (Program.ConnectionAvaliable && (!afterPeerCoordination || isInternal(startIP, endIP))) {
                        //Program.lastPathIndex++; 
                    }
                }
            }

            // ============= ROUTING ===============

            // =====================================

            GUIWindow.PrintLog("RC: Sending request: RouteTableQueryResponse()", getConnectonControl().GetNCC().getAsID());
            //TODO: tutaj mozna printowac sciezke czy cos, nie wiem czy trzeba
            cc.HandleRouteTableQueryResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination);

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
                        FindRouterConnectionAndSend(firstRouter, pathID, port);
                        firstRouter = (Router)edge.endPoints.Item2;
                    } else {
                        port = disconect ? -1 : edge.connPorts.Item2;
                        FindRouterConnectionAndSend(firstRouter, pathID, port);
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

        private void FindRouterConnectionAndSend(Router router, int id, int port) {
            foreach (RouterConnection routerConnection in Server.GetRouterConnections()) {
                if (routerConnection.router == router) {
                    Dictionary<int, int> routingTable = new Dictionary<int, int>();
                    routingTable.Add(id, port);
                    routerConnection.SendRoutingTable(routingTable);

                    break;
                }
            }
        }

        //metoda wyznaczajaca ilosc potrzebnych szczelin
        //na podstawie dlugosci calego polaczenia i wymaganej przepustowosci
        public int DetermineGapNo(double linkBandwidth, double pathLen) {
            int maxM = 5;   //tu wyznaczamy maksymalna modulacje - tu 5 dla QAM32
            int minD = 20;
            int maxD = 433; //tu wskazujemy najdluzsza mozliwa sciezke w sieci
            double gapLen = 12.5;
            //double modulation = Math.Round( (maxM - 1) / (minD - maxD) * pathLen + 1 - ((maxM - 1) * maxD) / (minD - maxD) );
            int modulation;
            if (pathLen > 400) modulation = 2;
            else if (pathLen > 300) modulation = 3;
            else if (pathLen > 200) modulation = 4;
            else if (pathLen > 100) modulation = 5;
            else modulation = 6;

            double band = (linkBandwidth * 2) / modulation;
            band *= 1.2; //tu dodajemy zapas - margines ochronny 20%, po 10% z obu stron
            //GUIWindow.PrintLog("Band: " + band);
            int n = (int)Math.Ceiling(band / gapLen);
            //GUIWindow.PrintLog("PathLen: " + pathLen + " channels" + n);
            return n;
        }

        //przykładowa LRMarray, normalnie do wczytania od poszczególnych LRM'ów
        //int[,] LRMarray = new int[,] {  { 1,1,0,0,1,1,0,0,0 },
        //                                { 0,0,0,0,1,1,0,0,0 },
        //                                { 0,0,0,0,1,1,1,1,1 },
        //                                { 1,1,0,1,0,0,0,0,0 },
        //                                { 1,1,0,1,0,0,0,0,0 }};

        public int[,] GetLRMarray(int[] connID) {
            int[,] LRMarray = new int[connID.Length, 90];
            int n = 0;
            foreach (int key in connID) {
                for (int j = 0; j < 90; j++) {
                    LRMarray[n, j] = ConfigLoader.connections[key].slot[j];
                }
                n++;
            }
            return LRMarray;
        }

        //metoda wyznaczajaca indeksy szczelin - zwraca tablice int dwoma wartosciam
        //od jakiego do jakiego indeksu sa wolne szczeliny do wykoszystania
        //obsługuje tylko tablice LRM o tej samej ilości szczelin!!! wiec trzeba przed
        //wywolaniem wyrownac wszystkie tablice do jednej dlugosci (najmniejszej)
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
    }
}
