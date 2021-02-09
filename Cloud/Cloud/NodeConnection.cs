using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using FrameLib;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Cloud {
    class NodeConnection {

        private readonly TcpClient client;
        private readonly NetworkStream stream;
        private readonly ConnectionListener connectionListener;
        private string ipAddress;

        public NodeConnection(TcpClient client, ConnectionListener connectionListener) {
            this.connectionListener = connectionListener;
            this.client = client;
            stream = client.GetStream();

            new Thread(ReceiveMessages).Start();
        }

        private void ReceiveMessages() {
            while (true) {

                byte[] receivedBufferL = new byte[16384];
                try {
                    stream.Read(receivedBufferL, 0, receivedBufferL.Length);
                } catch {
                    break;
                }

                byte[] receivedBuffer = new byte[8192];
                Buffer.BlockCopy(receivedBufferL, 0, receivedBuffer, 0, receivedBuffer.Length);

                Frame frame = (Frame)DeserializeObject(receivedBuffer);

                if (frame.Message.Equals("_register_")) {
                    Register(frame.SourceIP);
                    continue;
                }

                else if (frame.Message.Equals("_disconnect_")) {
                    GUIWindow.PrintLog("Node " + ipAddress + " has disconnected from the server");
                    connectionListener.DisconnectNode(ipAddress);
                    stream.Close();

                    continue;
                }

                GUIWindow.PrintLog("Received message from " + frame.SourceIP.Split('/')[0] + ":" + frame.SourcePort);

                RedirectFrame(frame.SourceIP.Split('/')[0], frame.SourcePort, frame); 
            }
        }

        private void RedirectFrame(string srcAddress, ushort srcPort, Frame frame) {

            string targetAddress = "";
            ushort targetPort = 0;
            Tuple<int, string, int, string, int> target = null;
            foreach (Tuple<int, string, int, string, int> tuple in Program.connectionTable) {
                if (tuple.Item2.Equals(srcAddress) && tuple.Item3 == srcPort) {
                    targetAddress = tuple.Item4;
                    targetPort = (ushort)tuple.Item5;
                    target = tuple;
                } else if (tuple.Item4.Equals(srcAddress) && tuple.Item5 == srcPort) {
                    targetAddress = tuple.Item2;
                    targetPort = (ushort)tuple.Item3;
                    target = tuple;
                }
            }

            if (targetAddress.Length == 0) {
                GUIWindow.PrintLog("Could not find a link that contains a Node with given address and port");
                return;
            }
            else if (Program.brokenConnections.Contains(target)) {
                GUIWindow.PrintLog("Redirecting message to " + targetAddress + ":" + targetPort);
                GUIWindow.PrintLog("Connection is not working! Packet is lost!");
                return;
            }

            frame.RouterInPort = targetPort;

            GUIWindow.PrintLog("Redirecting message to " + targetAddress + ":" + targetPort);
            connectionListener.RedirectFrame(targetAddress, targetPort, SerializeObject(frame));
        }

        public void SendMessage(byte[] messageBuffer) { 
            stream.Write(messageBuffer, 0, messageBuffer.Length);
        }

        

        private void Register(string ip) { 
            connectionListener.RegisterNode(ip, this);
            ipAddress = ip;
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
