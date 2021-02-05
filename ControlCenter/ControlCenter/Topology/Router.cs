using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
	public class Router : Node {

		protected bool subnetworkRouter = false;
		//protected LinkedList<Host> availableHost;

		//public Router(int id,String ip, LinkedList<String> ports, LinkedList<Host> hosts)
		public Router(int id, String ip, int asID, LinkedList<int> ports, bool subnetworkRouter) : base(id, ip, asID) {
			//this.availableHost = availableHost;
			availablePorts = ports;
			this.subnetworkRouter = subnetworkRouter;
		}

		public bool GetSubnetworkRouter() {
			return subnetworkRouter;
		}
	}
}
