using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class Algorithms {

        public static List<Path> AllPaths(Host sender, Host receiver) {

            if (sender == null || receiver == null) {
                GUIWindow.PrintLog("One or both host IDs do not exist!");
                return null;
            }

            Graph graph = new Graph(ConfigLoader.routers.Count + 1);
            Router initialRouter = null;
            Router finalRouter = null;

            int i = 0;
            foreach (Connection edge in ConfigLoader.connections.Values) {
                Node n1 = edge.endPoints.Item1;
                Node n2 = edge.endPoints.Item2;
                if (!n1.working || !n2.working)
                    continue;

                if (n1 is Router && n2 is Router) {
                    graph.addEdge(n1.GetRouterID(), n2.GetRouterID());
                    graph.addEdge(n2.GetRouterID(), n1.GetRouterID());
                } else if (n1 is Router && n2 is Host) {
                    if ((Host)n2 == sender)
                        initialRouter = (Router)n1;
                    else if ((Host)n2 == receiver)
                        finalRouter = (Router)n1;
                } else if (n1 is Host && n2 is Router) {
                    if ((Host)n1 == sender)
                        initialRouter = (Router)n2;
                    else if ((Host)n1 == receiver)
                        finalRouter = (Router)n2;
                }
            }

            if (initialRouter == null || finalRouter == null) {
                GUIWindow.PrintLog("Could not calculate shortest paths (either the IDs are incorrect or graph was incorrectly configured)!");
                return null;
            }

            //Console.WriteLine("Paths between " + initialRouter.GetRouterID() + " and " + finalRouter.GetRouterID());
            graph.printAllPaths(initialRouter.GetRouterID(), finalRouter.GetRouterID());
            List<List<int>> allPathsNodes = graph.allPaths;

            List<Path> allPaths = new List<Path>();

            foreach (List<int> path in allPathsNodes) {
                allPaths.Add(new Path(path, sender, receiver));
            }

            allPaths.Sort();

            return allPaths;
        }

    }
}
