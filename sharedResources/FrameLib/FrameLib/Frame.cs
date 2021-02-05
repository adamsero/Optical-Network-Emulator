using System;
using System.Collections;

namespace FrameLib {
    [Serializable]
    public class Frame
    {
        public string SourceIP;
        public ushort SourcePort;
        public string DestinationIP;
        public ushort DestinationPort;
        public string OriginIP;
        public ushort OriginPort;
        public ushort RouterInPort;
        public string Message;
        public int ConnectionID;

        public Frame(string SrcIP, ushort SrcP, string DesIP, ushort DesP, int ConnID, string Mes) {
            SourceIP = SrcIP;
            SourcePort = SrcP;
            DestinationIP = DesIP;
            DestinationPort = DesP;
            OriginIP = SourceIP;
            OriginPort = SourcePort;
            Message = Mes;
            ConnectionID = ConnID;
        }

        override public string ToString() {
            return "Source IP: " + SourceIP + " Source Port: " + SourcePort + " Destination IP: " + DestinationIP + " Destination Port: " + DestinationPort + " Coonection ID: " + ConnectionID + " Message: " + Message + " :) ";
        }
    }
}
