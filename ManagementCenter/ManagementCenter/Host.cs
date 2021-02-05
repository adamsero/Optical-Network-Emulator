using System;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;

namespace ManagementCenter {
	public class Host : Node {
	
		public Host(int id, String ip,int port):base(id, ip)
		{
			availablePorts.AddLast(port);
		}		
	}
}
