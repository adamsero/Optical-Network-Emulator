using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class NCC {

        private Dictionary<string, string> directory = new Dictionary<string, string>();

        //cached data
        private HostConnection lastCaller;
        private string lastRouterXIP;
        private string lastRouterYIP;
        private int lastSpeed;

        public NCC() {
            directory.Add("Host1", "10.0.1.1");
            directory.Add("Host2", "10.0.2.1");
            directory.Add("Host3", "10.0.5.1");
            directory.Add("Host4", "10.0.4.1");
            directory.Add("10.0.1.1", "Host1");
            directory.Add("10.0.2.1", "Host2");
            directory.Add("10.0.5.1", "Host3");
            directory.Add("10.0.4.1", "Host4");
        }

        public void HandleRequest(Dictionary<string, string> data, HostConnection caller) {
            try {
                switch (data["name"]) {
                    case "CallRequest":
                        GUIWindow.PrintLog("NCC: Received CallRequest(" + data["hostX"] + ", " + data["hostY"] + ", " + data["speed"] + " Gb/s) from CPCC");

                        Tuple<string, string> routerIPs = HandleDirectory(data["hostX"], data["hostY"]);
                        if (routerIPs.Item1.Equals("WRONG NAME") || routerIPs.Item2.Equals("WRONG NAME")) {
                            GUIWindow.PrintLog("NCC: Host name does not exist");
                            GUIWindow.PrintLog("NCC: Sent CallRequestResponse(UNSUCCESSFUL)");
                            RefuseConnection(caller);
                            return;
                        }
                        else if (routerIPs.Item1.Equals(routerIPs.Item2)) {
                            GUIWindow.PrintLog("NCC: Target Host name is the same as the source one");
                            GUIWindow.PrintLog("NCC: Sent CallRequestResponse(UNUSCCESSFUL)");
                            RefuseConnection(caller);
                            return;
                        }
                        HandlePolicy();

                        lastCaller = caller;
                        lastRouterXIP = routerIPs.Item1;
                        lastRouterYIP = routerIPs.Item2; ;
                        lastSpeed = Convert.ToInt32(data["speed"]);

                        string message = "component:NCC;name:CallCoordination;routerX:" + routerIPs.Item1 + ";routerY:" + routerIPs.Item2 + ";speed:" + data["speed"];
                        Program.peerConnection.SendMessage(message);
                        GUIWindow.PrintLog("NCC: Sent CallCoordination(" + routerIPs.Item1 + ", " + routerIPs.Item2 + ", " + data["speed"] + " Gb/s) to other NCC");

                        break;

                    case "CallCoordination":

                        GUIWindow.PrintLog("NCC: Received CallCoordination(" + data["routerX"] + ", " + data["routerY"] + ", " + data["speed"] + " Gb/s) from other NCC");

                        //odwrotne dzialanie funkcji -- dajemy IP dostajemy nazwy
                        routerIPs = HandleDirectory(data["routerX"], data["routerY"]);
                        HandlePolicy();
                        string hostYIP = data["routerY"].Remove(data["routerY"].Length - 1, 1) + "2";
                        HostConnection hostConnection = Server.GetHostConnectionByIP(hostYIP);
                        message = "component:CPCC;name:CallAccept;routerX:" + routerIPs.Item1 + ";routerY:" + routerIPs.Item2 + ";speed:" + data["speed"];
                        hostConnection.SendMessage(message);
                        GUIWindow.PrintLog("NCC: Sent CallAccept(" + routerIPs.Item1 + ", " + routerIPs.Item2 + ", " + data["speed"] + " Gb/s) to other NCC");
                        break;

                    case "CallCoordinationResponse":
                        if (!Convert.ToBoolean(data["succeeded"])) {
                            GUIWindow.PrintLog("NCC: Received CallRequestResponse(DENIED) from other NCC");
                            RefuseConnection(lastCaller);
                        }
                        else {
                            GUIWindow.PrintLog("NCC: Received CallRequestResponse(ACCEPTED) from other NCC");
                            GUIWindow.PrintLog("NCC: Sent ConnectionRequest(" + lastRouterXIP + ", " + lastRouterYIP + ", " + lastSpeed + " Gb/s) to CC");
                            Program.cc.ConnectionRequest(lastRouterXIP, lastRouterYIP, lastSpeed);
                            //message = "component:CPCC;name:CallRequestResponse;succeeded:true;connectionID:0"; //hard-kodowane connID na razie
                            //lastCaller.SendMessage(message);
                        }
                        break;

                    case "CallAcceptResponse":
                        string status = "ACCEPTED";
                        if (!Convert.ToBoolean(data["succeeded"])) {
                            message = "component:NCC;name:CallCoordinationResponse;succeeded:false";
                            status = "DENIED";
                        }
                        else {
                            message = "component:NCC;name:CallCoordinationResponse;succeeded:true";
                        }
                        Program.interCCServer.peerConnection.SendMessage(message);
                        GUIWindow.PrintLog("NCC: Sent CallCoordinationResponse(" + status + ") to other NCC");
                        break;
                }
            } catch(Exception e) {
                GUIWindow.PrintLog(e.Message);
                GUIWindow.PrintLog(e.StackTrace);
            }
        }

        private void HandlePolicy() {
            GUIWindow.PrintLog("NCC: Sent Policy() to CAC");
            GUIWindow.PrintLog("NCC: Received PolicyResponse(OK)");
        }

        private Tuple<string, string> HandleDirectory(string hostX, string hostY) {
            GUIWindow.PrintLog("NCC: Sent DirectoryRequest(" + hostX + ") to Directory");
            string routerXIP;
            try {
                routerXIP = directory[hostX];
            } catch(Exception) {
                routerXIP = "WRONG NAME";
            }
            GUIWindow.PrintLog("NCC: Received DirectoryRequestResponse(" + routerXIP + ") from Directory");
            GUIWindow.PrintLog("NCC: Sent DirectoryRequest(" + hostY + ") to Directory");
            string routerYIP;
            try {
                routerYIP = directory[hostY];
            } catch (Exception) {
                routerYIP = "WRONG NAME";
            }
            GUIWindow.PrintLog("NCC: Received DirectoryRequestResponse(" + routerYIP + ") from Directory");

            return new Tuple<string, string>(routerXIP, routerYIP);
        }

        private void RefuseConnection(HostConnection caller) {
            string message = "component:CPCC;name:CallRequestResponse;succeeded:false";
            caller.SendMessage(message);
        }

        
    }
}
