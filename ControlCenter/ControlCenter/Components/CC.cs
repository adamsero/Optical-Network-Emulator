using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class CC {

        public Path cachedPath;
        private string cachedMessage;
        private int cachedScenario;

        public void HandleRequest(Dictionary<string, string> data) {
            switch (data["name"]) {
                case "ConnectionRequest":
                    switch (data["layer"]) {
                        case "upper":
                            GUIWindow.PrintLog("CC: Received ConnectionRequest(" + data["routerX"] + ", " + data["routerY"] + ", " + data["speed"] + " Gb/s, InterDomainConnection: " + data["IDC"] + ") from NCC");

                            string networkCase = ConfigLoader.ccID == 3 ? "SN" : "FirstAS";
                            string message420 = "component:RC;name:RouteTableQuery;routerX:" + data["routerX"] + ";routerY:" + data["routerY"] + ";speed:" + data["speed"] + ";IDC:" + data["IDC"] + ";case:" + networkCase;
                            GUIWindow.PrintLog("CC: Sent RouteTableQuery(" + data["routerX"] + ", " + data["routerY"] + ", " + data["speed"] + " Gb/s, InterDomainConnection: " + data["IDC"] + ") to RC");
                            Program.rc.HandleRequest(Util.DecodeRequest(message420));
                            break;

                        case "lower":
                            GUIWindow.PrintLog("CC: Received ConnectionRequest(routerX: " + data["routerX"] + ", routerY: " + data["routerY"] + ", path:" + data["path"] + ") from parent CC");
                            List<int> routerID2 = new List<int>();
                            foreach (string id in data["path"].Split('-')) {
                                routerID2.Add(Convert.ToInt32(id));
                            }
                            Host hostX2 = ConfigLoader.FindHostAmongAll(Convert.ToInt32(data["endPoint1"]));
                            Host hostY2 = ConfigLoader.FindHostAmongAll(Convert.ToInt32(data["endPoint2"]));
                            cachedPath = new Path(routerID2, hostX2, hostY2);
                            cachedPath.channelRange = data["channelRange"];
                            Tuple<HostConnection, HostConnection> connections2 = GetHostConnections(cachedPath);
                            Call call2 = new Call(Convert.ToInt32(data["connID"]), true, (ConfigLoader.ccID == 1 ? 2 : 1), cachedPath.throughSN, connections2.Item1, connections2.Item2, cachedPath);
                            NCC.callRegister.Add(Convert.ToInt32(data["connID"]), call2);

                            //GUIWindow.PrintLog("CC: Sent SendConnectionTables(" + data["path"] + ", " + data["connID"] + ") to RC");
                            Program.rc.HandleRequest(Util.DecodeRequest("name:SendConnectionTables;connID:" + data["connID"] + ";path:" + data["path"]));
                            break;
                    }
                    break;

                case "ConnectionRequestResponse":
                    GUIWindow.PrintLog("CC: Received ConnectionRequestResponse() from child CC");
                    if (cachedScenario == 1) {
                        //scenariusz 1
                        GUIWindow.PrintLog("CC: Sent PeerCoordinationResponse() to peer CC");
                        Program.peerConnection.SendMessage("component:CC;name:PeerCoordinationResponse");
                    } else {
                        //scenariusz 2
                        var decoded = Util.DecodeRequest(cachedMessage);
                        GUIWindow.PrintLog("CC: Sent PeerCoordination(routerX: " + decoded["routerX"] + ", routerY: " + decoded["routerY"] + ", path:" + decoded["path"] + ") to peer CC");
                        Program.peerConnection.SendMessage(cachedMessage);
                    }
                    break;

                case "RouteTableQueryResponse":
                    //GUIWindow.PrintLog("Received RouteTableQueryResponse(" + data["path"] + ") from RC");
                    if (data["path"].Equals("null")) {
                        GUIWindow.PrintLog("CC: Requested connection could not be established");
                        string message1 = "component:CC;name:ConnectionRequestResponse;succeeded:false";
                        GUIWindow.PrintLog("CC: Sent ConnectionRequestResponse(SUCCEEDED: false) to NCC");
                        Program.ncc.HandleRequest(Util.DecodeRequest(message1), null);
                        break;
                    }

                    //tutaj wysyłamy LRMom request o alokacje, potem PeerCoordination jeśli to połączenie IDC
                    bool throughSN = ConfigLoader.ccID == 1 ? false : RC.currentPath.throughSN;
                    Tuple<HostConnection, HostConnection> connections = GetHostConnections(RC.currentPath);
                    Call call = new Call(RC.currentConnectionID, Convert.ToBoolean(RC.cachedData["IDC"]), ConfigLoader.ccID, throughSN, connections.Item1, connections.Item2, RC.currentPath);
                    NCC.callRegister.Add(RC.currentConnectionID, call);

                    //GUIWindow.PrintLog("CC: Sent LinkConnectionRequest(" + RC.cachedData["channelRange"] + ") to internal LRM");
                    string message2 = "name:LinkConnectionRequest;type:internal;channelRange:" + RC.cachedData["channelRange"] + ";asType:first";
                    Program.lrm.HandleRequest(Util.DecodeRequest(message2));

                    break;

                case "LinkConnectionRequestResponse":
                    switch (data["type"]) {
                        case "internal":
                            //GUIWindow.PrintLog("CC: Received LinkConnectionRequestResponse() from internal LRM");

                            if (data["asType"].Equals("second")) {
                                if (cachedPath.throughSN && ConfigLoader.ccID == 2) {
                                    //tu jeszcze external LinkConnectionRequest
                                    string message3 = "name:LinkConnectionRequest;type:external;respond:false;channelRange:" + cachedPath.channelRange;
                                    Program.lrm.HandleRequest(Util.DecodeRequest(message3));
                                    GUIWindow.PrintLog("External LRM: Sent LinkConnectionRequestResponse() to CC");
                                    GUIWindow.PrintLog("CC: Received LinkConnectionRequestResponse() from External LRM");

                                    var decoded = Util.DecodeRequest(cachedMessage);
                                    cachedScenario = 1;
                                    GUIWindow.PrintLog("CC: Sent ConnectionRequest(routerX: " + decoded["routerX"] + ", routerY: " + decoded["routerY"] + ", path:" + decoded["path"] + ") to child CC");
                                    Program.parentConnection.SendMessage(cachedMessage);
                                } else {
                                    //zwrotka do peera
                                    GUIWindow.PrintLog("CC: Sent PeerCoordinationResponse() to peer CC");
                                    Program.peerConnection.SendMessage("component:CC;name:PeerCoordinationResponse");
                                }
                                break;
                            } else if (data["asType"].Equals("SN")) {
                                //zwrotka do parent CC
                                GUIWindow.PrintLog("CC: Sent ConnectionRequestResponse() to parent CC");
                                Program.childConnection.SendMessage("component:CC;name:ConnectionRequestResponse");
                                break;
                            }


                            if (Convert.ToBoolean(RC.cachedData["IDC"])) {
                                GUIWindow.PrintLog("CC: Sent LinkConnectionRequest(" + RC.cachedData["channelRange"] + ") to extrenal LRM");
                                string message3 = "name:LinkConnectionRequest;type:external;respond:true;channelRange:" + RC.cachedData["channelRange"];
                                Program.lrm.HandleRequest(Util.DecodeRequest(message3));
                            } else {
                                //connectionRequestResponse
                                string message1 = "component:CC;name:ConnectionRequestResponse;succeeded:true;connID:" + RC.currentConnectionID;
                                GUIWindow.PrintLog("CC: Sent ConnectionRequestResponse(SUCCEEDED: true, connectionID: " + RC.currentConnectionID + ") to NCC");
                                Program.ncc.HandleRequest(Util.DecodeRequest(message1), null);
                            }
                            break;

                        case "external":
                            GUIWindow.PrintLog("CC: Received LinkConnectionRequestResponse() from external LRM");
                            
                            try {
                                string listOfRouters= "";
                                foreach (int router in RC.currentPath.routerIDs) {
                                    listOfRouters += router + "-";
                                }
                                listOfRouters = listOfRouters.Remove(listOfRouters.Length - 1, 1);

                                string message4 = "component:CC;name:PeerCoordination;connID:" + RC.currentConnectionID + ";routerX:" + RC.cachedData["routerX"] + ";routerY:" + RC.cachedData["routerY"]
                                    + ";routerIDs:" + listOfRouters + ";endPoint1:" + RC.currentPath.endPoints.Item1.GetHostID() + ";" + "endPoint2:" + RC.currentPath.endPoints.Item2.GetHostID()
                                    + ";path:" + listOfRouters + ";channelRange:" + RC.cachedData["channelRange"];

                                if(ConfigLoader.ccID == 1) {
                                    //robimy PeerCoordination
                                    GUIWindow.PrintLog("CC: Sent PeerCoordination(routerX: " + RC.cachedData["routerX"] + ", routerY: " + RC.cachedData["routerY"] + ", path:" + listOfRouters + ") to peer CC");
                                    Program.peerConnection.SendMessage(message4);
                                } else if (ConfigLoader.ccID == 2) {
                                    cachedScenario = 2;
                                    cachedMessage = message4;
                                    GUIWindow.PrintLog("CC: Sent ConnectionRequest(routerX: " + RC.cachedData["routerX"] + ", routerY: " + RC.cachedData["routerY"] + ", path:" + listOfRouters + ") to child CC");
                                    Program.parentConnection.SendMessage(message4 + ";layer:lower;actualName:ConnectionRequest");
                                }
                            }
                            catch (Exception e) {
                                GUIWindow.PrintLog(e.Message);
                                GUIWindow.PrintLog(e.StackTrace);
                                
                            }
                            break;
                    }
                    break;

                case "PeerCoordination":
                    try {
                        GUIWindow.PrintLog("CC: Received " + data["actualName"] +"(routerX: " + data["routerX"] + ", routerY: " + data["routerY"] + ", path:" + data["path"] + ") from parent CC");
                    } catch(Exception) {
                        GUIWindow.PrintLog("CC: Received PeerCoordination(routerX: " + data["routerX"] + ", routerY: " + data["routerY"] + ", path:" + data["path"] + ") from peer CC");
                    }
                    List<int> routerID = new List<int>();
                    foreach(string id in data["path"].Split('-')) {
                        routerID.Add(Convert.ToInt32(id)); 
                    }
                    Host hostX = ConfigLoader.FindHostAmongAll(Convert.ToInt32(data["endPoint1"]));
                    Host hostY = ConfigLoader.FindHostAmongAll(Convert.ToInt32(data["endPoint2"]));
                    cachedPath = new Path(routerID, hostX, hostY);
                    cachedPath.channelRange = data["channelRange"];
                    Tuple<HostConnection, HostConnection> connections1 = GetHostConnections(cachedPath);
                    //GUIWindow.PrintLog(connections1.Item1.ToString());
                    //GUIWindow.PrintLog(connections1.Item2.GetID().ToString());
                    Call call1 = new Call(Convert.ToInt32(data["connID"]), true, (ConfigLoader.ccID == 1 ? 2 : 1), cachedPath.throughSN, connections1.Item1, connections1.Item2, cachedPath);
                    NCC.callRegister.Add(Convert.ToInt32(data["connID"]), call1);

                    cachedMessage = "component:CC;name:ConnectionRequest;layer:lower;connID:" + data["connID"] + ";routerX:" + data["routerX"] + ";routerY:" + data["routerY"]
                                    + ";routerIDs:" + data["routerIDs"] + ";endPoint1:" + data["endPoint1"] + ";" + "endPoint2:" + data["endPoint2"] + ";path:"
                                    + data["path"] + ";channelRange:" + data["channelRange"];

                    //GUIWindow.PrintLog("CC: Sent SendConnectionTables(" + data["path"] + ", " + data["connID"] + ") to RC");
                    Program.rc.HandleRequest(Util.DecodeRequest("name:SendConnectionTables;connID:" + data["connID"] + ";path:" + data["path"]));

                    break;

                case "PeerCoordinationResponse":
                    GUIWindow.PrintLog("CC: Received PeerCoordinationResponse() from peer CC");
                    GUIWindow.PrintLog("CC: Sent ConnectionRequestResponse() to NCC");
                    Program.ncc.HandleRequest(Util.DecodeRequest("name:ConnectionRequestResponse;succeeded:true;connID:" + RC.currentConnectionID), null);
                    break;

                case "SendConnectionTablesResponse":
                    //GUIWindow.PrintLog("CC: Received SendConnectionTablesResponse() from RC");
                    try {
                        //GUIWindow.PrintLog("CC: Sent LinkConnectionRequest(" + cachedPath.channelRange + ") to internal LRM");
                        string message22 = "name:LinkConnectionRequest;type:internal;channelRange:" + cachedPath.channelRange + ";asType:" + (ConfigLoader.ccID == 3 ? "SN" : "second");
                        Program.lrm.HandleRequest(Util.DecodeRequest(message22));
                    } catch(Exception e) {
                        GUIWindow.PrintLog(e.Message);
                        GUIWindow.PrintLog(e.StackTrace);
                    }

                    break;

                case "ConnectionTeardown":
                    
                    
                    if (ConfigLoader.ccID == 3) {
                        //CHILD
                        GUIWindow.PrintLog("CC: Received ConnectionRequest(" + data["connectionID"] + ") from Parent CC");
                        Path path = NCC.callRegister[Int32.Parse(data["connectionID"])].GetPath();

                        //GUIWindow.PrintLog("CC: Sent SendConnectionTables(" + data["connectionID"] + ", " + path.ToString() + ", disconnect) to RC");
                        //GUIWindow.PrintLog("RC: Received SendConnectionTables(" + data["connectionID"] + ", " + path.ToString() + ", disconnect) from CC");

                        Program.rc.UpdateRoutingTables(path, Int32.Parse(data["connectionID"]), false, true);
                        //Program.rc.UpdateRoutingTables(path, Int32.Parse(data["connectionID"]), true, true);

                        //GUIWindow.PrintLog("RC: Sent SendConnectionTablesResponse() to CC : OK");
                        //GUIWindow.PrintLog("CC: Received SendConnectionTablesResponse() from RC : OK");
                        //GUIWindow.PrintLog("CC: Sent LinkConnectionInternalDeallocation(" + data["connectionID"] + ") to Internal LRM");
                        string message1 = "component:LRM;name:LinkConnectionInternalDeallocation;connectionID:" + data["connectionID"];
                        Program.lrm.HandleRequest(Util.DecodeRequest(message1));
                    }
                    else {
                        GUIWindow.PrintLog("CC: Received ConnectionRequest(" + data["connectionID"] + ") from NCC"); // InterDomainConnection: " + data["IDC"]
                        Path path = NCC.callRegister[Int32.Parse(data["connectionID"])].GetPath();

                        //GUIWindow.PrintLog("CC: Sent SendConnectionTables(" + data["connectionID"] + ", " + path.ToString() + ", disconnect) to RC");
                        //GUIWindow.PrintLog("RC: Received SendConnectionTables(" + data["connectionID"] + ", " + path.ToString() + ", disconnect) from CC");

                        Program.rc.UpdateRoutingTables(path, Int32.Parse(data["connectionID"]), false, true);
                        //Program.rc.UpdateRoutingTables(path, Int32.Parse(data["connectionID"]), true, true);

                        //GUIWindow.PrintLog("RC: Sent SendConnectionTablesResponse() to CC : OK");
                        //GUIWindow.PrintLog("CC: Received SendConnectionTablesResponse() from RC : OK");
                        var element = NCC.callRegister[Int32.Parse(data["connectionID"])];
                        bool interDomainConnectionFlag = element.GetInterDomainConnectionFlag();
                        int asID = element.GetStartAsID();

                        if (interDomainConnectionFlag && asID == ConfigLoader.ccID) {
                            GUIWindow.PrintLog("CC: Sent LinkConnectionExternalDeallocation(" + data["connectionID"] + ") to External LRM");
                            string message3 = "component:LRM;name:LinkConnectionExternalDeallocation;connectionID:" + data["connectionID"] + ";deleteChannels:true";
                            Program.lrm.HandleRequest(Util.DecodeRequest(message3));
                        }
                        else if (!interDomainConnectionFlag) {
                            //GUIWindow.PrintLog("CC: Sent LinkConnectionInternalDeallocation(" + data["connectionID"] + ") to Internal LRM");
                            string message3 = "component:LRM;name:LinkConnectionInternalDeallocation;connectionID:" + data["connectionID"];
                            Program.lrm.HandleRequest(Util.DecodeRequest(message3));
                        }
                        else { 
                            GUIWindow.PrintLog("CC: Sent LinkConnectionExternalDeallocation(" + data["connectionID"] + ") to External LRM");
                            string message3 = "component:LRM;name:LinkConnectionExternalDeallocation;connectionID:" + data["connectionID"] + ";deleteChannels:false";
                            Program.lrm.HandleRequest(Util.DecodeRequest(message3));
                        }
                    }
                    break;

                case "LinkConnectionExternalDeallocationResponse":
                    GUIWindow.PrintLog("CC: Received LinkConnectionExternalDeallocation(" + data["connectionID"] + ") from External LRM : OK");
                    //GUIWindow.PrintLog("CC: Sent LinkConnectionInternalDeallocation(" + data["connectionID"] + ") to Internal LRM");
                    string message = "component:LRM;name:LinkConnectionInternalDeallocation;connectionID:" + data["connectionID"];
                    Program.lrm.HandleRequest(Util.DecodeRequest(message));
                    break;

                case "LinkConnectionInternalDeallocationResponse":
                    //GUIWindow.PrintLog("CC: Received LinkConnectionInternalDeallocationResponse(" + data["connectionID"] + ") from Internal LRM : OK");

                    if (NCC.callRegister[Int32.Parse(data["connectionID"])].GetThroughSubnetwork() && ConfigLoader.ccID == 2) {
                        // PARENT
                        GUIWindow.PrintLog("CC: Sent ConnectionRequest(" + data["connectionID"] + ") to Child CC");
                        message = "component:CC;name:ConnectionTeardown;connectionID:" + data["connectionID"];
                        Program.parentConnection.SendMessage(message);
                    }
                    else if (ConfigLoader.ccID == 3) {
                        // CHILD
                        GUIWindow.PrintLog("CC: Sent ConnectionRequestResponse(" + data["connectionID"] + ") to Parent CC : OK");
                        message = "component:CC;name:ConnectionTeardownResponse;connectionID:" + data["connectionID"];
                        Program.childConnection.SendMessage(message);
                    }
                    else {
                        GUIWindow.PrintLog("CC: Sent ConnectionRequestResponse(" + data["connectionID"] + ") to NCC : OK");
                        message = "component:NCC;name:ConnectionTeardownResponse;connectionID:" + data["connectionID"];
                        Program.ncc.HandleRequest(Util.DecodeRequest(message), null);
                    }
                    break;

                case "ConnectionTeardownResponse":
                    //Odpowiedz od CHILD
                    GUIWindow.PrintLog("CC: Received ConnectionRequestResponse(" + data["connectionID"] + ") from Child CC : OK");
                    GUIWindow.PrintLog("CC: Sent ConnectionRequestResponse(" + data["connectionID"] + ") to NCC : OK");
                    message = "component:NCC;name:ConnectionTeardownResponse;connectionID:" + data["connectionID"];
                    Program.ncc.HandleRequest(Util.DecodeRequest(message), null);
                    break;
            }
        }

        private Tuple<HostConnection, HostConnection> GetHostConnections(Path path) {
            HostConnection hc1 = null, hc2 = null;

            foreach (HostConnection hostConnection in Server.GetHostConnections()) {
                if (hostConnection.GetHost() == path.endPoints.Item1)
                    hc1 = hostConnection;

                if (hostConnection.GetHost() == path.endPoints.Item2)
                    hc2 = hostConnection;
            }

            return new Tuple<HostConnection, HostConnection>(hc1, hc2);
        }

    }
}
