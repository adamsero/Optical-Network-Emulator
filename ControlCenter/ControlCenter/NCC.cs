using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ControlCenter {
    class NCC {

        private int asID;
        private NCC otherNCC = null;
        private ConnectionControl cc;
        private ConnectionControl childCC;
        public int disconnectingID = 0;

        public NCC(int asID) {
            this.asID = asID;
            //TODO: ogarniecie z wczytywania czy mamy zagniezdzenie i odpowiednie ustawienie zaleznosci
            //na podstawie configu
            cc = new ConnectionControl(this);
            if (asID == 2) {
                childCC = new ConnectionControl(this, cc); 
                cc.setChildConnectionControl(childCC);
            }
        }

        public ConnectionControl getConnectionControl() {
            return this.cc;
        }

        public void SetOtherNCC(NCC otherNCC) {
            this.otherNCC = otherNCC;
        }

        public NCC getOtherNCC() {
            return otherNCC;
        }

        public int getAsID() {
            return asID;
        }
        //od strony pierwszego NCC
        public void HandleCallRequest(String startHostName, String destinationHostName, int linkSpeed) {

            String startIP = GetHostIPByHostName(startHostName);
            String endIP = GetHostIPByHostName(destinationHostName);

            //Przyjecie requesta
            GUIWindow.PrintLog("Received request: CallRequest( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) from " + startHostName, asID);

            //Policy
            GUIWindow.PrintLog("Sending request: Policy() to CAC", asID);
            Thread.Sleep(500);
            GUIWindow.PrintLog("(CAC) Admission granted", asID);

            //Directory
            GUIWindow.PrintLog("Sending request: DirectoryRequest( " + destinationHostName + " ) to Directory", asID);
            Thread.Sleep(500);
            if (endIP == null) {
                GUIWindow.PrintLog("(Directory) replied: Unknown Host", asID);
                GUIWindow.PrintLog("Sending request: CallRequestResponse( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) to " + startHostName + " : " + "FAILED -- UNKNOWN HOST", asID);
                HostConnection hostConnection = GetHostConnectionByIp(startIP);
                hostConnection.SendMessage("ANSWER:CALLREQUEST:FAILED");
            }
            else {
                GUIWindow.PrintLog("(Directory) replied: " + destinationHostName + " IP Address is: " + endIP, asID);
                bool internalConnection = isInternal(startIP, endIP);
                if (internalConnection) {
                    
                    GUIWindow.PrintLog("Connection status: INTERNAL", asID);
                    GUIWindow.PrintLog("Sending request: CallAccept( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s )", asID);
                    HostConnection hostConnection = GetHostConnectionByIp(endIP);
                    hostConnection.SendMessage("REQUEST:CALLACCEPT:" + startIP + ":" + endIP + ":" + linkSpeed + ":" + startHostName + ":" + destinationHostName);
                }
                else {
                    //CallCoordination
                    GUIWindow.PrintLog("Sending request: CallCoordination( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) to other NCC", asID);
                    otherNCC.HandleCallCoordination(startIP, endIP, linkSpeed, startHostName, destinationHostName);
                }
            }
        }

        //od strony drugiego NCC
        public void HandleCallCoordination(String startIP, String endIP, int linkSpeed, String startHostName ,String destinationHostName) {
            GUIWindow.PrintLog("Received request: CallCoordination( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) from other NCC", asID);

            //Policy
            GUIWindow.PrintLog("Sending request: Policy() to CAC", asID);
            Thread.Sleep(500);
            GUIWindow.PrintLog("(CAC) Admission granted", asID);

            //Directory
            GUIWindow.PrintLog("Sending request: DirectoryRequest( " + destinationHostName + " ) to Directory", asID);
            Thread.Sleep(500);
            GUIWindow.PrintLog("(Directory) replied: " + destinationHostName + " IP Address is: " + endIP, asID);

            
            //CallAccept
            GUIWindow.PrintLog("Sending request: CallAccept( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s )", asID);
            HostConnection hostConnection = GetHostConnectionByIp(endIP);
            hostConnection.SendMessage("REQUEST:CALLACCEPT:" + startIP + ":" + endIP + ":" + linkSpeed + ":" + startHostName + ":" + destinationHostName);
            
        }

        //odebranie decyzji od drugiego Hosta
        public void HandleCallAcceptResponse(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool acceptation) {
            bool internalConnection = isInternal(startIP, endIP);

            if (internalConnection) {
                if (acceptation) {
                    GUIWindow.PrintLog("Sending request: ConnectionRequest( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) to ConnectionControl", asID);
                    cc.HandleConnectionRequestFromNCC(startIP, endIP, linkSpeed, startHostName, destinationHostName, internalConnection);
                }
                else {
                    GUIWindow.PrintLog("Sending request: CallRequestResponse( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) to " + startHostName + " : " + "FALIED", asID);
                    HostConnection hostConnection = GetHostConnectionByIp(startIP);
                    hostConnection.SendMessage("ANSWER:CALLREQUEST:FAILED");
                }
            }
            else {
                if (acceptation) {
                    GUIWindow.PrintLog("Received request: CallAcceptResponse( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) from " + destinationHostName + " : OK", asID);
                    GUIWindow.PrintLog("Sending request: CallCoordinationResponse( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) to other NCC : OK", asID);
                }
                else {
                    GUIWindow.PrintLog("Received request: CallAcceptResponse( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) from " + destinationHostName + " : DENIED", asID);
                    GUIWindow.PrintLog("Sending request: CallCoordinationResponse( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) to other NCC : DENIED", asID);
                }
                otherNCC.recieveDecisionFromOtherNCC(startIP, endIP, linkSpeed, startHostName, destinationHostName, acceptation);
            }
        }

        private String GetHostNameByIp(String ip) {

            LinkedList<Host> hostList = ConfigLoader.GetHosts();

            foreach (Host h in hostList) {
                if (h.getIP().Equals(ip)) {
                    return "Host" + h.GetHostID();
                }
            }
            return null;
        }

        private HostConnection GetHostConnectionByIp (String ip) {

            LinkedList <HostConnection> hostConnections = Server.GetHostConnections();

            foreach (HostConnection h in hostConnections) {
                if (h.GetHost().getIP().Equals(ip)) {
                    return h;
                }
            }
            return null;
        }

        public String GetHostIPByHostName(String hostName) {
            LinkedList<Host> hostList = ConfigLoader.GetHosts();

            foreach (Host h in hostList) {
                if (h.GetHostName().ToUpper().Equals(hostName.ToUpper())) {
                    return h.getIP();
                }
            }
            return null;

        }


        private void recieveDecisionFromOtherNCC(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool acceptation) {
            if (acceptation) {
                GUIWindow.PrintLog("Received request: CallCoordinationResponse( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) from other AS NCC : OK", asID);
                GUIWindow.PrintLog("Sending request: ConnectionRequest( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) to ConnectionControl", asID);
                cc.HandleConnectionRequestFromNCC(startIP, endIP, linkSpeed, startHostName, destinationHostName, false);
            }
            else {
                GUIWindow.PrintLog("Received request: CallCoordinationResponse( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) from other AS NCC : DENIED", asID);
                GUIWindow.PrintLog("Sending request: CallRequestResponse( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) to " + startHostName + " : " + "FALIED", asID);
                HostConnection hostConnection = GetHostConnectionByIp(startIP);
                hostConnection.SendMessage("ANSWER:CALLREQUEST:FAILED");
            }
            
        }

        public void HandleConnectionRequestResponse(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool success) {
            
            if (success) {
                GUIWindow.PrintLog("Received request: ConnectionRequestResponse( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) from CC : SUCCESS", asID);
                GUIWindow.PrintLog("Sending request: CallRequestResponse( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s, Connection#" + cc.GetRouteControl().GetLastPathIndex() + " ) to " + startHostName + " : " + "SUCCESS", asID);
                HostConnection hostConnection = GetHostConnectionByIp(startIP);
                hostConnection.SendMessage("ANSWER:CALLREQUEST:SUCCESS:" + cc.GetRouteControl().GetLastPathIndex());
                HostConnection otherHost = GetHostConnectionByIp(endIP);
                otherHost.SendMessage("NOTIFY:CONNECTIONID:" + cc.GetRouteControl().GetLastPathIndex());
            }
            else {
                GUIWindow.PrintLog("Received request: ConnectionRequestResponse( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) from CC : FAILED", asID);
                GUIWindow.PrintLog("Sending request: CallRequestResponse( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) to " + startHostName + " : " + "FAILED", asID);
                HostConnection hostConnection = GetHostConnectionByIp(startIP);
                hostConnection.SendMessage("ANSWER:CALLREQUEST:FAILED");
            }
        }

        private bool isInternal(String startIP, String endIP) {

            bool startMatch = false;
            bool endMatch = false;
            foreach (Host h in ConfigLoader.GetHosts()) {
                if (h.getIP().Equals(startIP) && h.GetAsID() == this.getAsID()) {
                    startMatch = true;
                }
                if (h.getIP().Equals(endIP) && h.GetAsID() == this.getAsID()) {
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

        public void SendInfoToSecondSideHost(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool internalConnection) {
            HostConnection hostConnection = GetHostConnectionByIp(endIP);
            GUIWindow.PrintLog("Sending request CallAcceptResponse to " + destinationHostName + ": DENIED", asID);
            hostConnection.SendMessage("NOTIFY:CALLDENIED");
        }

        //=========================================================================================

        //----------------------------- ROZLACZANIE POLACZENIA ------------------------------------

        //=========================================================================================

        public void HandleCallTeardown(String startHostName, String destinationHostName, int linkSpeed, int connectionID) {
            disconnectingID = connectionID;

            String startIP = GetHostIPByHostName(startHostName);
            String endIP = GetHostIPByHostName(destinationHostName);


            //Przyjecie requesta
            GUIWindow.PrintLog("Received request: CallTeardown( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) from " + startHostName, asID);
            /*
            //Policy
            GUIWindow.PrintLog("Sending request: Policy() to CAC", asID);
            Thread.Sleep(500);
            GUIWindow.PrintLog("(CAC) Admission granted", asID);

            //Directory
            GUIWindow.PrintLog("Sending request: DirectoryRequest( " + destinationHostName + " ) to Directory", asID);
            Thread.Sleep(500);
            GUIWindow.PrintLog("(Directory) replied: " + destinationHostName + " IP Address is: " + endIP, asID);
            */

            bool internalConnection = isInternal(startIP, endIP);
            if (internalConnection) {
                //CallCoordination:TEARDOWN
                //GUIWindow.PrintLog("Connection status: INTERNAL", asID);
                HostConnection hostConnection = GetHostConnectionByIp(endIP);
                hostConnection.SendMessage("REQUEST:CALLTEARDOWN:" + startIP + ":" + endIP + ":" + linkSpeed + ":" + startHostName + ":" + destinationHostName);
            }
            else {
                //CallCoordination:TEARDOWN
                GUIWindow.PrintLog("Sending request: CallCoordination-Teardown( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) to other NCC", asID);
                otherNCC.HandleCallCoordinationTeardown(startIP, endIP, linkSpeed, startHostName, destinationHostName);
            }
        }

        public void HandleCallCoordinationTeardown(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName) {
            GUIWindow.PrintLog("Received request: CallCoordination-Teardown( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) from other NCC", asID);
            /*
            //Policy
            GUIWindow.PrintLog("Sending request: Policy() to CAC", asID);
            Thread.Sleep(500);
            GUIWindow.PrintLog("(CAC) Admission granted", asID);

            //Directory
            GUIWindow.PrintLog("Sending request: DirectoryRequest( " + destinationHostName + " ) to Directory", asID);
            Thread.Sleep(500);
            GUIWindow.PrintLog("(Directory) replied: " + destinationHostName + " IP Address is: " + endIP, asID);
            */
            //CallAccept
            GUIWindow.PrintLog("Sending request: CallTeardown( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s )", asID);
            HostConnection hostConnection = GetHostConnectionByIp(endIP);
            hostConnection.SendMessage("REQUEST:CALLTEARDOWN:" + startIP + ":" + endIP + ":" + linkSpeed + ":" + startHostName + ":" + destinationHostName);
        }

        public void HandleCallTeardownResponse(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName) {
            GUIWindow.PrintLog("Received request: CallTeardownResponse( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) from " + destinationHostName + " : OK", asID);

            bool internalConnection = isInternal(startIP, endIP);

            if (internalConnection){
                GUIWindow.PrintLog("Sending request: ConnectionTeardownRequest( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) to ConnectionControl", asID);
                cc.HandleInternalConnectionTeardownRequest(startIP, endIP, linkSpeed, startHostName, destinationHostName);
            }
            else {
                GUIWindow.PrintLog("Sending request: ConnectionTeardownRequest( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) to ConnectionControl", asID);
                cc.HandleConnectionTeardownRequest(startIP, endIP, linkSpeed, startHostName, destinationHostName, false);
                GUIWindow.PrintLog("Sending request: CallCoordination-TeardownResponse( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) to other NCC : OK", asID);
                otherNCC.recieveCallTeardownResponseFromOtherNCC(startIP, endIP, linkSpeed, startHostName, destinationHostName);
            }
        }

        public void recieveCallTeardownResponseFromOtherNCC(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName) {
            GUIWindow.PrintLog("Received request: CallCoordination-TeardownResponse( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) from " + destinationHostName + " : OK", asID);
            GUIWindow.PrintLog("Sending request: ConnectionTeardownRequest( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) to ConnectionControl", asID);
            cc.HandleConnectionTeardownRequest(startIP, endIP, linkSpeed, startHostName, destinationHostName, true);
        }

        public void HandleConnectionTeardownResponse(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName, bool disconnectExternal) { 
            
            GUIWindow.PrintLog("Received request: ConnectionTeardownResponse( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) from ConnectionControl : OK", asID);
            
            if(disconnectExternal){
                //trzeba do hosta wysłać
                GUIWindow.PrintLog("Sending request: CallTeardownResponse( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) to NCC : OK", asID);
                HostConnection hostConnection = GetHostConnectionByIp(startIP);
                hostConnection.SendMessage("ANSWER:CALLTEARDOWN:SUCCESS");
            }
        }

        public void HandleInternalConnectionTeardownResponse(String startIP, String endIP, int linkSpeed, String startHostName, String destinationHostName) {
            GUIWindow.PrintLog("Received request: ConnectionTeardownResponse( " + startIP + ", " + endIP + ", " + linkSpeed + " Gb/s ) from ConnectionControl : OK", asID);
            GUIWindow.PrintLog("Sending request: CallTeardownResponse( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) to NCC : OK", asID);
            HostConnection hostConnection = GetHostConnectionByIp(startIP);
            hostConnection.SendMessage("ANSWER:CALLTEARDOWN:SUCCESS");
        }
    }

}
