using System;
using System.Collections;

public class Router
{
	String ip;
	ArrayList<String> availablePorts;
	ArrayList<Host> availableHost;

	public Router(String ip,ArrayList<String> ports, ArrayList<Host> hosts)
	{
		this.ip = ip;
		this.availablePorts = ports;
		this.availableHost = hosts;
	}
}
