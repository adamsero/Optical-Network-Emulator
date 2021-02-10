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
                    string message = "component:LRM;name:SNPRelase;routerX:" + routerIP + ";connectionID:" + data["connectionID"] + ";deleteChannels:" + data["deleteChannels"];
                    Program.peerConnection.SendMessage(message);
                    break;

                case "SNPRelase":
                    GUIWindow.PrintLog("External LRM: Received SNPRelase(" + data["routerX"] + ", " + data["connectionID"] + ") from other AS LRM");
                    GUIWindow.PrintLog("External LRM: Sent SNPRelaseResponse(" + data["routerX"] + ", " + data["connectionID"] + ") from other AS LRM : OK");
                    message = "component:LRM;name:SNPRelaseResponse;routerX:" + data["routerX"] + ";connectionID:" + data["connectionID"] + ";deleteChannels:" + data["deleteChannels"]; ;
                    Program.peerConnection.SendMessage(message);
                    break;

                case "SNPRelaseResponse":
                    GUIWindow.PrintLog("External LRM: Received SNPRelaseResponse(" + data["routerX"] + ", " + data["connectionID"] + ") from other AS LRM : OK");
                    // ===================================================================
                    //Dealokacja miedzy AS
                    //Tutaj musi byc wyslane Local Topology do RC ale nie wiem z czym ??

                    Connection extConnection = ConfigLoader.connections[8];
                    if (data["deleteChannels"].Equals("true")) {
                        
                        for (int i = 0; i < extConnection.slot.Length; i++) {
                            if (extConnection.slot[i] == Int32.Parse(data["connectionID"])) {
                                extConnection.slot[i] = 0;
                            }
                        }
                        GUIWindow.UpdateChannelTable();
                    }
                   
                    GUIWindow.PrintLog("External LRM: Sent LocalTopology(" + 8 + ": " + String.Join("",extConnection.slot) + ") to RC : DEALLOCATED");
                    GUIWindow.PrintLog("RC: Received LocalTopology(" + 8 + ": " + String.Join("", extConnection.slot) + ") from External LRM : DEALLOCATED");
                    GUIWindow.PrintLog("RC: Sent LocalTopologyResponse() to External LRM : OK");
                    GUIWindow.PrintLog("External LRM: Received LocalTopologyResponse() from RC : OK");

                    // ===================================================================
                    GUIWindow.PrintLog("External LRM: Sent LinkConnectionExternalDeallocationResponse(" + data["connectionID"] + ") to CC : OK");
                    message = "component:CC;name:LinkConnectionExternalDeallocationResponse;connectionID:" + data["connectionID"];
                    Program.cc.HandleRequest(Util.DecodeRequest(message));
                    break;

                case "LinkConnectionInternalDeallocation":
                    
                    // ===================================================================
                    //Dealokacja lokalna
                    //Tutaj musi byc wyslane Local Topology do RC ale nie wiem z czym ??

                    foreach (Connection connection in NCC.callRegister[Int32.Parse(data["connectionID"])].GetPath().edges){

                        if (connection.GetAsID() != ConfigLoader.ccID) {
                            continue;
                        }

                        for (int i = 0; i < connection.slot.Length; i++) {
                            if (connection.slot[i] == Int32.Parse(data["connectionID"])) {
                                connection.slot[i] = 0;
                            }
                        }

                        GUIWindow.PrintLog("CC: Sent LinkConnectionInternalDeallocation(" + connection.GetID() + ", " + data["connectionID"] + ") to Internal LRM");
                        GUIWindow.PrintLog("Internal LRM: Received LinkConnectionInternalDeallocation(" + data["connectionID"] + ") from CC");

                        GUIWindow.PrintLog("Internal LRM: Sent LocalTopology(" + connection.GetID() + ": " + String.Join("", connection.slot) + ") to RC : DEALLOCATED");
                        GUIWindow.PrintLog("RC: Received LocalTopology(" + connection.GetID() + ": " + String.Join("", connection.slot) + ") from Internal LRM : DEALLOCATED");
                        GUIWindow.PrintLog("RC: Sent LocalTopologyResponse() to Internal LRM : OK");
                        GUIWindow.PrintLog("Internal LRM: Received LocalTopologyResponse() from RC : OK");

                        GUIWindow.PrintLog("Internal LRM: Sent LinkConnectionInternalDeallocationResponse(" + data["connectionID"] + ") to CC : OK");
                        GUIWindow.PrintLog("CC: Received LinkConnectionInternalDeallocationResponse(" + data["connectionID"] + ") from Internal LRM : OK");
                    }
                    GUIWindow.UpdateChannelTable();

                    // ===================================================================
                    
                    message = "component:CC;name:LinkConnectionInternalDeallocationResponse;connectionID:" + data["connectionID"];
                    Program.cc.HandleRequest(Util.DecodeRequest(message));
                    break;

                case "LinkConnectionRequest":
                    switch(data["type"]) {
                        case "internal":
                            
                            string[] range = data["channelRange"].Split('-');
                            foreach(Connection connection in (RC.currentPath == null ? Program.cc.cachedPath.edges : RC.currentPath.edges)) {
                                if (!ConfigLoader.myConnections.ContainsValue(connection))
                                    continue;

                                for(int i = Convert.ToInt32(range[0]); i <= Convert.ToInt32(range[1]); i++) {
                                    connection.slot[i] = RC.currentConnectionID;
                                }

                                GUIWindow.PrintLog("CC: Sent LinkConnectionRequest(" + connection.GetID() +", " + data["channelRange"] + ") to internal LRM");
                                GUIWindow.PrintLog("Internal LRM: Received LinkConnectionRequest(" + connection.GetID() + ", " + data["channelRange"] + ") from CC");

                                GUIWindow.PrintLog("Internal LRM: Sent LocalTopology(" + connection.GetID() + ": " + String.Join("", connection.slot) + ") to RC");
                                GUIWindow.PrintLog("RC: Received LocalTopology(" + connection.GetID() + ": " + String.Join("", connection.slot) + ") from Internal LRM");
                                GUIWindow.PrintLog("RC: Sent LocalTopologyResponse() to Internal LRM : OK");
                                GUIWindow.PrintLog("Internal LRM: Received LocalTopologyResponse() from RC : OK");

                                GUIWindow.PrintLog("Internal LRM: Sent LinkConnectionRequestResponse() to CC");
                                GUIWindow.PrintLog("CC: Received LinkConnectionRequestResponse() from internal LRM");
                            }
                            GUIWindow.UpdateChannelTable();
                            
                            Program.cc.HandleRequest(Util.DecodeRequest("name:LinkConnectionRequestResponse;type:internal;asType:" + data["asType"]));
                            break;

                        case "external":
                            GUIWindow.PrintLog("Extrenal LRM: Received LinkConnectionRequest(" + data["channelRange"] + ") from CC");
                            string[] range2 = data["channelRange"].Split('-');
                            extConnection = ConfigLoader.connections[8];
                            for (int i = Convert.ToInt32(range2[0]); i <= Convert.ToInt32(range2[1]); i++) {
                                extConnection.slot[i] = RC.currentConnectionID;
                            }

                            GUIWindow.PrintLog("External LRM: Sent LocalTopology(" + 8 + ": " + String.Join("", extConnection.slot) + ") to RC");
                            GUIWindow.PrintLog("RC: Received LocalTopology(" + 8 + ": " + String.Join("", extConnection.slot) + ") from External LRM");
                            GUIWindow.PrintLog("RC: Sent LocalTopologyResponse() to External LRM : OK");
                            GUIWindow.PrintLog("External LRM: Received LocalTopologyResponse() from RC : OK");

                            if (Convert.ToBoolean(data["respond"])) {
                                GUIWindow.PrintLog("External LRM: Sent LinkConnectionRequestResponse() to CC");
                                Program.cc.HandleRequest(Util.DecodeRequest("name:LinkConnectionRequestResponse;type:external"));
                            }
                            break;
                    }
                    break;
            }
            
        }
    }
}
