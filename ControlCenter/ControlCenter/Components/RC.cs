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
                                //tutaj NetworkTopologyResponse
                            }
                            break;

                        case "child":
                            GUIWindow.PrintLog("RC: Received NetworkTopology() from parent RC");
                            //NetworkTopologyResponse do parenta
                            break;
                    }

                    break;

                case "RouteTableQuery":
                    GUIWindow.PrintLog("RC: Received RouteTableQuery(" + data["routerX"] + ", " + data["routerY"] + ", " + data["speed"] + " Gb/s, InterDomainConnection: " + data["IDC"] + ") from CC");

                    if (!Convert.ToBoolean(data["IDC"])) {
                        string message = "component:RC;name:NetworkTopology;receiver:peer";
                        GUIWindow.PrintLog("RC: Sent NetworkTopology() to peer RC");
                        Program.peerConnection.SendMessage(message);
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

    }
}
