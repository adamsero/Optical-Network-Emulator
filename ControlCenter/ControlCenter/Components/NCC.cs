using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class NCC {

        //TODO: można to wczytać w configu ale nie musi być
        private Dictionary<string, string> directory = new Dictionary<string, string>();

        public NCC() {
            directory.Add("Host1", "10.0.1.2");
            directory.Add("Host2", "10.0.2.2");
            directory.Add("Host3", "10.0.5.2");
            directory.Add("Host4", "10.0.4.2");
            directory.Add("10.0.1.2", "Host1");
            directory.Add("10.0.2.2", "Host2");
            directory.Add("10.0.5.2", "Host3");
            directory.Add("10.0.4.2", "Host4");
        }

        public void HandleRequest(Dictionary<string, string> data, HostConnection caller) {
            switch(data["name"]) {
                case "CallRequest":
                    GUIWindow.PrintLog("NCC: Received CallRequest(" + data["hostX"] + ", " + data["hostY"] + ", " + data["speed"] + " Gb/s) from CPCC");

                    Tuple<string, string> routerIPs = HandleDirectory(data["hostX"], data["hostY"]);
                    if(routerIPs.Item1.Equals("WRONG NAME") || routerIPs.Item2.Equals("WRONG NAME")) {
                        GUIWindow.PrintLog("NCC: Host name does not exist");
                        GUIWindow.PrintLog("NCC: Sent CallRequestResponse(UNSUCCESSFUL)");
                        RefuseConnection(caller);
                        return;
                    } else if(routerIPs.Item1.Equals(routerIPs.Item2)) {
                        GUIWindow.PrintLog("NCC: Target Host name is the same as the source one");
                        GUIWindow.PrintLog("NCC: Sent CallRequestResponse(UNUSCCESSFUL)");
                        RefuseConnection(caller);
                        return;
                    }
                    HandlePolicy();

                    string message = "component:NCC;name:CallCoordination;routerX:" + routerIPs.Item1 + ";routerY:" + routerIPs.Item2 + ";speed:" + data["speed"];
                    Program.peerConnection.SendMessage(message);
                    GUIWindow.PrintLog("NCC: Sent CallCoordination(" + routerIPs.Item1 + ", " + routerIPs.Item2 + ", " + data["speed"] + " Gb/s) to other NCC");

                    break;

                case "CallCoordination":
                    
                    GUIWindow.PrintLog("NCC: Received CallCoordination(" + data["routerX"] + ", " + data["routerY"] + ", " + data["speed"] + " Gb/s) from other NCC");

                    //odwrotne dzialanie funkcji -- dajemy IP dostajemy nazwy
                    routerIPs = HandleDirectory(data["routerX"], data["routerY"]);
                    /*
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
                    */
                    HandlePolicy();
                    HostConnection hostConnection = Server.GetHostConnectionByIP(data["routerY"]);
                    message = "component:CPCC;name:CallAccept;routerX:" + routerIPs.Item1 + ";routerY:" + routerIPs.Item2 + ";speed:" + data["speed"];
                    hostConnection.SendMessage(message);
                    GUIWindow.PrintLog("NCC: Sent CallAccept(" + routerIPs.Item1 + ", " + routerIPs.Item2 + ", " + data["speed"] + " Gb/s) to other NCC");
                    
                    
                    break;

                case "CallAcceptResponse":
                    GUIWindow.PrintLog("ODEBRALEM");
                    break;
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
