using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Cloud
{
    class ConfigLoader
    {
		

		public ConfigLoader() { }

		public LinkedList<Tuple<int, String, int, String, int>> loadConfig() 
		{
			//|id||ipA|portA||ipB|portB|
			//ta link lista jest do zamiany pewnie - na szybko ją akurat wzialem
			LinkedList<Tuple<int, String, int, String, int>> availableConnections = new LinkedList<Tuple<int, string, int, string, int>>();

			XmlDocument doc = new XmlDocument(); 
            doc.LoadXml(Properties.Resources.tsst_config); 
            //doc.LoadXml("./../sharedResources/tsst_config.xml");

            XmlElement root = doc.DocumentElement;
			XmlNodeList hostNodesList = root.SelectNodes("/config/hosts/host"); 
			XmlNodeList routerNodesList = root.SelectNodes("/config/routers/router");
			XmlNodeList connectionNodesList = root.SelectNodes("/config/cloud/connections/connection");

			//przeszukuje wszystkie polaczenia
			foreach (XmlNode node in connectionNodesList)
			{
				int id = Int32.Parse(node.Attributes["id"].Value);
				String connectionType = node.Attributes["type"].Value;

				XmlNodeList childNodes;
				childNodes = node.SelectNodes("endpoint");
				String ip1 = null, ip2 = null;
				int port1 = 0, port2 = 0;
				
				//sprawdzam typ polaczenia
				if (connectionType.Equals("host-router"))
				{ 
					//iteruje po endpointach - zbieram info do kupy i pakuję w liste tupli
					foreach (XmlNode childNode in childNodes)
					{
						
						int ID = Int32.Parse(childNode.Attributes["id"].Value);
						if (childNode.Attributes["type"].Value.Equals("host"))
						{
							foreach (XmlNode hostNode in hostNodesList)
							{
								if (Int32.Parse(hostNode.Attributes["id"].Value) == ID)
								{
									ip1 = hostNode.SelectSingleNode("host-ip").InnerText;
									port1 = Int32.Parse(hostNode.SelectSingleNode("host-port").InnerText);
									break;
								}
							}		
						}
						if (childNode.Attributes["type"].Value.Equals("router"))
						{
							foreach (XmlNode routerNode in routerNodesList)
							{
								if (Int32.Parse(routerNode.Attributes["id"].Value) == ID)
								{
									ip2 = routerNode.SelectSingleNode("router-ip").InnerText;
									break;
								}
							}
							port2 = Int32.Parse(childNode.Attributes["port"].Value);
						}
					}

					availableConnections.AddLast(new Tuple<int,String,int,String,int>(id,ip1,port1,ip2,port2));

				}
				else if (connectionType.Equals("router-router"))
				{
					//iteruje po endpointach - zbieram info do kupy i pakuję w liste tupli
					foreach (XmlNode childNode in childNodes)
					{
						int ID = Int32.Parse(childNode.Attributes["id"].Value);
						if (ip1 == null && port1 == 0) {
							foreach (XmlNode routerNode in routerNodesList)
							{
								if (Int32.Parse(routerNode.Attributes["id"].Value) == ID)
								{
									ip1 = routerNode.SelectSingleNode("router-ip").InnerText;
									break;
								}
							}
							port1 = Int32.Parse(childNode.Attributes["port"].Value);
						}
						if (ip1 != null && port1 != 0)
						{
							foreach (XmlNode routerNode in routerNodesList)
							{
								if (Int32.Parse(routerNode.Attributes["id"].Value) == ID)
								{
									ip2 = routerNode.SelectSingleNode("router-ip").InnerText;
									break;
								}
							}
							port2 = Int32.Parse(childNode.Attributes["port"].Value);
						}
					}
					availableConnections.AddLast(new Tuple<int, string, int, string, int>(id, ip1, port1, ip2, port2));
				}
			}
			return availableConnections;
		}

	}
}
