using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ControlCenter {
	class ConfigLoader {

		public static int ccID;
		public static int ccListeningPort;
		public static LinkedList<Host> hosts;
		public static LinkedList<Router> routers;
		//public static LinkedList<Connection> connections;
		public static Dictionary<int, Connection> connections;

		public static LinkedList<Host> myHosts;
		public static LinkedList<Host> otherHosts;

		public static LinkedList<Router> myRouters;
		public static LinkedList<Router> otherRouters;

		public static Dictionary<int, Connection> myConnections;
		public static Dictionary<int, Connection> otherConnections;

		public ConfigLoader() { }

		public static LinkedList<Host> GetHosts() {
			return hosts;
		}

		public static LinkedList<Router> GetRouters() {
			return routers;
		}

		public static void loadConfig(String config, String id) {

			ccID = Int32.Parse(id);

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(config);

			int nodeID;
			int asID;
			String ip;
			int port;
			bool subnetworkRouter = false;
			//LinkedList<int> ports = new LinkedList<int>();

			XmlElement root = doc.DocumentElement;
			XmlNodeList hostNodesList = root.SelectNodes("/config/hosts/host");
			XmlNodeList routerNodesList = root.SelectNodes("/config/routers/router");
			XmlNodeList connectionNodesList = root.SelectNodes("/config/cloud/connections/connection");
			XmlNodeList controlCenterNodesList = root.SelectNodes("/config/control-centers/control-center");

			//HOSTS:
			LinkedList<Host> hosts = new LinkedList<Host>();
			//ROUTERS: 
			LinkedList<Router> routers = new LinkedList<Router>();
			//CONNECTIONS:
			//LinkedList<Connection> connections = new LinkedList<Connection>();
			Dictionary<int, Connection> connections = new Dictionary<int, Connection>();

			foreach (XmlNode n in controlCenterNodesList) {
				if (Int32.Parse(n.Attributes["id"].Value) == ccID) {
					ccListeningPort = Int32.Parse(n.Attributes["listening-port"].Value);
					break;
				}
			}

			//routery
			foreach (XmlNode node in routerNodesList) {

				nodeID = Int32.Parse(node.Attributes["id"].Value);
				asID = Int32.Parse(node.Attributes["as-id"].Value);
				ip = node.SelectSingleNode("router-ip").InnerText;
				subnetworkRouter = Boolean.Parse(node.Attributes["subnetwork-router"].Value);

				LinkedList<int> ports = new LinkedList<int>();
				XmlNodeList routerPortsList = node.SelectNodes("router-ports/router-port");
				foreach (XmlNode portNode in routerPortsList) {
					ports.AddLast(Int32.Parse(portNode.InnerText));
				}
				routers.AddLast(new Router(nodeID, ip, asID, ports, subnetworkRouter));
			}

			//hosty
			foreach (XmlNode node in hostNodesList) {
				nodeID = Int32.Parse(node.Attributes["id"].Value);
				asID = Int32.Parse(node.Attributes["as-id"].Value);
				ip = node.SelectSingleNode("host-ip").InnerText;
				port = Int32.Parse(node.SelectSingleNode("host-port").InnerText);
				
				Router neighbor = null;
				foreach (Router r in routers) {
					if (r.GetRouterID() == Int32.Parse(node.Attributes["router-id"].Value)) {
						neighbor = r;
						break;
					}
				}

				hosts.AddLast(new Host(nodeID, ip, asID, port, neighbor)); 
			}

			//polaczenia
			foreach (XmlNode node in connectionNodesList) {
				nodeID = 0;
				asID = 0;
				int portA = 0;
				int portB = 0;
				
				nodeID = Int32.Parse(node.Attributes["id"].Value);
				String connectionType = node.Attributes["type"].Value;
				int distance = Int32.Parse(node.Attributes["distance"].Value);
				double maxBandwidth = Double.Parse(node.Attributes["max-bandwidth"].Value);
				bool external = Boolean.Parse(node.Attributes["external"].Value);
				asID = Int32.Parse(node.Attributes["as-id"].Value);

				XmlNodeList childNodes;
				childNodes = node.SelectNodes("endpoint");
				if (connectionType.Equals("host-router")) {
					Host host = null;
					Router router = null;

					foreach (XmlNode childNode in childNodes) {
						int ID = Int32.Parse(childNode.Attributes["id"].Value);
						if (childNode.Attributes["type"].Value.Equals("host")) {
							foreach (Host searchHost in hosts) {
								if (searchHost.GetHostID() == ID) {
									host = searchHost;
									portA = 100;
								}
							}
						}
						if (childNode.Attributes["type"].Value.Equals("router")) {
							foreach (Router searchRouter in routers) {
								if (searchRouter.GetRouterID() == ID) {
									router = searchRouter;
									portB = Int32.Parse(childNode.Attributes["port"].Value);
								}
							}
						}
					}
					if (router != null && host != null) {
						//connections.AddLast(new Connection(nodeID, host, router, distance, maxBandwidth, external, asID));
						connections.Add(nodeID, new Connection(nodeID, host, router, distance, maxBandwidth, external, asID,new Tuple<int,int>(portA,portB)));
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
									portA = Int32.Parse(childNode.Attributes["port"].Value);
								}
								else {
									b = searchRouter;
									portB = Int32.Parse(childNode.Attributes["port"].Value);
								}
							}
						}
					}
					if (a != null && b != null) {
						connections.Add(nodeID,new Connection(nodeID, a, b, distance, maxBandwidth, external, asID, new Tuple<int, int>(portA, portB)));
					}
				}
			}
			ConfigLoader.hosts = hosts;
			ConfigLoader.routers = routers;
			ConfigLoader.connections = connections;
            GUIWindow.ChangeWindowName("ControlCenter" + ccID);
        
			sortItems();

            //foreach (Connection edge in myConnections.Values) {
            //    GUIWindow.PrintLog(edge.GetID().ToString());
            //}
        }

		public static void sortItems() {

			myHosts = new LinkedList<Host>();
			otherHosts = new LinkedList<Host>();

			myRouters = new LinkedList<Router>();
			otherRouters = new LinkedList<Router>();

			myConnections = new Dictionary<int, Connection>();
			otherConnections = new Dictionary<int, Connection>();

			foreach (Host h in hosts) {
				if (h.GetAsID() == ccID) {
					myHosts.AddLast(h);
				}
				else {
					otherHosts.AddLast(h);
				}
			}

			foreach (Router r in routers) {
				if (r.GetAsID() == ccID) {
					myRouters.AddLast(r);
				}
				else {
					otherRouters.AddLast(r);
				}
			}

			foreach (var c in connections) {
				if (c.Value.GetAsID() == ccID) {
					myConnections.Add(c.Value.GetID(),c.Value);
				}
				else {
					otherConnections.Add(c.Value.GetID(), c.Value);
				}
			}

		}

        public static Node GetNodeByIP(string ip) {
            foreach (Host host in myHosts)
                if (host.getIP().Equals(ip))
                    return host;

            foreach (Host host in otherHosts)
                if (host.getIP().Equals(ip))
                    return host;

            foreach (Router router in myRouters)
                if (router.getIP().Equals(ip))
                    return router;

            foreach (Router router in otherRouters)
                if (router.getIP().Equals(ip))
                    return router;

            return null;
        }

        public static Host FindHostByID(int id) {
            foreach(Host host in myHosts) {
                if (host.GetHostID() == id)
                    return host;
            }
            return null;
        }

        public static Host FindHostAmongAll(int id) {
            foreach (Host host in myHosts) {
                if (host.GetHostID() == id)
                    return host;
            }

            foreach (Host host in otherHosts) {
                if (host.GetHostID() == id)
                    return host;
            }
            return null;
        }

        public static Router FindRouterByID(int id) {
            foreach (Router router in myRouters) {
                if (router.GetHostID() == id)
                    return router;
            }
            return null;
        }

        public static Router FindRouterAmongAll(int id) {
            foreach (Router router in myRouters) {
                if (router.GetHostID() == id)
                    return router;
            }

            foreach (Router router in otherRouters) {
                if (router.GetHostID() == id)
                    return router;
            }
            return null;
        }
    }
}
