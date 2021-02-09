using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class Path : IComparable {

        public LinkedList<Connection> edges = new LinkedList<Connection>();
        public List<int> routerIDs = new List<int>();
        public Tuple<Host, Host> endPoints;
        private int length = 0;
        public bool throughSN;
        public string channelRange;

        public Path(List<int> routersList, Host initial, Host final) {
            int[] routers = routersList.ToArray();
            routerIDs = new List<int>(routersList);
            endPoints = new Tuple<Host, Host>(initial, final);

            edges.AddLast(SelectEdge(initial, FindRouterByID(routers[0])));
            for (int i = 0; i < routers.Length - 1; i++) {
                edges.AddLast(SelectEdge(FindRouterByID(routers[i]), FindRouterByID(routers[i + 1])));
            }
            edges.AddLast(SelectEdge(FindRouterByID(routers[routers.Length - 1]), final));

            foreach (Connection edge in edges) {
                length += edge.distance;
            }
        }

        private Connection SelectEdge(Node from, Node to) {
            foreach (Connection edge in from.connections) {
                Node n1 = edge.endPoints.Item1;
                Node n2 = edge.endPoints.Item2;
                if ((n1 == from && n2 == to) || (n2 == from && n1 == to))
                    return edge;
            }
            return null;
        }

        private Router FindRouterByID(int id) {
            foreach (Router router in ConfigLoader.routers) {
                if (router.GetRouterID() == id) {
                    if (router.GetAsID() == 3)
                        throughSN = true;
                    return router;
                }
            }
            return null;
        }

        public int[] getEdges() {
            int[] IDs = new int[edges.Count];
            int i = 0;
            foreach (Connection n in edges) {
                IDs[i++] = n.GetID();
            }
            return IDs;
        }

        public int GetLength() {
            return length;
        }

        public int CompareTo(object obj) {
            Path otherPath = obj as Path;
            return length.CompareTo(otherPath.length);
        }

        public void PrintPath() {
            string line = "";
            foreach (Connection edge in edges) {
                line += edge.endPoints.Item1.GetRouterID() + "-" + edge.endPoints.Item2.GetRouterID() + "   ";
            }
            GUIWindow.PrintLog(line);
        }

        public override string ToString() {
            string line = "";
            int lastID = 0;
            foreach (Connection edge in edges) {
                int id1 = edge.endPoints.Item1.GetRouterID();
                int id2 = edge.endPoints.Item2.GetRouterID();

                if(id1 != lastID) {
                    line += id2 + "-" + id1 + " ";
                    lastID = id1;
                } else {
                    line += id1 + "-" + id2 + " ";
                    lastID = id2;
                }
                
            }
            return line;
        }
    }
}
