using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class CC {

        public void ConnectionRequest(string routerXIP, string routerYIP, int speed) {
            GUIWindow.PrintLog("CC: Received ConnectionRequest(" + routerXIP + ", " + routerYIP + ", " + speed + " Gb/s) from NCC");
        }

    }
}
