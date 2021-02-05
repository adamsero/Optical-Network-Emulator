using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientNode {
    //Calling Party Call Controller
    class CPCC {
        private readonly int centerPort = ConfigLoader.ccPort;
        private static NetworkStream stream;
        private static StreamReader reader;
        private static StreamWriter writer;
        public int connectionID = 0;

        public CPCC() {
            try {
                TcpClient tcpClient = new TcpClient("localhost", centerPort);
                stream = tcpClient.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                GUIWindow.PrintLog("Connection with Control Center has been established");

                GUIWindow.PrintLog("Sent registration request to Control Center");
                SendRegistrationRequest();
                new Thread(ReceiveMessages).Start();
            } catch (SocketException se) {
                GUIWindow.PrintLog("Connection with Control Center has NOT been established");
                GUIWindow.PrintLog(se.Message);
            }
        }

        private void SendRegistrationRequest() {
            writer.WriteLine("REGISTRATION:HOST:" + ConfigLoader.ip + ":" + ConfigLoader.nodeID);
            writer.Flush();
        }

        private void ReceiveMessages() {
            while (true) {
                string message = reader.ReadLine();
                string[] pieces = message.Split(';');
                Dictionary<string, string> data = new Dictionary<string, string>();
                foreach (string piece in pieces) {
                    string[] keyAndValue = piece.Split(':');
                    data.Add(keyAndValue[0], keyAndValue[1]);
                }

                switch (data["component"]) {
                    case "CPCC":
                        switch(data["name"]) {
                            case "CallRequestResponse":
                                if(Convert.ToBoolean(data["succeeded"])) {
                                    //zapisujemy ID i pozwalamy wysylac wiadomosci
                                } else {
                                    GUIWindow.PrintLog("CPCC: Received CallRequestResponse(UNSUCCESSFUL)");
                                }
                                break;

                            case "CallAccept":
                                GUIWindow.PrintLog("CPCC: Received CallAccept(" + data["routerX"] + ", " + data["routerY"] + ", " + data["speed"] + " Gb/s) from NCC");

                                string msg = "Incomming call received from " + data["routerX"] + "\nDo you want to accept it?";
                                string caption = "Call incoming to " + data["routerY"];
                                var dialogresult = MessageBox.Show(msg, caption,
                                                             MessageBoxButtons.YesNo,
                                                             MessageBoxIcon.Question);

                                if(dialogresult == DialogResult.Yes) {

                                    GUIWindow.ManageCallButton(false);
                                    GUIWindow.ManageMessageBox(true);
                                    GUIWindow.ManageSendButton(true);
                                    SendCallAcceptResponse(data["routerX"], data["routerY"], data["speed"], true);
                                } else if(dialogresult == DialogResult.No) {
                                    SendCallAcceptResponse(data["routerX"], data["routerY"], data["speed"], false);
                                } 

                                break;
                        }
                        break;

                    case "NONE":
                        switch(data["name"]) {
                            case "RegistrationResponse":
                                if (Convert.ToBoolean(data["succeeded"])) {
                                    GUIWindow.PrintLog("Control Center confirmed registration");
                                } else {
                                    GUIWindow.PrintLog("Control Center denied registration");
                                }
                                    break;
                        }
                        break;
                }
            }
        }

        private void SendMessage(string message) {
            writer.WriteLine(message);
            writer.Flush();
        }

        public void SendCallRequest(String myHostName, String targetHostName, int linkSpeed) {
            string message = "component:NCC;name:CallRequest;hostX:" + myHostName + ";hostY:" + targetHostName + ";speed:" + linkSpeed;
            SendMessage(message);
            GUIWindow.PrintLog("CPCC: Sent CallRequest(" + myHostName + ", " + targetHostName + ", " + linkSpeed + " Gb/s) to NCC");
        }

        public void SendCallAcceptResponse(String hostXName, String hostYName, String speed, bool response) {
            String status;
            if (response) {
                status = "OK";
            }
            else {
                status = "DENIED";
            }

            String message = "component:NCC;name:CallAcceptResponse;routerX:" + hostXName + ";routerY:" + hostYName + ";speed:" + speed + ";response:" + status;
            SendMessage(message);
            GUIWindow.PrintLog("NCC: Sent CallAcceptResponse(" + hostXName + ", " + hostYName + ", " + speed + " Gb/s) to NCC : " + status);
        }

        ////TODO: dodac ip w komunikatach przekazywanych do loga np. NCC CallRequest(10.0.0.1, 10.0.0.2)
        //private void ReceiveMessages() {
        //    while (true) {
        //        String recievedMsg = reader.ReadLine();

        //        String[] cut = recievedMsg.Split(':');
        //        if (cut[0].Equals("ANSWER")) {
        //            if (cut[1].Equals("CALLREQUEST")) {
        //                if (cut[2].Equals("SUCCESS")) {
        //                    connectionID = Int32.Parse(cut[3]);
        //                    GUIWindow.UnlockSendingMessages();
        //                    connectionEstablished = true;
        //                    GUIWindow.PrintLog("NCC CallRequest Response: Confirmation -- Connection#" + connectionID);
        //                    GUIWindow.PrintLog(">> Connection has been established <<");

        //                }
        //                if (cut[2].Equals("FAILED")) {
        //                    GUIWindow.PrintLog("NCC CallRequest Response: Failure");
        //                    GUIWindow.PrintLog(">> Connection has NOT been established <<");
        //                }
        //            }
        //            if (cut[1].Equals("CALLTEARDOWN")) {
        //                if (cut[2].Equals("SUCCESS")) {
        //                    GUIWindow.LockSendingMessages();
        //                    connectionEstablished = false;
        //                    GUIWindow.PrintLog(">> Disconnected successfuly <<");
        //                }
        //                if (cut[2].Equals("FAILED")) {
        //                    GUIWindow.PrintLog(">> Disconnecting failed <<");
        //                }
        //            }
        //        }

        //        if (cut[0].Equals("REQUEST") && cut[1].Equals("CALLACCEPT")) {

        //            String startIP = cut[2]; 
        //            String endIP = cut[3];
        //            int linkSpeed = Int32.Parse(cut[4]);
        //            String startHostName = cut[5]; 
        //            String destinationHostName = cut[6];
        //            //TODO: obsluga odrzucenia w GUI
        //            GUIWindow.PrintLog("Received request: CallAccept(" + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s )");

        //            string message = "Incomming call received from " + startHostName + "\nDo you want to accept it?";
        //            string caption = "Call incoming to " + destinationHostName;
        //            var dialogresult = MessageBox.Show(message, caption,
        //                                         MessageBoxButtons.YesNo,
        //                                         MessageBoxIcon.Question);

        //            //PopOutQuestionBox popup = new PopOutQuestionBox();
        //            //popup.labelText("Polaczenie od "+startHostName+"\nZaakceptowac?");
        //            //DialogResult dialogresult = popup.ShowDialog();
        //            if(dialogresult == DialogResult.Yes) {

        //                GUIWindow.ManageCallButton(false);
        //                GUIWindow.ManageMessageBox(true);
        //                GUIWindow.ManageSendButton(true);
        //                sendCallAcceptResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, true);
        //            } else if(dialogresult == DialogResult.No) {
        //                sendCallAcceptResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, false);
        //            }  
        //        }

        //        if (cut[0].Equals("REQUEST") && cut[1].Equals("CALLTEARDOWN")) {
        //            String startIP = cut[2];
        //            String endIP = cut[3];
        //            int linkSpeed = Int32.Parse(cut[4]);
        //            String startHostName = cut[5];
        //            String destinationHostName = cut[6];
        //            GUIWindow.PrintLog("Received request: CallTeardown() from " + cut[6]);
        //            GUIWindow.PrintLog("Proceeding..."); 
        //            Thread.Sleep(200);

        //            GUIWindow.ManageCallButton(true);
        //            GUIWindow.ManageMessageBox(false);
        //            GUIWindow.ManageSendButton(false);
        //            sendCallTeardownResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName);
        //            connectionID = 0;
        //        }

        //        if (cut[0].Equals("REGISTRATION")){
        //            if (cut[1].Equals("OK")) {
        //                GUIWindow.PrintLog("Control Center confirmed registration");
        //            }
        //            else {
        //                GUIWindow.PrintLog("Control Center denied registration");
        //                break;
        //            }
        //        }

        //        if (cut[0].Equals("NOTIFY") && cut[1].Equals("CALLDENIED")) {
        //            GUIWindow.PrintLog("NCC informed that agreed connection cannot be set up");
        //            GUIWindow.ManageCallButton(true);
        //        }

        //        //NOTIFY:CONNECTIONID
        //        if (cut[0].Equals("NOTIFY") && cut[1].Equals("CONNECTIONID")) {
        //            connectionID = Int32.Parse(cut[2]);
        //        }
        //    }
        //}


        //public void sendCallRequestToNCC(String myHostName, String targetHostName, int linkSpeed) {
        //    writer.WriteLine("REQUEST:CALLREQUEST:" + myHostName + ":" + targetHostName + ":" + linkSpeed);
        //    writer.Flush();
        //    GUIWindow.PrintLog("Sending CallRequest( "+ targetHostName +", " + linkSpeed + " Gb/s) to NCC"); 

        //}

        //public void sendCallTeardownToNCC(String myHostName, String targetHostName, int linkSpeed) {
        //    writer.WriteLine("REQUEST:CALLTEARDOWN:" + myHostName + ":" + targetHostName + ":" + linkSpeed + ":" + connectionID);
        //    writer.Flush();
        //    GUIWindow.PrintLog("Sending CallTeardown( " + targetHostName + ", " + linkSpeed + " Gb/s, Connection #" + connectionID + " ) to NCC");
        //}

        //public void sendCallAcceptResponse(String startIP, String targetIP, int linkSpeed, String startHostName, String destinationHostName, bool positive) {

        //    if (positive) {
        //        writer.WriteLine("ANSWER:CALLACCEPT:OK:" + startIP + ":" + targetIP + ":" + linkSpeed+ ":" + startHostName + ":" + destinationHostName);
        //        writer.Flush();
        //        GUIWindow.PrintLog("Sending CallAccept Response( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) to NCC : OK");
        //    }
        //    else {
        //        writer.WriteLine("ANSWER:CALLACCEPT:DENIED:" + startIP + ":" + targetIP + ":" + linkSpeed + ":" + startHostName + ":" + destinationHostName);
        //        writer.Flush();
        //        GUIWindow.PrintLog("Sending CallAccept Response( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) to NCC : DENIED");
        //    }
        //}

        //public void sendCallTeardownResponse(String startIP, String targetIP, int linkSpeed, String startHostName, String destinationHostName) {
        //    writer.WriteLine("ANSWER:CALLTEARDOWN:OK:" + startIP + ":" + targetIP + ":" + linkSpeed + ":" + startHostName + ":" + destinationHostName);
        //    writer.Flush();
        //    GUIWindow.PrintLog("Sending CallTeardown Response( " + startHostName + ", " + destinationHostName + ", " + linkSpeed + " Gb/s ) to NCC : OK");
        //}
    }
}
