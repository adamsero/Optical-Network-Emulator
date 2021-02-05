using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
	public class Host : Node {

		protected String hostName;

		public Host(int id, String ip, int asID, int port) : base(id, ip, asID) {
			availablePorts.AddLast(port);
			hostName = "Host" + this.id;
		}

		public String GetHostName() {
			return hostName;
		}
	}
}
