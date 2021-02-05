﻿using System;
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
        }

        public void HandleRequest(Dictionary<string, string> data, HostConnection caller) {
            switch(data["name"]) {
                case "CallRequest":
                    GUIWindow.PrintLog("NCC: Received CallRequest(" + data["hostX"] + ", " + data["hostY"] + ", " + data["speed"] + " Gb/s) from CPCC");

                    
                    Tuple<string, string> routerIPs = HandleDirectory(data["hostX"], data["hostY"]);
                    if(routerIPs.Item1.Equals("WRONG NAME") || routerIPs.Item2.Equals("WRONG NAME")) {
                        GUIWindow.PrintLog("NCC: Sent CallRequestResponse(UNSUCCESSFUL)");
                        RefuseConnection(caller);
                        return;
                    }
                    HandlePolicy();

                    //tutaj CallCoordination

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
