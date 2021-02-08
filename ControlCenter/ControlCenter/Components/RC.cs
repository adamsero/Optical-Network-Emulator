using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class RC {

        private readonly Dictionary<string, string> cachedData = new Dictionary<string, string>();
        private Dictionary<int, int[]> cachedChannels = new Dictionary<int, int[]>();

        public void HandleRequest(Dictionary<string, string> data) {
            switch(data["name"]) {
                case "NetworkTopology":
                    switch(data["receiver"]) {
                        case "peer":
                            GUIWindow.PrintLog("RC: Received NetworkTopology() from peer RC");
                            
                            if(data["scenario"].Equals("1")) {
                                //scenariusz #1
                                GUIWindow.PrintLog("RC: Sent NetworkTopology() to child RC");
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
                                    channelList2 += connection.GetID() + ":" + string.Join("", connection.GetSlot()) + "-";
                                }                                    
                                linksList2 = linksList2.Remove(linksList2.Length - 2, 2);
                                channelList2 = channelList2.Remove(channelList2.Length - 1, 1);

                                GUIWindow.PrintLog("RC: Sent NetworkTopologyResponse(" + routersList2 + hostList + linksList2 + ") to peer RC");
                                string message = "component:RC;name:NetworkTopologyResponse;receiver:peer;routersList:" + routersList2 + ";linksList:" + linksList2 + ";hostList:" + hostList + ";scenario:" + data["scenario"] + ";channels:" + channelList2;
                                Program.peerConnection.SendMessage(message);
                            }
                            break;

                        case "child":
                            //scenariusz #1, 2 lub 3
                            GUIWindow.PrintLog("RC: Received NetworkTopology() from parent RC");
                            string routersList = "Routers = ";
                            foreach (Router router in ConfigLoader.myRouters)
                                routersList += router.GetRouterID() + ", ";
                            routersList = routersList.Remove(routersList.Length - 2, 2);

                            string linksList = " Connections = ";
                            string channelList = "";
                            foreach (Connection connection in ConfigLoader.myConnections.Values) {
                                linksList += connection.endPoints.Item1.GetHostID() + "-" + connection.endPoints.Item2.GetHostID() + ", ";
                                channelList += connection.GetID() + ":" + string.Join("", connection.GetSlot()) + "-";
                            }
                            linksList = linksList.Remove(linksList.Length - 2, 2);
                            channelList = channelList.Remove(channelList.Length - 1, 1);

                            GUIWindow.PrintLog("RC: Sent NetworkTopologyResponse(" + routersList + linksList + ") to parent RC");
                            string message1 = "component:RC;name:NetworkTopologyResponse;receiver:parent;routersList:" + routersList + ";linksList:" + linksList + ";scenario:" + data["scenario"] + ";channels:" + channelList;
                            Program.childConnection.SendMessage(message1);
                            break;
                    }

                    break;

                case "NetworkTopologyResponse":
                    switch (data["receiver"]) {
                        case "parent":
                            GUIWindow.PrintLog("RC: Received NetworkTopologyResponse(" + data["routersList"] + data["linksList"] + ") from child RC");
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
                                string channelList = data["channels"] + "";
                                foreach (Connection connection in ConfigLoader.myConnections.Values) {
                                    linksList += connection.endPoints.Item1.GetHostID() + "-" + connection.endPoints.Item2.GetHostID() + ", ";
                                    channelList += connection.GetID() + ":" + string.Join("", connection.GetSlot()) + "-";
                                }
                                linksList = linksList.Remove(linksList.Length - 2, 2);
                                channelList = channelList.Remove(channelList.Length - 1, 1);

                                GUIWindow.PrintLog("RC: Sent NetworkTopologyResponse(" + routersList + hostList + linksList + ") to peer RC");
                                string message = "component:RC;name:NetworkTopologyResponse;receiver:peer;routersList:" + routersList + ";linksList:" + linksList + ";hostList:" + hostList + ";scenario:" + data["scenario"] + ";channels:" + channelList;
                                Program.peerConnection.SendMessage(message);
                            } else if(data["scenario"].Equals("2")) {
                                //scenariusz #2
                                GUIWindow.PrintLog("RC: Sent NetworkTopology() to peer RC");
                                CacheChannels(data["channels"]);
                                string message = "component:RC;name:NetworkTopology;receiver:peer;routersList:" + data["routersList"] + ";linksList:" + data["linksList"] + ";scenario:" + data["scenario"];
                                Program.peerConnection.SendMessage(message);
                            }
                            else if (data["scenario"].Equals("3")) {
                                //koniec scenariusza #3
                                CacheChannels(data["channels"]);
                                foreach (Connection conn in ConfigLoader.myConnections.Values) {
                                    cachedChannels.Add(conn.GetID(), conn.GetSlot());
                                }
                                CalculateAllPaths();
                            }
                            break;

                        case "peer":
                            GUIWindow.PrintLog("RC: Received NetworkTopologyResponse(" + data["routersList"] + data["hostList"] + data["linksList"] + ") from peer RC");
                            if (data["scenario"].Equals("1")) {
                                //koniec scenariusza #1
                                CacheChannels(data["channels"]);
                                foreach(Connection conn in ConfigLoader.myConnections.Values) {
                                    cachedChannels.Add(conn.GetID(), conn.GetSlot());
                                }
                                CalculateAllPaths();
                            } else if (data["scenario"].Equals("2")) {
                                //koniec scenariusza #2
                                CacheChannels(data["channels"]);
                                foreach (Connection conn in ConfigLoader.myConnections.Values) {
                                    cachedChannels.Add(conn.GetID(), conn.GetSlot());
                                }
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

                    if (ConfigLoader.ccID == 1) {
                        if(Convert.ToBoolean(data["IDC"])) {
                            //scenariusz #1
                            string message = "component:RC;name:NetworkTopology;receiver:peer;scenario:1";
                            GUIWindow.PrintLog("RC: Sent NetworkTopology() to peer RC");
                            Program.peerConnection.SendMessage(message);
                        } else {
                            //scenariusz #4
                        }
                    } else if(ConfigLoader.ccID == 2) {
                        if (Convert.ToBoolean(data["IDC"])) {
                            //scenariusz #2
                            string message = "component:RC;name:NetworkTopology;receiver:child;scenario:2";
                            GUIWindow.PrintLog("RC: Sent NetworkTopology() to child RC");
                            Program.parentConnection.SendMessage(message);
                        }
                        else {
                            //scenariusz #3
                            string message = "component:RC;name:NetworkTopology;receiver:child;scenario:3";
                            GUIWindow.PrintLog("RC: Sent NetworkTopology() to child RC");
                            Program.parentConnection.SendMessage(message);
                        }
                    }
                    //scenariusz #4
                    foreach (Connection conn in ConfigLoader.myConnections.Values) {
                        cachedChannels.Add(conn.GetID(), conn.GetSlot());
                    }
                    CalculateAllPaths();
                    break;
            }
        }

        private void CacheChannels(string data) {
            string[] pieces = data.Split('-');
            foreach(string link in pieces) {
                string[] keyAndValue = link.Split(':');
                cachedChannels.Add(Convert.ToInt32(keyAndValue[0]), stringToIntArray(keyAndValue[1]));
            }
        }

        private int[] stringToIntArray(string data) {
            int[] parsedData = new int[data.Length];
            for(int i=0; i<data.Length; i++) {
                parsedData[i] = Convert.ToInt32(data[i]);
            }
            return parsedData;
        }

        private void CalculateAllPaths() {
            Node startingNode = ConfigLoader.GetNodeByIP(cachedData["routerX"]);
            Node endingNode = ConfigLoader.GetNodeByIP(cachedData["routerY"]);
            int speed = Convert.ToInt32(cachedData["speed"]);

            GUIWindow.PrintLog("Path from Router" + startingNode.GetRouterID() + " to Router" + endingNode.GetRouterID() + " at " + speed + " Gb/s");
        }

    }
}
