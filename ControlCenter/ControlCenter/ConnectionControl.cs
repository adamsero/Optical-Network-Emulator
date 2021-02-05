using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ControlCenter {
    class ConnectionControl {

        private NCC ncc;
        private ConnectionControl childCC = null;
        private ConnectionControl parentCC = null;
        private RouteControl rc;
        private LRM lrm;
        private bool child = false;
        private bool parent = false;
        private bool internalConnection = false;
        private static String parentStartIP;
        private static String parentEndIP;

        public ConnectionControl(NCC ncc) {
            this.ncc = ncc;
            this.rc = new RouteControl(this);
            this.lrm = new LRM(this, rc);
            rc.setLRM(lrm);
        }

        public LRM getLRM() {
            return lrm;
        }

        public ConnectionControl(NCC ncc, ConnectionControl parentCC) {
            this.ncc = ncc;
            this.parentCC = parentCC;
            this.rc = new RouteControl(this);
            this.lrm = new LRM(this, rc);
            rc.setLRM(lrm);

            //jestesmy w tym nizszym CC
            if (parentCC != null & childCC == null) {
                child = true;
            }

            if (parentCC == null & childCC != null) {
                parent = true;
            }
        }

        public NCC GetNCC() {
            return ncc;
        }

        public RouteControl GetRouteControl() {
            return rc;
        }

        public void setChildConnectionControl(ConnectionControl childCC) {
            this.childCC = childCC;

            //jestesmy w tym nizszym CC
            if (parentCC != null & childCC == null) {
                child = true;
            }

            if (parentCC == null & childCC != null) {
                parent = true;
            }
        }
        
        public void HandleConnectionRequestFromNCC(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool internalConnection) {
            GUIWindow.PrintLog("CC: Received request: ConnectionRequest( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s )", ncc.getAsID());
            GUIWindow.PrintLog("CC: Sending request: RouteTableQuery( " + startIP + ", " + endIP + " )", ncc.getAsID());
            this.internalConnection = internalConnection;
            if (internalConnection) {
                //UWAGA
                //TUTAJ DAJEMY afterPeerCoordination na TRUE bo nie chcemy miec gadania z ext LRM
                //POTEM PRZECHWYTUJEMY CIAG ZDARZEN W MOMENCIE PRZED WYSLANIEM PeerCoordinationResponse
                //I ZWRACAMY SUKCES/BLAD DO NCC
                rc.HandleRouteTableQuery(startIP, endIP, linkSpeed, startHostName, destinationHostName, true, false);
            }
            else {
                rc.HandleRouteTableQuery(startIP, endIP, linkSpeed, startHostName, destinationHostName, false, false);
            }
        }

        public void HandleConnectionRequestFromParentCC(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool afterPeerCoordination) {
            GUIWindow.PrintLog("#-------------------------------#", ncc.getAsID());
            GUIWindow.PrintLog("CHILD#CC: Received request: ConnectionRequest( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s )", ncc.getAsID());
            GUIWindow.PrintLog("CHILD#CC: Sending request: RouteTableQuery()", ncc.getAsID());
            //Tutaj dajemy w argumencie afterPeerCoordination na TRUE tylko po to żeby z niższego poziomu nie robił peer coordination
            rc.HandleRouteTableQuery(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination, true);
        }

        public void HandleConnectionRequestChildResponse(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool afterPeerCoordination, bool success){
            if (success){
                GUIWindow.PrintLog("CC: Received request: ConnectionRequestResponse( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) from CHILD CC : SUCCESS", ncc.getAsID());
                GUIWindow.PrintLog("CC: Sending request: LinkConnectionRequest() to internal LRM", ncc.getAsID());
                lrm.HandleLinkConnectionRequestInternal(parentStartIP, parentEndIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination);
            }
            else{
                 GUIWindow.PrintLog("CC: Received request: ConnectionRequestResponse(" + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s) from CHILD CC : FAILED", ncc.getAsID());
                 GUIWindow.PrintLog("CC: Sending request: ConnectionRequestResponse( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) to NCC : FAILED", ncc.getAsID());
                 ncc.HandleConnectionRequestResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, false);
            }
        }

        //w tej metodzie zestaw argumentow moze byc inny
        public void HandleRouteTableQueryResponse(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool afterPeerCoordination) {
            //TODO: mozna dodac print sciezki -- nw czy trzeba

            String logPrefix;
            if (child) {
                logPrefix = "CHILD#CC";
            }
            else {
                logPrefix = "CC";
            }
            GUIWindow.PrintLog(logPrefix + ": Received request: RouteTableQueryResponse()", ncc.getAsID());

            //Trzeba zdecydowac czy sciezka przebiega przez podsiec 
            //Na podstawie tego wywolac lub nie zestawianie polaczenia na nizszym poziomie
            bool throughSubnet = true;

            if (this.internalConnection) {
                throughSubnet = false;
            }

            if (throughSubnet && parent) {
                //Przejscie poziom nizej do child CC
                parentStartIP = startIP;
                parentEndIP = endIP;
                GUIWindow.PrintLog("Sending request: ConnectionRequest( " + RouteControl.subnetPair.Item1.getIP() + ", " + RouteControl.subnetPair.Item2.getIP() + ", " + linkSpeed + " Gb/s ) to ChildConnectionControl", this.GetNCC().getAsID());
                childCC.HandleConnectionRequestFromParentCC(RouteControl.subnetPair.Item1.getIP(), RouteControl.subnetPair.Item2.getIP(), linkSpeed, startHostName, destinationHostName, afterPeerCoordination);
            }
            else{
                GUIWindow.PrintLog(logPrefix + ": Sending request: LinkConnectionRequest() to internal LRM", ncc.getAsID());
                lrm.HandleLinkConnectionRequestInternal(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination);
            }
        
        }
        
        public void HandleLinkConnectionRequestInternalResponse(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool afterPeerCoordination, bool allowed){
            if (allowed) {
                if (child) {
                    GUIWindow.PrintLog("CHILD#CC: Received request: LinkConnectionRequest() from internal LRM : ALLOCATED", ncc.getAsID());
                }
                else {
                    GUIWindow.PrintLog("CC: Received request: LinkConnectionRequest() from internal LRM : ALLOCATED", ncc.getAsID());
                }
                
                if (!afterPeerCoordination && !child){
                    GUIWindow.PrintLog("CC: Sending request: LinkConnectionRequest() to external LRM", ncc.getAsID());
                    lrm.HandleLinkConnectionRequestExternal(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination);
                }
                else if (!child) {
                    if (internalConnection) {
                        GUIWindow.PrintLog("CC: Sending request: ConnectionRequestResponse( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) to NCC : SUCCESS", ncc.getAsID());
                        ncc.HandleConnectionRequestResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, true);
                    }
                    else {
                        //AS 2 górny
                        PrintLogsContaining(ncc.getAsID().ToString(), ncc.getAsID());

                        GUIWindow.PrintLog("CC: Sending request: PeerCoordinationResponse() to other AS CC", ncc.getAsID());
                        GetNCC().getOtherNCC().getConnectionControl().HandlePeerCoordinationResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, true);
                    }
                }
                else {
                    GUIWindow.PrintLog("CHILD#CC:Sending request: ConnectionRequestResponse( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) to ParentConnectionControl : OK", this.GetNCC().getAsID());
                    //AS dziecięcy
                    //foreach(var pair in RouteControl.savedLogs)
                    //    GUIWindow.PrintLog(pair.Item1 + " --- " + pair.Item2, ncc.getAsID());
                    PrintLogsContaining("SN", ncc.getAsID());
                    
                    GUIWindow.PrintLog("#-------------------------------#", ncc.getAsID());
                    parentCC.HandleConnectionRequestChildResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination, true);
                }
            }
            else{
                GUIWindow.PrintLog("CC: Received request: LinkConnectionRequest() from internal LRM : DENIED", ncc.getAsID());
                GUIWindow.PrintLog("CC: Sending request: ConnectionRequestResponse() to NCC : DENIED", ncc.getAsID());
                ncc.HandleConnectionRequestResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, false);
            }
            /*
            if (!afterPeerCoordination && !child){
                GUIWindow.PrintLog(logPrefix + ": Sending request: LinkConnectionRequest() to external LRM", ncc.getAsID());
                String extResponse = lrm.HandleLinkConnectionRequestExternal(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination);
                if (response.Equals("OK")) {
                    GUIWindow.PrintLog(logPrefix + ": Received request: LinkConnectionRequest() from external LRM : ALLOCATED", ncc.getAsID());

                    GUIWindow.PrintLog(logPrefix + ": Sending request: PeerCoordination() to other AS CC", ncc.getAsID());
                    this.GetNCC().getOtherNCC().getConnectionControl().HandlePeerCoordination(startIP, endIP, linkSpeed, startHostName, destinationHostName);

                }
                else {
                    GUIWindow.PrintLog(logPrefix + ": Received request: LinkConnectionRequest() from external LRM : DENIED", ncc.getAsID());
                }    

            }
            else if (!child){
                
            }
            */
    
        }

        public static void PrintLogsContaining(string keyWord, int id) {
            List<string> knownLogs = new List<string>();
            for (int i = RouteControl.savedLogs.Count - 1; i >= 0; i--) {
                Tuple<string, string> pair = RouteControl.savedLogs[i];
                if (pair.Item1.Equals(keyWord)) {
                    if (!knownLogs.Contains(pair.Item2)) {
                        GUIWindow.PrintLog(pair.Item2, id);
                        knownLogs.Add(pair.Item2);
                    }
                    RouteControl.savedLogs.Remove(pair);
                }
            }
        }

        public void HandleLinkConnectionRequestExternalResponse(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool afterPeerCoordination, bool success){
            
            if (success){
                GUIWindow.PrintLog("CC: Received request: LinkConnectionRequestResponse() from external LRM : ALLOCATED", ncc.getAsID());
                // AS 1 górny
                PrintLogsContaining(ncc.getAsID().ToString(), ncc.getAsID());
                GUIWindow.PrintLog("CC: Sending request: PeerCoordinaton(" + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s) to other AS CC", ncc.getAsID());
                this.GetNCC().getOtherNCC().getConnectionControl().HandlePeerCoordination(startIP, endIP, linkSpeed, startHostName, destinationHostName);
            }
            else {
                GUIWindow.PrintLog("CC: Received request: LinkConnectionRequestResponse() from external LRM : DENIED", ncc.getAsID());
                GUIWindow.PrintLog("CC: Sending request: ConnectionRequestResponse(" + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s) to other AS CC : FAILED", ncc.getAsID());
                ncc.HandleConnectionRequestResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, false);
            }
        }

        public void HandlePeerCoordination(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName) {
            GUIWindow.PrintLog("CC: Received request: PeerCoordination(" + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s) from other AS CC", ncc.getAsID());
            GUIWindow.PrintLog("CC: Sending request: RouteTableQuery() to RC", ncc.getAsID());
            bool afterPeerCoordination = true;
            rc.HandleRouteTableQuery(startIP, endIP, linkSpeed, startHostName, destinationHostName, afterPeerCoordination, false);         
        }

        public void HandlePeerCoordinationResponse(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool success) {
            if (success) {
                GUIWindow.PrintLog("CC: Received request: PeerCoordinationResponse(" + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s) from other AS CC : SUCCESS", ncc.getAsID());
                GUIWindow.PrintLog("CC: Sending request: ConnectionRequestResponse( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) to NCC : SUCCESS", ncc.getAsID());
                ncc.HandleConnectionRequestResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, true);
            }
            else {
                GUIWindow.PrintLog("CC: Received request: PeerCoordinationResponse(" + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s) from other AS CC : FAILED", ncc.getAsID());
                GUIWindow.PrintLog("CC: Sending request: ConnectionRequestResponse( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) to NCC : FAILED", ncc.getAsID());
                ncc.HandleConnectionRequestResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, false);
            }
        }

        //===========================================================================================================
        //ROZLACZANIE
        //===========================================================================================================

        public void HandleConnectionTeardownRequest(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool disconnectExternal) {
            
            GUIWindow.PrintLog("CC: Received request: ConnectionTeardown( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) from NCC", ncc.getAsID());
            if (disconnectExternal) {
                //GUIWindow.PrintLog("Discon ID: " + ncc.disconnectingID);
                RouteControl.LinkConnID.TryGetValue(ncc.disconnectingID, out Path pathToRemove);
                rc.UpdateRoutingTables(pathToRemove, ncc.disconnectingID, false, true);
                rc.ClearConnectionGaps(ncc.disconnectingID);
                GUIWindow.UpdateChannelTable();
            }

            //jesli ma childa to najpierw jego rozlacz
            if (parent) {
                //TUTAJ NA SZTYWNO SB WYPISUJE LOGA BO NIE MA SENSU ODWOŁYWAĆ SIĘ DO OBIEKTÓW ŻEBY TYLKO WPISAŁY COŚ DO LOGA
                GUIWindow.PrintLog("CC: Sending request: ConnectionTeardown( " + RouteControl.subnetPair.Item1.getIP() + ", " + RouteControl.subnetPair.Item2.getIP() + ", " + linkSpeed + " Gb/s ) to Child ConnectionControl", ncc.getAsID());
                GUIWindow.PrintLog("CHILD#CC: Received request: ConnectionTeardown( " + RouteControl.subnetPair.Item1.getIP() + ", " + RouteControl.subnetPair.Item2.getIP() + ", " + linkSpeed + " Gb/s ) from Parent ConnectionControl", ncc.getAsID());
                GUIWindow.PrintLog("CHILD#CC: Sending request: LinkConnectionDeallocation() to internal LRM", ncc.getAsID());
                GUIWindow.PrintLog("LRM: Received request: LinkConnectionDeallocation() from CHILD#CC", ncc.getAsID());
                PrintLogsContaining("SN", ncc.getAsID());
                GUIWindow.PrintLog("LRM: Sending request: LinkConnectionDeallocationResponse() to CHILD#CC : DEALLOCATED", ncc.getAsID());
                GUIWindow.PrintLog("CHILD#CC: Received request: LinkConnectionDeallocationResponse() from LRM : DEALLOCATED", ncc.getAsID());
                GUIWindow.PrintLog("CHILD#CC: Sending request: ConnectionTeardownResponse(" + RouteControl.subnetPair.Item1.getIP() + ", " + RouteControl.subnetPair.Item2.getIP() + ", " + linkSpeed + " Gb/s ) to Parent ConnectionControl", ncc.getAsID());
                GUIWindow.PrintLog("CC: Received request: ConnectionTeardownResponse( " + RouteControl.subnetPair.Item1.getIP() + ", " + RouteControl.subnetPair.Item2.getIP() + ", " + linkSpeed + " Gb/s ) from Child ConnectionControl", ncc.getAsID());
            }

            if (disconnectExternal) {
                GUIWindow.PrintLog("CC: Sending request: LinkConnectionDeallocation() to external LRM", ncc.getAsID());
                GUIWindow.PrintLog("LRM: Received request: LinkConnectionDeallocation() from CC", ncc.getAsID());
                GUIWindow.PrintLog("LRM: Sending request: SNPRelease() to other AS LRM", ncc.getAsID());
                this.GetNCC().getOtherNCC().getConnectionControl().getLRM().HandleSNPRelease(startIP, endIP, linkSpeed, startHostName, destinationHostName);
                GUIWindow.PrintLog("LRM: Received request: SNPReleaseResponse() from other AS LRM", ncc.getAsID());
                GUIWindow.PrintLog("LRM: Sending request: LinkConnectionDeallocationResponse() to CC : DEALLOCATED", ncc.getAsID());
                GUIWindow.PrintLog("CC: Received request: LinkConnectionDeallocationResponse() from LRM : DEALLOCATED", ncc.getAsID());
            }

            Thread.Sleep(100);
            GUIWindow.PrintLog("CC: Sending request: LinkConnectionDeallocation() to internal LRM", ncc.getAsID());
            GUIWindow.PrintLog("LRM: Received request: LinkConnectionDeallocation() from CC", ncc.getAsID());
            PrintLogsContaining(ncc.getAsID().ToString(), ncc.getAsID());
            GUIWindow.PrintLog("LRM: Sending request: LinkConnectionDeallocationResponse() to CC : DEALLOCATED", ncc.getAsID());
            GUIWindow.PrintLog("CC: Received request: LinkConnectionDeallocationResponse() from LRM : DEALLOCATED", ncc.getAsID());

            Thread.Sleep(100);
            GUIWindow.PrintLog("CC: Sending request: ConnectionTeardownResponse( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) to NCC : OK", ncc.getAsID());
            ncc.HandleConnectionTeardownResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, disconnectExternal);
        }

        //---------------- INTERNAL -----------------------------

        public void HandleInternalConnectionTeardownRequest(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName) {
            //GUIWindow.PrintLog("Discon ID: " + ncc.disconnectingID);
            RouteControl.LinkConnID.TryGetValue(ncc.disconnectingID, out Path pathToRemove);
            rc.UpdateRoutingTables(pathToRemove, ncc.disconnectingID, false, true);
            rc.ClearConnectionGaps(ncc.disconnectingID);
            GUIWindow.UpdateChannelTable();
            GUIWindow.PrintLog("CC: Received request: ConnectionTeardown( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) from NCC", ncc.getAsID());

            /*
            if (parent) {
                //TUTAJ NA SZTYWNO SB WYPISUJE LOGA BO NIE MA SENSU ODWOŁYWAĆ SIĘ DO OBIEKTÓW ŻEBY TYLKO WPISAŁY COŚ DO LOGA
                GUIWindow.PrintLog("CC: Sending request: ConnectionTeardown( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) to Child ConnectionControl", ncc.getAsID());
                GUIWindow.PrintLog("CHILD#CC: Received request: ConnectionTeardown( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) from Parent ConnectionControl", ncc.getAsID());
                GUIWindow.PrintLog("CHILD#CC: Sending request: LinkConnectionDeallocation() to internal LRM", ncc.getAsID());
                GUIWindow.PrintLog("LRM: Received request: LinkConnectionDeallocation() from CHILD#CC :", ncc.getAsID());
                GUIWindow.PrintLog("LRM: Sending request: LinkConnectionDeallocationResponse() to CHILD#CC : DEALLOCATED", ncc.getAsID());
                GUIWindow.PrintLog("CHILD#CC: Received request: LinkConnectionDeallocationResponse() from LRM : DEALLOCATED", ncc.getAsID());
                GUIWindow.PrintLog("CHILD#CC: Sending request: ConnectionTeardownResponse(" + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) to Parent ConnectionControl", ncc.getAsID());
                GUIWindow.PrintLog("CC: Received request: ConnectionTeardownResponse( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) from Child ConnectionControl", ncc.getAsID());
            }
            */
            Thread.Sleep(100);
            GUIWindow.PrintLog("CC: Sending request: LinkConnectionDeallocation() to internal LRM", ncc.getAsID());
            GUIWindow.PrintLog("LRM: Received request: LinkConnectionDeallocation() from CC :", ncc.getAsID());
            PrintLogsContaining(ncc.getAsID().ToString(), ncc.getAsID());
            GUIWindow.PrintLog("LRM: Sending request: LinkConnectionDeallocationResponse() to CC : DEALLOCATED", ncc.getAsID());
            GUIWindow.PrintLog("CC: Received request: LinkConnectionDeallocationResponse() from LRM : DEALLOCATED", ncc.getAsID());

            Thread.Sleep(100);
            GUIWindow.PrintLog("CC: Sending request: ConnectionTeardownResponse(" + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) to NCC : OK", ncc.getAsID());
            ncc.HandleInternalConnectionTeardownResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName);
        }

    }
}
