using ManagementCenter;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ManagementCenter { 
	public class Router : Node {

		
		protected LinkedList<Host> availableHost;

		//public Router(int id,String ip, LinkedList<String> ports, LinkedList<Host> hosts)
		public Router(int id, String ip, LinkedList<int> ports, LinkedList<Host> availableHost) : base(id, ip)
		{
			this.availableHost=availableHost;
			availablePorts = ports;
		}

	}
}
