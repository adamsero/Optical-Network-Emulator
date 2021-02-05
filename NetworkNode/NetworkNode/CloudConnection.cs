using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FrameLib;

namespace NetworkNode
{
    class CloudConnection
    {
        //do wczytania z config
        private readonly int CloudPort = 9999;
        private static bool connected = false;
        private static NetworkStream stream;
        public static ushort[] ClientPorts;
        public static string ClientIP;
        public static int asID;
        public static bool subnetworkRouter; 
        public static string[,] MPLSTab;

        public CloudConnection() {
            try {
                TcpClient tcpClient = new TcpClient("localhost", CloudPort);
                stream = tcpClient.GetStream();

                connected = true;
                GUIWindow.PrintLog("Connection with Cloud has been established");

                SendRegistrationRequest();
                GUIWindow.PrintLog("Sent registration request to Cloud");

                new Thread(ReceiveMessages).Start();
            } catch (SocketException se) {
                GUIWindow.PrintLog("Connection with Cloud has NOT been established"); 
                connected = false;
            }
        }

        public void ReceiveMessages() {
            while (true) {
                byte[] receivedBuffer = new byte[8192];
                try {
                    stream.Read(receivedBuffer, 0, receivedBuffer.Length);
                }
                catch {
                    break;
                }

                Frame frame = (Frame)DeserializeObject(receivedBuffer);
                GUIWindow.PrintLog("Received message addressed to " + frame.DestinationIP + " with Connection ID #" + frame.ConnectionID);

                int outPort = 0;
                List<int> ports = new List<int>();
                foreach(Tuple<int, int> tuple in Program.routingTable) {
                    if (tuple.Item1 == frame.ConnectionID)
                        ports.Add(tuple.Item2);
                }
                if(ports[0] == frame.RouterInPort) {
                    outPort = ports[1];
                } else {
                    outPort = ports[0];
                }

                GUIWindow.PrintLog("Redirecting message through port " + frame.SourcePort);
                //GUIWindow.PrintLog(outPort.ToString());
                frame.SourcePort = (ushort)outPort;
                frame.SourceIP = ClientIP;

                SendMessage(frame);
            }
        }

        private void SendRegistrationRequest() {
            string addressWithoutMask = ClientIP.Split('/')[0];
            //string addressWithoutMask = ConfigLoader.ip;
            Frame registrationFrame = new Frame(addressWithoutMask, 0, "", 0, 0, "_register_");

            byte[] data = SerializeObject(registrationFrame);
            stream.Write(data, 0, data.Length);
        }

        public void DisconnectFromCloud() {
            Frame disconnectionFrame = new Frame("", 0, "", 0, 0, "_disconnect_");
            byte[] data = SerializeObject(disconnectionFrame);
            stream.Write(data, 0, data.Length);
        }

        public static void SendMessage(Frame frame) {

            if (connected) {
                byte[] bytes = SerializeObject(frame);
                stream.Write(bytes, 0, bytes.Length);
            }
            else {
                GUIWindow.PrintLog("Connect with Cloud to send a message...");
            }

        }

        public static byte[] SerializeObject(object o) {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            return stream.ToArray();
        }

        public static object DeserializeObject(byte[] bytes) {
            MemoryStream stream = new MemoryStream(bytes);
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(stream);
        }
    }
}
