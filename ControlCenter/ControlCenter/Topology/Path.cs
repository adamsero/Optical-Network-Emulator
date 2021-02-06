using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class Path : IComparable {

        public LinkedList<Connection> edges = new LinkedList<Connection>();
        public LinkedList<Router> pathRouters;
        public Tuple<Node, Node> startEnd;
        private int length = 0;

        public Path(List<int> nodeList) {
            int[] nodeArray = nodeList.ToArray();

            for(int i = 0; i < nodeArray.Length - 1; i++) {
                Node n1, n2;
                if (nodeArray[i] <= 10)
                    n1 = ConfigLoader.FindRouterByID(nodeArray[i]);
                else
                    n1 = ConfigLoader.FindHostByID(nodeArray[i] - 10);
                if (nodeArray[i + 1] <= 10)
                    n2 = ConfigLoader.FindRouterByID(nodeArray[i + 1]);
                else
                    n2 = ConfigLoader.FindHostByID(nodeArray[i + 1] - 10);

                edges.AddLast(SelectEdge(n1, n2));
            }
        }

        private Connection SelectEdge(Node from, Node to) {
            foreach (Connection edge in ConfigLoader.myConnections.Values) {
                Node n1 = edge.endPoints.Item1;
                Node n2 = edge.endPoints.Item2;
                if ((n1 == from && n2 == to) || (n2 == from && n1 == to))
                    return edge;
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
                line += edge.endPoints.Item1.GetRouterID() + "-" + edge.endPoints.Item2.GetRouterID() + "(" + edge.GetID() + ")    ";
            }
            GUIWindow.PrintLog(line);
        }
    }
}
