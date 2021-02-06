﻿using System;
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

                case "ConnectionTeardown":
                    GUIWindow.PrintLog("CC: Received ConnectionTeardown(" + data["connectionID"] + ") from NCC"); // InterDomainConnection: " + data["IDC"]

                    var element = NCC.callRegister[Int32.Parse(data["connectionID"])];
                    bool interDomainConnectionFlag = element.GetInterDomainConnectionFlag();
                    int asID = element.GetStartAsID();

                    if (!interDomainConnectionFlag && asID == ConfigLoader.ccID) {
                        //do External LRM
                    }
                    else { 
                        //do Local LRM
                    }

                    //string message = "component:RC;name:RouteTableQuery;routerX:" + data["routerX"] + ";routerY:" + data["routerY"] + ";speed:" + data["speed"] + ";IDC:" + data["IDC"] + ";case:" + networkCase;
                    break;
            }
        }

    }
}
