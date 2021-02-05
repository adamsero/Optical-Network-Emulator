using ManagementCenter;
using System;
using System.Collections.Generic;
using System.Xml;

public class ConfigLoader
{
	private readonly LinkedList<Router> routers = new LinkedList<Router>();
    private readonly LinkedList<Host> hosts = new LinkedList<Host>();
    private readonly LinkedList<Connection> connections = new LinkedList<Connection>();
	
	// ||id hosta|<id wpisu, id hosta docelowego, etykieta>|
	public readonly List<Tuple<int, Tuple<int, int, int>>> hostPossibleDestinations = new List<Tuple<int, Tuple<int, int, int>>>();
    // ||id routera|<id wpisu, inputport, inputlabel, action, outputport, outputlabel, next-action-row-id(wskaznik na id wpisu), layer)>| 
    //public readonly List<Tuple<int, Tuple<int, int, int, string, int, int, int, Tuple<int>>>> routerMPLSTables = new List<Tuple<int, Tuple<int, int, int, string, int, int, int, Tuple<int>>>>();

	private LinkedList<Router> AvailableRouters;
	private LinkedList<Host> AvailableHosts;
	//private readonly LinkedList<Connection> connections = new LinkedList<Connection>();

	public void copyNodes() {
		AvailableHosts = new LinkedList<Host>(hosts);
		AvailableRouters = new LinkedList<Router>(routers);
	}
	public void LoadConfig(string config) {
		XmlDocument doc = new XmlDocument();

		doc.LoadXml(config);

		//GUIWindow.PrintLog(mode);

		XmlElement root = doc.DocumentElement;
		XmlNodeList hostNodesList = root.SelectNodes("/config/hosts/host");
		XmlNodeList routerNodesList = root.SelectNodes("/config/routers/router");
		XmlNodeList connectionNodesList = root.SelectNodes("/config/cloud/connections/connection");
		XmlNodeList hostDestinationsNodeList = root.SelectNodes("/config/management-center/hosts-config/host-possible-destinations");
		XmlNodeList routerMPLSTableNodeList = root.SelectNodes("/config/management-center/router-config/router-mpls-table");

		//hosty
		foreach (XmlNode node in hostNodesList) {
			XmlAttribute attribute = node.Attributes["id"];
			int id = Int32.Parse(attribute.Value);
			String ip = node.SelectSingleNode("host-ip").InnerText;
			int port= Int32.Parse(node.SelectSingleNode("host-port").InnerText);
			hosts.AddLast(new Host(id, ip, port));
		}

		//routery
		foreach (XmlNode node in routerNodesList) {
			XmlAttribute attribute = node.Attributes["id"];
			int id = Int32.Parse(attribute.Value);
			String ip = node.SelectSingleNode("router-ip").InnerText;

			XmlNodeList portsNodeList = node.SelectNodes("router-ports/router-port");
			LinkedList<int> portsList = new LinkedList<int>(); 
			foreach (XmlNode currentNode in portsNodeList) {
				portsList.AddLast(Int32.Parse(currentNode.InnerText));
			}

			XmlNodeList assignedHostNodeList = node.SelectNodes("assigned-hosts/assigned-host");
			LinkedList<Host> routerHosts = new LinkedList<Host>();
			foreach (XmlNode currentNode in assignedHostNodeList) {
				int assignedId = Int32.Parse(currentNode.Attributes["id"].Value);
				foreach (Host host in hosts) {
					if (host.GetHostID() == assignedId){
						routerHosts.AddLast(host);
					}
				}
			}

			routers.AddLast(new Router(id, ip, portsList, routerHosts));
		}

		//polaczenia
		foreach (XmlNode node in connectionNodesList) {
			int id = Int32.Parse(node.Attributes["id"].Value);
			String connectionType = node.Attributes["type"].Value;

			XmlNodeList childNodes;
			childNodes = node.SelectNodes("endpoint");
			if (connectionType.Equals("host-router")) {
				Host host = null;
				Router router = null;

				foreach (XmlNode childNode in childNodes) {
					int ID = Int32.Parse(childNode.Attributes["id"].Value);
					if (childNode.Attributes["type"].Value.Equals("host")) {
						foreach (Host searchHost in hosts) {
							if (searchHost.GetHostID() == ID)  {
								host = searchHost;
							}
						}
					}
					if (childNode.Attributes["type"].Value.Equals("router")) {
						foreach (Router searchRouter in routers) {
							if (searchRouter.GetRouterID() == ID) {
								router = searchRouter;
							}
						}
					}
				}
				if (router != null && host != null) {
					connections.AddLast(new Connection(id, host, router));
				}
				
			}
			else if (connectionType.Equals("router-router")) {
				Router a = null, b = null;

				foreach (XmlNode childNode in childNodes) {
					int ID = Int32.Parse(childNode.Attributes["id"].Value);

					foreach (Router searchRouter in routers) {
						if (searchRouter.GetRouterID() == ID) {
							if (a == null) {
								a = searchRouter;
							}
							else  {
								b = searchRouter;
							}
						}
					}
				}
				if (a != null && b != null) {
					connections.AddLast(new Connection(id, a, b));
				}
			}
		}

		//dostepne hosty i ich etykiety
		foreach (XmlNode hdests in hostDestinationsNodeList) {
			XmlNodeList dests = hdests.SelectNodes("host-possible-destination");

			foreach (XmlNode n in dests) {
				int id = int.Parse(n.Attributes["id"].Value);
				int destHostID = int.Parse(n.Attributes["destination-host-id"].Value);
				int label = int.Parse(n.Attributes["label"].Value);
				hostPossibleDestinations.Add(new Tuple<int, Tuple<int, int, int>>(int.Parse(hdests.Attributes["host-id"].Value), new Tuple<int, int, int>(id, destHostID, label)));
			}
		}

		//tablice mpls dla routerow
		//foreach (XmlNode mplsTableNode in routerMPLSTableNodeList) {
		//	int routerID = int.Parse(mplsTableNode.Attributes["router-id"].Value);
		//	XmlNodeList mplsTableRows = mplsTableNode.SelectNodes("router-mpls-row");

		//	foreach (XmlNode mplsRow in mplsTableRows) {
		//		int rowID = int.Parse(mplsRow.Attributes["row-id"].Value);
		//		int inputPort = int.Parse(mplsRow.Attributes["input-port"].Value);
		//		int inputLabel = int.Parse(mplsRow.Attributes["input-label"].Value);
		//		string action = mplsRow.Attributes["action"].Value;
		//		int outputPort = int.Parse(mplsRow.Attributes["output-port"].Value);
		//		int outputLabel = int.Parse(mplsRow.Attributes["output-label"].Value);
		//		int nextActionRowID = int.Parse(mplsRow.Attributes["next-action-row-id"].Value);
		//		int layer = int.Parse(mplsRow.Attributes["layer"].Value);

		//		var row = Tuple.Create(rowID, inputPort, inputLabel, action, outputPort, outputLabel, nextActionRowID, layer);
		//		routerMPLSTables.Add(new Tuple<int, Tuple<int, int, int, string, int, int, int, Tuple<int>>>(routerID, row));
		//	}
		//}

		this.copyNodes();
		GUIWindow.PrintLog("Config Loaded");
	}

	public LinkedList<Host> GetHosts()
	{
		return hosts;
	}

	public LinkedList<Router> GetRouters()
	{
		return routers;
	}

	public LinkedList<Connection> GetConnections()
	{
		return this.connections;
	}

	public Host GetAvailableHost()
	{
		if (AvailableHosts.Count > 0){
			Host CurrentHost = AvailableHosts.First.Value;
			AvailableHosts.RemoveFirst();
			return CurrentHost;
		}
		else {
			return null;
		}
	}

	public Router GetAvailableRouter() {
		if (AvailableRouters.Count > 0) {
			Router CurrentRouter = AvailableRouters.First.Value;
			AvailableRouters.RemoveFirst();
			return CurrentRouter;
		}
		else {
			return null;
		}

	}
	
	
	

}
