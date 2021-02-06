using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class RC {

        public void HandleRequest(Dictionary<string, string> data) {
            switch(data["name"]) {
                case "NetworkTopology":
                    switch(data["receiver"]) {
                        case "peer":
                            GUIWindow.PrintLog("RC: Received NetworkTopology() from peer RC");
                            string message1 = "component:RC;name:NetworkTopology;receiver:child";
                            
                            if(ConfigLoader.ccID == 2) {
                                GUIWindow.PrintLog("RC: Sent NetworkTopology() to child RC");
                                Program.parentConnection.SendMessage(message1);
                            } else {
                                //tutaj NetworkTopologyResponse do peera
                                SendPeerNetworkTopologyResponse("Routers = ", " Connections = ");
                            }
                            break;

                        case "child":
                            GUIWindow.PrintLog("RC: Received NetworkTopology() from parent RC");
                            string routersList = "Routers = ";
                            foreach (Router router in ConfigLoader.myRouters)
                                routersList += router.GetRouterID() + ", ";
                            routersList = routersList.Remove(routersList.Length - 2, 2);

                            string linksList = " Connections = ";
                            foreach (Connection connection in ConfigLoader.myConnections.Values)
                                linksList += connection.endPoints.Item1.GetHostID() + "-" + connection.endPoints.Item2.GetHostID() + ", ";
                            linksList = linksList.Remove(linksList.Length - 2, 2);

                            GUIWindow.PrintLog("RC: Sent NetworkTopologyResponse(" + routersList + linksList + ") to parent RC");
                            message1 = "component:RC;name:NetworkTopologyResponse;receiver:parent;routersList:" + routersList + ";linksList:" + linksList;
                            Program.childConnection.SendMessage(message1);
                            break;
                    }

                    break;

                case "NetworkTopologyResponse":
                    switch (data["receiver"]) {
                        case "parent":
                            GUIWindow.PrintLog("RC: Received NetworkTopologyResponse(" + data["routersList"] + data["linksList"] + ") from child RC");
                            if (Program.ncc.lastIDC) {
                                //tutaj NetworkTopologyResponse do peera
                                SendPeerNetworkTopologyResponse(data["routersList"] + ", ", data["linksList"] + ", ");
                            } else {
                                //polaczenie lokalne w ASie 2gim
                            }
                            break;

                        case "peer":
                            GUIWindow.PrintLog("RC: Received NetworkTopologyResponse(" + data["routersList"] + data["hostList"] + data["linksList"] + ") from peer RC");
                            break;
                    }
                    break;

                case "RouteTableQuery":
                    GUIWindow.PrintLog("RC: Received RouteTableQuery(" + data["routerX"] + ", " + data["routerY"] + ", " + data["speed"] + " Gb/s, InterDomainConnection: " + data["IDC"] + ") from CC");

                    if (Convert.ToBoolean(data["IDC"])) {
                        string message = "component:RC;name:NetworkTopology;receiver:peer";
                        GUIWindow.PrintLog("RC: Sent NetworkTopology() to peer RC");
                        Program.peerConnection.SendMessage(message);
                    } else if(ConfigLoader.ccID == 2) {
                        string message = "component:RC;name:NetworkTopology;receiver:child";
                        GUIWindow.PrintLog("RC: Sent NetworkTopology() to child RC");
                        Program.parentConnection.SendMessage(message);
                    }

                    //string startingNode, endingNode;
                    //if(!Convert.ToBoolean(data["IDC"])) {
                    //    startingNode = data["routerX"].Remove(data["routerX"].Length - 1, 1) + "2";
                    //    endingNode = data["routerY"].Remove(data["routerY"].Length - 1, 1) + "2";
                    //} else {
                    //    if(data["case"].Equals("FirstAS")) {
                    //        startingNode = data["routerX"].Remove(data["routerX"].Length - 1, 1) + "2";
                    //        endingNode = maxRouteReach.getIP();
                    //    } else if(data["case"].Equals("SecondAS")) {
                    //        startingNode = maxRouteReach.getIP();
                    //        endingNode = data["routerY"].Remove(data["routerY"].Length - 1, 1) + "2";
                    //    } else {
                    //        startingNode = data["routerX"];
                    //        endingNode = data["routerY"];
                    //    }
                    //}

                    //GUIWindow.PrintLog(startingHost + "   " + endingNode);

                    //List<Path> allPaths = Algorithms.AllPaths(ConfigLoader.GetNodeByIP(startingNode), ConfigLoader.GetNodeByIP(endingNode));
                    //foreach (Path path in allPaths)
                    //    path.PrintPath();

                    //Path shortest = allPaths[0];

                    break;
            }
        }

        private void SendPeerNetworkTopologyResponse(string routersList, string linksList) {
            string hostList = " Hosts = ";
            foreach (Host host in ConfigLoader.myHosts)
                hostList += host.GetHostID() + ", ";
            hostList = hostList.Remove(hostList.Length - 2, 2);

            foreach (Router router in ConfigLoader.myRouters)
                routersList += router.GetRouterID() + ", ";
            routersList = routersList.Remove(routersList.Length - 2, 2);

            foreach (Connection connection in ConfigLoader.myConnections.Values)
                linksList += connection.endPoints.Item1.GetHostID() + "-" + connection.endPoints.Item2.GetHostID() + ", ";
            linksList = linksList.Remove(linksList.Length - 2, 2);

            GUIWindow.PrintLog("RC: Sent NetworkTopologyResponse(" + routersList + hostList + linksList + ") to peer RC");
            string message = "component:RC;name:NetworkTopologyResponse;receiver:peer;routersList:" + routersList + ";linksList:" + linksList + ";hostList:" + hostList;
            Program.peerConnection.SendMessage(message);
        }

    }
}
