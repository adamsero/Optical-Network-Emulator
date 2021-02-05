using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace ManagementCenter
{
    class HostConnection
    {
        private readonly Host host;
        private readonly TcpClient client;
        private readonly int id;
        private readonly Server server;

        public HostConnection(Host host, TcpClient client, int id, Server server) {
            this.host = host;
            this.client = client;
            this.id = id;
            this.server = server;

            new Thread(ListenForPrompts).Start();
        }

        public int GetID() {
            return id;
        }

        public Host GetHost() {
            return host;
        }

        private void ListenForPrompts() {
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);

            while (true) {

                string request;
                try {
                    request = reader.ReadLine();
                } catch (IOException ioe) {
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
