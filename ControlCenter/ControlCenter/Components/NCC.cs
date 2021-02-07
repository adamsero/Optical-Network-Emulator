using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class NCC {

        private Dictionary<string, Tuple<string, int>> directory = new Dictionary<string, Tuple<string, int>>();

        //cached data
        public HostConnection lastCaller;
        public string lastRouterXIP;
        public string lastRouterYIP;
        public int lastSpeed;
        public bool lastIDC;

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
                }
            } catch(Exception e) {
                GUIWindow.PrintLog(e.Message);
                GUIWindow.PrintLog(e.StackTrace);
            }
        }

        private void ConnectionRequest(bool interDomainConnection) {
            GUIWindow.PrintLog("NCC: Sent ConnectionRequest(" + lastRouterXIP + ", " + lastRouterYIP + ", " + lastSpeed + " Gb/s, InterDomainConnection: " + interDomainConnection + ") to CC");
            string message = "name:ConnectionRequest;routerX:" + lastRouterXIP + ";routerY:" + lastRouterYIP + ";speed:" + lastSpeed + ";IDC:" + interDomainConnection.ToString();
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
