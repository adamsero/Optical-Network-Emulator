using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace NetworkNode {
    class ControlCenterConnection {
        private readonly int centerPort = ConfigLoader.ccPort;
        private static bool connected = false;
        private static NetworkStream stream;
        private static StreamReader reader;
        private static StreamWriter writer;
        private TimeSpan timeSpan = TimeSpan.Zero;

        public ControlCenterConnection() {
            try {
                TcpClient tcpClient = new TcpClient("localhost", centerPort);
                stream = tcpClient.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                connected = true;
                GUIWindow.PrintLog("Connection with Control Center has been established");

                GUIWindow.PrintLog("Sent registration request to Control Center");
                SendRegistrationRequest();
                RecieveRegistrationInfo();
                SendKeepAlive();
                ReceiveRoutingTable();

            } catch (SocketException se) {
                GUIWindow.PrintLog("Connection with Control Center has NOT been established");
                GUIWindow.PrintLog(se.Message);
                connected = false;
            }
        }

        private void SendRegistrationRequest() {
            writer.WriteLine("REGISTRATION:ROUTER:" + ConfigLoader.ip + ":" + ConfigLoader.nodeID);
            writer.Flush();
        }

        private void RecieveRegistrationInfo() {
            try {
                string msg = reader.ReadLine();
                string[] parameters = msg.Split(':');

                if (parameters[0].Equals("REGISTRATION") && parameters[1].Equals("OK")) {
                    GUIWindow.PrintLog("Control Center accepted registration request");
                }
                else {
                    GUIWindow.PrintLog("Control Center denied registration request");
                }
            } catch (IOException e) {
                GUIWindow.PrintLog(e.Message);
            }
        }

        private void SendKeepAlive() {
            new Thread(() => {
                Thread.Sleep(1000);

                while(true) {
                    string connIds = "";
                    for(int i = 0; i < Program.routingTable.Count; i += 2) {
                        connIds += Program.routingTable[i].Item1 + " ";
                    }
                    //connIds = connIds.Remove(connIds.Length - 1);
                    
                    try {
                        writer.WriteLine("KEEP-ALIVE:" + connIds);
                        writer.Flush();
                       // GUIWindow.PrintLog("KEEP-ALIVE sent");
                    } catch(Exception e) {
                    }

                    Thread.Sleep(1000);
                }

            }).Start();
        }

        private void ReceiveRoutingTable() {
            while (true) {
                try {
                    byte[] receivedBuffer = new byte[8192];
                    try {
                        stream.Read(receivedBuffer, 0, receivedBuffer.Length);
                    }
                    catch {
                        break;
                    }

                    Dictionary<int, int> routingTable = (Dictionary<int, int>)CloudConnection.DeserializeObject(receivedBuffer);
                    //foreach (int key in routingTable.Keys) {
                    //    routingTable.TryGetValue(key, out int value);
                    //    GUIWindow.PrintLog("ID: " + key + " port: " + value);
                    //}


                    foreach (int key in routingTable.Keys) {
                        int val;
                        routingTable.TryGetValue(key, out val);
                        Program.routingTable.Add(new Tuple<int, int>(key, val));
                        GUIWindow.PrintLog("CC: Received MatrixConnection(" + key + ", " + val + ") from network's CC");
                        GUIWindow.PrintLog("CC: Sent MatrixConnectionResponse() to network's CC");
                    }

                    TimeSpan currentTime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
                    TimeSpan diff = currentTime - timeSpan;
                    timeSpan = currentTime;

                    //if (diff.TotalMilliseconds > 500)
                    

                    LinkedList<string[]> rows = new LinkedList<string[]>();
                    foreach (int key in routingTable.Keys) {
                        routingTable.TryGetValue(key, out int val);
                        rows.AddLast(new string[] { key.ToString(), val.ToString() });
                    }
                    GUIWindow.UpdateRoutingTable(rows);
                }
                catch (Exception ex) {
                    GUIWindow.PrintLog(ex.StackTrace);
                }
                
            }
        }
    }
}
