using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NetworkNode {
    class ConfigLoader {

		public static int nodeID; 
		public static String ip;
		public static int asID;
		public static int ccPort;
		public static bool subnetworkRouter = false;
		public static readonly LinkedList<int> ports = new LinkedList<int>();	    //ID|IP|PORT

		public static void LoadConfig(string file, string id) {

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(file);
			XmlElement root = doc.DocumentElement;
			XmlNodeList routerNodesList = root.SelectNodes("/config/routers/router");
			XmlNodeList controlCenterList = root.SelectNodes("/config/control-centers/control-center");

			nodeID = Int32.Parse(id);
		
			foreach (XmlNode node in routerNodesList) {
				if (nodeID == Int32.Parse(node.Attributes["id"].Value)) {
					asID = Int32.Parse(node.Attributes["as-id"].Value);
					ip = node.SelectSingleNode("router-ip").InnerText;
					subnetworkRouter = Boolean.Parse(node.Attributes["subnetwork-router"].Value);

					XmlNodeList routerPortsList = node.SelectNodes("router-ports/router-port");
					foreach (XmlNode portNode in routerPortsList) {
						ports.AddLast(Int32.Parse(portNode.InnerText));
					}
					foreach (XmlNode n in controlCenterList) {
						if (Int32.Parse(n.Attributes["id"].Value) == asID) {
							ccPort = Int32.Parse(n.Attributes["listening-port"].Value);
							break;
						}
					}
					break;
				}
			}
			String msg = "";
			int iter = 0;
			ushort[] tmpPorts = new ushort[ports.Count];
			foreach (int i in ports) {
				msg = msg + "|" + i;
				tmpPorts[iter] = (ushort)ports.ToArray()[iter];
				iter++;
			}

			CloudConnection.ClientIP = ip + "/24";
			CloudConnection.ClientPorts = tmpPorts;
			CloudConnection.asID = asID;
			CloudConnection.subnetworkRouter = subnetworkRouter;
			GUIWindow.PrintLog("Config loaded: " + id + "|" + ip + msg);
			GUIWindow.ChangeWindowName("Router" + nodeID);
            GUIWindow.ChangeIP(ip);
		}
	}
    
}
