using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class CC {

        public void HandleRequest(Dictionary<string, string> data) {
            switch(data["name"]) {
                case "ConnectionRequest":
                    GUIWindow.PrintLog("CC: Received ConnectionRequest(" + data["routerX"] + ", " + data["routerY"] + ", " + data["speed"] + " Gb/s, InterDomainConnection: " + data["IDC"] + ") from NCC");

                    string networkCase = ConfigLoader.ccID == 3 ? "SN" : "FirstAS";
                    string message = "component:RC;name:RouteTableQuery;routerX:" + data["routerX"] + ";routerY:" + data["routerY"] + ";speed:" + data["speed"] + ";IDC:" + data["IDC"] + ";case:" + networkCase;
                    GUIWindow.PrintLog("CC: Sent RouteTableQuery(" + data["routerX"] + ", " + data["routerY"] + ", " + data["speed"] + " Gb/s, InterDomainConnection: " + data["IDC"] + ") to RC");
                    Program.rc.HandleRequest(Util.DecodeRequest(message));
                    break;

                case "RouteTableQueryResponse":
                    GUIWindow.PrintLog("Received RouteTableQueryResponse(" + data["path"] + ") from RC");
                    if (data["path"].Equals("null")) {
                        GUIWindow.PrintLog("CC: Requested connection could not be established");
                        string message1 = "component:CC;name:ConnectionRequestResponse;succeeded:false";
                        GUIWindow.PrintLog("CC: Sent ConnectionRequestResponse(false) to NCC");
                        Program.ncc.HandleRequest(Util.DecodeRequest(message1), null);
                        break;
                    }

                    //tutaj wysyłamy LRMom request o alokacje, potem PeerCoordination jeśli to połączenie IDC
                    bool throughSN = ConfigLoader.ccID == 1 ? false : RC.currentPath.throughSN;
                    Tuple<HostConnection, HostConnection> connections = GetHostConnections(RC.currentPath);
                    Call call = new Call(RC.currentConnectionID, Convert.ToBoolean(RC.cachedData["IDC"]), ConfigLoader.ccID, throughSN, connections.Item1, connections.Item2);
                    NCC.callRegister.Add(RC.currentConnectionID, call);

                    GUIWindow.PrintLog("CC: Sent LinkConnectionRequest(" + RC.cachedData["channelRange"] + ") to internal LRM");
                    string message2 = "name:LinkConnectionRequest;type:internal;channelRange:" + RC.cachedData["channelRange"];
                    Program.lrm.HandleRequest(Util.DecodeRequest(message2));

                    break;

                case "LinkConnectionRequestResponse":
                    switch (data["type"]) {
                        case "internal":
                            GUIWindow.PrintLog("CC: Received LinkConnectionRequestResponse() from internal LRM");

                            if (Convert.ToBoolean(RC.cachedData["IDC"])) {
                                GUIWindow.PrintLog("CC: Sent LinkConnectionRequest(" + RC.cachedData["channelRange"] + ") to extrenal LRM");
                                string message3 = "name:LinkConnectionRequest;type:external;channelRange:" + RC.cachedData["channelRange"];
                                Program.lrm.HandleRequest(Util.DecodeRequest(message3));
                            } else {
                                //można robić PeerCoordination
                            }
                            break;

                        case "external":
                            GUIWindow.PrintLog("CC: Received LinkConnectionRequestResponse() from external LRM");
                            //można robić PeerCoordination
                            break;
                    }
                    break;

                case "ConnectionTeardown":
                    if (ConfigLoader.ccID == 3) {
                        //CHILD
                        GUIWindow.PrintLog("CC: Received ConnectionTeardown(" + data["connectionID"] + ") from Parent CC");
                        GUIWindow.PrintLog("CC: Sent LinkConnectionInternalDeallocation(" + data["connectionID"] + ") to Internal LRM");
                        message = "component:LRM;name:LinkConnectionInternalDeallocation;connectionID:" + data["connectionID"];
                        Program.lrm.HandleRequest(Util.DecodeRequest(message));
                    }
                    else {
                        GUIWindow.PrintLog("CC: Received ConnectionTeardown(" + data["connectionID"] + ") from NCC"); // InterDomainConnection: " + data["IDC"]

                        var element = NCC.callRegister[Int32.Parse(data["connectionID"])];
                        bool interDomainConnectionFlag = element.GetInterDomainConnectionFlag();
                        int asID = element.GetStartAsID();

                        if (!interDomainConnectionFlag && asID == ConfigLoader.ccID) {
                            GUIWindow.PrintLog("CC: Sent LinkConnectionExternalDeallocation(" + data["connectionID"] + ") to External LRM");
                            message = "component:LRM;name:LinkConnectionExternalDeallocation;connectionID:" + data["connectionID"];
                            Program.lrm.HandleRequest(Util.DecodeRequest(message));
                        }
                        else {
                            GUIWindow.PrintLog("CC: Sent LinkConnectionInternalDeallocation(" + data["connectionID"] + ") to Internal LRM");
                            message = "component:LRM;name:LinkConnectionInternalDeallocation;connectionID:" + data["connectionID"];
                            Program.lrm.HandleRequest(Util.DecodeRequest(message));
                        }
                    }
                    break;

                case "LinkConnectionExternalDeallocationResponse":
                    GUIWindow.PrintLog("CC: Received LinkConnectionExternalDeallocation(" + data["connectionID"] + ") from External LRM : OK");
                    GUIWindow.PrintLog("CC: Sent LinkConnectionInternalDeallocation(" + data["connectionID"] + ") to Internal LRM");
                    message = "component:LRM;name:LinkConnectionInternalDeallocation;connectionID:" + data["connectionID"];
                    Program.lrm.HandleRequest(Util.DecodeRequest(message));
                    break;

                case "LinkConnectionInternalDeallocationResponse":
                    GUIWindow.PrintLog("CC: Received LinkConnectionInternalDeallocation(" + data["connectionID"] + ") from Internal LRM : OK");

                    if (NCC.callRegister[Int32.Parse(data["connectionID"])].GetThroughSubnetwork() && ConfigLoader.ccID == 2) {
                        // PARENT
                        GUIWindow.PrintLog("CC: Sent ConnectionTeardown(" + data["connectionID"] + ") to Child CC");
                        message = "component:CC;name:ConnectionTeardown;connectionID:" + data["connectionID"];
                        Program.childConnection.SendMessage(message);
                    }
                    else if (ConfigLoader.ccID == 3) {
                        // CHILD
                        GUIWindow.PrintLog("CC: Sent ConnectionTeardownResponse(" + data["connectionID"] + ") to Parent CC : OK");
                        message = "component:CC;name:ConnectionTeardownResponse;connectionID:" + data["connectionID"];
                        Program.parentConnection.SendMessage(message);
                    }
                    else {
                        GUIWindow.PrintLog("CC: Sent ConnectionTeardownResponse(" + data["connectionID"] + ") to NCC : OK");
                        message = "component:NCC;name:ConnectionTeardownResponse;connectionID:" + data["connectionID"];
                        Program.ncc.HandleRequest(Util.DecodeRequest(message), null);
                    }
                    break;

                case "ConnectionTeardownResponse":
                    //Odpowiedz od CHILD
                    GUIWindow.PrintLog("CC: Received ConnectionTeardownResponse(" + data["connectionID"] + ") from Child CC : OK");
                    GUIWindow.PrintLog("CC: Sent ConnectionTeardownResponse(" + data["connectionID"] + ") to NCC : OK");
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
