using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementCenter {
    public class Connection {
        private readonly int id;
        protected Tuple<Node,Node> connections;

        public Connection(int id,Node NodeA,Node NodeB) {
            this.id = id;
            this.connections = new Tuple<Node, Node>(NodeA, NodeB);
        }
    }
}
