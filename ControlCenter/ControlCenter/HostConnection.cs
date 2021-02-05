using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace ControlCenter {
    class HostConnection {
        private readonly Host host;
        private readonly TcpClient client;
        private readonly int id;
        private readonly Server server;
        private readonly int asID;
        NCC ncc;
        NetworkStream stream;
        StreamReader reader;
        StreamWriter writer;


        public HostConnection(Host host, TcpClient client, int id, Server server, int asID, NCC ncc) {
            this.host = host;
            this.client = client;
            this.id = id;
            this.server = server;
            this.asID = asID;
            this.ncc = ncc;

            stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);

            new Thread(RecieveMessages).Start();
        }

        public int GetID() {
            return id;
        }

        public Host GetHost() {
            return host;
        }

        public void SendMessage(String content) {
            writer.WriteLine(content);
            writer.Flush();
        }

        private void RecieveMessages() {

            while (true) {

                string request;
                try {
                    request = reader.ReadLine();
                    String[] cut = request.Split(':');

                    if (cut[0].Equals("REQUEST") && cut[1].Equals("CALLREQUEST")) {
                        String startHostName = cut[2];
                        String endHostName = cut[3];
                        String linkSpeed = cut[4];

                        ncc.HandleCallRequest(startHostName, endHostName, Int32.Parse(linkSpeed));
                    }
                    
                    if (cut[0].Equals("REQUEST") && cut[1].Equals("CALLTEARDOWN")) {
                        String startHostName = cut[2];
                        String endHostName = cut[3];
                        String linkSpeed = cut[4];
                        int connectionID = Int32.Parse(cut[5]); 

                        ncc.HandleCallTeardown(startHostName, endHostName, Int32.Parse(linkSpeed), connectionID);
                    }

                    if (cut[0].Equals("ANSWER") && cut[1].Equals("CALLACCEPT")) {
                        String startIP = cut[3];
                        String endIP = cut[4];
                        int linkSpeed = Int32.Parse(cut[5]);
                        String startHostName = cut[6];
                        String destinationHostName = cut[7];
                        
                        if (cut[2].Equals("OK")) {
                            ncc.HandleCallAcceptResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, true);
                        }

                        if (cut[2].Equals("DENIED")) {
                            ncc.HandleCallAcceptResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName, false);
                        }
                    }

                    if (cut[0].Equals("ANSWER") && cut[1].Equals("CALLTEARDOWN")) {
                        String startIP = cut[3];
                        String endIP = cut[4];
                        int linkSpeed = Int32.Parse(cut[5]);
                        String startHostName = cut[6];
                        String destinationHostName = cut[7];
                        ncc.HandleCallTeardownResponse(startIP, endIP, linkSpeed, startHostName, destinationHostName);
                    }
                }
                catch (IOException ioe) {
                    server.RemoveHostConnection(this);
                    reader.Close();
                    writer.Close();
                    stream.Close();
                    return;
                }
            }
        }
    }
}
