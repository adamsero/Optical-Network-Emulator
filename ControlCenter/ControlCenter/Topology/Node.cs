using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    public class Node {
        protected readonly int id;
        protected readonly String ip;
        protected readonly int asID;
        //protected readonly int[] ports;
        protected LinkedList<int> availablePorts;
        public readonly LinkedList<Connection> connections = new LinkedList<Connection>();
        public bool working = true;

        public Node(int id, String ip, int asID) {

            this.id = id;
            this.ip = ip;
            this.asID = asID;
            availablePorts = new LinkedList<int>();
        }

        public void AddConnection(Connection connection) {
            connections.AddLast(connection);
        }

        public int GetHostID() {
            return id;
        }

        public int GetRouterID() {
            return id;
        }

        public int GetAlgorithmID() {
            if (this is Host)
                return id + 10;
            else
                return id;
        }

        public String getIP() {
            return ip;
        }

        public String getPorts() {
            String ports = "";
            foreach (int i in availablePorts) {
                ports = ports + ":" + i.ToString();
            }
            return ports;
        }

        public int GetAsID() { 
            return asID;
        }
    }
}
