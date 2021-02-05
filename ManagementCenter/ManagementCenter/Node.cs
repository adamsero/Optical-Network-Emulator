using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ManagementCenter {
    public class Node {
        protected readonly int id;
        protected readonly String ip;
        //protected readonly int[] ports;
        protected LinkedList<int> availablePorts;
        protected readonly LinkedList<Connection> connections;

        public Node(int id, String ip) {

            this.id = id;
            this.ip = ip;
            availablePorts = new LinkedList<int>();
        }

        public int GetHostID(){
            return id;
        }

        public int GetRouterID() {
            return id;
        }

        public String getIP() {
            return ip;
        }
        
        public String getPorts(){
            String ports = "";
            foreach (int i in availablePorts) {
                ports = ports + ":" + i.ToString();
            }
            return ports;
        }
    }
}
