using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter.Components {
    class LRM {

        public void HandleRequest(Dictionary<string, string> data) {
            
            switch (data["name"]) {
                case "LinkConnectionExternalDeallocation":
                    
                    GUIWindow.PrintLog("External LRM: Received LinkConnectionExternalDeallocation(" + data["connectionID"] + ") from CC");

                    String routerIP = null;
                    if (ConfigLoader.ccID == 1) {
                        routerIP = "10.0.3.1";
                    }
                    else if (ConfigLoader.ccID == 2) {
                        routerIP = "10.0.10.1";
                    }

                    GUIWindow.PrintLog("External LRM: Sent SNPRelase(" + routerIP + ", " + data["connectionID"] + ") to other AS LRM");
                    string message = "component:LRM;name:SNPRelase;routerX:" + routerIP + ";connectionID:" + data["connectionID"];
                    Program.peerConnection.SendMessage(message);
                    break;

                case "SNPRelase":
                    GUIWindow.PrintLog("External LRM: Received SNPRelase(" + data["routerX"] + ", " + data["connectionID"] + ") from other AS LRM");
                    GUIWindow.PrintLog("External LRM: Sent SNPRelaseResponse(" + data["routerX"] + ", " + data["connectionID"] + ") from other AS LRM : OK");
                    message = "component:LRM;name:SNPRelaseResponse;routerX:" + data["routerX"] + ";connectionID:" + data["connectionID"];
                    Program.peerConnection.SendMessage(message);
                    break;

                case "SNPRelaseResponse":
                    GUIWindow.PrintLog("External LRM: Received SNPRelaseResponse(" + data["routerX"] + ", " + data["connectionID"] + ") from other AS LRM : OK");
                    // ===================================================================

                    //Dealokacja miedzy AS
                    //Tutaj musi byc wyslane Local Topology do RC ale nie wiem z czym ??

                    // ===================================================================
                    GUIWindow.PrintLog("External LRM: Sent LinkConnectionExternalDeallocationResponse(" + data["connectionID"] + ") to CC : OK");
                    message = "component:CC;name:LinkConnectionExternalDeallocationResponse;connectionID:" + data["connectionID"];
                    Program.cc.HandleRequest(Util.DecodeRequest(message));
                    break;

                case "LinkConnectionInternalDeallocation":
                    GUIWindow.PrintLog("Internal LRM: Received LinkConnectionInternalDeallocation(" + data["connectionID"] + ") from CC");
                    // ===================================================================

                    //Dealokacja lokalna
                    //Tutaj musi byc wyslane Local Topology do RC ale nie wiem z czym ??

                    // ===================================================================
                    GUIWindow.PrintLog("Internal LRM: Sent LinkConnectionInternalDeallocationResponse(" + data["connectionID"] + ") to CC : OK");
                    message = "component:CC;name:LinkConnectionInternalDeallocationResponse;connectionID:" + data["connectionID"];
                    Program.cc.HandleRequest(Util.DecodeRequest(message));
                    break;

                case "LinkConnectionRequest":
                    switch(data["type"]) {
                        case "internal":
                            GUIWindow.PrintLog("Internal LRM: Received LinkConnectionRequest(" + data["channelRange"] + ") from CC");
                            string[] range = data["channelRange"].Split('-');
                            foreach(Connection connection in (RC.currentPath == null ? Program.cc.cachedPath.edges : RC.currentPath.edges)) {
                                if (!ConfigLoader.myConnections.ContainsValue(connection))
                                    continue;

                                for(int i = Convert.ToInt32(range[0]); i <= Convert.ToInt32(range[1]); i++) {
                                    connection.slot[i] = RC.currentConnectionID;
                                }
                                //GUIWindow.PrintLog("Edge #" + connection.GetID() + ": " + string.Join("", connection.slot));
                            }
                            GUIWindow.UpdateChannelTable();
                            GUIWindow.PrintLog("Internal LRM: Sent LinkConnectionRequestResponse() to CC");
                            Program.cc.HandleRequest(Util.DecodeRequest("name:LinkConnectionRequestResponse;type:internal;asType:" + data["asType"]));
                            break;

                        case "external":
                            GUIWindow.PrintLog("Extrenal LRM: Received LinkConnectionRequest(" + data["channelRange"] + ") from CC");
                            string[] range2 = data["channelRange"].Split('-');
                            Connection extConnection = ConfigLoader.connections[8];
                            for (int i = Convert.ToInt32(range2[0]); i <= Convert.ToInt32(range2[1]); i++) {
                                extConnection.slot[i] = RC.currentConnectionID;
                            }
                            //GUIWindow.PrintLog("Edge #" + extConnection.GetID() + ": " + string.Join("", extConnection.slot));
                            GUIWindow.PrintLog("External LRM: Sent LinkConnectionRequestResponse() to CC");
                            Program.cc.HandleRequest(Util.DecodeRequest("name:LinkConnectionRequestResponse;type:external"));
                            break;
                    }
                    break;
            }
            
        }
    }
}
