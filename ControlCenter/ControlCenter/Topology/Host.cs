using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
	public class Host : Node {

		protected String hostName;
		protected Router neighborRouter;

		public Host(int id, String ip, int asID, int port, Router neighborRouter) : base(id, ip, asID) {
			availablePorts.AddLast(port);
			hostName = "Host" + this.id;
			this.neighborRouter = neighborRouter;
		}

		public String GetHostName() {
			return hostName;
		}
		public Router GetNeighborRouter() {
			return neighborRouter;
        }
	}
}
