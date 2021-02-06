using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class Algorithms {

        public static List<Path> AllPaths(Node staring, Node ending) {

            if(staring == null || ending == null) {
                GUIWindow.PrintLog("One or both host Nodes do not exist!");
                return null;
            }

            Graph graph = new Graph(ConfigLoader.routers.Count + ConfigLoader.hosts.Count + 1);

            foreach(Connection edge in ConfigLoader.myConnections.Values) {
                Node n1 = edge.endPoints.Item1;
                Node n2 = edge.endPoints.Item2;
                if (!n1.working || !n2.working)
                    continue;

                graph.addEdge(n1.GetAlgorithmID(), n2.GetAlgorithmID());
                graph.addEdge(n2.GetAlgorithmID(), n1.GetAlgorithmID());
            }

            //Console.WriteLine("Paths between " + initialRouter.GetRouterID() + " and " + finalRouter.GetRouterID());
            graph.printAllPaths(staring.GetAlgorithmID(), ending.GetAlgorithmID());
            List<List<int>> allPathsNodes = graph.allPaths;

            List<Path> allPaths = new List<Path>();
            foreach (List<int> path in allPathsNodes) {
                allPaths.Add(new Path(path));
            }

            allPaths.Sort();
  
            return allPaths;
        }

    }
}
