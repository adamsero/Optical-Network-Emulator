using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    public class Connection {
        private readonly int id;
        private readonly bool external = false;
        private readonly int asID;
        public readonly int distance;
        private readonly double maxBandwidth;
        //private readonly int maxSlotQuantity;
        public int[] slot = new int[90];

        public Tuple<Node, Node> endPoints;
        public Tuple<int, int> connPorts;

        public Connection(int id, Node NodeA, Node NodeB, int distance, double maxBandwidth, bool external, int asID, Tuple<int,int> connPorts) {
            this.id = id;
            this.distance = distance;
            this.maxBandwidth = maxBandwidth;
            //this.maxSlotQuantity = (int) Math.Floor(maxBandwidth / 12.5);
            this.external = external;
            this.asID = asID;
            this.endPoints = new Tuple<Node, Node>(NodeA, NodeB);
            this.connPorts = connPorts;
            NodeA.AddConnection(this);
            NodeB.AddConnection(this);

            for(int i = 0; i < slot.Length; i++) {
                this.slot[i] = 0;
            }
        }

        public int GetID() {
            return id;
        }

        public int[] GetSlot() {
            return slot;
        }

        public int GetAsID() {
            return asID;
        }
    }
}
