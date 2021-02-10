using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class NCC {

        private Dictionary<string, Tuple<string, int>> directory = new Dictionary<string, Tuple<string, int>>();

        //cached data
        private HostConnection lastCaller;
        private string lastRouterXIP;
        private string lastRouterYIP;
        private int lastSpeed;
        private bool lastIDC;
        public static Dictionary<int, Call> callRegister = new Dictionary<int, Call>();
        private HostConnection cachedHostYConnection;
        private bool cachedSuccess;

        public NCC() {
            directory.Add("Host1", new Tuple<string, int>("10.0.1.1", 1));
            directory.Add("Host2", new Tuple<string, int>("10.0.2.1", 1));
            directory.Add("Host3", new Tuple<string, int>("10.0.5.1", 2));
            directory.Add("Host4", new Tuple<string, int>("10.0.4.1", 2));
            directory.Add("10.0.1.1", new Tuple<string, int>("Host1", 1));
            directory.Add("10.0.2.1", new Tuple<string, int>("Host2", 1));
            directory.Add("10.0.5.1", new Tuple<string, int>("Host3", 2));
            directory.Add("10.0.4.1", new Tuple<string, int>("Host4", 2));
        }

        public void HandleRequest(Dictionary<string, string> data, HostConnection caller) {
            try {
                switch (data["name"]) {
                    case "CallRequest":
                        GUIWindow.PrintLog("NCC: Received CallRequest(" + data["hostX"] + ", " + data["hostY"] + ", " + data["speed"] + " Gb/s) from CPCC");

                        Tuple<string, string, int, int> routerData = HandleDirectory(data["hostX"], data["hostY"]);
                        if (routerData.Item1.Equals("WRONG NAME") || routerData.Item2.Equals("WRONG NAME")) {
                            GUIWindow.PrintLog("NCC: Host name does not exist");
                            GUIWindow.PrintLog("NCC: Sent CallRequestResponse(UNSUCCESSFUL)");
                            RefuseConnection(caller);
                            break;
                        }
                        else if (routerData.Item1.Equals(routerData.Item2)) {
                            GUIWindow.PrintLog("NCC: Target Host name is the same as the source one");
                            GUIWindow.PrintLog("NCC: Sent CallRequestResponse(UNUSCCESSFUL)");
                            RefuseConnection(caller);
                            break;
                        }
                        HandlePolicy();

                        lastCaller = caller;
                        lastRouterXIP = routerData.Item1;
                        lastRouterYIP = routerData.Item2; ;
                        lastSpeed = Convert.ToInt32(data["speed"]);
                        lastIDC = routerData.Item3 != routerData.Item4;

                        if (!lastIDC) {
                            string hostYIP1 = routerData.Item2.Remove(routerData.Item2.Length - 1, 1) + "2";
                            HostConnection hostConnection1 = Server.GetHostConnectionByIP(hostYIP1);
                            string message1 = "component:CPCC;name:CallAccept;routerX:" + data["hostX"] + ";routerY:" + data["hostY"] + ";speed:" + data["speed"];
                            hostConnection1.SendMessage(message1);
                            cachedHostYConnection = hostConnection1;
                            GUIWindow.PrintLog("NCC: Sent CallAccept(" + data["hostX"] + ", " + data["hostY"] + ", " + data["speed"] + " Gb/s) to other NCC");
                            break;
                        }

                        string message = "component:NCC;name:CallCoordination;routerX:" + routerData.Item1 + ";routerY:" + routerData.Item2 + ";speed:" + data["speed"] + ";IDC:" + lastIDC.ToString();
                        Program.peerConnection.SendMessage(message);
                        GUIWindow.PrintLog("NCC: Sent CallCoordination(" + routerData.Item1 + ", " + routerData.Item2 + ", " + data["speed"] + " Gb/s) to other NCC");

                        break;

                    case "CallCoordination":

                        GUIWindow.PrintLog("NCC: Received CallCoordination(" + data["routerX"] + ", " + data["routerY"] + ", " + data["speed"] + " Gb/s) from other NCC");
                        lastIDC = Convert.ToBoolean(data["IDC"]);

                        //odwrotne dzialanie funkcji -- dajemy IP dostajemy nazwy
                        routerData = HandleDirectory(data["routerX"], data["routerY"]);
                        HandlePolicy();
                        string hostYIP = data["routerY"].Remove(data["routerY"].Length - 1, 1) + "2";
                        HostConnection hostConnection = Server.GetHostConnectionByIP(hostYIP);
                        cachedHostYConnection = hostConnection;
                        message = "component:CPCC;name:CallAccept;routerX:" + routerData.Item1 + ";routerY:" + routerData.Item2 + ";speed:" + data["speed"];
                        hostConnection.SendMessage(message);
                        GUIWindow.PrintLog("NCC: Sent CallAccept(" + routerData.Item1 + ", " + routerData.Item2 + ", " + data["speed"] + " Gb/s) to other NCC");
                        break;

                    case "CallCoordinationResponse":
                        if (!Convert.ToBoolean(data["succeeded"])) {
                            GUIWindow.PrintLog("NCC: Received CallRequestResponse(DENIED) from other NCC");
                            RefuseConnection(lastCaller);
                        }
                        else {
                            GUIWindow.PrintLog("NCC: Received CallRequestResponse(ACCEPTED) from other NCC");
                            ConnectionRequest(lastIDC);
                            //message = "component:CPCC;name:CallRequestResponse;succeeded:true;connectionID:0"; //hard-kodowane connID na razie
                            //lastCaller.SendMessage(message);
                        }
                        break;

                    case "CallAcceptResponse":
                        string status = "ACCEPTED";
                        if(lastIDC) {
                            if (!Convert.ToBoolean(data["succeeded"])) {
                                message = "component:NCC;name:CallCoordinationResponse;succeeded:false";
                                status = "DENIED";
                            }
                            else {
                                message = "component:NCC;name:CallCoordinationResponse;succeeded:true";
                            }
                            Program.peerConnection.SendMessage(message);
                            GUIWindow.PrintLog("NCC: Sent CallCoordinationResponse(" + status + ") to other NCC");
                        } else {
                            if (!Convert.ToBoolean(data["succeeded"])) {
                                GUIWindow.PrintLog("NCC: Received CallAcceptResponse(DENIED) from CPCC");
                                RefuseConnection(lastCaller);
                            } else {
                                GUIWindow.PrintLog("NCC: Received CallAcceptResponse(ACCEPTED) from CPCC");
                                ConnectionRequest(lastIDC);
                            }
                        }
                        break;
                    
                    case "CallTeardownCPCC":
                        GUIWindow.PrintLog("NCC: Received CallTeardownCPCC(" + data["connectionID"] + ") from CPCC");

                        GUIWindow.PrintLog("NCC: Sent ConnectionRequest(" + data["connectionID"] + ") to CC");
                        message = "component:CC;name:ConnectionTeardown;connectionID:" + data["connectionID"];
                        Program.cc.HandleRequest(Util.DecodeRequest(message));
                        break;

                    case "ConnectionTeardownResponse":
                        GUIWindow.PrintLog("NCC: Received ConnectionRequestResponse(" + data["connectionID"] + ") from CC : OK");
                        
                        if (callRegister[Int32.Parse(data["connectionID"])].GetStartAsID() == ConfigLoader.ccID) {
                            // Pierwszy AS
                            if (!callRegister[Int32.Parse(data["connectionID"])].GetInterDomainConnectionFlag()) {
                                //polaczenie wewnatrzdomenowe
                                GUIWindow.PrintLog("NCC: Sent CallTeardownCPCC(" + data["connectionID"] + ") to CPCC");
                                message = "component:CPCC;name:CallTeardownCPCC;connectionID:" + data["connectionID"];
                                callRegister[Int32.Parse(data["connectionID"])].GetTargetHostConnection().SendMessage(message);
                                break;
                            }
                            else {
                                //polaczenie zewnatrzdomenowe
                                GUIWindow.PrintLog("NCC: Sent CallTeardownNCC(" + data["connectionID"] + ") to other AS NCC");
                                message = "component:NCC;name:CallTeardownNCC;connectionID:" + data["connectionID"];
                                Program.peerConnection.SendMessage(message);
                            }
                        }
                        else {
                            // Drugi AS
                            GUIWindow.PrintLog("NCC: Sent CallTeardownNCCResponse(" + data["connectionID"] + ") to other AS NCC : OK");
                            message = "component:NCC;name:CallTeardownNCCResponse;connectionID:" + data["connectionID"];
                            Program.peerConnection.SendMessage(message);
                        }
                        break;

                    case "CallTeardownNCC":
                        GUIWindow.PrintLog("NCC: Received CallTeardownNCC(" + data["connectionID"] + ") from other AS NCC");

                        GUIWindow.PrintLog("NCC: Sent CallTeardownCPCC(" + data["connectionID"] + ") to CPCC");
                        message = "component:CPCC;name:CallTeardownCPCC;connectionID:" + data["connectionID"];
                        callRegister[Int32.Parse(data["connectionID"])].GetTargetHostConnection().SendMessage(message);
                        break;

                    case "CallTeardownCPCCResponse":
                        GUIWindow.PrintLog("NCC: Received CallTeardownCPCCResponse(" + data["connectionID"] + ") from CPCC: OK");
                        if (!callRegister[Int32.Parse(data["connectionID"])].GetInterDomainConnectionFlag()) {
                            //polaczenie wewnatrzdomenowe
                            GUIWindow.PrintLog("NCC: Sent CallTeardownCPCCResponse(" + data["connectionID"] + ") to CPCC : OK");
                            message = "component:CPCC;name:CallTeardownCPCCResponse;connectionID:" + data["connectionID"];
                            callRegister[Int32.Parse(data["connectionID"])].GetStartHostConnection().SendMessage(message);
                        }
                        else {
                            //polaczenie zewnatrzdomenowe
                            GUIWindow.PrintLog("NCC: Sent ConnectionRequest(" + data["connectionID"] + ") to CC");
                            message = "component:CC;name:ConnectionTeardown;connectionID:" + data["connectionID"];
                            Program.cc.HandleRequest(Util.DecodeRequest(message));
                        }
                        break;

                    case "CallTeardownNCCResponse":
                        GUIWindow.PrintLog("NCC: Received CallTeardownNCCResponse(" + data["connectionID"] + ") from other AS NCC : OK");
                        GUIWindow.PrintLog("NCC: Sent CallTeardownCPCCResponse(" + data["connectionID"] + ") to CPCC : OK");
                        message = "component:CPCC;name:CallTeardownCPCCResponse;connectionID:" + data["connectionID"];
                        callRegister[Int32.Parse(data["connectionID"])].GetStartHostConnection().SendMessage(message);
                        break;

                    case "CallConfirmation":
                        if (!Convert.ToBoolean(data["succeeded"])) {
                            GUIWindow.PrintLog("NCC: Received CallConfirmation(SUCCEEDED: false) from other NCC");
                            GUIWindow.PrintLog("NCC: Sent CallConfirmation(SUCCEEDED: false) to CPCC");
                            cachedHostYConnection.SendMessage("component:CPCC;name:CallConfirmation;IDC:" + data["IDC"] +";succeeded:false");
                        }
                        else {
                            GUIWindow.PrintLog("NCC: Received CallConfirmation(SUCCEEDED: true, connectionID: " + data["connID"] + ") from other NCC");
                            GUIWindow.PrintLog("NCC: Sent CallConfirmation(SUCCEEDED: true, connectionID: " + data["connID"] + ") to CPCC");
                            cachedHostYConnection.SendMessage("component:CPCC;name:CallConfirmation;IDC:" + data["IDC"] + ";succeeded:true;connID:" + data["connID"]);
                        }
                        break;

                    case "CallConfirmationResponse":
                        switch(data["sender"]) {
                            case "CPCC":
                                GUIWindow.PrintLog("NCC: Received CallConfirmationResponse() from CPCC");
                                if (Convert.ToBoolean(data["IDC"])) {
                                    GUIWindow.PrintLog("NCC: Sent CallConfirmationResponse() to other NCC");
                                    Program.peerConnection.SendMessage("component:NCC;name:CallConfirmationResponse;sender:NCC");
                                } else {
                                    if (!cachedSuccess) {
                                        GUIWindow.PrintLog("NCC: Sent CallRequestResponse(SUCCEEDED: false) to CPCC");
                                        lastCaller.SendMessage("component:CPCC;name:CallRequestResponse;succeeded:false");
                                    }
                                    else {
                                        GUIWindow.PrintLog("NCC: Sent CallRequestResponse(SUCCEEDED: true, connectionID: " + RC.currentConnectionID + ") to CPCC");
                                        lastCaller.SendMessage("component:CPCC;name:CallRequestResponse;succeeded:true;connectionID:" + RC.currentConnectionID);
                                    }
                                }
                                break;

                            case "NCC":
                                GUIWindow.PrintLog("NCC: Received CallConfirmationResponse() from other NCC");
                                if (!cachedSuccess) {
                                    GUIWindow.PrintLog("NCC: Sent CallRequestResponse(SUCCEEDED: false) to CPCC");
                                    lastCaller.SendMessage("component:CPCC;name:CallRequestResponse;succeeded:false");
                                }
                                else {
                                    GUIWindow.PrintLog("NCC: Sent CallRequestResponse(SUCCEEDED: true, connectionID: " + RC.currentConnectionID + ") to CPCC");
                                    lastCaller.SendMessage("component:CPCC;name:CallRequestResponse;succeeded:true;connectionID:" + RC.currentConnectionID);
                                }
                                break;
                        }
                        break;

                    case "ConnectionRequestResponse":
                        cachedSuccess = Convert.ToBoolean(data["succeeded"]);
                        if(lastIDC) {
                            if (!Convert.ToBoolean(data["succeeded"])) {
                                GUIWindow.PrintLog("NCC: Received ConnectionRequestResponse(SUCCEEDED: false) from CC");
                                GUIWindow.PrintLog("NCC: Sent CallConfirmation(SUCCEEDED: false) to other NCC");
                                Program.peerConnection.SendMessage("component:NCC;name:CallConfirmation;IDC:true;succeeded:false");
                            }
                            else {
                                GUIWindow.PrintLog("NCC: Received ConnectionRequestResponse(SUCCEEDED: true, connectionID: " + data["connID"] + ") from CC");
                                GUIWindow.PrintLog("NCC: Sent CallConfirmation(SUCCEEDED: true, connectionID: " + data["connID"] + ") to other NCC");
                                Program.peerConnection.SendMessage("component:NCC;name:CallConfirmation;IDC:true;succeeded:true;connID:" + data["connID"]);
                            }
                        } else {
                            if (!Convert.ToBoolean(data["succeeded"])) {
                                GUIWindow.PrintLog("NCC: Received ConnectionRequestResponse(SUCCEEDED: false) from CC");
                                GUIWindow.PrintLog("NCC: Sent CallConfirmation(SUCCEEDED: false) to CPCC");
                                cachedHostYConnection.SendMessage("component:CPCC;name:CallConfirmation;succeeded:false;IDC:false");
                            }
                            else {
                                GUIWindow.PrintLog("NCC: Received ConnectionRequestResponse(SUCCEEDED: true, connectionID: " + data["connID"] + ") from CC");
                                GUIWindow.PrintLog("NCC: Sent CallConfirmation(SUCCEEDED: true, connectionID: " + data["connID"] + ") to CPCC");
                                cachedHostYConnection.SendMessage("component:CPCC;name:CallConfirmation;succeeded:true;IDC:false;connID:" + data["connID"]);
                            }
                        }
                        break;
                }
            } catch(Exception e) {
                GUIWindow.PrintLog(e.Message);
                GUIWindow.PrintLog(e.StackTrace);
            }
        }

        private void ConnectionRequest(bool interDomainConnection) {
            GUIWindow.PrintLog("NCC: Sent ConnectionRequest(" + lastRouterXIP + ", " + lastRouterYIP + ", " + lastSpeed + " Gb/s, InterDomainConnection: " + interDomainConnection + ") to CC");
            string message = "name:ConnectionRequest;routerX:" + lastRouterXIP + ";routerY:" + lastRouterYIP + ";speed:" + lastSpeed + ";IDC:" + interDomainConnection.ToString() + ";layer:upper";
            Program.cc.HandleRequest(Util.DecodeRequest(message));
        }

        private void HandlePolicy() {
            GUIWindow.PrintLog("NCC: Sent Policy() to CAC");
            GUIWindow.PrintLog("NCC: Received PolicyResponse(OK)");
        }

        private Tuple<string, string, int, int> HandleDirectory(string hostX, string hostY) {
            GUIWindow.PrintLog("NCC: Sent DirectoryRequest(" + hostX + ") to Directory");
            string routerXIP;
            int routerXAsId = 0;
            try {
                Tuple<string, int> info = directory[hostX];
                routerXIP = info.Item1;
                routerXAsId = info.Item2;
            } catch(Exception) {
                routerXIP = "WRONG NAME";
            }
            GUIWindow.PrintLog("NCC: Received DirectoryRequestResponse(" + routerXIP + ", "+ routerXAsId + ") from Directory");
            GUIWindow.PrintLog("NCC: Sent DirectoryRequest(" + hostY + ") to Directory");

            string routerYIP;
            int routerYAsId = 0;
            try {
                Tuple<string, int> info = directory[hostY];
                routerYIP = info.Item1;
                routerYAsId = info.Item2;
            } catch (Exception) {
                routerYIP = "WRONG NAME";
            }
            GUIWindow.PrintLog("NCC: Received DirectoryRequestResponse(" + routerYIP + ", " + routerYAsId + ") from Directory");

            return new Tuple<string, string, int, int>(routerXIP, routerYIP, routerXAsId, routerYAsId);
        }

        private void RefuseConnection(HostConnection caller) {
            string message = "component:CPCC;name:CallRequestResponse;succeeded:false";
            caller.SendMessage(message);
        }

        
    }
}
