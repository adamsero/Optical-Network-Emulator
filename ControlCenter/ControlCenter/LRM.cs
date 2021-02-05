using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class LRM {

        ConnectionControl cc;
        RouteControl rc;

        public LRM (ConnectionControl cc, RouteControl rc){
            this.cc = cc;
            this.rc = rc;
        }

        public void HandleLocalTopology(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool afterPeerCoordination, bool childLevel) {
            GUIWindow.PrintLog("LRM: Received request: LocalTopology() from RC", cc.GetNCC().getAsID());
            GUIWindow.PrintLog("LRM: Sending request: LocalTopologyResponse() from RC", cc.GetNCC().getAsID());
            rc.HandleLocalTopologyResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination, childLevel);
        }

        public void HandleLinkConnectionRequestInternal(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool afterPeerCoordination) {
            GUIWindow.PrintLog("LRM: Received request: LinkConnectionRequest() from CC", cc.GetNCC().getAsID());
            
            //TODO: Decyzja o tym czy wewnetrzne lacze mozna 
            //==============================================
            bool allowed = true;

            if (isInternal(startIP, endIP) && !Program.ConnectionAvaliable) {
                allowed = false;
            }

            //allowed = false ??
            //==============================================
            if (allowed) {
                 GUIWindow.PrintLog("LRM: Sending request: LinkConnectionRequestResponse() to CC : ALLOWED", cc.GetNCC().getAsID());            
            }
            else {
                GUIWindow.PrintLog("LRM: Sending request: LinkConnectionRequestResponse() to CC : DENIED", cc.GetNCC().getAsID());
            }
            cc.HandleLinkConnectionRequestInternalResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination, allowed);
            
            if (isInternal(startIP, endIP) && !Program.ConnectionAvaliable) {
                HandleSecondSideAllocationFailed(startIP, endIP, linkSpeed, startHostName, destinationHostName, true);
            }
        }

        public void HandleLinkConnectionRequestExternal(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool afterPeerCoordination) {
            GUIWindow.PrintLog("extLRM: Received request: LinkConnectionRequest() from CC", cc.GetNCC().getAsID());

            GUIWindow.PrintLog("extLRM: Sending request: SNPNegotiation() to other AS LRM", cc.GetNCC().getAsID());
            LRM other = cc.GetNCC().getOtherNCC().getConnectionControl().getLRM();
            String resp = other.HandleSNPNegotiation(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination);
            
            
            if (resp.Equals("OK")) {
                GUIWindow.PrintLog("extLRM: Received request: SNPNegotiationResponse() from CC : SUCCESS", cc.GetNCC().getAsID());
                GUIWindow.PrintLog("extLRM: Sending request: LinkConnectionRequestResponse() to CC : ALLOCATED", cc.GetNCC().getAsID());
                cc.HandleLinkConnectionRequestExternalResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination, true);
            }
            else {
                GUIWindow.PrintLog("extLRM: Received request: SNPNegotiationResponse() from CC : DENIED", cc.GetNCC().getAsID());
                GUIWindow.PrintLog("extLRM: Sending request: LinkConnectionRequestResponse() to CC : DENIED", cc.GetNCC().getAsID());
                cc.HandleLinkConnectionRequestExternalResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination, false);
                bool internalConnection = isInternal(startIP, endIP);
                if (internalConnection) {
                    HandleSecondSideAllocationFailed(startIP, endIP, linkSpeed, startHostName, destinationHostName, internalConnection);
                }
                else {
                    cc.GetNCC().getOtherNCC().getConnectionControl().getLRM().HandleSecondSideAllocationFailed(startIP, endIP, linkSpeed, startHostName, destinationHostName, internalConnection);
                }
            }
        }

        public String HandleSNPNegotiation(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool afterPeerCoordination) {
            GUIWindow.PrintLog("extLRM: Received request: SNPNegotiation() from other AS LRM", cc.GetNCC().getAsID());
            GUIWindow.PrintLog("extLRM: Sending request: SNPNegotiationResponse() to other AS LRM", cc.GetNCC().getAsID());
            bool ok = true;
            //TODO: obsługa czy przydzielono czy nie
            // =======================

            if (!Program.ConnectionAvaliable) {
                ok = false;
            }
            else {
                ok = true;
            }

            // =======================


            if (ok) {
                return "OK";
            }
            else {
                return "DENIED";
            }                
        }

        public void HandleSNPRelease(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName) {
            GUIWindow.PrintLog("LRM: Received request: SNPRelease() from other AS LRM", cc.GetNCC().getAsID());
            GUIWindow.PrintLog("LRM: Sending request: SNPReleaseResponse() to other AS LRM : DEALLOCATED", cc.GetNCC().getAsID());
        }

        private bool isInternal(String startIP, String endIP) {

            bool startMatch = false;
            bool endMatch = false;
            foreach (Host h in ConfigLoader.GetHosts()) {
                if (h.getIP().Equals(startIP) && h.GetAsID() == cc.GetNCC().getAsID()) {
                    startMatch = true;
                }
                if (h.getIP().Equals(endIP) && h.GetAsID() == cc.GetNCC().getAsID()) {
                    endMatch = true;
                }
            }

            if (startMatch && endMatch) {
                return true;
            }
            else {
                return false;
            }

        }

        private void HandleSecondSideAllocationFailed(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool internalConnection) {
            if (internalConnection) {
                this.cc.GetNCC().SendInfoToSecondSideHost(startIP, endIP, linkSpeed, startHostName, destinationHostName, internalConnection);
            }
            else {
                GUIWindow.PrintLog("LRM: Sending request: SNPNegotiation(): FAILURE to CC", cc.GetNCC().getAsID());
                GUIWindow.PrintLog("CC: Received request: SNPNegotiation(): FAILURE from LRM", cc.GetNCC().getAsID());
                GUIWindow.PrintLog("CC: Sending request: ConnectionRequest(): FAILURE to NCC", cc.GetNCC().getAsID());
                GUIWindow.PrintLog("NCC: Received request: ConnectionRequest(): FAILURE from CC", cc.GetNCC().getAsID());
                this.cc.GetNCC().SendInfoToSecondSideHost(startIP, endIP, linkSpeed, startHostName, destinationHostName, internalConnection);
            }
        }
    }
}
