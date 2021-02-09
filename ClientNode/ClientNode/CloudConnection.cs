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
using FrameLib;

namespace ClientNode
{
    class CloudConnection
    {
        //do wczytania z config
        private readonly int CloudPort = 9999;
        private static bool connected = false;
        private static NetworkStream stream;
        public static ushort ClientPort;
        public static string ClientIP;
        public static int asID;

        public CloudConnection() {
            
            try {
                TcpClient tcpClient = new TcpClient("localhost", CloudPort);
                stream = tcpClient.GetStream();

                connected = true;
                GUIWindow.PrintLog("Connection with Cloud has been established");

                SendRegistrationRequest();
                GUIWindow.PrintLog("Sent registration request to Cloud");

                new Thread(ReceiveMessages).Start();
            }
            catch (SocketException) {
                GUIWindow.PrintLog("Connection with Cloud has NOT been established :(");
                connected = false;
            }
        }

        private void ReceiveMessages() {
            GUIWindow.PrintLog("Listening for Cloud messages...");
            while (true) {
                try {
                    byte[] receivedBuffer = new byte[8192];
                    try {
                        stream.Read(receivedBuffer, 0, receivedBuffer.Length);
                    } catch {
                        break;
                    }

                    Frame frame = (Frame)DeserializeObject(receivedBuffer);
                    CPCC.connectionID = frame.ConnectionID;

                    GUIWindow.PrintLog(GetMessageFromFrame(frame));
                } catch (IOException ex) {
                    GUIWindow.PrintLog(ex.Message);
                }
            }
        }

        private string GetMessageFromFrame(Frame frame) {
            return "Message " + "\"" + frame.Message + "\"" + " received from " + CPCC.cachedDestination;
        }

        private void SendRegistrationRequest() {
            string addressWithoutMask = ClientIP.Split('/')[0];
            Frame registrationFrame = new Frame(addressWithoutMask, 0, "", 0, 0, "_register_");

            byte[] data = SerializeObject(registrationFrame);
            stream.Write(data, 0, data.Length);
        }

        public static void SendMessage(int connectionID, String message, String destinationName) {

            if (connected) {
                //nie wiem czy destination jeszcze wgl potrzebne do czegokolwiek
                Frame frame = new Frame(ClientIP, ClientPort, destinationName, 50000, connectionID, message);
                byte[] bytes = SerializeObject(frame);

                stream.Write(bytes, 0, bytes.Length);
                GUIWindow.PrintLog("Message " + "\"" + message + "\"" + " sent to " + destinationName);

            }
            else {
                GUIWindow.PrintLog("Connect with Cloud to send a message...");
            }            
        }

        public void DisconnectFromCloud() {
            Frame disconnectionFrame = new Frame("", 0, "", 0, 0, "_disconnect_");
            byte[] data = SerializeObject(disconnectionFrame);
            stream.Write(data, 0, data.Length);
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
