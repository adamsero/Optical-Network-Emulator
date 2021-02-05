using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ClientNode {
    class ConfigLoader {
		public static int nodeID;
		public static String ip;
		public static int port;
		private static int asID;
		private static int routerID;
		public static int ccPort;

		public static readonly LinkedList<Tuple<int,String,int, int>> otherHosts = new LinkedList<Tuple<int, String, int, int>>();    //ID|IP|PORT
		//public static readonly LinkedList<Tuple<int, String, int, int, int>> otherAvailableHosts = new LinkedList<Tuple<int, String, int, int,>>(); //ID|IP|PORT|LABEL

		public static void LoadConfig(string file,string id) {

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(file);
			XmlElement root = doc.DocumentElement;
			XmlNodeList hostNodesList = root.SelectNodes("/config/hosts/host");
			XmlNodeList controlCenterList = root.SelectNodes("/config/control-centers/control-center");
			//XmlNodeList otherHostsNodesList = root.SelectNodes("/config/management-center/hosts-config/host-possible-destinations");

			nodeID = Int32.Parse(id);
			
			foreach (XmlNode node in hostNodesList) {

				if (nodeID == Int32.Parse(node.Attributes["id"].Value)) {
					asID = Int32.Parse(node.Attributes["as-id"].Value);
					routerID = Int32.Parse(node.Attributes["router-id"].Value);
					ip = node.SelectSingleNode("host-ip").InnerText;
					port = Int32.Parse(node.SelectSingleNode("host-port").InnerText);

					foreach (XmlNode n in controlCenterList) { 
						if(Int32.Parse(n.Attributes["id"].Value) == asID) {
							ccPort = Int32.Parse(n.Attributes["listening-port"].Value);
							break;
						}
					}

				}
				else {
					otherHosts.AddLast(new Tuple<int, String, int, int>(Int32.Parse(node.Attributes["id"].Value), node.SelectSingleNode("host-ip").InnerText, Int32.Parse(node.SelectSingleNode("host-port").InnerText), Int32.Parse(node.Attributes["as-id"].Value)));
				}

			}

			/*
			//inne hosty i ich etykiety
			foreach (XmlNode n in otherHostsNodesList) {
				if (int.Parse(n.Attributes["host-id"].Value) == nodeID) {
					XmlNodeList list = n.SelectNodes("host-possible-destination");

					foreach (XmlNode hostNode in list) {
						int label = Int32.Parse(hostNode.Attributes["label"].Value);

						foreach (Tuple<int,string,int> t in otherHosts) {
							if (int.Parse(hostNode.Attributes["destination-host-id"].Value) == t.Item1) {
								
								otherAvailableHosts.AddLast(new Tuple<int,string,int,int>(t.Item1, t.Item2, t.Item3, label));
								break;
							}
						}
					}
					break;
				}
			}
			*/
			CloudConnection.ClientIP = ip + "/24";
			CloudConnection.ClientPort = (ushort)port;
			CloudConnection.asID = asID;
			GUIWindow.PrintLog("Config loaded: " + id + "|" + ip + "|" + port);
			GUIWindow.ChangeWindowName("Host" + nodeID);
			//GUIWindow.AddDestinations();
            GUIWindow.ChangeIP(ip);
		}

	}
}
